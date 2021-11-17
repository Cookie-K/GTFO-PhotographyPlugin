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
            options.Add(UIOption.MovementSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSpeedSlider), false, FreeCamController.MovementSpeedDefault, FreeCamController.MovementSpeedMin, FreeCamController.MovementSpeedMax));
            options.Add(UIOption.MovementSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSmoothingSlider), false, FreeCamController.MovementSmoothTimeDefault, FreeCamController.MovementSmoothTimeMin, FreeCamController.MovementSmoothTimeMax));
            options.Add(UIOption.RotationSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.RotationSpeedSlider), false, FreeCamController.RotationSpeedDefault, FreeCamController.RotationSpeedMin, FreeCamController.RotationSpeedMax));
            options.Add(UIOption.RotationSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.RotationSmoothingSlider), false, FreeCamController.RotationSmoothTimeDefault, FreeCamController.RotationSmoothTimeMin, FreeCamController.RotationSmoothTimeMax));
            
            options.Add(UIOption.ToggleDynamicRoll, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleDynamicRoll), false, false));
            options.Add(UIOption.DynamicRollIntensitySlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.DynamicRollIntensitySlider), false, FreeCameraController.DynamicRollIntensityDefault, FreeCameraController.DynamicRollIntensityMin, FreeCameraController.DynamicRollIntensityMax));
            options.Add(UIOption.ToggleMouseCtrlAltitude, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleMouseCtrlAltitude), true, false));

            options.Add(UIOption.ToggleLookSmoothing, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleLookSmoothing), false, true));
            options.Add(UIOption.LookSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.LookSmoothingSlider), false, LookSmoothingController.SmoothDefault, LookSmoothingController.SmoothMin, LookSmoothingController.SmoothMax));
            
            options.Add(UIOption.ToggleCameraRoll, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleCameraRoll), false, false));
            options.Add(UIOption.CameraRollSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.CameraRollSlider), false, CameraRollController.RollDefault, CameraRollController.RollMin, CameraRollController.RollMax));
            options.Add(UIOption.CameraRollSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.CameraRollSpeedSlider), false, CameraRollController.RollSpeedDefault, CameraRollController.RollSpeedMin, CameraRollController.RollSpeedMax));
            options.Add(UIOption.CameraRollSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.CameraRollSmoothingSlider), false, CameraRollController.RollTimeDefault, CameraRollController.RollTimeMin, CameraRollController.RollTimeMax));
            
            options.Add(UIOption.ToggleTimeScale, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleTimeScale), false, true));
            options.Add(UIOption.TimeScaleSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.TimeScaleSlider), false, TimeScaleController.TimeScaleDefault, TimeScaleController.TimeScaleMin, TimeScaleController.TimeScaleMax));
            
            options.Add(UIOption.ToggleFoV, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleFoV), false, true));
            options.Add(UIOption.FoVSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FoVSlider), false, FoVController.GetDefaultFoV(), FoVController.FovMin, FoVController.FovMax));
            options.Add(UIOption.FoVSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FoVSpeedSlider), false, FoVController.FovSpeedDefault, FoVController.FovSpeedMin, FoVController.FovSpeedMax));
            options.Add(UIOption.FoVSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FoVSmoothingSlider), false, FoVController.FoVTimeDefault, FoVController.FoVTimeMin, FoVController.FoVTimeMax));
            
            options.Add(UIOption.ToggleVignette, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleVignette), true, true));
            
            options.Add(UIOption.ToggleDoF, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleDoF), false, true));
            options.Add(UIOption.FocusDistanceSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FocusDistanceSlider), false, PostProcessingController.GetDefaultFocusDistance(), PostProcessingController.FocusDistanceMin, PostProcessingController.FocusDistanceMax));
            options.Add(UIOption.ApertureSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.ApertureSlider), false, PostProcessingController.GetDefaultAperture(), PostProcessingController.ApertureMin, PostProcessingController.ApertureMax));
            options.Add(UIOption.FocalLenghtSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FocalLenghtSlider), false, PostProcessingController.GetDefaultFocalLenght(), PostProcessingController.FocalLenghtMin, PostProcessingController.FocalLenghtMax));


            // Add sub options
            options[UIOption.ToggleFreeCamera].SubOptions.AddRange(new []
            {
                options[UIOption.MovementSpeedSlider],
                options[UIOption.MovementSmoothingSlider],
                options[UIOption.RotationSpeedSlider],
                options[UIOption.RotationSmoothingSlider],
                options[UIOption.ToggleDynamicRoll],
                options[UIOption.ToggleCameraRoll],
                options[UIOption.ToggleMouseCtrlAltitude],
            });
            
            options[UIOption.ToggleDynamicRoll].SubOptions.Add(options[UIOption.DynamicRollIntensitySlider]);
            
            options[UIOption.ToggleLookSmoothing].SubOptions.Add(options[UIOption.LookSmoothingSlider]);
            
            options[UIOption.ToggleCameraRoll].SubOptions.AddRange(new []
            {
                options[UIOption.CameraRollSlider],   
                options[UIOption.CameraRollSpeedSlider],
                options[UIOption.CameraRollSmoothingSlider],
            });
            
            options[UIOption.ToggleTimeScale].SubOptions.Add(options[UIOption.TimeScaleSlider]);
            
            options[UIOption.ToggleFoV].SubOptions.AddRange(new []
            {
                options[UIOption.FoVSlider],  
                options[UIOption.FoVSpeedSlider],  
                options[UIOption.FoVSmoothingSlider],  
            });
            
            options[UIOption.ToggleDoF].SubOptions.AddRange(new []
            {
                options[UIOption.FocusDistanceSlider],
                options[UIOption.ApertureSlider],
                options[UIOption.FocalLenghtSlider]
            });
            
            // Add options to disable on select
            options[UIOption.ToggleFreeCamera].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleUI], false);
            options[UIOption.ToggleFreeCamera].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleBody], false);
            options[UIOption.ToggleCameraRoll].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleBody], false);
            options[UIOption.ToggleDynamicRoll].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleCameraRoll], false);
            options[UIOption.ToggleFoV].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleBody], false);
            
            return options;
        }

        public static Dictionary<UIOption, ToggleOption> GetToggles(Dictionary<UIOption, Option> options)
        {
            var toggles = new Dictionary<UIOption, ToggleOption>();
            
            toggles.Add(UIOption.ToggleUI, (ToggleOption) options[UIOption.ToggleUI]);
            toggles.Add(UIOption.ToggleBody, (ToggleOption) options[UIOption.ToggleBody]);
            toggles.Add(UIOption.ToggleFreeCamera, (ToggleOption) options[UIOption.ToggleFreeCamera]);
            toggles.Add(UIOption.ToggleMouseCtrlAltitude, (ToggleOption) options[UIOption.ToggleMouseCtrlAltitude]);
            toggles.Add(UIOption.ToggleDynamicRoll, (ToggleOption) options[UIOption.ToggleDynamicRoll]);
            toggles.Add(UIOption.ToggleCameraRoll, (ToggleOption) options[UIOption.ToggleCameraRoll]);
            toggles.Add(UIOption.ToggleVignette, (ToggleOption) options[UIOption.ToggleVignette]);
            toggles.Add(UIOption.ToggleFoV, (ToggleOption) options[UIOption.ToggleFoV]);
            toggles.Add(UIOption.ToggleDoF, (ToggleOption) options[UIOption.ToggleDoF]);
            toggles.Add(UIOption.ToggleLookSmoothing, (ToggleOption) options[UIOption.ToggleLookSmoothing]);
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
            sliders.Add(UIOption.CameraRollSlider, (SliderOption) options[UIOption.CameraRollSlider]);
            sliders.Add(UIOption.CameraRollSpeedSlider, (SliderOption) options[UIOption.CameraRollSpeedSlider]);
            sliders.Add(UIOption.CameraRollSmoothingSlider, (SliderOption) options[UIOption.CameraRollSmoothingSlider]);
            sliders.Add(UIOption.LookSmoothingSlider, (SliderOption) options[UIOption.LookSmoothingSlider]);
            sliders.Add(UIOption.TimeScaleSlider, (SliderOption) options[UIOption.TimeScaleSlider]);
            sliders.Add(UIOption.FoVSpeedSlider, (SliderOption) options[UIOption.FoVSlider]);
            sliders.Add(UIOption.FoVSmoothingSlider, (SliderOption) options[UIOption.FoVSmoothingSlider]);
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