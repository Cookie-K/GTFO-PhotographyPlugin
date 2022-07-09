using UnityEngine;

namespace CinematographyPlugin.Cinematography.Settings;

public class CinCamSettings : ScriptableObject
{
    /// Camera controller settings
    public const float FastSpeedScale = 2;
    public const float SlowSpeedScale = 1f/2f;
        
    public const float MovementSpeedDefault = 0.5f;
    public const float MovementSpeedMin = 0f;
    public const float MovementSpeedMax = 2f;
    public const float MovementSpeedScale = 10;

    public const float MovementSmoothTimeDefault = 0.2f;
    public const float MovementSmoothTimeMin = 0f;
    public const float MovementSmoothTimeMax = 1f;

    public const float SensitivityScaling = 100f;
    public const float RotationSpeedDefault = 0.9f;
    public const float RotationSpeedMin = 0f;
    public const float RotationSpeedMax = 2f;
    public const float RotationDiffMax = 90f;

    public const float RotationSmoothTimeDefault = 0.1f;
    public const float RotationSmoothTimeMin = 0f;
    public const float RotationSmoothTimeMax = 1f;

    public static readonly float ZoomDefault = CellSettingsManager.GetIntValue(eCellSettingID.Video_WorldFOV);
    public const float ZoomMin = 1f;
    public const float ZoomMax = 160f;

    public const float ZoomScaling = 200f;
    public const float ZoomSpeedDefault = 0.9f;
    public const float ZoomSpeedMin = 0f;
    public const float ZoomSpeedMax = 1f;
        
    public const float OrbitSmoothingFactor = 0f;
    public const float OrbitDistanceDefault = 2f;
    public const float OrbitDistanceMoveSpeedDefault = 0.2f;

    public const float ZoomSmoothTimeDefault = 0.1f;
    public const float ZoomSmoothTimeMin = 0f;
    public const float ZoomSmoothTimeMax = 1f;

    public const float DynamicRotationDefault = 1f;
    public const float DynamicRotationMin = 0f;
    public const float DynamicRotationMax = 2f;
    
    public const float DynamicRotationSpeedScale = 10f;
    public const float DynamicRotationSmoothFactor = 0.4f;
    public const float DynamicRotationRollMax = 180f;
    
    /// FPS look smoothing settings
    public const float LookSmoothDefault = 0.2f;
    public const float LookSmoothMax = 5f;
    public const float LookSmoothMin = 0f;
    public const float LookSmoothingScale = 2f;
    
    /// Post processing settings
    public const float FocusDistanceMin = 0f;
    public const float FocusDistanceMax = 100f;
    public const float FocusDistanceDefault = 90;
        
    public const float ApertureMin = 0f;
    public const float ApertureMax = 10f;
    public const float ApertureDefault = 0.15f;
        
    public const float FocalLenghtMin = 0f;
    public const float FocalLenghtMax = 50f;
    public const float FocalLenghtDefault = 0.98f;
    
    /// Time scale
    public const float TimeScaleDefault = 1;
    public const float TimeScaleMin = 0.01f;
    public const float TimeScaleMax = 1;
    
    /// Light Settings
    public const float FlashLightOffsetDefault = 0.7f;
    
    public const float PointLightRangeDefault = 5f;
    public const float PointLightRangeMin = 0f;
    public const float PointLightRangeMax = 100f;
    
    public const float PointLightIntensityDefault = 0.005f;
    public const float PointLightIntensityMin = 0.005f;
    public const float PointLightIntensityMax = 1f;

    public const float SpotLightRangeDefault = 11f;
    public const float SpotLightRangeMin = 0f;
    public const float SpotLightRangeMax = 100f;
    
    public const float SpotLightIntensityDefault = 0.35f;
    public const float SpotLightIntensityMin = 0f;
    public const float SpotLightIntensityMax = 1f;
    
    public const float SpotLightAngleDefault = 40f;
    public const float SpotLightAngleMin = 0f;
    public const float SpotLightAngleMax = 179f;
}