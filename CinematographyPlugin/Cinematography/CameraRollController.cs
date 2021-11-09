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
        private float _currAngle;
        
        private FPSCamera _fpsCamera;
        private FPSCameraHolder _fpsCamHolder;


        public CameraRollController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        public void Start()
        {
            _fpsCamera = FindObjectOfType<FPSCamera>();
            _fpsCamHolder = FindObjectOfType<FPSCameraHolder>();

            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleCameraRoll]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnRollToggle);
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSlider]).Slider.onValueChanged.AddListener((UnityAction<float>) OnRollAngleChange);
        }

        private void Update()
        {
            if (_rollOn)
            {
                _fpsCamHolder.transform.rotation = Quaternion.identity;
                _fpsCamHolder.transform.RotateAround(_fpsCamera.Position, _fpsCamera.Forward, _currAngle); 
            }
        }

        private void OnRollToggle(bool value)
        {
            _rollOn = value;
        }
        
        private void OnRollAngleChange(float value)
        {
            _currAngle = value;
        }
        
        public void SetRoll(float value)
        {
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSlider]).Slider.Set(value);
        }

    }

}