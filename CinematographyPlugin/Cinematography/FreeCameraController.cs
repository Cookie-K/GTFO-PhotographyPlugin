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
    public class FreeCameraController : MonoBehaviour
    {
	    public static event Action<float> OnRollAngleChange;
	    
        private const float FastMoveFactor = 2;
        private const float SlowMoveFactor = 1f/2f;
        
        public const float SmoothTimeDefault = 0.2f;
        public const float SmoothTimeMin = 0f;
        public const float SmoothTimeMax = 2f;

        public const float MovementSpeedDefault = 5f;
        public const float MovementSpeedMin = 0f;
        public const float MovementSpeedMax = 10f;
        
        public const float DynamicRollIntensityDefault = 5f;
        public const float DynamicRollIntensityMin = 0f;
        public const float DynamicRollIntensityMax = 10f;

        private const float DynamicRollSmoothTimeDefault = 1;
        private const float DynamicRollMax = 90f;
        
        private const float DelayBeforeLocomotionEnable = 0.1f;

        private static readonly Dictionary<string, float> PlayerPrevMaxHealthByName = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> PlayerPrevHealthByName = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> PlayerPrevInfectionByName = new Dictionary<string, float>();
        
        private static float _smoothTime = SmoothTimeDefault;
        private static float _rollIntensity = DynamicRollIntensityDefault;
        private static float _dynamicRollSmoothTime = DynamicRollSmoothTimeDefault;
        private static float _movementSpeed = MovementSpeedDefault;
        
        private static float _prevRoll;
        private static float _rollVelocity;
        private static float _lastInterval;

        private static Vector3 _smoothVelocity;
        private static Vector3 _targetPosition;
        private static Vector3 _fpsStartingPosition;
        private static Vector3 _bodyStartingPosition;

        private static Transform _prevParent;
        public static Transform FreeCamCtrl;
        private static Transform _fpsCamHolderSubstitute;
        
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

        public FreeCameraController(IntPtr intPtr) : base(intPtr)
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

			var freeCam = new GameObject("FreeCam").transform;
			var freeCamRotation = new GameObject("FreeCamRotation").transform;
			_fpsCamHolderSubstitute = new GameObject("FPSCamHolderSubstitute").transform;
			FreeCamCtrl = new GameObject("FreeCamControl").transform;
			
			_fpsCamHolderSubstitute.parent = freeCamRotation;
			freeCamRotation.parent = FreeCamCtrl;
			FreeCamCtrl.parent = freeCam;
        }

        private void Start()
        {
	        ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFreeCamera]).OnValueChanged += OnFreeCameraToggle;
	        ((SliderOption) CinemaUIManager.Options[UIOption.MovementSpeedSlider]).OnValueChanged += OnMovementSpeedChange;
	        ((SliderOption) CinemaUIManager.Options[UIOption.MovementSmoothingSlider]).OnValueChanged += OnSmoothTimeChange;

	        ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDynamicRoll]).OnValueChanged += OnDynamicRollToggle;
	        ((SliderOption) CinemaUIManager.Options[UIOption.DynamicRollIntensitySlider]).OnValueChanged += OnDynamicRollIntensityChange;
	        
	        ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleMouseCtrlAltitude]).OnValueChanged += OnMouseIndependentCtrlToggle;

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
			EnableOrDisableFreeCam(value);
		}

        private void OnSmoothTimeChange(float value)
		{
			_smoothTime = value;
		}

        private void OnMovementSpeedChange(float value)
		{
			_movementSpeed = value;
		}

        private void OnDynamicRollToggle(bool value)
		{
			_dynamicRollEnabled = value;
			if (!value)
			{
				_rollVelocity = 0;
				_prevRoll = 0;
			}
		}

        private void OnMouseIndependentCtrlToggle(bool value)
		{
			_mouseCtrlAltitudeEnabled = value;
		}

        private void OnDynamicRollIntensityChange(float value)
		{
			_rollIntensity = value;
		}

		private void EnableOrDisableFreeCam(bool enable)
		{
			SetCameraManHealth(_playerAgent, enable);
			ToggleAllScreenClutterExceptWaterMark(!enable);

			if (enable)
			{
				_playerLocomotion.enabled = false;
				_prevParent = _fpsCamera.m_orgParent.parent;
				_fpsStartingPosition = _prevParent.position;
				_targetPosition = _fpsStartingPosition;
				_bodyStartingPosition = _player.transform.position;
				
				FreeCamCtrl.parent.position = _fpsStartingPosition;
				FreeCamCtrl.parent.rotation = _fpsCamera.Rotation;
				_fpsCamera.m_orgParent.parent = _fpsCamHolderSubstitute.transform;

				FreeCamCtrl.gameObject.AddComponent<FreeCamController>();
				_fpsCamera.MouseLookEnabled = false;

				_prevParent.gameObject.active = false;
				_player.transform.position = Vector3.Scale(_bodyStartingPosition, new Vector3(1, -3, 1));
				_fpsCamera.SetPitchLimit(new Vector2(360, -360));
				_dynamicRollSmoothTime = DynamicRollSmoothTimeDefault;
			}
			else
			{
				_targetPosition = _fpsStartingPosition;
				_movementSpeed = 1;
				_smoothTime = 0;
				_dynamicRollSmoothTime = 0;
				FreeCamCtrl.rotation = Quaternion.identity;
				// UpdateMovement();

				FreeCamCtrl.gameObject.GetComponent<FreeCamController>().Destroy();
				_fpsCamera.MouseLookEnabled = true;
				
				_fpsCamera.ResetPitchLimit();

				_player.transform.position = _bodyStartingPosition;
				_prevParent.gameObject.active = true;
				_fpsCamera.m_orgParent.parent = _prevParent;
				_prevFreeCamDisabledTime = Time.realtimeSinceStartup;
				_prevFreeCamDisabled = true;
			}

			CinematographyCore.log.LogMessage(enable ? "Free cam enabled" : "Free cam disabled");
		}

		private void UpdateMovement()
		{
			_targetPosition += CalculateTargetPositionDelta();
			var position = Vector3.SmoothDamp(FreeCamCtrl.position, _targetPosition, ref _smoothVelocity, _smoothTime * Time.timeScale);
			FreeCamCtrl.position = position;
			
			if (_dynamicRollEnabled)
			{
				OnRollAngleChange?.Invoke(CalculateDynamicRoll());
			}

			var cullPosition = CalculateCullerPosition();
			_playerAgent.m_movingCuller.WarpPosition(cullPosition);
			_playerAgent.m_movingCuller.UpdatePosition(cullPosition);
		}
	
		private float CalculateDynamicRoll()
		{
			var projection = Vector3.Project(_smoothVelocity, _fpsCamera.FlatRight);
			var prep = Vector3.Cross(_fpsCamera.Forward, projection);
			var dir = Vector3.Dot(prep, _fpsCamera.transform.up);
			var roll = Mathf.Clamp(projection.magnitude * Time.timeScale, -DynamicRollMax, DynamicRollMax) * Mathf.Sign(dir);
			roll = Utils.SmoothDampNoOvershootProtection(_prevRoll, roll, ref _rollVelocity, _dynamicRollSmoothTime * Time.timeScale);
			_prevRoll = roll;
			return roll * _rollIntensity;
		}

		private Vector3 CalculateCullerPosition()
		{
			var currPosition = FreeCamCtrl.position;
			var raycastOrigWithOffset = currPosition + _fpsCamera.FlatForward.normalized;
			return Physics.Raycast(raycastOrigWithOffset, Vector3.down,
				out var hit) ? hit.point : currPosition;
		}

		private Vector3 CalculateTargetPositionDelta()
		{
			var delta = Vector3.zero;
			
			var speedMulti = 
				Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? SlowMoveFactor : 
			Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? FastMoveFactor : 1;

			var x = Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0f;
			var y = Input.GetKey(KeyCode.LeftControl) ? -1f : Input.GetKey(KeyCode.Space) ? 1f : 0f;
			var z = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
			
			var right = _fpsCamera.FlatRight;
			var forward = _mouseCtrlAltitudeEnabled ? _fpsCamera.Forward : _fpsCamera.FlatForward;
			var up = Vector3.Cross(forward, right);

			var now = Time.realtimeSinceStartup;
			var customDeltaTime = now - _lastInterval;
			_lastInterval = now;

			delta += customDeltaTime * _movementSpeed * speedMulti * x * right;
			delta += customDeltaTime * _movementSpeed * speedMulti * y * up;
			delta += customDeltaTime * _movementSpeed * speedMulti * z * forward;

			return delta;
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
			((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFreeCamera]).OnValueChanged -= OnFreeCameraToggle;
			((SliderOption) CinemaUIManager.Options[UIOption.MovementSpeedSlider]).OnValueChanged -= OnMovementSpeedChange;
			((SliderOption) CinemaUIManager.Options[UIOption.MovementSmoothingSlider]).OnValueChanged -= OnSmoothTimeChange;

			((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDynamicRoll]).OnValueChanged -= OnDynamicRollToggle;
			((SliderOption) CinemaUIManager.Options[UIOption.DynamicRollIntensitySlider]).OnValueChanged -= OnDynamicRollIntensityChange;
	        
			((ToggleOption) CinemaUIManager.Options[UIOption.ToggleMouseCtrlAltitude]).OnValueChanged -= OnMouseIndependentCtrlToggle;
			
			CinemaNetworkingManager.OnOtherPlayerEnterExitFreeCam -= OnOtherPlayerEnterOrExitFreeCam;
		}
    }
}