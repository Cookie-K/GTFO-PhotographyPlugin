using CinematographyPlugin.Cinematography.Settings;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using UnityEngine;

namespace CinematographyPlugin.Cinematography;

public class LightManager : MonoBehaviour
{
   
    private bool _freeCamOn;
    private float _currSpotLightRange;
    private float _currSpotLightIntensity;
    private float _currSpotLightAngle;
    
    private FPSCamera _fpsCamera;
    private EffectLight _pointLight;
    private Light _spotLight;
    private CL_SpotLight _clSpotLight;

    private void Awake()
    {
        CinemaUIManager.Current.Sliders[UIOption.PointLightRangeSlider].OnValueChanged += SetPointLightRange;
        CinemaUIManager.Current.Sliders[UIOption.PointLightIntensitySlider].OnValueChanged += SetPointLightIntensity;
        
        CinemaUIManager.Current.Toggles[UIOption.ToggleFreeCamera].OnValueChanged += OnFreeCamToggle;
        CinemaUIManager.Current.Toggles[UIOption.ToggleSpotLight].OnValueChanged += SetSpotLightOffset;
        CinemaUIManager.Current.Sliders[UIOption.SpotLightRangeSlider].OnValueChanged += SetSpotLightRange;
        CinemaUIManager.Current.Sliders[UIOption.SpotLightIntensitySlider].OnValueChanged += SetSpotLightIntensity;
        CinemaUIManager.Current.Sliders[UIOption.SpotLightAngleSlider].OnValueChanged += SetSpotLightAngle;
        
        _fpsCamera = FindObjectOfType<FPSCamera>();
        var camTransform = _fpsCamera.gameObject.transform;
        
        _pointLight = camTransform.FindChild("Ambient Light").GetComponent<EffectLight>();
        _spotLight = camTransform.FindChild("Flashlight").GetComponent<Light>();
        _clSpotLight = camTransform.FindChild("Flashlight").GetComponent<CL_SpotLight>();
    }

    private void Update()
    {
        if (_freeCamOn)
        {
            UpdateLightOnOffSettings(_clSpotLight.m_isOn);
        }
    }
    
    private void OnFreeCamToggle(bool value)
    {
        _freeCamOn = !_freeCamOn;
    }    
    
    private void UpdateLightOnOffSettings(bool value)
    {
        if (value)
        {
            // Light seems to get reset on every toggle so it must be reset
            _spotLight.range = _currSpotLightRange;
            _spotLight.intensity = _currSpotLightIntensity;
            _spotLight.spotAngle = _currSpotLightAngle;
            _spotLight.color = Color.white;
        }
    }    

    private void SetSpotLightOffset(bool value)
    {
        _fpsCamera.FPSFlashlightAdjustment = value ? 0f : CinemaCamSettings.FlashLightOffsetDefault;
    }
    
    private void SetPointLightRange(float value)
    {
        _pointLight.Range = value;
    }
    
    private void SetPointLightIntensity(float value)
    {
        _pointLight.Intensity = value;
    }
    
    private void SetSpotLightRange(float value)
    {
        _currSpotLightRange = value;
        _spotLight.range = value;
    }
    
    private void SetSpotLightIntensity(float value)
    {
        _currSpotLightIntensity = value;
        _spotLight.intensity = value;
    }
    
    private void SetSpotLightAngle(float value)
    {
        _currSpotLightAngle = value;
        _spotLight.spotAngle = value;
    }

    private void OnDestroy()
    {
        CinemaUIManager.Current.Sliders[UIOption.PointLightRangeSlider].OnValueChanged -= SetPointLightRange;
        CinemaUIManager.Current.Sliders[UIOption.PointLightIntensitySlider].OnValueChanged -= SetPointLightIntensity;
        
        CinemaUIManager.Current.Toggles[UIOption.ToggleFreeCamera].OnValueChanged -= SetSpotLightOffset;
        CinemaUIManager.Current.Sliders[UIOption.SpotLightRangeSlider].OnValueChanged -= SetSpotLightRange;
        CinemaUIManager.Current.Sliders[UIOption.SpotLightIntensitySlider].OnValueChanged -= SetSpotLightIntensity;
        CinemaUIManager.Current.Sliders[UIOption.SpotLightAngleSlider].OnValueChanged -= SetSpotLightAngle;
    }
}