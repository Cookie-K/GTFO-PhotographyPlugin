using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class LookSmoothingController : MonoBehaviour
    {
        public const float SmoothDefault = 0.2f;
        public const float SmoothMax = 5f;
        public const float SmoothMin = 0f;
        
        private const float SmoothingScale = 4f;
        private float _currVal;
        private int _initialSamples = 8;
               
        private FPSCamera _fpsCamera;

        private void Awake()
        {
            _fpsCamera = FindObjectOfType<FPSCamera>();
            _initialSamples = _fpsCamera.MouseSmoother.Samples;
        }

        private void Start()
        {
            CinemaUIManager.Toggles[UIOption.ToggleFpsLookSmoothing].OnValueChanged += OnFpsSmoothToggle;
            CinemaUIManager.Sliders[UIOption.FpsLookSmoothingSlider].OnValueChanged += OnFpsSmoothValChange;
            CinemaUIManager.Sliders[UIOption.TimeScaleSlider].OnValueChanged += OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer += OnTimeScaleChange;
        }

        private void OnFpsSmoothToggle(bool value)
        {
            if (!value)
            {
                _currVal = SmoothDefault;
                _fpsCamera.MouseSmoother.m_curve = SmoothDefault;
            }
        }

        private void OnFpsSmoothValChange(float value)
        {
            if (Time.timeScale > TimeScaleController.TimeScaleMax)
            {
                // Do not update the sensitivity for values above max as it is more disorienting than useful
                return;
            }
            
            _currVal = value;
            _fpsCamera.MouseSmoother.Curve = _currVal + SmoothingScale * (1 - Time.timeScale);
            _fpsCamera.MouseSmoother.Samples = Mathf.RoundToInt(_initialSamples / Time.timeScale);
        }

        private void OnTimeScaleChange(float value)
        {
            OnFpsSmoothValChange(_currVal);
        }

        private void OnDestroy()
        {
            CinemaUIManager.Toggles[UIOption.ToggleFpsLookSmoothing].OnValueChanged -= OnFpsSmoothToggle;
            CinemaUIManager.Sliders[UIOption.FpsLookSmoothingSlider].OnValueChanged -= OnFpsSmoothValChange;
            CinemaUIManager.Sliders[UIOption.TimeScaleSlider].OnValueChanged -= OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer -= OnTimeScaleChange;
        }
    }
}