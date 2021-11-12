using System;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.Events;

namespace CinematographyPlugin.Cinematography
{
    public class TimeScaleController : MonoBehaviour
    {
        public const float TimeScaleDefault = 1;
        public const float TimeScaleMin = 0.01f;
        public const float TimeScaleMax = 2;
        
        public TimeScaleController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        private void Start()
        {
            ((SliderOption) CinemaUIManager.Options[UIOption.TimeScaleSlider]).OnValueChanged +=  OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer += OnTimeScaleChange;
        }

        public void OnTimeScaleChange(float value)
        {
            Time.timeScale = value;
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;
            ((SliderOption) CinemaUIManager.Options[UIOption.TimeScaleSlider]).OnValueChanged -=  OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer -= OnTimeScaleChange;
        }
    }
}