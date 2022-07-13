using Agents;
using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using CinematographyPlugin.UI.UiInput;
using Enemies;
using FluffyUnderware.DevTools.Extensions;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class CinemaCamManager : MonoBehaviour
    {
        public static CinemaCamManager Current;

        private const float RayCastMax = 50;
        private const float RayCastRadius = 0.5f;
        private const float OrbitReselectDelay = 0.5f;
        private const float PlayerBodyOffset = 2f;
        private const float GodModeDisableDelay = 3f;

        private readonly int _playerLayerMask = LayerMask.GetMask("PlayerSynced");
        private readonly int _enemyLayerMask = LayerMask.GetMask("EnemyDamagable");
        private readonly int _enemyLayer = LayerMask.NameToLayer("EnemyDamagable");

        private bool _freeCamEnabled;
        private bool _inOrbit;
        private bool _orbitTargetSet;
        private string _orbitTargetName;
        private float _lastOrbitDeselect;
        private float _lastFreeCamEnded;
        private Vector3 _originalPlayerPos;
        private RaycastHit _cameraHit;
        private GameObject _prevHit = new ();
        private FPSCamera _fpsCamera;
        private Agent _orbitTarget;
        private Transform _prevParent;
        private Transform _cinemaCamCtrlHolder;
        private Transform _cinemaCam;
        private Transform _fpsCamHolderSubstitute;
        private PlayerAgent _playerAgent;
        private PlayerLocomotion _playerLocomotion;
        private CinemaCamController _cinemaCamController;
        private Dictionary<String, SphereCollider> _shieldSphereByAgentName = new ();

        private void Awake()
        {
            Current = this;
            
            // Comps reference set up
            _playerAgent = PlayerManager.GetLocalPlayerAgent();
            _fpsCamera = _playerAgent.FPSCamera;
            _playerLocomotion = _playerAgent.GetComponent<PlayerLocomotion>();

            // Cinema cam obj set up
            _cinemaCam = new GameObject("CinemaCam").transform;
            var cinemaCamCamRotation = new GameObject("CinemaCamRotation").transform;
            _fpsCamHolderSubstitute = new GameObject("FPSCamHolderSubstitute").transform;
            _cinemaCamCtrlHolder = new GameObject("CinemaCamControl").transform;
			
            _fpsCamHolderSubstitute.parent = cinemaCamCamRotation;
            cinemaCamCamRotation.parent = _cinemaCamCtrlHolder;
            _cinemaCamCtrlHolder.parent = _cinemaCam;

            _cinemaCamController = _cinemaCamCtrlHolder.gameObject.AddComponent<CinemaCamController>();
            _cinemaCamController.enabled = false;
        }

        private void Start()
        {
            CinemaUIManager.Current.Toggles[UIOption.ToggleFreeCamera].OnValueChanged += OnFreeCameraToggle;

            CinemaNetworkingManager.OnOtherPlayerEnterExitFreeCam += OnOtherPlayerEnterOrExitFreeCam;
        }

        private void Update()
        {
            if (_freeCamEnabled)
            {
                CheckAndForceUiHidden();
                DivertEnemiesAwayFromCameraMan();
                UpdateOrbitCamState();
                CheckPlayerWarp();
            }
            
            CheckFreeCamHotKey();
        }

        public bool FreeCamEnabled()
        {
            return _freeCamEnabled;
        }
        
        public bool InGodMode()
        {
            return _freeCamEnabled && Time.realtimeSinceStartup - _lastFreeCamEnded < GodModeDisableDelay;
        }

        public Vector3 GetOriginalPlayerPosition()
        {
            return _originalPlayerPos;
        }
        
        public void EnableOrDisableCinemaCam(bool enable)
        {
            ScreenClutterController.GetInstance().ToggleAllScreenClutter(!enable);

            if (enable)
            {
                _playerLocomotion.enabled = false;
                _prevParent = _fpsCamera.m_orgParent.parent;
				
                _cinemaCamController.SyncWithCameraTransform();
                _fpsCamera.m_orgParent.parent = _fpsCamHolderSubstitute;

                _fpsCamera.MouseLookEnabled = false;
                _prevParent.gameObject.active = false;
                _cinemaCamController.enabled = true;

                UpdatePlayerPositionAndShield(_playerAgent, true);
            }
            else
            {
                _cinemaCamController.enabled = false;
                _fpsCamera.MouseLookEnabled = true;
                _prevParent.gameObject.active = true;
                
                _fpsCamera.m_orgParent.parent = _prevParent;
                
                _fpsCamera.m_orgParent.localPosition = Vector3.zero;
                _playerLocomotion.enabled = true;

                if (_inOrbit)
                {
                    DisconnectOrbit();
                }
                
                _lastFreeCamEnded = Time.realtimeSinceStartup;
                
                UpdatePlayerPositionAndShield(_playerAgent, false);
                
                _playerLocomotion.DisableFallDamageTemporarily = true;
            }

            CinematographyCore.log.LogMessage(enable ? "Cinema cam enabled" : "Cinema cam disabled");
        }

        private void UpdatePlayerPositionAndShield(PlayerAgent agent, bool setUp)
        {
            UpdatePlayerPositionAndShield(agent, agent.transform.position, setUp);
        }

        public void UpdatePlayerPositionAndShield(PlayerAgent agent, Vector3 playerPosition, bool setUp)
        {
            if (setUp)
            {
                _originalPlayerPos = playerPosition;
            }
            
            var newPlayerPosition = new Vector3(playerPosition.x, playerPosition.y + (setUp ? -PlayerBodyOffset : +PlayerBodyOffset), playerPosition.z);
            agent.transform.position = newPlayerPosition;
            _playerAgent.TryCast<LocalPlayerAgent>()?.PlayerCharacterController.ManualMoveTo(newPlayerPosition);
            
            var agentName = agent.Sync.name;
            if (!_shieldSphereByAgentName.ContainsKey(agentName))
            {
                _shieldSphereByAgentName[agentName] = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<SphereCollider>();
                _shieldSphereByAgentName[agentName].GetComponent<MeshRenderer>().enabled = false;
                _shieldSphereByAgentName[agentName].transform.localScale = new Vector3(3f, 3f, 3f);
            }
            
            _shieldSphereByAgentName[agentName].transform.position = newPlayerPosition;
            _shieldSphereByAgentName[agentName].enabled = setUp;
        }

        private void CheckFreeCamHotKey()
        {
            if (KeyBindInputManager.GetFreeCamToggle())
            {
                CinemaUIManager.Current.Toggles[UIOption.ToggleFreeCamera].Toggle.isOn = !_freeCamEnabled;
            }
        }

        private void CheckAndForceUiHidden()
        {
            if (ScreenClutterController.GetInstance().IsBodyOrUiVisible())
            {
                // force hide all ui when in free cam
                ScreenClutterController.GetInstance().ToggleAllScreenClutter(false);
            }
        }

        private void OnFreeCameraToggle(bool value)
        {
            EnableOrDisableCinemaCam(value);
            _freeCamEnabled = value;
        }

        private void UpdateOrbitCamState()
        {
            if (KeyBindInputManager.GetOrbitTargetSelect() && Time.realtimeSinceStartup - _lastOrbitDeselect > OrbitReselectDelay)
            {
                if (Physics.SphereCast(_fpsCamera.m_camRay, RayCastRadius, out _cameraHit, RayCastMax, _playerLayerMask | _enemyLayerMask))
                {
                    if (_prevHit.GetInstanceID() != _cameraHit.collider.gameObject.GetInstanceID())
                    {
                        var agent = _cameraHit.collider.GetComponentInParent<Agent>();
                        _orbitTarget = agent;
                        
                        if (_cameraHit.collider.gameObject.layer == _enemyLayer)
                        {
                            _orbitTargetName = agent.TryCast<EnemyAgent>()!.EnemyData.name;
                        }
                        else
                        {
                            _orbitTargetName = agent.TryCast<PlayerAgent>()!.PlayerName;
                        }

                    }

                    _prevHit = _cameraHit.collider.gameObject;
                    _orbitTargetSet = true;
                    CinemaUIManager.Current.ShowTextOnScreen(_orbitTargetName);
                }
                else
                {
                    CinemaUIManager.Current.ShowNoTargetTextOnScreen();
                    _orbitTargetSet = false;
                }
                
                if (_inOrbit)
                {
                    DisconnectOrbit();
                }
            }
            else
            {
                CinemaUIManager.Current.HideTextOnScreen();
                if (!_inOrbit && _orbitTargetSet)
                {
                    SetOrbit();
                }
            }

            if (_orbitTargetSet && !_orbitTarget.Alive)
            {
                DisconnectOrbit();
            }
        }

        private void SetOrbit()
        {
            _cinemaCamCtrlHolder.transform.parent = _orbitTarget.transform;
            _cinemaCamController.SetOrbit(_orbitTarget);
            _inOrbit = true;
        }

        private void DisconnectOrbit()
        {
            _cinemaCamCtrlHolder.transform.parent = _cinemaCam;
            _cinemaCamController.DisableOrbit();
            _inOrbit = false;
            _orbitTargetSet = false;
            _lastOrbitDeselect = Time.realtimeSinceStartup;
        }

        private void OnOtherPlayerEnterOrExitFreeCam(PlayerAgent playerAgent, bool enter)
        {
            var enterExitTxt = enter ? "entering" : "exiting";
            CinematographyCore.log.LogInfo($"{playerAgent.Sync.PlayerNick} {enterExitTxt} free cam");
            ScreenClutterController.GetInstance().ToggleClientVisibility(playerAgent, !enter);
            UpdatePlayerPositionAndShield(playerAgent, enter);
        }
		
        private void DivertEnemiesAwayFromCameraMan()
        {
            if (PlayerManager.PlayerAgentsInLevel.Count == 1 || CinemaNetworkingManager.GetPlayersInFreeCam().Count() == PlayerManager.PlayerAgentsInLevel.Count) return;
			     
            foreach (var playerAgent in CinemaNetworkingManager.GetPlayersInFreeCam())
            {
                foreach (var attacker in new List<Agent>(playerAgent.GetAttackers().ToArray()))
                {
                    if (attacker.Type != AgentType.Enemy) continue;
                    
                    playerAgent.UnregisterAttacker(attacker);
                    var delegateAgent = CinemaNetworkingManager.GetPlayersNotInFreeCam().Aggregate(
                        (currMin, pa) => pa.GetAttackersScore() < currMin.GetAttackersScore() ? pa : currMin);
                    // CinematographyCore.log.LogDebug($"Diverting {attacker.name} to {delegateAgent.name}");
                    attacker.TryCast<EnemyAgent>()!.AI.SetTarget(delegateAgent);
                }
            }
        }
        
        private void CheckPlayerWarp()
        {
            if (KeyBindInputManager.GetPlayerWarp())
            {
                var pos = _fpsCamera.transform.position;
                var lookDir = _fpsCamera.Forward;
                
                CinemaUIManager.Current.Toggles[UIOption.ToggleFreeCamera].Toggle.isOn = false;
                
                _playerAgent.transform.position = pos;
                _playerAgent.TryCast<LocalPlayerAgent>()!.PlayerCharacterController.ManualMoveTo(pos);
                _fpsCamera.SetLookDirection(lookDir);
                _fpsCamera.TransitionFX.Play();
            }
        }

        private void OnDestroy()
        {
            CinemaUIManager.Current.Toggles[UIOption.ToggleFreeCamera].OnValueChanged -= OnFreeCameraToggle;
	
            CinemaNetworkingManager.OnOtherPlayerEnterExitFreeCam -= OnOtherPlayerEnterOrExitFreeCam;
        }
    }
}