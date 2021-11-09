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
        public const float SmoothMax = 1f;
        public const float SmoothMin = 0f;
               
        private bool smoothingOn;
        
        private FPSCamera _fpsCamera;

        public LookSmoothingController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        public void Start()
        {
            _fpsCamera = FindObjectOfType<FPSCamera>();

            ((ToggleOption) CinemaUIManager.Options[UIOption.ToggleLookSmoothing]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnSmoothToggle);
            ((SliderOption) CinemaUIManager.Options[UIOption.LookSmoothingSlider]).Slider.onValueChanged.AddListener((UnityAction<float>) OnSmoothTimeChange);
        }

        private void OnSmoothToggle(bool value)
        {
            smoothingOn = value;
            _fpsCamera.MouseSmoother.m_curve = SmoothDefault;
        }
        
        private void OnSmoothTimeChange(float value)
        {
            _fpsCamera.MouseSmoother.m_curve = value;
        }
    }
}