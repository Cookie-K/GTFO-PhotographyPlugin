using System;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace CinematographyPlugin.Cinematography
{
    public class FoVController : MonoBehaviour
    {
        public const float FovMin = 1f;
        public const float FovMax = 160f;
        private static float _foVDefault;
        
        public const float FovSpeedDefault = 5f;
        public const float FovSpeedMin = 0f;
        public const float FovSpeedScaling = 5f;
        public const float FovSpeedMax = 10f;
        
        public const float FoVTimeDefault = 0.2f;
        public const float FoVTimeMin = 0f;
        public const float FoVTimeMax = 2f;

        private bool _fovChangeOn;
        private float _speed;
        private float _velocity;
        private float _currFoV;
        private float _targetFov;
        private float _smoothTime;
        
        private FPSCamera _fpsCamera;
        
        public FoVController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        private void Awake()
        {
            _fpsCamera = FindObjectOfType<FPSCamera>();
        }

        private void Start()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFoV]).OnValueChanged += OnFoVToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.FoVSlider]).OnValueChanged += OnFoVChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FoVSpeedSlider]).OnValueChanged += OnFoVSpeedChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FoVSmoothingSlider]).OnValueChanged += OnFoVSmoothChange;
        }

        private void Update()
        {
            if (!_fovChangeOn) return;

            var dir = Input.GetKey(KeyCode.UpArrow) ? 1f : Input.GetKey(KeyCode.DownArrow) ? -1f : 0f;
            _targetFov = _currFoV + _speed * FovSpeedScaling * dir;
            _currFoV = Mathf.Clamp(Utils.SmoothDampNoOvershootProtection(_currFoV, _targetFov, ref _velocity, _smoothTime * Time.timeScale), FovMin, FovMax);
            _fpsCamera.m_camera.fieldOfView = _currFoV;
            ((SliderOption) CinemaUIManager.Options[UIOption.FoVSlider]).Slider.Set(_currFoV);
        }

        public static float GetDefaultFoV()
        {
            if (_foVDefault < 0.01)
            {
                _foVDefault = FindObjectOfType<FPSCamera>().m_camera.fieldOfView;
            }
            return _foVDefault;
        }

        private void OnFoVToggle(bool value)
        {
            _fovChangeOn = value;
            if (!value)
            {
                _targetFov = GetDefaultFoV();
                _speed = 1;
                _velocity = 0;
                Update();
            }
        }

        private void OnFoVChange(float value)
        {
            _currFoV = value;
        }
        
        private void OnFoVSpeedChange(float value)
        {
            _speed = value;
        }

        private void OnFoVSmoothChange(float value)
        {
            _smoothTime = value;
        }
        
        private void OnDestroy()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFoV]).OnValueChanged -= OnFoVToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.FoVSlider]).OnValueChanged -= OnFoVChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FoVSpeedSlider]).OnValueChanged -= OnFoVSpeedChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FoVSmoothingSlider]).OnValueChanged -= OnFoVSmoothChange;
        }
    }
}