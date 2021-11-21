using System.Collections.Generic;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public static class UIFactory
    {
        
        public static Dictionary<UIOption, Option> BuildOptions(GameObject cinemaUI)
        {
            var options = new Dictionary<UIOption, Option>();
            options.Add(UIOption.ToggleUI, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleUI), true, true));
            options.Add(UIOption.ToggleBody, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleBody), true, true));
            
            options.Add(UIOption.ToggleFreeCamera, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleFreeCamera), false, true));
            options.Add(UIOption.MovementSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSpeedSlider), false, CinemaCamController.MovementSpeedDefault, CinemaCamController.MovementSpeedMin, CinemaCamController.MovementSpeedMax));
            options.Add(UIOption.MovementSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSmoothingSlider), false, CinemaCamController.MovementSmoothTimeDefault, CinemaCamController.MovementSmoothTimeMin, CinemaCamController.MovementSmoothTimeMax));
            options.Add(UIOption.RotationSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.RotationSpeedSlider), false, CinemaCamController.GetDefaultRotationSpeed(), CinemaCamController.RotationSpeedMin, CinemaCamController.RotationSpeedMax));
            options.Add(UIOption.RotationSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.RotationSmoothingSlider), false, CinemaCamController.RotationSmoothTimeDefault, CinemaCamController.RotationSmoothTimeMin, CinemaCamController.RotationSmoothTimeMax));
            options.Add(UIOption.ZoomSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.ZoomSpeedSlider), false, CinemaCamController.ZoomSpeedDefault, CinemaCamController.ZoomSpeedMin, CinemaCamController.ZoomSpeedMax));
            options.Add(UIOption.ZoomSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.ZoomSmoothingSlider), false, CinemaCamController.ZoomSmoothTimeDefault, CinemaCamController.ZoomSmoothTimeMin, CinemaCamController.ZoomSmoothTimeMax));

            options.Add(UIOption.ToggleDynamicRoll, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleDynamicRoll), false, false));
            options.Add(UIOption.DynamicRollIntensitySlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.DynamicRollIntensitySlider), false, CinemaCamController.DynamicRotationDefault, CinemaCamController.DynamicRotationMin, CinemaCamController.DynamicRotationMax));
            options.Add(UIOption.ToggleMouseCtrlAltitude, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleMouseCtrlAltitude), true, false));
            options.Add(UIOption.ToggleRollCtrlLateralAxis, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleRollCtrlLateralAxis), false, false));

            options.Add(UIOption.ToggleFpsLookSmoothing, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleFpsLookSmoothing), false, true));
            options.Add(UIOption.FpsLookSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FpsLookSmoothingSlider), false, LookSmoothingController.SmoothDefault, LookSmoothingController.SmoothMin, LookSmoothingController.SmoothMax));

            options.Add(UIOption.ToggleDoF, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleDoF), false, true));
            options.Add(UIOption.FocusDistanceSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FocusDistanceSlider), false, PostProcessingController.GetDefaultFocusDistance(), PostProcessingController.FocusDistanceMin, PostProcessingController.FocusDistanceMax));
            options.Add(UIOption.ApertureSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.ApertureSlider), false, PostProcessingController.GetDefaultAperture(), PostProcessingController.ApertureMin, PostProcessingController.ApertureMax));
            options.Add(UIOption.FocalLenghtSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FocalLenghtSlider), false, PostProcessingController.GetDefaultFocalLenght(), PostProcessingController.FocalLenghtMin, PostProcessingController.FocalLenghtMax));
            
            options.Add(UIOption.ToggleVignette, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleVignette), true, true));

            options.Add(UIOption.ToggleTimeScale, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleTimeScale), false, true));
            options.Add(UIOption.TimeScaleSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.TimeScaleSlider), false, TimeScaleController.TimeScaleDefault, TimeScaleController.TimeScaleMin, TimeScaleController.TimeScaleMax));

            // Add sub options
            options[UIOption.ToggleFreeCamera].SubOptions.AddRange(new []
            {
                options[UIOption.MovementSpeedSlider],
                options[UIOption.MovementSmoothingSlider],
                options[UIOption.RotationSpeedSlider],
                options[UIOption.RotationSmoothingSlider],
                options[UIOption.ZoomSpeedSlider],
                options[UIOption.ZoomSmoothingSlider],
                options[UIOption.ToggleDynamicRoll],
                options[UIOption.ToggleMouseCtrlAltitude],
                options[UIOption.ToggleRollCtrlLateralAxis],
            });
            
            options[UIOption.ToggleDynamicRoll].SubOptions.Add(options[UIOption.DynamicRollIntensitySlider]);
            
            options[UIOption.ToggleFpsLookSmoothing].SubOptions.Add(options[UIOption.FpsLookSmoothingSlider]);
            
            options[UIOption.ToggleTimeScale].SubOptions.Add(options[UIOption.TimeScaleSlider]);
            
            options[UIOption.ToggleDoF].SubOptions.AddRange(new []
            {
                options[UIOption.FocusDistanceSlider],
                options[UIOption.ApertureSlider],
                options[UIOption.FocalLenghtSlider]
            });
            
            // Add options to disable on select
            options[UIOption.ToggleFreeCamera].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleUI], false);
            options[UIOption.ToggleFreeCamera].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleBody], false);
            options[UIOption.ToggleFreeCamera].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleFpsLookSmoothing], false);
            
            return options;
        }

        public static Dictionary<UIOption, ToggleOption> GetToggles(Dictionary<UIOption, Option> options)
        {
            var toggles = new Dictionary<UIOption, ToggleOption>();
            
            toggles.Add(UIOption.ToggleUI, (ToggleOption) options[UIOption.ToggleUI]);
            toggles.Add(UIOption.ToggleBody, (ToggleOption) options[UIOption.ToggleBody]);
            toggles.Add(UIOption.ToggleFreeCamera, (ToggleOption) options[UIOption.ToggleFreeCamera]);
            toggles.Add(UIOption.ToggleMouseCtrlAltitude, (ToggleOption) options[UIOption.ToggleMouseCtrlAltitude]);
            toggles.Add(UIOption.ToggleRollCtrlLateralAxis, (ToggleOption) options[UIOption.ToggleRollCtrlLateralAxis]);
            toggles.Add(UIOption.ToggleDynamicRoll, (ToggleOption) options[UIOption.ToggleDynamicRoll]);
            toggles.Add(UIOption.ToggleVignette, (ToggleOption) options[UIOption.ToggleVignette]);
            toggles.Add(UIOption.ToggleDoF, (ToggleOption) options[UIOption.ToggleDoF]);
            toggles.Add(UIOption.ToggleFpsLookSmoothing, (ToggleOption) options[UIOption.ToggleFpsLookSmoothing]);
            toggles.Add(UIOption.ToggleTimeScale, (ToggleOption) options[UIOption.ToggleTimeScale]);

            return toggles;
        }
        
        public static Dictionary<UIOption, SliderOption> GetSliders(Dictionary<UIOption, Option> options)
        {
            var sliders = new Dictionary<UIOption, SliderOption>();
            
            sliders.Add(UIOption.MovementSpeedSlider, (SliderOption) options[UIOption.MovementSpeedSlider]);
            sliders.Add(UIOption.MovementSmoothingSlider, (SliderOption) options[UIOption.MovementSmoothingSlider]);
            sliders.Add(UIOption.RotationSpeedSlider, (SliderOption) options[UIOption.RotationSpeedSlider]);
            sliders.Add(UIOption.RotationSmoothingSlider, (SliderOption) options[UIOption.RotationSmoothingSlider]);
            sliders.Add(UIOption.DynamicRollIntensitySlider, (SliderOption) options[UIOption.DynamicRollIntensitySlider]);
            sliders.Add(UIOption.FpsLookSmoothingSlider, (SliderOption) options[UIOption.FpsLookSmoothingSlider]);
            sliders.Add(UIOption.TimeScaleSlider, (SliderOption) options[UIOption.TimeScaleSlider]);
            sliders.Add(UIOption.ZoomSpeedSlider, (SliderOption) options[UIOption.ZoomSpeedSlider]);
            sliders.Add(UIOption.ZoomSmoothingSlider, (SliderOption) options[UIOption.ZoomSmoothingSlider]);
            sliders.Add(UIOption.FocusDistanceSlider, (SliderOption) options[UIOption.FocusDistanceSlider]);
            sliders.Add(UIOption.FocalLenghtSlider, (SliderOption) options[UIOption.FocalLenghtSlider]);
            sliders.Add(UIOption.ApertureSlider, (SliderOption) options[UIOption.ApertureSlider]);

            return sliders;
        }

        private static GameObject GetOptionObj(GameObject cinemaUI, UIOption option)
        {
            // CinemaUI/Canvas/Window/ViewPort
            var windowViewPort = cinemaUI.transform.GetChild(0).GetChild(0).GetChild(0);
            // ViewPort/Body/ViewPort
            var bodyViewPort = windowViewPort.GetChild(1).GetChild(0);
            var gameObject = bodyViewPort.transform.Find(option.ToString()).gameObject;
            return gameObject;
        }
    }
}