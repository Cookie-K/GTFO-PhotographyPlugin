using System;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace CinematographyPlugin.Cinematography
{
    public class FoVController : MonoBehaviour
    {
        public const float FovMin = 1f;
        public const float FovMax = 160f;
        private static float _foVDefault;
        
        private FPSCamera _fpsCamera;
        
        public FoVController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        private void Awake()
        {
            _fpsCamera = FindObjectOfType<FPSCamera>();
        }

        private void Start()
        {
            ((SliderOption) CinemaUIManager.Options[UIOption.FoVSlider]).OnValueChanged += OnFoVChange;
        }

        public static float GetDefaultFoV()
        {
            if (_foVDefault < 0.01)
            {
                _foVDefault = FindObjectOfType<FPSCamera>().m_camera.fieldOfView;
            }
            return _foVDefault;
        }

        private void OnFoVChange(float value)
        {
            _fpsCamera.m_camera.fieldOfView = value;
        }

        private void OnDestroy()
        {
            ((SliderOption) CinemaUIManager.Options[UIOption.FoVSlider]).OnValueChanged -= OnFoVChange;
        }
    }
}