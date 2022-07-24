using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using Player;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace CinematographyPlugin.Cinematography
{
    public class PostProcessingController : MonoBehaviour
    {
        private static float _currFocusDistance;
        private static float _currAperture;
        private static float _currFocalLenght;
        private static bool _init;

        private static ToggleOption _dofToggle;
        private PostProcessVolume _ppv;
        private DepthOfField _dof;
        private Vignette _vin;
        private AmbientParticles _ambientParticles;
        
        private void Awake()
        {
            var fpsCamera = PlayerManager.GetLocalPlayerAgent().FPSCamera;

            _dofToggle = (ToggleOption) CinemaUIManager.Current.Options[UIOption.ToggleVignette];
            _ambientParticles = fpsCamera.GetComponent<AmbientParticles>();
            _ppv = fpsCamera.GetComponent<PostProcessVolume>();
            _dof = _ppv.profile.GetSetting<DepthOfField>();
            _vin = _ppv.profile.GetSetting<Vignette>();
            
            _init = true;
        }

        private void Start()
        {
            ((ToggleOption) CinemaUIManager.Current.Options[UIOption.ToggleVignette]).OnValueChanged += OnVignetteToggle;
            ((ToggleOption) CinemaUIManager.Current.Options[UIOption.ToggleAmbientParticles]).OnValueChanged += OnAmbientParticleToggle;
            ((SliderOption) CinemaUIManager.Current.Options[UIOption.FocusDistanceSlider]).OnValueChanged +=  OnFocusDistanceChange;
            ((SliderOption) CinemaUIManager.Current.Options[UIOption.ApertureSlider]).OnValueChanged +=  OnApertureChange;
            ((SliderOption) CinemaUIManager.Current.Options[UIOption.FocalLenghtSlider]).OnValueChanged +=  OnFocalLenghtChange;
        }

        public static bool IsDoFSet()
        {
            return _init && _dofToggle.Toggle.isOn;
        }

        private void SetDoF()
        {
            _dof.focusDistance.value = _currFocusDistance;
            _dof.aperture.value = _currAperture;
            _dof.focalLength.value = _currFocalLenght;
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
            ((ToggleOption) CinemaUIManager.Current.Options[UIOption.ToggleVignette]).OnValueChanged -= OnVignetteToggle;
            ((ToggleOption) CinemaUIManager.Current.Options[UIOption.ToggleAmbientParticles]).OnValueChanged -= OnAmbientParticleToggle;
            ((SliderOption) CinemaUIManager.Current.Options[UIOption.FocusDistanceSlider]).OnValueChanged -=  OnFocusDistanceChange;
            ((SliderOption) CinemaUIManager.Current.Options[UIOption.ApertureSlider]).OnValueChanged -=  OnApertureChange;
            ((SliderOption) CinemaUIManager.Current.Options[UIOption.FocalLenghtSlider]).OnValueChanged -=  OnFocalLenghtChange;

            _init = false;
        }
    }
}