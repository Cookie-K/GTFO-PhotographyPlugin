using System;
using Agents;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using CullingSystem;
using Enemies;
using Globals;
using Player;
using ToggleUIPlugin.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PostProcessing;
using Object = System.Object;

namespace CinematographyPlugin.Cinematography
{
    public class FreeCameraController : MonoBehaviour
    {
        private const float FastMoveFactor = 3;
        private const float SlowMoveFactor = 1f/3f;
        
        public const float SmoothTimeDefault = 0.25f;
        public const float SmoothTimeMin = 0f;
        public const float SmoothTimeMax = 1f;

        public const float MovementSpeedDefault = 10f;
        public const float MovementSpeedMin = 0f;
        public const float MovementSpeedMax = 30f;
        
        public const float DynamicRollIntensityDefault = 5f;
        public const float DynamicRollIntensityMin = 1f;
        public const float DynamicRollIntensityMax = 10f;
        
        private const float DynamicRollSmoothTime = 1;
        private const float DynamicRollMax = 90f;

        private static float _smoothTime = SmoothTimeDefault;
        private static float _rollIntensity = DynamicRollIntensityDefault;
        private static float _movementSpeed = MovementSpeedDefault;
        private static float _prevRoll;
        private static float _RollVelocity;
        private static float _RayCastOffset = -1;

        private static Vector3 _smoothVelocity;
        private static Vector3 _targetPosition;
        
        private static bool _noClipEnabled;
        private static bool _dynamicRollEnabled;
        private static bool _mouseIndependntCtrlEnabled;

        private FPSCamera _fpsCamera;
        private FPSCameraHolder _fpsCamHolder;
        private CameraRollController _rollController;
        private GameObject _player;
        private GameObject _fpArms;
        private PlayerAgent _playerAgent;
        private PlayerVoice _playerVoice;
        private PE_FPSDamageFeedback _damageFeedback;
        private PlayerInteraction _playerInteraction;
        private PlayerLocomotion _playerLocomotion;
        
        public FreeCameraController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }
        
        public void Start()
        {
			_fpsCamera = FindObjectOfType<FPSCamera>();
			_fpsCamHolder = FindObjectOfType<FPSCameraHolder>();
			_rollController = FindObjectOfType<CameraRollController>();

			_fpArms = PlayerManager.GetLocalPlayerAgent().FPItemHolder.gameObject;

			_player = _fpsCamHolder.m_owner.gameObject;
			_damageFeedback = _fpsCamera.gameObject.GetComponent<PE_FPSDamageFeedback>();
			_playerAgent = PlayerManager.GetLocalPlayerAgent();
			_playerVoice = _player.GetComponent<PlayerVoice>();
			_playerInteraction = _player.GetComponent<PlayerInteraction>();
			_playerLocomotion = _player.GetComponent<PlayerLocomotion>();

			((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFreeCamera]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnFreeCameraToggle);
			((SliderOption) CinemaUIManager.Options[UIOption.MovementSpeedSlider]).Slider.onValueChanged.AddListener((UnityAction<float>) OnMovementSpeedChange);
			((SliderOption) CinemaUIManager.Options[UIOption.MovementSmoothingSlider]).Slider.onValueChanged.AddListener((UnityAction<float>) OnSmoothTimeChange);
			((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDynamicRoll]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnDynamicRollToggle);
			((SliderOption) CinemaUIManager.Options[UIOption.DynamicRollIntensitySlider]).Slider.onValueChanged.AddListener((UnityAction<float>) OnRollIntensityChange);
			((ToggleOption) CinemaUIManager.Options[UIOption.ToggleMouseIndependentCtrl]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnMouseIndependentCtrlToggle);
		}

		public void Update()
		{
			if (_noClipEnabled && !CinemaUIManager.MenuOpen)
			{
				if (_fpArms.active)
				{
					ToggleUIManager.HideBody();
				}
				UpdateMovement();
			}
		}

		private void OnFreeCameraToggle(bool value)
		{
			_noClipEnabled = value;

			if (value)
			{
				EnableNoClip();
			}
			else
			{
				ExitNoClip();
			}
		}

		private void EnableNoClip()
		{
			_targetPosition = _player.transform.position;
			C_CullingManager.CullingEnabled = false;
			C_CullingManager.ShowAll();
					
			_playerLocomotion.enabled = false;
			_playerVoice.enabled = false;
			_playerInteraction.enabled = false;
			_damageFeedback.enabled = false;
			Global.EnemyPlayerDetectionEnabled = false;
			ScreenLiquidManager.LiquidSystem.enabled = false;
			CinematographyCore.log.LogMessage("NoClip Enabled");
		}

		private void ExitNoClip()
		{
			ToggleUIManager.ShowBody();
			C_CullingManager.CullingEnabled = true;
			C_CullingManager.HideAll();

			_playerLocomotion.enabled = true;
			_playerVoice.enabled = true;
			_playerInteraction.enabled = true;
			_damageFeedback.enabled = true;
			Global.EnemyPlayerDetectionEnabled = true;
			ScreenLiquidManager.LiquidSystem.enabled = true;
			CinematographyCore.log.LogMessage("NoClip Disabled");
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
		}
		
		private void OnMouseIndependentCtrlToggle(bool value)
		{
			_mouseIndependntCtrlEnabled = value;
		}
		
		private void OnRollIntensityChange(float value)
		{
			_rollIntensity = value;
		}

		private void DivertEnemies()
		{
			if (PlayerManager.PlayerAgentsInLevel.Count == 1) return;
			
			foreach (var agent in _playerAgent.GetAttackers())
			{
				PlayerAgent playerLowestScore = null;
				foreach (var playerAgent in PlayerManager.PlayerAgentsInLevel)
				{
					if (playerAgent.name == _playerAgent.name) continue;
					if (playerLowestScore == null || playerAgent.GetAttackersScore() < playerLowestScore.GetAttackersScore())
					{
						playerLowestScore = playerAgent;
					}
				}
				agent.TryCast<EnemyAgent>().AI.SetTarget(playerLowestScore);
			}
		}

		private void UpdateMovement()
		{
			_targetPosition += CalculateTargetPosition();
			
			var position = Vector3.SmoothDamp(_player.transform.position, _targetPosition, ref _smoothVelocity, _smoothTime);

			if (_dynamicRollEnabled)
			{
				_rollController.SetRoll(CalculateRoll());
			}

			var raycastOrigWithOffset = _fpsCamera.Position + new Vector3(1, 0, 0);
			var cullPos = Physics.Raycast(raycastOrigWithOffset, Vector3.Cross(_fpsCamera.FlatRight, _fpsCamera.FlatForward),
				out var hit) ? hit.point : _fpsCamera.Position;

			_playerAgent.m_movingCuller.WarpPosition(cullPos);
			_playerAgent.m_movingCuller.UpdatePosition(cullPos);
			
			_playerAgent.TeleportTo(position);
			_playerLocomotion.SyncedSetPosition(position);
			_playerAgent.Sync.SendLocomotion(_playerLocomotion.m_currentStateEnum, position, _fpsCamera.Forward, 0.0f, 0.0f);
		}

		private float CalculateRoll()
		{
			var projection = Vector3.Project(_smoothVelocity, _fpsCamera.FlatRight);
			var prep = Vector3.Cross(_fpsCamera.Forward, projection);
			var dir = Vector3.Dot(prep, _fpsCamera.transform.up);
			var roll = Mathf.Clamp(projection.magnitude, -DynamicRollMax, DynamicRollMax) * Mathf.Sign(dir);
			roll = Mathf.SmoothDampAngle(_prevRoll, roll, ref _RollVelocity, DynamicRollSmoothTime);
			_prevRoll = roll;
			return roll * _rollIntensity;
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
			var forward = _mouseIndependntCtrlEnabled ? _fpsCamera.FlatForward : _fpsCamera.Forward;
			var up = Vector3.Cross(forward, right);

			delta += Time.deltaTime * _movementSpeed * speedMulti * x * right;
			delta += Time.deltaTime * _movementSpeed * speedMulti * y * up;
			delta += Time.deltaTime * _movementSpeed * speedMulti * z * forward;

			return delta;
		}
		//
		// public void ChangeDepthOfFieldAtRuntime( float val)
		// {
		// 	//copy current "depth of field" settings from the profile into a temporary variable
		// 	var ppProfile = _fpsCamera.GetComponent<PostProcessingBehaviour>().profile;
		// 	var dofSettings = ppProfile.depthOfField.settings;
		// 	dofSettings.aperture = val;
		// 	//set the "depth of field" settings in the actual profile to the temp settings with the changed value
		// 	ppProfile.depthOfField.settings = dofSettings;
		// }
    }
}