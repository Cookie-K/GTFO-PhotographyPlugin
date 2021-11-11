using System;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace CinematographyPlugin.Cinematography
{
    public class CameraRollController : MonoBehaviour
    {
        public const float RollDefault = 0f;
        public const float RollMax = 180f;
        public const float RollMin = -180f;
               
        private bool _rollOn;
        private bool _dynamicRollOn;
        private float _currAngle;
        
        private FPSCamera _fpsCamera;
        private FPSCameraHolder _fpsCamHolder;

        public CameraRollController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        public void Awake()
        {
            _fpsCamera = FindObjectOfType<FPSCamera>();
            _fpsCamHolder = FindObjectOfType<FPSCameraHolder>();
        }

        private void Start()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleCameraRoll]).OnValueChanged += OnRollToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSlider]).OnValueChanged += OnRollAngleChange;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDynamicRoll]).OnValueChanged += OnDynamicRollToggle;
            FreeCameraController.OnDynamicRollAngleChange += OnRollAngleChange;
        }

        private void Update()
        {
            if (_rollOn || _dynamicRollOn)
            {
                _fpsCamHolder.transform.rotation = Quaternion.identity;
                if (_currAngle != 0)
                {
                    _fpsCamHolder.transform.RotateAround(_fpsCamera.Position, _fpsCamera.Forward, _currAngle);
                }
            }
        }

        public void OnRollToggle(bool value)
        {
            _rollOn = value;
        }
        
        public void OnDynamicRollToggle(bool value)
        {
            _dynamicRollOn = value;
        }
        
        public void OnRollAngleChange(float value)
        {
            _currAngle = value;
        }

        private void OnDestroy()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleCameraRoll]).OnValueChanged -= OnRollToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSlider]).OnValueChanged -= OnRollAngleChange;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDynamicRoll]).OnValueChanged -= OnDynamicRollToggle;
            FreeCameraController.OnDynamicRollAngleChange -= OnRollAngleChange;
        }
    }

}