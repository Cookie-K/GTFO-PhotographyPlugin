using System;
using System.Collections.Generic;
using System.Linq;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using CullingSystem;
using Enemies;
using Globals;
using Player;
using ToggleUIPlugin.Managers;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class FreeCameraController : MonoBehaviour
    {
	    public static event Action<float> OnDynamicRollAngleChange;
	    
        private const float FastMoveFactor = 2;
        private const float SlowMoveFactor = 1f/2f;
        
        public const float SmoothTimeDefault = 0.25f;
        public const float SmoothTimeMin = 0f;
        public const float SmoothTimeMax = 2f;

        public const float MovementSpeedDefault = 5f;
        public const float MovementSpeedMin = 0f;
        public const float MovementSpeedMax = 10f;
        
        public const float DynamicRollIntensityDefault = 5f;
        public const float DynamicRollIntensityMin = 1f;
        public const float DynamicRollIntensityMax = 10f;
        
        private const float DynamicRollSmoothTime = 1;
        private const float DynamicRollMax = 90f;

        private static readonly Dictionary<string, float> PlayerPrevMaxHealthByName = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> PlayerPrevHealthByName = new Dictionary<string, float>();
        private static readonly Dictionary<string, float> PlayerPrevInfectionByName = new Dictionary<string, float>();
        
        private static float _smoothTime = SmoothTimeDefault;
        private static float _rollIntensity = DynamicRollIntensityDefault;
        private static float _movementSpeed = MovementSpeedDefault;
        private static float _prevRoll;
        private static float _rollVelocity;
        private static float _lastInterval;

        private static Vector3 _smoothVelocity;
        private static Vector3 _targetPosition;
        private static Vector3 _startingPosition;
        
        private static bool _freeCamEnabled;
        private static bool _dynamicRollEnabled;
        private static bool _mouseIndependentCtrlEnabled;


        private FPSCamera _fpsCamera;
        private FPSCameraHolder _fpsCamHolder;
        private GameObject _player;
        private GameObject _fpArms;
        private GameObject _ui;
        private PlayerAgent _playerAgent;

        // Toggle comps when free cam enabled
        private PlayerVoice _playerVoice;
        private PE_FPSDamageFeedback _damageFeedback;
        private PlayerInteraction _playerInteraction;
        private PlayerLocomotion _playerLocomotion;

        public FreeCameraController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }
        
        public void Awake()
        {
	        _fpsCamera = FindObjectOfType<FPSCamera>();
			_fpsCamHolder = FindObjectOfType<FPSCameraHolder>();
			_fpArms = PlayerManager.GetLocalPlayerAgent().FPItemHolder.gameObject;
			_ui = GuiManager.PlayerLayer.Root.FindChild("PlayerLayer").gameObject;
			_player = _fpsCamHolder.m_owner.gameObject;
			_damageFeedback = _fpsCamera.gameObject.GetComponent<PE_FPSDamageFeedback>();
			_playerAgent = PlayerManager.GetLocalPlayerAgent();
			_playerVoice = _player.GetComponent<PlayerVoice>();
			_playerInteraction = _player.GetComponent<PlayerInteraction>();
			_playerLocomotion = _player.GetComponent<PlayerLocomotion>();
		}

        public void Start()
        {
	        ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFreeCamera]).OnValueChanged += OnFreeCameraToggle;
	        ((SliderOption) CinemaUIManager.Options[UIOption.MovementSpeedSlider]).OnValueChanged += OnMovementSpeedChange;
	        ((SliderOption) CinemaUIManager.Options[UIOption.MovementSmoothingSlider]).OnValueChanged += OnSmoothTimeChange;
	        
	        ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDynamicRoll]).OnValueChanged += OnDynamicRollToggle;
	        ((SliderOption) CinemaUIManager.Options[UIOption.DynamicRollIntensitySlider]).OnValueChanged += OnDynamicRollIntensityChange;
	        
	        ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleMouseIndependentCtrl]).OnValueChanged += OnMouseIndependentCtrlToggle;

	        CinemaNetworkingManager.OnOtherPlayerEnterExitFreeCam += OnOtherPlayerEnterOrExitFreeCam;
        }

        public void Update()
        {
	        if (_freeCamEnabled && !CinemaUIManager.MenuOpen)
	        {
		        if (_fpArms.active || _ui.active)
		        {
			        ToggleUIManager.HideBody();
			        ToggleUIManager.HideUI();
		        }
		        UpdateMovement();
		        DivertEnemiesAwayFromCameraMan();
	        }
        }

		public void OnFreeCameraToggle(bool value)
		{
			_freeCamEnabled = value;
			EnableFreeCam(value);
		}

		public void OnSmoothTimeChange(float value)
		{
			_smoothTime = value;
		}

		public void OnMovementSpeedChange(float value)
		{
			_movementSpeed = value;
		}

		public void OnDynamicRollToggle(bool value)
		{
			_dynamicRollEnabled = value;
		}

		public void OnMouseIndependentCtrlToggle(bool value)
		{
			_mouseIndependentCtrlEnabled = value;
		}

		public void OnDynamicRollIntensityChange(float value)
		{
			_rollIntensity = value;
		}

		private void EnableFreeCam(bool enable)
		{
			SetCameraManHealth(_playerAgent, enable);

			var initPosition = _player.transform.position;
			_targetPosition = enable ? initPosition : _startingPosition;
			_startingPosition = enable ? initPosition : _startingPosition;
			
			C_CullingManager.CullingEnabled = !enable;
			if (enable)
			{
				C_CullingManager.ShowAll();
			}
			else
			{
				C_CullingManager.HideAll();
			}
					
			_playerLocomotion.enabled = !enable;
			_playerInteraction.enabled = !enable;
			_damageFeedback.enabled = !enable;
			Global.EnemyPlayerDetectionEnabled = !enable;
			CinematographyCore.log.LogMessage(enable ? "Free cam enabled" : "Free cam disabled");
		}

		private void UpdateMovement()
		{
			_targetPosition += CalculateTargetPosition();
			var position = Vector3.SmoothDamp(_player.transform.position, _targetPosition, ref _smoothVelocity, _smoothTime * Time.timeScale);

			if (_dynamicRollEnabled)
			{
				OnDynamicRollAngleChange?.Invoke(CalculateDynamicRoll());
			}

			var cullPosition = CalculateCullerPosition();
			_playerAgent.m_movingCuller.WarpPosition(cullPosition);
			_playerAgent.m_movingCuller.UpdatePosition(cullPosition);
			
			_playerAgent.TeleportTo(position);
			_playerLocomotion.SyncedSetPosition(position);
			_playerAgent.Sync.SendLocomotion(_playerLocomotion.m_currentStateEnum, position, _fpsCamera.Forward, 0.0f, 0.0f);
		}

		private float CalculateDynamicRoll()
		{
			var projection = Vector3.Project(_smoothVelocity, _fpsCamera.FlatRight);
			var prep = Vector3.Cross(_fpsCamera.Forward, projection);
			var dir = Vector3.Dot(prep, _fpsCamera.transform.up);
			var roll = Mathf.Clamp(projection.magnitude * Time.timeScale, -DynamicRollMax, DynamicRollMax) * Mathf.Sign(dir);
			roll = Mathf.SmoothDampAngle(_prevRoll, roll, ref _rollVelocity , DynamicRollSmoothTime * Time.timeScale);
			_prevRoll = roll;
			return roll * _rollIntensity;
		}

		private Vector3 CalculateCullerPosition()
		{
			var raycastOrigWithOffset = _fpsCamera.Position + new Vector3(0, 0, -1);
			return Physics.Raycast(raycastOrigWithOffset, Vector3.Cross(_fpsCamera.FlatRight, _fpsCamera.FlatForward),
				out var hit) ? hit.point : _fpsCamera.Position;
		}

		private Vector3 CalculateTargetPosition()
		{
			var delta = Vector3.zero;
			
			var speedMulti = 
				Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? FastMoveFactor : 
				Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? SlowMoveFactor : 1;

			var x = Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0f;
			var y = Input.GetKey(KeyCode.LeftControl) ? -1f : Input.GetKey(KeyCode.Space) ? 1f : 0f;
			var z = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
			
			var right = _fpsCamera.FlatRight;
			var forward = _mouseIndependentCtrlEnabled ? _fpsCamera.FlatForward : _fpsCamera.Forward;
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

			CinematographyCore.log.LogInfo($"setting health for {playerName} entering free cam : {enteringFreeCam}");
			if (enteringFreeCam)
			{
				PlayerPrevMaxHealthByName.Add(playerName, damage.HealthMax);
				PlayerPrevHealthByName.Add(playerName, damage.Health);
				PlayerPrevInfectionByName.Add(playerName, damage.Infection);

				damage.HealthMax = 999999;
				damage.Health = 999999;
				damage.Infection = 0;
			}
			else
			{
				damage.HealthMax = PlayerPrevMaxHealthByName[playerName];
				damage.Health = PlayerPrevHealthByName[playerName];
				damage.Infection = PlayerPrevInfectionByName[playerName];
				
				PlayerPrevMaxHealthByName.Remove(playerName);
				PlayerPrevHealthByName.Remove(playerName);
				PlayerPrevInfectionByName.Remove(playerName);
			}
		}
		
		private void ShowOrHideFreeCamPlayer(PlayerAgent player, bool enteringFreeCam)
		{
			player.AnimatorBody.gameObject.active = !enteringFreeCam;
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
				var i = 0;
				foreach (var attacker in playerAgent.GetAttackers())
				{
					i = i < CinemaNetworkingManager.PlayersNotInFreeCamByName.Count ? i + 1 : 0;
					attacker.TryCast<EnemyAgent>().AI.SetTarget(CinemaNetworkingManager.PlayersNotInFreeCamByName.ElementAt(i).Value);
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
	        
			((ToggleOption) CinemaUIManager.Options[UIOption.ToggleMouseIndependentCtrl]).OnValueChanged -= OnMouseIndependentCtrlToggle;
			
			CinemaNetworkingManager.OnOtherPlayerEnterExitFreeCam -= OnOtherPlayerEnterOrExitFreeCam;
		}
    }
}