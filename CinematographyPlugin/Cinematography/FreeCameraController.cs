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

        private static float _smoothTime;
        private static float _movementSpeed;
        
        private static Vector3 _smoothVelocity;
        private static Vector3 _targetPosition;
        
        private static bool _noClipEnabled;

        // private Camera _pluginCamera;
        // private PostProcessingBehaviour _pluginPostProcess;
        private FPSCamera _fpsCamera;
        private FPSCameraHolder _fpsCamHolder;
        private GameObject _player;
        private GameObject _fpArms;
        private PlayerAgent _playerAgent;
        private PlayerLocomotion _playerLocomotion;
        
        public FreeCameraController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }
        
        public void Start()
        {
			_fpsCamera = FindObjectOfType<FPSCamera>();
			_fpsCamHolder = FindObjectOfType<FPSCameraHolder>();

			_fpArms = PlayerManager.GetLocalPlayerAgent().FPItemHolder.gameObject;

			_player = _fpsCamHolder.m_owner.gameObject;
			_playerAgent = PlayerManager.GetLocalPlayerAgent();
			_playerLocomotion = _player.GetComponent<PlayerLocomotion>();

			_smoothTime = SmoothTimeDefault;
			_movementSpeed = MovementSpeedDefault;
				
			((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFreeCamera]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnFreeCameraToggle);
			((SliderOption) CinemaUIManager.Options[UIOption.MovementSpeedSlider]).Slider.onValueChanged.AddListener((UnityAction<float>) OnMovementSpeedChange);
			((SliderOption) CinemaUIManager.Options[UIOption.MovementSmoothingSlider]).Slider.onValueChanged.AddListener((UnityAction<float>) OnSmoothTimeChange);
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
			Global.EnemyPlayerDetectionEnabled = false;
			CinematographyCore.log.LogMessage("NoClip Enabled");
		}

		private void ExitNoClip()
		{
			ToggleUIManager.ShowBody();
			C_CullingManager.CullingEnabled = true;
			C_CullingManager.HideAll();

			_playerLocomotion.enabled = true;
			Global.EnemyPlayerDetectionEnabled = true;
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

		private void UpdateMovement()
		{
			_targetPosition += CalculateTargetPosition();
			
			var position = Vector3.SmoothDamp(_player.transform.position, _targetPosition, ref _smoothVelocity, _smoothTime);

			_playerAgent.TeleportTo(position);
			_playerLocomotion.SyncedSetPosition(position);
			_playerAgent.m_movingCuller.WarpPosition(position);
			_playerAgent.m_movingCuller.UpdatePosition(position);
			_playerAgent.Sync.SendLocomotion(_playerLocomotion.m_currentStateEnum, position, _fpsCamera.Forward, 0.0f, 0.0f);
		}

		private Vector3 CalculateTargetPosition()
		{
			var delta = Vector3.zero;
			
			var speedMulti = 
				Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? FastMoveFactor : 
				Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ? SlowMoveFactor : 1;

			var x = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
			var y = Input.GetKey(KeyCode.LeftControl) ? -1 : Input.GetKey(KeyCode.Space) ? 1 : 0;
			var z = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;

			delta += Time.deltaTime * _movementSpeed * speedMulti * x * _player.transform.right;
			delta += Time.deltaTime * _movementSpeed * speedMulti * y * _player.transform.up;
			delta += Time.deltaTime * _movementSpeed * speedMulti * z * _fpsCamera.transform.forward;

			return delta;
		}
    }
}