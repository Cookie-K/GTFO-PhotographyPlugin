using CinematographyPlugin.Cinematography.Settings;
using CinematographyPlugin.UI.Enums;
using UnityEngine;

namespace CinematographyPlugin.UI
{
    public static class UIFactory
    {
        
        public static Dictionary<UIOption, Option> BuildOptions(GameObject cinemaUI)
        {
            var options = new Dictionary<UIOption, Option>
            {
                { UIOption.ToggleUI, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleUI), true, true) },
                { UIOption.ToggleBio, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleBio), false, false, true) },
                { UIOption.ToggleAspectRatio, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleAspectRatio), false, true) },
                { UIOption.AspectRatioSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.AspectRatioSlider), false, 1.777f, 0, 5) },
                { UIOption.ToggleBody, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleBody), true, true) },
                { UIOption.ToggleFreeCamera, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleFreeCamera), false, true) },
                { UIOption.MovementSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSpeedSlider), false, CinemaCamSettings.MovementSpeedDefault, CinemaCamSettings.MovementSpeedMin, CinemaCamSettings.MovementSpeedMax) },
                { UIOption.MovementSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.MovementSmoothingSlider), false, CinemaCamSettings.MovementSmoothTimeDefault, CinemaCamSettings.MovementSmoothTimeMin, CinemaCamSettings.MovementSmoothTimeMax) },
                { UIOption.RotationSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.RotationSpeedSlider), false, CinemaCamSettings.RotationSpeedDefault, CinemaCamSettings.RotationSpeedMin, CinemaCamSettings.RotationSpeedMax) },
                { UIOption.RotationSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.RotationSmoothingSlider), false, CinemaCamSettings.RotationSmoothTimeDefault, CinemaCamSettings.RotationSmoothTimeMin, CinemaCamSettings.RotationSmoothTimeMax) },
                { UIOption.ZoomSpeedSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.ZoomSpeedSlider), false, CinemaCamSettings.ZoomSpeedDefault, CinemaCamSettings.ZoomSpeedMin, CinemaCamSettings.ZoomSpeedMax) },
                { UIOption.ZoomSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.ZoomSmoothingSlider), false, CinemaCamSettings.ZoomSmoothTimeDefault, CinemaCamSettings.ZoomSmoothTimeMin, CinemaCamSettings.ZoomSmoothTimeMax) },
                { UIOption.ToggleDynamicRoll, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleDynamicRoll), false, false) },
                { UIOption.DynamicRollIntensitySlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.DynamicRollIntensitySlider), false, CinemaCamSettings.DynamicRotationDefault, CinemaCamSettings.DynamicRotationMin, CinemaCamSettings.DynamicRotationMax) },
                { UIOption.ToggleAlignPitchAxisWCam, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleAlignPitchAxisWCam), true, false) },
                { UIOption.ToggleAlignRollAxisWCam, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleAlignRollAxisWCam), false, false) },
                { UIOption.ToggleTargetingVisibility, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleTargetingVisibility), false, false) },
                { UIOption.ToggleFpsLookSmoothing, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleFpsLookSmoothing), false, true) },
                { UIOption.FpsLookSmoothingSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FpsLookSmoothingSlider), false, CinemaCamSettings.LookSmoothDefault, CinemaCamSettings.LookSmoothMin, CinemaCamSettings.LookSmoothMax) },
                { UIOption.ToggleDoF, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleDoF), false, true) },
                { UIOption.FocusDistanceSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FocusDistanceSlider), false, CinemaCamSettings.FocusDistanceDefault, CinemaCamSettings.FocusDistanceMin, CinemaCamSettings.FocusDistanceMax) },
                { UIOption.ApertureSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.ApertureSlider), false, CinemaCamSettings.ApertureDefault, CinemaCamSettings.ApertureMin, CinemaCamSettings.ApertureMax) },
                { UIOption.FocalLenghtSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.FocalLenghtSlider), false, CinemaCamSettings.FocalLenghtDefault, CinemaCamSettings.FocalLenghtMin, CinemaCamSettings.FocalLenghtMax) },
                { UIOption.ToggleVignette, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleVignette), true, true) },
                { UIOption.ToggleAmbientParticles, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleAmbientParticles), true, true) },
                { UIOption.ToggleTimeScale, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleTimeScale), false, true) },
                { UIOption.TimeScaleSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.TimeScaleSlider), false, CinemaCamSettings.TimeScaleDefault, CinemaCamSettings.TimeScaleMin, CinemaCamSettings.TimeScaleMax) },
                
                { UIOption.TogglePointLight, new ToggleOption(GetOptionObj(cinemaUI, UIOption.TogglePointLight), false, true) },
                { UIOption.PointLightIntensitySlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.PointLightIntensitySlider), false, CinemaCamSettings.PointLightIntensityDefault, CinemaCamSettings.PointLightIntensityMin, CinemaCamSettings.PointLightIntensityMax) },
                { UIOption.PointLightRangeSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.PointLightRangeSlider), false, CinemaCamSettings.PointLightRangeDefault, CinemaCamSettings.PointLightRangeMin, CinemaCamSettings.PointLightRangeMax) },

                { UIOption.ToggleSpotLight, new ToggleOption(GetOptionObj(cinemaUI, UIOption.ToggleSpotLight), false, false) },
                { UIOption.SpotLightIntensitySlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.SpotLightIntensitySlider), false, CinemaCamSettings.SpotLightIntensityDefault, CinemaCamSettings.SpotLightIntensityMin, CinemaCamSettings.SpotLightIntensityMax) },
                { UIOption.SpotLightRangeSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.SpotLightRangeSlider), false, CinemaCamSettings.SpotLightRangeDefault, CinemaCamSettings.SpotLightRangeMin, CinemaCamSettings.SpotLightRangeMax) },
                { UIOption.SpotLightAngleSlider, new SliderOption(GetOptionObj(cinemaUI, UIOption.SpotLightAngleSlider), false, CinemaCamSettings.SpotLightAngleDefault, CinemaCamSettings.SpotLightAngleMin, CinemaCamSettings.SpotLightAngleMax) },
            };

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
                options[UIOption.ToggleAlignPitchAxisWCam],
                options[UIOption.ToggleAlignRollAxisWCam],
                options[UIOption.ToggleTargetingVisibility],
                options[UIOption.ToggleSpotLight],
            });
            
            options[UIOption.ToggleDynamicRoll].SubOptions.Add(options[UIOption.DynamicRollIntensitySlider]);
            
            options[UIOption.ToggleAspectRatio].SubOptions.Add(options[UIOption.AspectRatioSlider]);
            
            options[UIOption.ToggleFpsLookSmoothing].SubOptions.Add(options[UIOption.FpsLookSmoothingSlider]);
            
            options[UIOption.ToggleTimeScale].SubOptions.Add(options[UIOption.TimeScaleSlider]);
            
            options[UIOption.ToggleUI].SubOptions.Add(options[UIOption.ToggleBio]);
            
            options[UIOption.TogglePointLight].SubOptions.AddRange(new []
            {
                options[UIOption.PointLightRangeSlider],
                options[UIOption.PointLightIntensitySlider]
            });
            
            options[UIOption.ToggleSpotLight].SubOptions.AddRange(new []
            {
                options[UIOption.SpotLightRangeSlider],
                options[UIOption.SpotLightIntensitySlider],
                options[UIOption.SpotLightAngleSlider]
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
            options[UIOption.ToggleFreeCamera].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleFpsLookSmoothing], false);
            
            options[UIOption.ToggleTimeScale].StateByDisableOnSelectOptions.Add(options[UIOption.ToggleFpsLookSmoothing], false);
            
            return options;
        }

        public static Dictionary<UIOption, ToggleOption> GetToggles(Dictionary<UIOption, Option> options)
        {
            return options.Where(o => o.Value is ToggleOption).ToDictionary(o => o.Key, o => o.Value as ToggleOption);
        }
        
        public static Dictionary<UIOption, SliderOption> GetSliders(Dictionary<UIOption, Option> options)
        {
            return options.Where(o => o.Value is SliderOption).ToDictionary(o => o.Key, o => o.Value as SliderOption);
        }

        private static GameObject GetOptionObj(GameObject cinemaUI, UIOption option)
        {
            // CinemaUI/Canvas/Window/ViewPort
            var windowViewPort = cinemaUI.transform.GetChild(0).GetChild(1).GetChild(0);
            // ViewPort/Body/ViewPort
            var bodyViewPort = windowViewPort.GetChild(1).GetChild(0);
            var gameObject = bodyViewPort.transform.Find(option.ToString()).gameObject;
            return gameObject;
        }
    }
}