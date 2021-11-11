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
            return FindObjectOfType<FPSCamera>().m_camera.fieldOfView;
        }

        public void OnFoVChange(float value)
        {
            _fpsCamera.m_camera.fieldOfView = value;
        }

        private void OnDestroy()
        {
            ((SliderOption) CinemaUIManager.Options[UIOption.FoVSlider]).OnValueChanged -= OnFoVChange;
        }
    }
}