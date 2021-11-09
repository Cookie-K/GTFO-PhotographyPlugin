using System.Collections.Generic;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public static class UIUtils
    {
        
        public static Dictionary<UIOption, Option> BuildOptions(GameObject cinemaUI)
        {
            var options = new Dictionary<UIOption, Option>();
            options.Add(UIOption.ToggleUI, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleUI), true, true));
            options.Add(UIOption.ToggleBody, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleBody), true, true));
            
            options.Add(UIOption.ToggleFreeCamera, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleFreeCamera), false, true));
            options.Add(UIOption.MovementSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSpeedSlider), false, FreeCameraController.MovementSpeedDefault, FreeCameraController.MovementSpeedMin, FreeCameraController.MovementSpeedMax));
            options.Add(UIOption.MovementSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSmoothingSlider), false, FreeCameraController.SmoothTimeDefault, FreeCameraController.SmoothTimeMin, FreeCameraController.SmoothTimeMax));
            options.Add(UIOption.ToggleDynamicRoll, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleDynamicRoll), false, false));
            options.Add(UIOption.DynamicRollIntensitySlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.DynamicRollIntensitySlider), false, FreeCameraController.DynamicRollIntensityDefault, FreeCameraController.DynamicRollIntensityMin, FreeCameraController.DynamicRollIntensityMax));
            options.Add(UIOption.ToggleMouseIndependentCtrl, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleMouseIndependentCtrl), false, false));
            
            options.Add(UIOption.ToggleLookSmoothing, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleLookSmoothing), false, true));
            options.Add(UIOption.LookSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.LookSmoothingSlider), false, LookSmoothingController.SmoothDefault, LookSmoothingController.SmoothMin, LookSmoothingController.SmoothMax));
            
            options.Add(UIOption.ToggleCameraRoll, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleCameraRoll), false, true));
            options.Add(UIOption.CameraRollSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.CameraRollSlider), false, CameraRollController.RollDefault, CameraRollController.RollMin, CameraRollController.RollMax));
            
            options.Add(UIOption.ToggleTimeScale, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleTimeScale), false, true));
            options.Add(UIOption.TimeScaleSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.TimeScaleSlider), false, 0, 0, 1));
            
            options.Add(UIOption.ToggleFoV, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleFoV), false, true));
            options.Add(UIOption.FoVSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FoVSlider), false, 0, 0, 1));

            return options;
        }

        private static GameObject GetOptionObj(GameObject cinemaUI, UIOption option)
        {
            // CinemaUI/Canvas/Window/ViewPort
            var windowViewPort = cinemaUI.transform.GetChild(0).GetChild(0).GetChild(0);
            // ViewPort/Body/ViewPort
            var bodyViewPort = windowViewPort.GetChild(1).GetChild(0);
            CinematographyCore.log.LogInfo($"Looking for {option.ToString()}");
            var gameObject = bodyViewPort.transform.Find(option.ToString()).gameObject;
            CinematographyCore.log.LogInfo($"Found       {option.ToString()}");
            return gameObject;
        }
    }
}