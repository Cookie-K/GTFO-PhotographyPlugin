using System;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace CinematographyPlugin.Cinematography
{
    public class PostProcessingController : MonoBehaviour
    {
        public const float FocusDistanceMin = 0f;
        public const float FocusDistanceMax = 10f;
        private static float _focusDistanceDefault;
        
        public const float ApertureMin = 0f;
        public const float ApertureMax = 5f;
        private static float _apertureDefault;
        
        public const float FocalLenghtMin = 0.1f; // in cm
        public const float FocalLenghtMax = 10f; // in cm
        private static float _focalLenghtDefault; // in cm

        private PostProcessingProfile _ppp;
        
        public PostProcessingController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        private void Awake()
        {
            var fpsCamera = FindObjectOfType<FPSCamera>();
            var ppBehaviour = fpsCamera.GetComponent<PostProcessingBehaviour>();
            _ppp = ppBehaviour.profile;
        }

        private void Start()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleVignette]).OnValueChanged += OnVignetteToggle;
            // ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDoF]).OnValueChanged += OnDoFChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocusDistanceSlider]).OnValueChanged +=  OnFocusDistanceChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.ApertureSlider]).OnValueChanged +=  OnApertureChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocalLenghtSlider]).OnValueChanged +=  OnFocalLenghtChange;
        }

        public static float GetDefaultFocusDistance()
        {
            if (_focusDistanceDefault < 0.001)
            {
                var ppp = FindObjectOfType<FPSCamera>().GetComponent<PostProcessingBehaviour>().profile;
                _focusDistanceDefault = ppp.depthOfField.settings.focusDistance;
            }
            return _focusDistanceDefault;
        }
        
        public static float GetDefaultAperture()
        {
            if (_apertureDefault < 0.001)
            {
                var ppp = FindObjectOfType<FPSCamera>().GetComponent<PostProcessingBehaviour>().profile;
                _apertureDefault = ppp.depthOfField.settings.aperture;
            }

            return _apertureDefault;
        }
        
        public static float GetDefaultFocalLenght()
        {
            if (_focalLenghtDefault < 0.001)
            {
                var ppp = FindObjectOfType<FPSCamera>().GetComponent<PostProcessingBehaviour>().profile;
                _focalLenghtDefault = ppp.depthOfField.settings.focalLength / 10;
            }

            return _focalLenghtDefault;
        }
        
        private void OnVignetteToggle(bool value)
        {
            _ppp.vignette.enabled = value;
        }
        
        // private void OnDoFChange(bool value)
        // {
        //     var newSettings = _ppp.depthOfField.settings;
        //     newSettings.focusDistance = GetDefaultFocusDistance();
        //     _ppp.depthOfField.settings = newSettings;
        // }
        
        private void OnFocusDistanceChange(float value)
        {
            var newSettings = _ppp.depthOfField.settings;
            newSettings.focusDistance = value;
            _ppp.depthOfField.settings = newSettings;
        }
        
        private void OnApertureChange(float value)
        {
            var newSettings = _ppp.depthOfField.settings;
            newSettings.aperture = value;
            _ppp.depthOfField.settings = newSettings;
        }
        
        private void OnFocalLenghtChange(float value)
        {
            var newSettings = _ppp.depthOfField.settings;
            newSettings.focalLength = value * 10;
            _ppp.depthOfField.settings = newSettings;
        }

        private void OnDestroy()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleVignette]).OnValueChanged -= OnVignetteToggle;
            // ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleDoF]).OnValueChanged -= OnDoFChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocusDistanceSlider]).OnValueChanged -=  OnFocusDistanceChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.ApertureSlider]).OnValueChanged -=  OnApertureChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocalLenghtSlider]).OnValueChanged -=  OnFocalLenghtChange;
        }
    }
}