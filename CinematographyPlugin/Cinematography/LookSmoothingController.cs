using System;
using System.Numerics;
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

        private bool _inCtrlOfTime;
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
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFpsLookSmoothing]).OnValueChanged += OnFpsSmoothToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.FpsLookSmoothingSlider]).OnValueChanged += OnFpsSmoothValChange;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleTimeScale]).OnValueChanged += OnTimeCtrlToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.TimeScaleSlider]).OnValueChanged += OnTimeScaleChange;
        }

        private void OnFpsSmoothToggle(bool value)
        {
            _currVal = SmoothDefault;
            _fpsCamera.MouseSmoother.m_curve = SmoothDefault;
        }

        private void OnTimeCtrlToggle(bool value)
        {
            _inCtrlOfTime = value;
        }

        private void OnFpsSmoothValChange(float value)
        {
            _currVal = value;
            _fpsCamera.MouseSmoother.Curve = _currVal + (_inCtrlOfTime ? 0 : 2 * (1 - Time.timeScale));
            _fpsCamera.MouseSmoother.Samples = Mathf.RoundToInt(_initialSamples / Time.timeScale);
        }

        private void OnTimeScaleChange(float value)
        {
            OnFpsSmoothValChange(_currVal);
        }

        private void OnDestroy()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleFpsLookSmoothing]).OnValueChanged -= OnFpsSmoothToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.FpsLookSmoothingSlider]).OnValueChanged -= OnFpsSmoothValChange;
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleTimeScale]).OnValueChanged -= OnTimeCtrlToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.TimeScaleSlider]).OnValueChanged -= OnTimeScaleChange;
        }
    }
}