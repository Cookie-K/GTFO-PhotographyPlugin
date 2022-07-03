using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using CinematographyPlugin.UI.UiInput;
using CinematographyPlugin.Util;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class TimeScaleController : MonoBehaviour
    {
        public const float TimeScaleDefault = 1;
        public const float TimeScaleMin = 0.01f;
        public const float TimeScaleMax = 1;
        private float TimeChangeSpeed = 1f;
        
        private float _targetTimeScale = TimeScaleDefault;
        
        private ToggleOption _timeScaleToggle;
        private SliderOption _timeScaleSlider;

        private void Start()
        {
            _timeScaleToggle = (ToggleOption) CinemaUIManager.Options[UIOption.ToggleTimeScale];
            _timeScaleSlider = (SliderOption) CinemaUIManager.Options[UIOption.TimeScaleSlider];
            _timeScaleSlider.OnValueChanged += OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer += OnTimeScaleChange;
        }

        private void Update()
        {
            if (_timeScaleToggle.Toggle.isOn)
            {
                UpdateTimeScaleFromKeyBinds();
            }
        }

        public static void ResetTimeScale()
        {
            Time.timeScale = 1;
        }

        private void OnTimeScaleChange(float value)
        {
            Time.timeScale = value;
        }

        private void UpdateTimeScaleFromKeyBinds()
        {
            _targetTimeScale = Mathf.Clamp(_targetTimeScale + InputManager.GetTimeScaleInput(), TimeScaleMin, TimeScaleMax);
            if (Math.Abs(_targetTimeScale - Time.timeScale) > 0.001)
            {
                var newTimeScale = Mathf.MoveTowards(Time.timeScale, _targetTimeScale, IndependentDeltaTimeManager.GetDeltaTime() * TimeChangeSpeed);
                _timeScaleSlider.OnSliderChange(newTimeScale);
            }
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;
            ((SliderOption) CinemaUIManager.Options[UIOption.TimeScaleSlider]).OnValueChanged -=  OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer -= OnTimeScaleChange;
        }
    }
}