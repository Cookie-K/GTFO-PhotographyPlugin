using System;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using Il2CppSystem.Linq.Expressions.Interpreter;
using UnityEngine;
using UnityEngine.Events;

namespace CinematographyPlugin.Cinematography
{
    public class CameraRollController : MonoBehaviour
    {
        public const float RollDefault = 0f;
        public const float RollMax = 180f;
        public const float RollMin = -180f;
        
        public const float RollSpeedDefault = 5f;
        public const float RollSpeedScaling = 5f;
        public const float RollSpeedMin = 0f;
        public const float RollSpeedMax = 10f;
        
        public const float RollTimeDefault = 0.2f;
        public const float RollTimeMin = 0f;
        public const float RollTimeMax = 2f;
        
        private bool _rollSet;
        private bool _dynRollSet;
        private float _currAngle;
        private float _targetAngle;
        private float _speed;
        private float _velocity;
        private float _smoothTime;

        private FPSCamera _fpsCamera;

        public CameraRollController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        public void Awake()
        {
            _fpsCamera = FindObjectOfType<FPSCamera>();
        }

        private void Start()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleCameraRoll]).OnValueChanged += OnRollToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSlider]).OnValueChanged += OnRollAngleChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSpeedSlider]).OnValueChanged += OnRollSpeedChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSmoothingSlider]).OnValueChanged += OnRollSmoothChange;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDynamicRoll]).OnValueChanged += OnDynamicRollToggle;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFreeCamera]).OnValueChanged += OnFreeCamToggle;
            FreeCameraController.OnRollAngleChange += OnRollAngleChange;
        }

        private void Update()
        {
            if (!_rollSet && !_dynRollSet) return;
            if (_rollSet)
            {
                var dir = Input.GetKey(KeyCode.LeftArrow) ? 1f : Input.GetKey(KeyCode.RightArrow) ? -1f : 0f;
                _targetAngle = _currAngle + _speed * RollSpeedScaling * dir;
                _currAngle = Utils.SmoothDampNoOvershootProtection(_currAngle, _targetAngle, ref _velocity, _smoothTime * Time.timeScale);

                WrapAngles();
                ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSlider]).Slider.Set(_currAngle);
            }

            FreeCameraController.FreeCamCtrl.rotation = Quaternion.identity;
            FreeCameraController.FreeCamCtrl.Rotate(_fpsCamera.Forward, _currAngle);
        }

        // set angle at +-180
        private void WrapAngles()
        {
            _currAngle = (_currAngle + 180f) % 360f;
            _currAngle += _currAngle < 0 ? 180 : -180;
            
            _targetAngle = (_targetAngle + 180f) % 360f;
            _targetAngle += _targetAngle < 0 ? 180 : -180;
        }

        private void OnRollToggle(bool value)
        {
            _rollSet = value;
            ResetAngle(value);
        }

        private void OnDynamicRollToggle(bool value)
        {
            _dynRollSet = value;
            ResetAngle(value);
            if (!value)
            {
                _smoothTime = 0;
            }
        }

        private void OnFreeCamToggle(bool value)
        {
            ResetAngle(!value);
        }

        private void ResetAngle(bool toggleValue)
        {
            if (!toggleValue)
            {
                FreeCameraController.FreeCamCtrl.rotation = Quaternion.identity;
                _currAngle = 0;
                _speed = RollSpeedDefault;
                _smoothTime = RollTimeDefault;
                _velocity = 0f;
                Update();
            }
        }

        private void OnRollAngleChange(float value)
        {
            _currAngle = value;
        }

        private void OnRollSpeedChange(float value)
        {
            _speed = value;
        }
        
        private void OnRollSmoothChange(float value)
        {
            _smoothTime = value;
        }

        private void OnDestroy()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleCameraRoll]).OnValueChanged -= OnRollToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSlider]).OnValueChanged -= OnRollAngleChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSpeedSlider]).OnValueChanged -= OnRollSpeedChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSmoothingSlider]).OnValueChanged -= OnRollSmoothChange;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDynamicRoll]).OnValueChanged -= OnDynamicRollToggle;
            FreeCameraController.OnRollAngleChange -= OnRollAngleChange;
        }
    }

}