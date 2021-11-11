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
        public static LookSmoothingController Current;
        public const float SmoothDefault = 0.2f;
        public const float SmoothMax = 2f;
        public const float SmoothMin = 0f;

        private float _currVal;
        private int _samples = 8;
               
        private FPSCamera _fpsCamera;

        public LookSmoothingController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        public void Awake()
        {
            Current = this;

            _fpsCamera = FindObjectOfType<FPSCamera>();
        }

        public void Start()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleLookSmoothing]).OnValueChanged += OnSmoothToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.LookSmoothingSlider]).OnValueChanged += OnSmoothValChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.TimeScaleSlider]).OnValueChanged += OnTimeScaleChange;
        }

        public void OnSmoothToggle(bool value)
        {
            _currVal = SmoothDefault;
            _fpsCamera.MouseSmoother.m_curve = SmoothDefault;
        }
        
        public void OnSmoothValChange(float value)
        {
            _currVal = value;
            _fpsCamera.MouseSmoother.m_curve = _currVal;
            _fpsCamera.MouseSmoother.Samples = Mathf.RoundToInt(_samples / Time.timeScale);
        }
        
        public void OnTimeScaleChange(float value)
        {
            OnSmoothValChange(_currVal);
        }

        private void OnDestroy()
        {
            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleLookSmoothing]).OnValueChanged -= OnSmoothToggle;
            ((SliderOption) CinemaUIManager.Options[UIOption.LookSmoothingSlider]).OnValueChanged -= OnSmoothValChange;
            ((SliderOption) CinemaUIManager.Options[UIOption.TimeScaleSlider]).OnValueChanged -= OnTimeScaleChange;
        }
    }
}