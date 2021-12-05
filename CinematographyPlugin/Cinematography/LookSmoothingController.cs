using System;
using System.Numerics;
using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using InControl;
using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace CinematographyPlugin.Cinematography
{
    public class LookSmoothingController : MonoBehaviour
    {
        public const float SmoothDefault = 0.2f;
        public const float SmoothMax = 2f;
        public const float SmoothMin = 0f;

        private float _currVal;
        private int _initialSamples = 8;
               
        private FPSCamera _fpsCamera;

        public LookSmoothingController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

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
            _currVal = value;
            _fpsCamera.MouseSmoother.Curve = _currVal + 2 * (1 - Time.timeScale);
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