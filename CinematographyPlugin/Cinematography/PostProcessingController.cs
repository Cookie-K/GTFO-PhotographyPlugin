using System;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using Player;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.Rendering.PostProcessing;

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
        
        public const float FocalLenghtMin = 0.1f;
        public const float FocalLenghtMax = 10f;
        private static float _focalLenghtDefault;

        private PostProcessVolume _ppv;
        private DepthOfField _dof;
        private Vignette _vin;
        private AmbientParticles _ambientParticles;
        
        private void Awake()
        {
            _ambientParticles = PlayerManager.GetLocalPlayerAgent().FPSCamera.GetComponent<AmbientParticles>();
            _ppv = GetPostProcessVolume();
            _dof = _ppv.profile.GetSetting<DepthOfField>();
            _vin = _ppv.profile.GetSetting<Vignette>();
        }

        private void Start()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleVignette]).OnValueChanged += OnVignetteToggle;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleAmbientParticles]).OnValueChanged += OnAmbientParticleToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocusDistanceSlider]).OnValueChanged +=  OnFocusDistanceChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.ApertureSlider]).OnValueChanged +=  OnApertureChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocalLenghtSlider]).OnValueChanged +=  OnFocalLenghtChange;
        }

        private static PostProcessVolume GetPostProcessVolume()
        {
            return PlayerManager.GetLocalPlayerAgent().FPSCamera.GetComponent<PostProcessVolume>();
        }

        public static float GetDefaultFocusDistance()
        {
            if (_focusDistanceDefault < 0.001)
            {
                var dof = GetPostProcessVolume().profile.GetSetting<DepthOfField>();
                _focusDistanceDefault = dof.focusDistance;
            }
            return _focusDistanceDefault;
        }
        
        public static float GetDefaultAperture()
        {
            if (_apertureDefault < 0.001)
            {
                var dof = GetPostProcessVolume().profile.GetSetting<DepthOfField>();
                _apertureDefault = dof.aperture;
            }

            return _apertureDefault;
        }
        
        public static float GetDefaultFocalLenght()
        {
            if (_focalLenghtDefault < 0.001)
            {
                var dof = GetPostProcessVolume().profile.GetSetting<DepthOfField>();
                _focalLenghtDefault = dof.focalLength;
            }

            return _focalLenghtDefault;
        }
        
        public void OnAmbientParticleToggle(bool value)
        {
            _ambientParticles.enabled = value;
        }
        
        private void OnVignetteToggle(bool value)
        {
            _vin.active = value;
        }

        private void OnFocusDistanceChange(float value)
        {
            _dof.focusDistance.value = value;
        }
        
        private void OnApertureChange(float value)
        {
            _dof.focusDistance.value = value;
        }
        
        private void OnFocalLenghtChange(float value)
        {
            _dof.focusDistance.value = value;
        }

        private void OnDestroy()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleVignette]).OnValueChanged -= OnVignetteToggle;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleAmbientParticles]).OnValueChanged -= OnAmbientParticleToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocusDistanceSlider]).OnValueChanged -=  OnFocusDistanceChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.ApertureSlider]).OnValueChanged -=  OnApertureChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocalLenghtSlider]).OnValueChanged -=  OnFocalLenghtChange;
        }
    }
}