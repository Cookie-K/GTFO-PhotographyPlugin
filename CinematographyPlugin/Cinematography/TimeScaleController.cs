using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.Cinematography.Settings;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using CinematographyPlugin.UI.UiInput;
using CinematographyPlugin.Util;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class TimeScaleController : MonoBehaviour
    {
        private ToggleOption _timeScaleToggle;
        private SliderOption _timeScaleSlider;

        private void Start()
        {
            _timeScaleToggle = (ToggleOption) CinemaUIManager.Current.Options[UIOption.ToggleTimeScale];
            _timeScaleSlider = (SliderOption) CinemaUIManager.Current.Options[UIOption.TimeScaleSlider];
            _timeScaleSlider.OnValueChanged += OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer += OnTimeScaleChange;
        }

        private void Update()
        {
            if (CinemaCamManager.Current.FreeCamEnabled())
            {
                UpdateTimeScaleFromKeyBinds();
            }
        }

        public static void ResetTimeScale()
        {
            Time.timeScale = CinemaCamSettings.TimeScaleDefault;
        }

        private void OnTimeScaleChange(float value)
        {
            Time.timeScale = value;
        }
 
        private void UpdateTimeScaleFromKeyBinds()
        {
            var timeScaleInput = KeyBindInputManager.GetTimeScaleInput();
            if (timeScaleInput != 0f)
            {
                _timeScaleSlider.OnSliderChange(Mathf.Clamp(Time.timeScale + timeScaleInput, CinemaCamSettings.TimeScaleMin, CinemaCamSettings.TimeScaleMax));    
            }

            if (KeyBindInputManager.GetTimeScalePausePlay())
            {
                TogglePausePlay();
            }
            
            // Automatically turn on the time scale toggle if it's off and are using the key binds
            if (Math.Abs(Time.timeScale - CinemaCamSettings.TimeScaleMax) > 0.001 && !_timeScaleToggle.Toggle.isOn)
            {
                _timeScaleToggle.Toggle.Set(true);
            }
        }
        
        private void TogglePausePlay()
        {
            var newTimeScale = Math.Abs(Time.timeScale - CinemaCamSettings.TimeScaleMin) > 0.01 ? CinemaCamSettings.TimeScaleMin : CinemaCamSettings.TimeScaleMax;
            _timeScaleSlider.OnSliderChange(newTimeScale);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;
            ((SliderOption) CinemaUIManager.Current.Options[UIOption.TimeScaleSlider]).OnValueChanged -=  OnTimeScaleChange;
            CinemaNetworkingManager.OnTimeScaleChangedByOtherPlayer -= OnTimeScaleChange;
        }
    }
}