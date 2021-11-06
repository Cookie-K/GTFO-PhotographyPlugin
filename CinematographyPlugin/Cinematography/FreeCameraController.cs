using System;
using CullingSystem;
using Globals;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Photography
{
    public class FreeCameraController : MonoBehaviour
    {
        private const float ClimbSpeed = 10;
	    private const float NormalMoveSpeed = 10;
        private const float FastMoveFactor = 3;
        private const float SlowMoveFactor = 1f/3f;

        private static bool _smooth = true;
        private static Vector3 _smoothVelocity;
        private static Vector3 _targetPosition;
        
        private static bool _noClipEnabled;
        private static bool _currState;
        private bool _prevState;
        
        private FPSCamera _fpsCamera;
        private FPSCameraHolder _fpsCamHolder;
        private GameObject _player;
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
			_player = _fpsCamHolder.m_owner.gameObject;
			_playerAgent = PlayerManager.GetLocalPlayerAgent();
			_playerLocomotion = _player.GetComponent<PlayerLocomotion>();
		}

		public void Update()
		{
			if (_currState != _prevState)
			{
				_prevState = _currState;
				
				if (_noClipEnabled)
				{
					_targetPosition = _player.transform.position;
					C_CullingManager.CullingEnabled = false;
					C_CullingManager.ShowAll();
					
					_playerLocomotion.enabled = false;
					Global.EnemyPlayerDetectionEnabled = false;
					CinematographyCore.log.LogMessage("NoClip Enabled");
				}
				else
				{
					C_CullingManager.CullingEnabled = true;
					C_CullingManager.HideAll();

					_playerLocomotion.enabled = true;
					Global.EnemyPlayerDetectionEnabled = true;
					CinematographyCore.log.LogMessage("NoClip Disabled");
				}
			}

			if (_noClipEnabled)
			{
				UpdateMovement();
			}
		}

		public static void ToggleNoClip()
		{
			_currState = !_noClipEnabled;
			_noClipEnabled = !_noClipEnabled;
		}

		/**
		 * Mimics the Input.GetAxis call (GTFO does not seem to implement this) 
		 */
		int GetAxis(string dir)
		{
			if(dir.Equals("Vertical"))
			{
				return Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
			}

			return Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
		}

		void UpdateMovement()
		{
			Vector3 delta = Vector3.zero;
			var speedMulti = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? FastMoveFactor : 1;
			var verticalV = Input.GetKey(KeyCode.LeftControl) ? -1 : Input.GetKey(KeyCode.Space) ? 1 : 0;

			delta += Time.deltaTime * (NormalMoveSpeed * speedMulti) * GetAxis("Vertical") * _fpsCamera.transform.forward;
			delta += Time.deltaTime * (NormalMoveSpeed * speedMulti) * GetAxis("Horizontal") * _player.transform.right;

			delta += _player.transform.up * (Time.deltaTime * ClimbSpeed * speedMulti * verticalV);

			_targetPosition += delta;
			
			// position = Vector3.SmoothDamp(_player.transform.position, position, ref _smoothVelocity, Time.deltaTime); // moves with no smoothing
			var position = Vector3.SmoothDamp(_player.transform.position, _targetPosition, ref _smoothVelocity, 0.5f);

			_playerLocomotion.SyncedSetPosition(position);
			_playerAgent.TeleportTo(position);
			_playerAgent.m_movingCuller.WarpPosition(position);
			_playerAgent.m_movingCuller.UpdatePosition(position);
			_playerAgent.Sync.SendLocomotion(_playerLocomotion.m_currentStateEnum, (_playerAgent).transform.position, _fpsCamera.Forward, 0.0f, 0.0f);
		}
        
    }
}