using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
using ChainedPuzzles;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using CullingSystem;
using Detection;
using Enemies;
using FluffyUnderware.DevTools.Extensions;
using GameData;
using Globals;
using LibCpp2IL;
using Player;
using ToggleUIPlugin.Managers;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class CinemaCamManager : MonoBehaviour
    {
	    public static event Action<float> OnRollAngleChange;

        private const float DelayBeforeLocomotionEnable = 0.1f;

        private static readonly Dictionary<string, float> PlayerPrevMaxHealthByName = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> PlayerPrevHealthByName = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> PlayerPrevInfectionByName = new Dictionary<string, float>();
 
        private static Vector3 _smoothVelocity;
        private static Vector3 _targetPosition;
        private static Vector3 _fpsStartingPosition;
        private static Quaternion _fpsStartingRotation;
        private static Vector3 _bodyStartingPosition;

        private static Transform _prevParent;
        private static Transform _cinemaCamCtrlHolder;
        private static Transform _cinemaCam;
        private static Transform _fpsCamHolderSubstitute;
        private static CinemaCamController _cinemaCamController;
        
        private static bool _freeCamEnabled;
        private static bool _dynamicRollEnabled;
        private static bool _mouseCtrlAltitudeEnabled;
        private static bool _prevFreeCamDisabled;
        private static float _prevFreeCamDisabledTime;

        private FPSCamera _fpsCamera;
        private GameObject _player;
        private GameObject _fpArms;
        private GameObject _uiPlayerLayer;
        private GameObject _uiInteractionLayer;
        private GameObject _uiNavMarkerLayer;
        private PlayerAgent _playerAgent;

        // Toggle comps when free cam enabled
        private PE_FPSDamageFeedback _damageFeedback;
        private PlayerLocomotion _playerLocomotion;

        public CinemaCamManager(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        private void Awake()
        {
	        _fpsCamera = FindObjectOfType<FPSCamera>();
			_fpArms = PlayerManager.GetLocalPlayerAgent().FPItemHolder.gameObject;
			_uiPlayerLayer = GuiManager.PlayerLayer.Root.FindChild("PlayerLayer").gameObject;
			_uiInteractionLayer = GuiManager.PlayerLayer.Root.FindChild("InteractionLayer").gameObject;
			_uiNavMarkerLayer = GuiManager.PlayerLayer.Root.FindChild("NavMarkerLayer").gameObject;

			_player = _fpsCamera.m_owner.gameObject;
			_damageFeedback = _fpsCamera.gameObject.GetComponent<PE_FPSDamageFeedback>();
			_playerAgent = PlayerManager.GetLocalPlayerAgent();
			_playerLocomotion = _player.GetComponent<PlayerLocomotion>();

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
	        CinemaUIManager.Toggles[UIOption.ToggleFreeCamera].OnValueChanged += OnFreeCameraToggle;

	        CinemaNetworkingManager.OnOtherPlayerEnterExitFreeCam += OnOtherPlayerEnterOrExitFreeCam;
        }

        private void Update()
        {
	        if (_freeCamEnabled)
	        {
		        if (_fpArms.active || _uiPlayerLayer.active)
		        {
			        // force hide all ui when in free cam
			        ToggleAllScreenClutterExceptWaterMark(false);
		        }

		        // if (!CinemaUIManager.MenuOpen)
		        // {
			       //  UpdateMovement();
		        // }
		        DivertEnemiesAwayFromCameraMan();
	        }

	        // Update locomotion a frame after to avoid rubber banding 
	        if (_prevFreeCamDisabled && Time.realtimeSinceStartup - _prevFreeCamDisabledTime > DelayBeforeLocomotionEnable / Time.timeScale)
	        {
		        _fpsCamera.m_orgParent.localPosition = Vector3.zero;
		        _playerLocomotion.enabled = true;
		        _prevFreeCamDisabled = false;
	        }
        }

        private void ToggleAllScreenClutterExceptWaterMark(bool value)
        {
	        if (value)
	        {
		        ToggleUIManager.ShowBody();
		        ToggleUIManager.ShowUI();
	        }
	        else
	        {
		        ToggleUIManager.HideBody();
		        ToggleUIManager.HideUI();
	        }

	        _uiPlayerLayer.active = value;
	        _uiInteractionLayer.active = value;
	        _uiNavMarkerLayer.active = value;
	        _damageFeedback.enabled = value;
	        ScreenLiquidManager.hasSystem = value;
        }

        private void OnFreeCameraToggle(bool value)
		{
			_freeCamEnabled = value;
			EnableOrDisableCinemaCam(value);
		}

		private void EnableOrDisableCinemaCam(bool enable)
		{
			SetCameraManHealth(_playerAgent, enable);
			ToggleAllScreenClutterExceptWaterMark(!enable);

			if (enable)
			{
				_playerLocomotion.enabled = false;
				_prevParent = _fpsCamera.m_orgParent.parent;
				
				_cinemaCamController.SyncWithCameraTransform();
				_fpsCamera.m_orgParent.parent = _fpsCamHolderSubstitute;

				_fpsCamera.MouseLookEnabled = false;
				_prevParent.gameObject.active = false;
				_cinemaCamController.enabled = true;
			}
			else
			{
				_cinemaCamController.enabled = false;
				_fpsCamera.MouseLookEnabled = true;
				_prevParent.gameObject.active = true;

				_fpsCamera.m_orgParent.parent = _prevParent;

				// Enable player locomotion later to avoid rubber banding
				_prevFreeCamDisabledTime = Time.realtimeSinceStartup;
				_prevFreeCamDisabled = true;
			}

			CinematographyCore.log.LogMessage(enable ? "Cinema cam enabled" : "Cinema cam disabled");
		}

		private void SetCameraManHealth(PlayerAgent player, bool enteringFreeCam)
		{
			var playerName = player.Sync.PlayerNick;
			var damage = player.Damage;

			if (enteringFreeCam)
			{
				PlayerPrevMaxHealthByName.TryAdd(playerName, damage.HealthMax);
				PlayerPrevHealthByName.TryAdd(playerName, damage.Health);
				PlayerPrevInfectionByName.TryAdd(playerName, damage.Infection);

				damage.HealthMax = 999999;
				damage.Health = 999999;
				damage.Infection = 0;
			}
			else if (PlayerPrevMaxHealthByName.ContainsKey(playerName))
			{
				damage.HealthMax = PlayerPrevMaxHealthByName[playerName];
				damage.Health = PlayerPrevHealthByName[playerName];
				damage.Infection = PlayerPrevInfectionByName[playerName];
				
				PlayerPrevMaxHealthByName.Remove(playerName);
				PlayerPrevHealthByName.Remove(playerName);
				PlayerPrevInfectionByName.Remove(playerName);

				if (player.IsLocallyOwned)
				{
					damage.TryCast<Dam_PlayerDamageLocal>().UpdateHealthGui();
				}
			}
		}
		
		private void ShowOrHideFreeCamPlayer(PlayerAgent player, bool enteringFreeCam)
		{
			player.AnimatorBody.gameObject.active = !enteringFreeCam;
			player.NavMarker.m_marker.enabled = !enteringFreeCam;
		}

		private void OnOtherPlayerEnterOrExitFreeCam(PlayerAgent playerAgent, bool enteringFreeCam)
		{
			CinematographyCore.log.LogInfo($"{playerAgent.Sync.PlayerNick} entering free cam : {enteringFreeCam}");
			SetCameraManHealth(playerAgent, enteringFreeCam);
			ShowOrHideFreeCamPlayer(playerAgent, enteringFreeCam);
		}
		
		private void DivertEnemiesAwayFromCameraMan()
		{
			if (PlayerManager.PlayerAgentsInLevel.Count == 1) return;
			
			foreach (var playerAgent in CinemaNetworkingManager.PlayersInFreeCamByName.Values)
			{
				foreach (var attacker in new List<Agent>(playerAgent.GetAttackers().ToArray()))
				{
					playerAgent.UnregisterAttacker(attacker);
					var delegateAgent = CinemaNetworkingManager.PlayersNotInFreeCamByName.Values.Aggregate(
							(currMin, pa) => pa.GetAttackersScore() < currMin.GetAttackersScore() ? pa : currMin);
					CinematographyCore.log.LogInfo($"Diverting {attacker.name} to {delegateAgent}");
					attacker.TryCast<EnemyAgent>().AI.SetTarget(delegateAgent);
				}
			}
		}

		private void OnDestroy()
		{
			CinemaUIManager.Toggles[UIOption.ToggleFreeCamera].OnValueChanged -= OnFreeCameraToggle;
	
			CinemaNetworkingManager.OnOtherPlayerEnterExitFreeCam -= OnOtherPlayerEnterOrExitFreeCam;
		}
    }
}