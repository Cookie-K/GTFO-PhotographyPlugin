using Agents;
using CinematographyPlugin.Cinematography.Settings;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using CinematographyPlugin.UI.UiInput;
using CinematographyPlugin.Util;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class CinemaCamController : MonoBehaviour
    {
        private readonly Vector3 _warpOffset = new (0, 0.8f, 0);

        private bool _alignPitchAxisWCam = true;
        private bool _alignRollAxisWCam;
        private bool _dynamicRotation = true;
        private bool _inOrbit;

        private float _movementSpeed = CinemaCamSettings.MovementSpeedDefault;
        private float _rotationSpeed = CinemaCamSettings.RotationSpeedDefault;
        private float _movementSmoothFactor = CinemaCamSettings.MovementSmoothTimeDefault;
        private float _rotationSmoothFactor = CinemaCamSettings.RotationSmoothTimeDefault;
        private float _orbitSmoothFactor = CinemaCamSettings.OrbitSmoothingFactor;
        private float _targetZoom = CinemaCamSettings.ZoomDefault;
        private float _currZoom = CinemaCamSettings.ZoomDefault;
        private float _zoomSpeed = CinemaCamSettings.ZoomSpeedDefault;
        private float _orbitDistance = CinemaCamSettings.OrbitDistanceDefault;
        private float _zoomSmoothFactor = CinemaCamSettings.ZoomSmoothTimeDefault;
        private float _dynamicRotationSpeed = CinemaCamSettings.DynamicRotationDefault;

        private FPSCamera _fpsCamera;
        private PlayerAgent _playerAgent;
        private Transform _childTrans;
        private Transform _orbitOffsetTrans;
        private Vector3 _targetPos = Vector3.zero;
        private Vector3 _prevPos = Vector3.zero;
        private Vector3 _movementVelocity = Vector3.zero;
        private Quaternion _targetWorldRot = Quaternion.identity;
        private Quaternion _targetLocalRot = Quaternion.identity;
        private Vector3 _orbitOffset;
        private Transform _orbitTarget;

        private void Awake()
        {
            CinemaUIManager.Current.Sliders[UIOption.MovementSpeedSlider].OnValueChanged += SetMovementSpeed;
            CinemaUIManager.Current.Sliders[UIOption.MovementSmoothingSlider].OnValueChanged += SetMovementSmoothTime;
            
            CinemaUIManager.Current.Sliders[UIOption.RotationSpeedSlider].OnValueChanged += SetRotationSpeed;
            CinemaUIManager.Current.Sliders[UIOption.RotationSmoothingSlider].OnValueChanged += SetRotationSmoothTime;

            CinemaUIManager.Current.Sliders[UIOption.ZoomSpeedSlider].OnValueChanged += SetZoomSpeed;
            CinemaUIManager.Current.Sliders[UIOption.ZoomSmoothingSlider].OnValueChanged += SetZoomSmoothTime;

            CinemaUIManager.Current.Toggles[UIOption.ToggleDynamicRoll].OnValueChanged += SetDynamicRotation;
            CinemaUIManager.Current.Sliders[UIOption.DynamicRollIntensitySlider].OnValueChanged += SetDynamicRotationSpeed;
            CinemaUIManager.Current.Toggles[UIOption.ToggleAlignPitchAxisWCam].OnValueChanged += SetAlignPitchAxisWCam;
            CinemaUIManager.Current.Toggles[UIOption.ToggleAlignRollAxisWCam].OnValueChanged += SetAlignRollAxisWCam;

            DimensionManager.OnDimensionWarp += OnDimensionWarp;

            _playerAgent = PlayerManager.GetLocalPlayerAgent();
            
            _fpsCamera = FindObjectOfType<FPSCamera>();
            _childTrans = transform.GetChild(0);
            _orbitOffsetTrans = _childTrans;
        }

        // Positions and Rotations must be synced before parenting to align with camera transform    
        public void SyncWithCameraTransform()
        {
            var fpsCamRot = _fpsCamera.m_orgParent.eulerAngles;
            var worldRot = Quaternion.Euler(0, fpsCamRot.y, 0);
            var localRot = Quaternion.Euler(fpsCamRot.x, 0, 0);

            transform.localRotation = _targetWorldRot = worldRot;
            _childTrans.localRotation = _targetLocalRot = localRot;
            _targetPos = transform.position = _fpsCamera.Position;
        }

        private void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;
            
            UpdateRotation();
            CheckAndResetFpsCamRotation();
            
            if (_inOrbit)
            {
                UpdateOrbitPosition();
                UpdateOrbitLocalPosition();
            }
            else
            {
                UpdatePosition();
                UpdateZoom();
                
                if (_dynamicRotation)
                {
                    CalculateDynamicRotationDelta();
                }
            }
            
            UpdateCuller();
            CheckReset();
        }
        
        public void OnDimensionWarp(Vector3 targetPos)
        {
            // Reset the camera back to where the player will be warped to 
            var rot = _playerAgent.GetHeadCamTransform().rotation.eulerAngles;
            targetPos += _warpOffset; // move warp pos up a bit since target is on the ground
            transform.position = targetPos;
            _targetPos = targetPos;
            
            _targetWorldRot = Quaternion.Euler(0, rot.y, rot.z);
            _targetLocalRot = Quaternion.Euler(rot.x, 0, 0);
        }

        public void SetOrbit(Agent targetAgent)
        {
            _orbitTarget = targetAgent.transform;
            _orbitOffset = targetAgent.AimTarget.position - _orbitTarget.position;
            _orbitOffsetTrans.localPosition = _orbitOffset;
            
            var euler = transform.localRotation.eulerAngles;
            _targetWorldRot = Quaternion.Euler(0, euler.y, 0);
            _targetLocalRot = Quaternion.Euler(euler.x, 0, 0);
            _targetPos = transform.position;

            _inOrbit = true;
        }

        public void DisableOrbit()
        {
            transform.position += _orbitOffsetTrans.localPosition; 
            _orbitOffsetTrans.localPosition = Vector3.zero;
            
            var euler = transform.localRotation.eulerAngles;
            _targetWorldRot = Quaternion.Euler(0, euler.y, 0);
            _targetLocalRot = Quaternion.Euler(euler.x, 0, 0);
            _targetPos = transform.position;
            
            _inOrbit = false;
        }

        private void UpdatePosition()
        {
            var independentDeltaTime = IndependentDeltaTimeManager.GetDeltaTime();

            // get directional vectors
            var x = KeyBindInputManager.GetAxis(AxisName.PosX);
            var y = KeyBindInputManager.GetAxis(AxisName.PosY);
            var z = KeyBindInputManager.GetAxis(AxisName.PosZ);
                
            var delta = Vector3.zero;

            // calculate speed and smoothing time
            var speedAxis = KeyBindInputManager.GetAxis(AxisName.Speed);
            var speedScale = CinemaCamSettings.MovementSpeedScale * (speedAxis > 0 ? CinemaCamSettings.FastSpeedScale : speedAxis < 0 ? CinemaCamSettings.SlowSpeedScale : 1);
            var speed = _movementSpeed * speedScale;

            var right = _alignRollAxisWCam ? _childTrans.right : FlatRight();
            var forward = _alignPitchAxisWCam ? _childTrans.forward : FlatForward();
            var up = _alignPitchAxisWCam ? _childTrans.up : Vector3.up;

            // calculate translation delta with smoothing
            delta += independentDeltaTime * speed * x * right;
            delta += independentDeltaTime * speed * y * up;
            delta += independentDeltaTime * speed * z * forward;
            
            _targetPos += delta;
            
            var currPos = transform.position;
            var t = 1.0f - Mathf.Pow(_movementSmoothFactor, independentDeltaTime);
            var newPos = Vector3.Lerp(currPos, _targetPos, t);

            _movementVelocity = (newPos - _prevPos)/independentDeltaTime;
            _prevPos = newPos;
            
            transform.position = newPos;
        }
        
        private void UpdateOrbitPosition()
        {
            var independentDeltaTime = IndependentDeltaTimeManager.GetDeltaTime();

            var focusDir = KeyBindInputManager.GetAxis(AxisName.Zoom);
            _orbitDistance += focusDir * CinemaCamSettings.OrbitDistanceMoveSpeedDefault;

            var focusPosition = _orbitTarget.transform.position;
            _targetPos = focusPosition - _fpsCamera.transform.forward * _orbitDistance;

            var currPos = transform.position;
            var t = 1.0f - Mathf.Pow(_orbitSmoothFactor, independentDeltaTime);
            var newPos = Vector3.Lerp(currPos, _targetPos, t);

            _movementVelocity = (newPos - _prevPos)/independentDeltaTime;
            _prevPos = newPos;
            
            transform.position = newPos;
        }
        
        /// <summary>
        ///  Sets minor adjustments to the orbit center offset via regular controls
        /// </summary>
        private void UpdateOrbitLocalPosition()
        {
            var independentDeltaTime = IndependentDeltaTimeManager.GetDeltaTime();

            // get directional vectors
            var x = KeyBindInputManager.GetAxis(AxisName.PosX);
            var y = KeyBindInputManager.GetAxis(AxisName.PosY);
            var z = KeyBindInputManager.GetAxis(AxisName.PosZ);

            var delta = Vector3.zero;

            // calculate translation delta with smoothing
            delta += independentDeltaTime * 1f * x * Vector3.right;
            delta += independentDeltaTime * 1f * y * Vector3.up;
            delta += independentDeltaTime * 1f * z * Vector3.forward;
            
            _orbitOffsetTrans.localPosition += delta;
        }

        private void UpdateRotation()
        {
            var independentDeltaTime = IndependentDeltaTimeManager.GetDeltaTime();
            
            var worldTrans = transform;
            var localTrans = _childTrans;
            
            // get directional vectors
            var pitch = KeyBindInputManager.GetAxis(AxisName.RotX);
            var yaw = KeyBindInputManager.GetAxis(AxisName.RotY);
            var roll = KeyBindInputManager.GetAxis(AxisName.RotZ);

            var upsideDown = Math.Sign(Vector3.Dot(localTrans.up, Vector3.up));
            yaw *= upsideDown; // invert yaw controls when upside down to keep mose directions consistent

            // calculate speed and smoothing time
            var speed = _rotationSpeed * CinemaCamSettings.SensitivityScaling;

            var deltaEuler = new Vector3(pitch, yaw, roll);
            deltaEuler *= independentDeltaTime * speed;
            
            var deltaWorld = Quaternion.Euler(0, deltaEuler.y, deltaEuler.z);
            var deltaLocal = Quaternion.Euler(deltaEuler.x, 0, 0);
            
            // calculate rotation delta with smoothing
            if (!IsYawRotationFlippedByNewRotation(deltaWorld))
            {
                _targetWorldRot *= deltaWorld;
            }
            
            if (!IsPitchRotationFlippedByNewRotation(deltaLocal))
            {
                _targetLocalRot *= deltaLocal;
            }

            var t = 1.0f - Mathf.Pow(_rotationSmoothFactor, independentDeltaTime);
            var newWorldRot = Quaternion.Slerp(worldTrans.localRotation, _targetWorldRot, t);
            var newLocalRot = Quaternion.Slerp(localTrans.localRotation, _targetLocalRot, t);

            worldTrans.localRotation = newWorldRot;
            localTrans.localRotation = newLocalRot;
        }

        // Prevents slerp from sudden direction change when shortest angle flips around to opposite dir 
        private bool IsYawRotationFlippedByNewRotation(Quaternion delta)
        {
            var preTargetForward = _targetWorldRot * Vector3.fwd;
            var aftTargetForward = _targetWorldRot * delta * Vector3.fwd;
            var currRot = transform.localRotation * Vector3.fwd;
            
            var rotAxis = Vector3.Cross(_fpsCamera.Forward, preTargetForward);
            var preAngle = Vector3.SignedAngle(currRot, preTargetForward, rotAxis);
            var aftAngle = Vector3.SignedAngle(currRot, aftTargetForward, rotAxis);

            return Math.Abs(preAngle) >= CinemaCamSettings.RotationDiffMax && Math.Sign(preAngle) != Math.Sign(aftAngle);
        }
        
        private bool IsPitchRotationFlippedByNewRotation(Quaternion delta)
        {
            var preTargetForward = _targetLocalRot * Vector3.fwd;
            var aftTargetForward = _targetLocalRot * delta * Vector3.fwd;
            var currRot = _childTrans.localRotation * Vector3.fwd;
            
            var rotAxis = Vector3.Cross(_fpsCamera.Forward, preTargetForward);
            var preAngle = Vector3.SignedAngle(currRot, preTargetForward, rotAxis);
            var aftAngle = Vector3.SignedAngle(currRot, aftTargetForward, rotAxis);

            return Math.Abs(preAngle) >= CinemaCamSettings.RotationDiffMax && Math.Sign(preAngle) != Math.Sign(aftAngle);
        }
        
        // Pitch causes more trouble than good so it is commented out
        private void CalculateDynamicRotationDelta()
        {
            var independentDeltaTime = IndependentDeltaTimeManager.GetDeltaTime();
            
            var worldTrans = transform;
            var localTrans = _childTrans;
            
            var up = _alignPitchAxisWCam ? localTrans.up : Vector3.up;
            var right = _alignPitchAxisWCam ? localTrans.right : FlatRight();
            // var forward = _mouseCtrlAltitude ? localTrans.forward : FlatForward();
            
            var velocityXZ = Vector3.ProjectOnPlane(_movementVelocity, up);
            var vector = velocityXZ * (_dynamicRotationSpeed * CinemaCamSettings.DynamicRotationSpeedScale);

            // var pitchDir = Mathf.Sign(Vector3.Dot(vector, forward));
            var rollDir = Mathf.Sign(Vector3.Dot(vector, right));

            // var pitch = Mathf.Clamp(Vector3.Project(vector, forward).magnitude, 0, DynamicRotationPitchMax) * pitchDir;
            var roll = Mathf.Clamp(Vector3.Project(vector, right).magnitude, 0, CinemaCamSettings.DynamicRotationRollMax) * rollDir;
            // var targetPitch = worldTrans.rotation * Quaternion.Euler(pitch, 0 , 0);
            var targetRoll = worldTrans.rotation * Quaternion.Euler(0, 0 , roll);

            var t = 1.0f - Mathf.Pow(CinemaCamSettings.DynamicRotationSmoothFactor, independentDeltaTime);
            // var newLocalRot = Quaternion.Slerp(localTrans.localRotation, targetPitch, t);
            var newWorldRot = Quaternion.Slerp(worldTrans.localRotation, targetRoll, t);

            // localTrans.localRotation = newLocalRot;
            worldTrans.localRotation = newWorldRot;
        }

        /// <summary>
        /// The cam controller hijacks the original camera at its default rotation and position
        /// but some functions of the game can change those values like dimensional warps and needs
        /// to be rest periodically
        /// </summary>
        private void CheckAndResetFpsCamRotation()
        {
            if (_fpsCamera.transform.parent.localRotation != Quaternion.identity)
            {
                _fpsCamera.transform.parent.localRotation = Quaternion.identity;
            }
        }

        private void UpdateZoom()
        {
            var independentDeltaTime = IndependentDeltaTimeManager.GetDeltaTime();

            var dir = KeyBindInputManager.GetAxis(AxisName.Zoom);
            var speed = _zoomSpeed * CinemaCamSettings.ZoomScaling;
            
            _targetZoom = Mathf.Clamp(_targetZoom + independentDeltaTime * speed * dir, CinemaCamSettings.ZoomMin, CinemaCamSettings.ZoomMax);
            
            var t = 1.0f - Mathf.Pow(_zoomSmoothFactor, independentDeltaTime);
            var newZoom = Mathf.Lerp(_currZoom, _targetZoom, t);
            
            _fpsCamera.m_camera.fieldOfView = newZoom;
            // Have separate zoom since accessing the field seems to reset it
            _currZoom = newZoom;
        }
        
        private void UpdateCuller()
        {
            var currPosition = transform.position;
            var raycastOrigWithOffset = currPosition + FlatForward();
            var newCullPos = Physics.Raycast(raycastOrigWithOffset, Vector3.down, out var hit) ? hit.point : currPosition;
            
            _fpsCamera.m_owner.m_movingCuller.UpdatePosition(_playerAgent.m_dimensionIndex, newCullPos);
        }

        private void CheckReset()
        {
            if (KeyBindInputManager.GetMiddleMouse())
            {
                OnReset();
            }
        }
        
        private void OnReset()
        {
            _targetZoom = CinemaCamSettings.ZoomDefault;
            _targetWorldRot = Quaternion.Euler(0, _targetWorldRot.eulerAngles.y, 0);
            _orbitDistance = CinemaCamSettings.OrbitDistanceDefault;

            if (_inOrbit)
            {
                _orbitOffsetTrans.localPosition = _orbitOffset;
            }
        }

        private Vector3 FlatForward()
        {
            return Vector3.ProjectOnPlane(_childTrans.forward, Vector3.up).normalized;
        }
        
        private Vector3 FlatRight()
        {
            return Vector3.ProjectOnPlane(_childTrans.right, Vector3.up).normalized;
        }

        private void SetMovementSpeed(float value)
        {
            _movementSpeed = value;
        }
        
        private void SetMovementSmoothTime(float value)
        {
            _movementSmoothFactor = value;
        }
        
        private void SetRotationSpeed(float value)
        {
            _rotationSpeed = value;
        }
        
        private void SetRotationSmoothTime(float value)
        {
            _rotationSmoothFactor = value;
        }
        
        private void SetZoomSpeed(float value)
        {
            _zoomSpeed = value;
        }
        
        private void SetZoomSmoothTime(float value)
        {
            _zoomSmoothFactor = value;
        }
        
        private void SetAlignPitchAxisWCam(bool value)
        {
            _alignPitchAxisWCam = value;
        }
        
        private void SetAlignRollAxisWCam(bool value)
        {
            _alignRollAxisWCam = value;
        }
        
        private void SetDynamicRotation(bool value)
        {
            _dynamicRotation = value;
        }

        private void SetDynamicRotationSpeed(float value)
        {
            _dynamicRotationSpeed = value;
        }

        private void OnDestroy()
        {
            CinemaUIManager.Current.Sliders[UIOption.MovementSpeedSlider].OnValueChanged -= SetMovementSpeed;
            CinemaUIManager.Current.Sliders[UIOption.MovementSmoothingSlider].OnValueChanged -= SetMovementSmoothTime;
            
            CinemaUIManager.Current.Sliders[UIOption.RotationSpeedSlider].OnValueChanged -= SetRotationSpeed;
            CinemaUIManager.Current.Sliders[UIOption.RotationSmoothingSlider].OnValueChanged -= SetRotationSmoothTime;

            CinemaUIManager.Current.Sliders[UIOption.ZoomSpeedSlider].OnValueChanged -= SetZoomSpeed;
            CinemaUIManager.Current.Sliders[UIOption.ZoomSmoothingSlider].OnValueChanged -= SetZoomSmoothTime;
            
            CinemaUIManager.Current.Toggles[UIOption.ToggleAlignPitchAxisWCam].OnValueChanged -= SetAlignPitchAxisWCam;
            CinemaUIManager.Current.Toggles[UIOption.ToggleAlignRollAxisWCam].OnValueChanged -= SetAlignRollAxisWCam;
            CinemaUIManager.Current.Toggles[UIOption.ToggleDynamicRoll].OnValueChanged -= SetDynamicRotation;
            CinemaUIManager.Current.Sliders[UIOption.DynamicRollIntensitySlider].OnValueChanged -= SetDynamicRotationSpeed;
            
            DimensionManager.OnDimensionWarp -= OnDimensionWarp;
        }
    }
}