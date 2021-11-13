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
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDynamicRoll]).OnValueChanged += OnDynamicRollToggle;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFreeCamera]).OnValueChanged += OnFreeCamToggle;
            FreeCameraController.OnRollAngleChange += OnRollAngleChange;
        }

        private void OnRollToggle(bool value)
        {
            ResetAngle(value);
        }

        private void OnDynamicRollToggle(bool value)
        {
            ResetAngle(value);
        }

        private void OnFreeCamToggle(bool value)
        {
            ResetAngle(!value);
        }

        private void ResetAngle(bool reset)
        {
            if (reset)
            {
                SetRoll(0);
            }
        }

        private void OnRollAngleChange(float value)
        {
            SetRoll(value);
        }

        private void SetRoll(float value)
        {
            FreeCameraController.FreeCam.rotation = Quaternion.identity;
            FreeCameraController.FreeCam.RotateAround(FreeCameraController.FreeCam.position, _fpsCamera.Forward, value);
        }

        private void OnDestroy()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleCameraRoll]).OnValueChanged -= OnRollToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.CameraRollSlider]).OnValueChanged -= OnRollAngleChange;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDynamicRoll]).OnValueChanged -= OnDynamicRollToggle;
            FreeCameraController.OnRollAngleChange -= OnRollAngleChange;
        }
    }

}