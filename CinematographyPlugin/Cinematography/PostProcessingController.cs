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
        public const float FocusDistanceMax = 50f;
        private static float _focusDistanceDefault;
        
        public const float ApertureMin = 0f;
        public const float ApertureMax = 10f;
        private static float _apertureDefault;
        
        public const float FocalLenghtMin = 0f;
        public const float FocalLenghtMax = 100f;
        private static float _focalLenghtDefault;

        private static float _currFocusDistance;
        private static float _currAperture;
        private static float _currFocalLenght;
        private static bool _init;

        private static ToggleOption _dofToggle;
        private PostProcessVolume _ppv;
        private DepthOfFieldModel _dofModel;
        private DepthOfField _dof;
        private Vignette _vin;
        private AmbientParticles _ambientParticles;
        
        private void Awake()
        {
            var fpsCamera = PlayerManager.GetLocalPlayerAgent().FPSCamera;

            _dofToggle = (ToggleOption) CinemaUIManager.Options[UIOption.ToggleVignette];
            _ambientParticles = fpsCamera.GetComponent<AmbientParticles>();
            _ppv = fpsCamera.GetComponent<PostProcessVolume>();
            _dof = _ppv.profile.GetSetting<DepthOfField>();
            _vin = _ppv.profile.GetSetting<Vignette>();
            
            _init = true;
        }

        private void Start()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleVignette]).OnValueChanged += OnVignetteToggle;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleAmbientParticles]).OnValueChanged += OnAmbientParticleToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocusDistanceSlider]).OnValueChanged +=  OnFocusDistanceChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.ApertureSlider]).OnValueChanged +=  OnApertureChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocalLenghtSlider]).OnValueChanged +=  OnFocalLenghtChange;
        }

        public static bool IsDoFSet()
        {
            return _init && _dofToggle.Toggle.isOn;
        }

        private void SetDoF()
        {
            _dofModel.settings = _dofModel.settings with
            {
                focusDistance = _currFocusDistance,
                aperture = _currAperture,
                focalLength = _currFocalLenght,
            };

            _dof.focusDistance.value = _currFocusDistance;
            _dof.aperture.value = _currAperture;
            _dof.focalLength.value = _currFocalLenght;
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
            _currFocusDistance = value;
            SetDoF();
        }
        
        private void OnApertureChange(float value)
        {
            _currAperture = value;
            SetDoF();
        }
        
        private void OnFocalLenghtChange(float value)
        {
            _currFocalLenght = value;
            SetDoF();
        }

        private void OnDestroy()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleVignette]).OnValueChanged -= OnVignetteToggle;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleAmbientParticles]).OnValueChanged -= OnAmbientParticleToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocusDistanceSlider]).OnValueChanged -=  OnFocusDistanceChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.ApertureSlider]).OnValueChanged -=  OnApertureChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.FocalLenghtSlider]).OnValueChanged -=  OnFocalLenghtChange;

            _init = false;
        }
    }
}