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

        private bool _rollSet;
        private bool _dynRollSet;
        private float _currAngle;
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

        private void Update()
        {
            if (_rollSet || _dynRollSet)
            {
                FreeCameraController.FreeCam.rotation = Quaternion.AngleAxis(_currAngle, _fpsCamera.Forward);
            }
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
        }

        private void OnFreeCamToggle(bool value)
        {
            ResetAngle(!value);
        }

        private void ResetAngle(bool reset)
        {
            if (reset)
            {
                FreeCameraController.FreeCam.rotation = Quaternion.identity;
            }
        }

        private void OnRollAngleChange(float value)
        {
            _currAngle = value;
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