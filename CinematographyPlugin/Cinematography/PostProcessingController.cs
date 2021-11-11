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
        public const float ApertureMin = 0f;
        public const float ApertureMax = 5f;
        public const float FocalLenghtMin = 0.1f; // in cm
        public const float FocalLenghtMax = 10f; // in cm

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
            ((SliderOption) CinemaUIManager.Options[UIOption.FocusDistanceSlider]).OnValueChanged +=  OnFocusDistanceChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.ApertureSlider]).OnValueChanged +=  OnApertureChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocalLenghtSlider]).OnValueChanged +=  OnFocalLenghtChange;
        }

        public static float GetDefaultFocusDistance()
        {
            var ppp = FindObjectOfType<FPSCamera>().GetComponent<PostProcessingBehaviour>().profile;
            return ppp.depthOfField.settings.focusDistance;
        }
        
        public static float GetDefaultAperture()
        {
            var ppp = FindObjectOfType<FPSCamera>().GetComponent<PostProcessingBehaviour>().profile;
            return ppp.depthOfField.settings.aperture;
        }
        
        public static float GetDefaultFocalLenght()
        {
            var ppp = FindObjectOfType<FPSCamera>().GetComponent<PostProcessingBehaviour>().profile;
            return ppp.depthOfField.settings.focalLength;
        }
        
        private void OnVignetteToggle(bool value)
        {
            _ppp.vignette.enabled = value;
        }
        
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
            ((SliderOption) CinemaUIManager.Options[UIOption.FocusDistanceSlider]).OnValueChanged -=  OnFocusDistanceChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.ApertureSlider]).OnValueChanged -=  OnApertureChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocalLenghtSlider]).OnValueChanged -=  OnFocalLenghtChange;
        }
    }
}