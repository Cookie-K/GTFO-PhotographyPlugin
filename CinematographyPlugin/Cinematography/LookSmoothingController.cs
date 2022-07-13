using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.Cinematography.Settings;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class LookSmoothingController : MonoBehaviour
    {
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
            CinemaUIManager.Current.Toggles[UIOption.ToggleFpsLookSmoothing].OnValueChanged += OnFpsSmoothToggle;
            CinemaUIManager.Current.Sliders[UIOption.FpsLookSmoothingSlider].OnValueChanged += OnFpsSmoothValChange;
            CinemaUIManager.Current.Sliders[UIOption.TimeScaleSlider].OnValueChanged += OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer += OnTimeScaleChange;
        }

        private void OnFpsSmoothToggle(bool value)
        {
            if (!value)
            {
                _currVal = CinemaCamSettings.LookSmoothDefault;
                _fpsCamera.MouseSmoother.m_curve = CinemaCamSettings.LookSmoothDefault;
            }
        }

        private void OnFpsSmoothValChange(float value)
        {
            if (Time.timeScale > CinemaCamSettings.TimeScaleMax)
            {
                // Do not update the sensitivity for values above max as it is more disorienting than useful
                return;
            }
            
            _currVal = value;
            _fpsCamera.MouseSmoother.Curve = _currVal + CinemaCamSettings.LookSmoothingScale * (1 - Time.timeScale);
            _fpsCamera.MouseSmoother.Samples = Mathf.RoundToInt(_initialSamples / Time.timeScale);
        }

        private void OnTimeScaleChange(float value)
        {
            OnFpsSmoothValChange(_currVal);
        }

        private void OnDestroy()
        {
            CinemaUIManager.Current.Toggles[UIOption.ToggleFpsLookSmoothing].OnValueChanged -= OnFpsSmoothToggle;
            CinemaUIManager.Current.Sliders[UIOption.FpsLookSmoothingSlider].OnValueChanged -= OnFpsSmoothValChange;
            CinemaUIManager.Current.Sliders[UIOption.TimeScaleSlider].OnValueChanged -= OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer -= OnTimeScaleChange;
        }
    }
}