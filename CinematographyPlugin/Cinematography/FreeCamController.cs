using System;
using CellMenu;
using CinematographyPlugin.Cinematography.CinemaInput;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class FreeCamController : MonoBehaviour
    {
        private const float FastSpeedScale = 2;
        private const float SlowSpeedScale = 1f/2f;

        public const float MovementSpeedDefault = 5f;
        public const float MovementSpeedMin = 0f;
        public const float MovementSpeedMax = 10f;

        public const float MovementSmoothTimeDefault = 0.2f;
        public const float MovementSmoothTimeMin = 0f;
        public const float MovementSmoothTimeMax = 2f;

        public const float SensitivityScaling = 100f;
        public static float RotationSpeedDefault = .65f;
        public const float RotationSpeedMin = 0f;
        public const float RotationSpeedMax = 1f;

        public const float RotationSmoothTimeDefault = 0.2f;
        public const float RotationSmoothTimeMin = 0f;
        public const float RotationSmoothTimeMax = 2f;
        
        public static float ZoomDefault = 88f;
        public const float ZoomMin = 1f;
        public const float ZoomMax = 160f;
        
        public const float ZoomScaling = 100f;
        public const float ZoomSpeedDefault = .65f;
        public const float ZoomSpeedMin = 0f;
        public const float ZoomSpeedMax = 1f;

        public const float ZoomSmoothTimeDefault = 0.2f;
        public const float ZoomSmoothTimeMin = 0f;
        public const float ZoomSmoothTimeMax = 2f;

        private bool _mouseCtrlAltitude = true;
        
        private float _lastInterval;
        private float _independentDeltaTime;
        private float _movementSpeed = MovementSpeedDefault;
        private float _rotationSpeed = RotationSpeedDefault;
        private float _movementSmoothTime = MovementSmoothTimeDefault;
        private float _rotationSmoothTime = RotationSmoothTimeDefault;
        private float _targetZoom = ZoomDefault;
        private float _currZoom = ZoomDefault;
        private float _zoomVelocity;
        private float _zoomSpeed = ZoomSpeedDefault;
        private float _zoomSmoothTime = ZoomSmoothTimeDefault;

        private FPSCamera _fpsCamera;
        private Transform _rotTrans;
        private Vector3 _targetPos = Vector3.zero;
        private Vector3 _movementVelocity = Vector3.zero;
        private Quaternion _targetWorldRot = Quaternion.identity;
        private Quaternion _targetLocalRot = Quaternion.identity;
        private Quaternion _worldRotVelocity = Quaternion.identity;
        private Quaternion _localRotVelocity = Quaternion.identity;

        public FreeCamController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        private void Awake()
        {
            CinemaUIManager.Sliders[UIOption.MovementSpeedSlider].OnValueChanged += SetMovementSpeed;
            CinemaUIManager.Sliders[UIOption.MovementSmoothingSlider].OnValueChanged += SetMovementSmoothTime;
            CinemaUIManager.Sliders[UIOption.RotationSpeedSlider].OnValueChanged += SetRotationSpeed;
            CinemaUIManager.Sliders[UIOption.RotationSmoothingSlider].OnValueChanged += SetRotationSmoothTime;
            CinemaUIManager.Toggles[UIOption.ToggleMouseCtrlAltitude].OnValueChanged += SetMouseCtrlAltitude;
        }

        private void Start()
        {
            _fpsCamera = FindObjectOfType<FPSCamera>();

            var trans = transform;
            var lookRotation= Quaternion.LookRotation(_fpsCamera.Forward);

            // set starting position and rotations
            _rotTrans = trans.GetChild(0);
            trans.localRotation.SetEulerRotation(0, lookRotation.eulerAngles.y, 0);
            _rotTrans.localRotation.SetEulerRotation(lookRotation.eulerAngles.x, 0, 0);
            _targetPos = trans.position;
            
            // Get default sensitivity 
            RotationSpeedDefault = CellSettingsManager.GetFloatValue(eCellSettingID.Gameplay_LookSpeed);
            _rotationSpeed = RotationSpeedDefault;
            
            // Get default FoV
            ZoomDefault = CellSettingsManager.GetIntValue(eCellSettingID.Video_WorldFOV);
            _targetZoom = ZoomDefault;
            _currZoom = ZoomDefault;
        }

        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;
            
            UpdateIndependentDeltaTime();
            UpdateRotation();
            UpdatePosition();
            UpdateCuller();
            UpdateZoom();
            CheckReset();
        }

        private void UpdatePosition()
        {
            
            // get directional vectors
            var x = InputManager.GetAxis(AxisName.PosX);
            var y = InputManager.GetAxis(AxisName.PosY);
            var z = InputManager.GetAxis(AxisName.PosZ);
                
            var delta = Vector3.zero;

            // calculate speed and smoothing time
            var speedAxis = InputManager.GetAxis(AxisName.Speed);
            var speedScale = speedAxis > 0 ? FastSpeedScale : speedAxis < 0 ? SlowSpeedScale : 1;
            var speed = _movementSpeed * speedScale;
            var smoothTime = _movementSmoothTime * Time.timeScale; // Scale by adjusted time scale

            var right = _mouseCtrlAltitude ? _rotTrans.right : FlatRight();
            var forward = _mouseCtrlAltitude ? _rotTrans.forward : FlatForward();
            var up = _mouseCtrlAltitude ? _rotTrans.up : Vector3.up;

            // calculate translation delta with smoothing
            delta += _independentDeltaTime * speed * x * right;
            delta += _independentDeltaTime * speed * y * up;
            delta += _independentDeltaTime * speed * z * forward;
            
            _targetPos += delta;
            var newPos = Vector3.SmoothDamp(transform.position, _targetPos, ref _movementVelocity, smoothTime);

            transform.position = newPos;
        }
        
        private void UpdateRotation()
        {
            // get directional vectors
            var pitch = InputManager.GetAxis(AxisName.RotX);
            var yaw = InputManager.GetAxis(AxisName.RotY);
            var roll = InputManager.GetAxis(AxisName.RotZ);
            
            var worldTrans = transform;
            var localTrans = _rotTrans;
        
            // calculate speed and smoothing time
            var speed = _rotationSpeed * SensitivityScaling;
            var smoothTime = _rotationSmoothTime * Time.timeScale; // Scale by adjusted time scale
        
            var deltaEuler = new Vector3(pitch, yaw, roll);
            deltaEuler *= _independentDeltaTime * speed;
            
            var deltaWorld = Quaternion.Euler(0, deltaEuler.y, deltaEuler.z);
            var deltaLocal = Quaternion.Euler(deltaEuler.x, 0, 0);
            
            // calculate rotation delta with smoothing 
            _targetWorldRot *= deltaWorld;
            _targetLocalRot *= deltaLocal;
            
            var newWorldRot = Utils.QuaternionSmoothDamp(worldTrans.localRotation, _targetWorldRot, ref _worldRotVelocity, smoothTime);
            var newLocalRot = Utils.QuaternionSmoothDamp(localTrans.localRotation, _targetLocalRot, ref _localRotVelocity, smoothTime);

            worldTrans.localRotation = newWorldRot;
            localTrans.localRotation = newLocalRot;
        }

        private void UpdateZoom()
        {
            var dir = InputManager.GetAxis(AxisName.Zoom);
            var speed = _zoomSpeed * ZoomScaling;
            var smoothTime = _zoomSmoothTime * Time.timeScale;
            
            _targetZoom += _independentDeltaTime * speed * dir;
            var newZoom = Mathf.Clamp(Utils.SmoothDampNoOvershootProtection(_currZoom, _targetZoom, ref _zoomVelocity, smoothTime), ZoomMin, ZoomMax);
            _fpsCamera.m_camera.fieldOfView = newZoom;
            // Have separate zoom since accessing the field seems to reset it
            _currZoom = newZoom;
        }
        
        private void UpdateCuller()
        {
            var currPosition = transform.position;
            var raycastOrigWithOffset = currPosition + FlatForward();
            var newCullPos = Physics.Raycast(raycastOrigWithOffset, Vector3.down, out var hit) ? hit.point : currPosition;
            _fpsCamera.m_owner.m_movingCuller.UpdatePosition(newCullPos);
        }

        private void CheckReset()
        {
            if (InputManager.GetReset())
            {
                OnReset();
            }
        }
        
        private void OnReset()
        {
            _targetZoom = ZoomDefault;
            _targetWorldRot = Quaternion.Euler(0, _targetWorldRot.eulerAngles.y, 0);
        }

        private void UpdateIndependentDeltaTime()
        {
            var now = Time.realtimeSinceStartup;
            _independentDeltaTime = now - _lastInterval;
            _lastInterval = now;
        }

        private Vector3 FlatForward()
        {
            return Vector3.ProjectOnPlane(_rotTrans.forward, Vector3.up).normalized;
        }
        
        private Vector3 FlatRight()
        {
            return Vector3.ProjectOnPlane(_rotTrans.right, Vector3.up).normalized;
        }

        private void SetMovementSpeed(float value)
        {
            _movementSpeed = value;
        }
        
        private void SetMovementSmoothTime(float value)
        {
            _movementSmoothTime = value;
        }
        
        private void SetRotationSpeed(float value)
        {
            _rotationSpeed = value;
        }
        
        private void SetRotationSmoothTime(float value)
        {
            _rotationSmoothTime = value;
        }

        private void SetMouseCtrlAltitude(bool value)
        {
            _mouseCtrlAltitude = value;
        }
        
        private void OnDestroy()
        {
            CinemaUIManager.Sliders[UIOption.MovementSpeedSlider].OnValueChanged -= SetMovementSpeed;
            CinemaUIManager.Sliders[UIOption.MovementSmoothingSlider].OnValueChanged -= SetMovementSmoothTime;
            CinemaUIManager.Sliders[UIOption.RotationSpeedSlider].OnValueChanged -= SetRotationSpeed;
            CinemaUIManager.Sliders[UIOption.RotationSmoothingSlider].OnValueChanged -= SetRotationSmoothTime;
            CinemaUIManager.Toggles[UIOption.ToggleMouseCtrlAltitude].OnValueChanged -= SetMouseCtrlAltitude;
        }
    }
}