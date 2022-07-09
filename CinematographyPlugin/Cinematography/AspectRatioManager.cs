using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace CinematographyPlugin.Cinematography;

public class AspectRatioManager : MonoBehaviour
{
    
    private GameObject _cinematicBars;
    private RectTransform _topBar;
    private RectTransform _bottomBar;

    private void Awake()
    {
        CinemaUIManager.Current.Toggles[UIOption.ToggleAspectRatio].OnValueChanged += OnAspectRatioToggle;
        CinemaUIManager.Current.Sliders[UIOption.AspectRatioSlider].OnValueChanged += OnAspectRatioChange;
    }

    private void Start()
    {
        _cinematicBars = CinemaUIManager.Current.GetCinematicBars();
        _topBar = _cinematicBars.transform.GetChild(0).GetComponent<RectTransform>();
        _bottomBar = _cinematicBars.transform.GetChild(1).GetComponent<RectTransform>();;
    }

    private void OnAspectRatioToggle(bool value)
    {
        _cinematicBars.active = value;
    }

    private void OnAspectRatioChange(float value)
    {
        var height = Screen.width / value;
        var barHeight = (Screen.height - height) / 2;
        
        _topBar.sizeDelta = new Vector2(_topBar.sizeDelta.x, barHeight);
        _bottomBar.sizeDelta = new Vector2(_bottomBar.sizeDelta.x, barHeight);
    }

    private void OnDestroy()
    {
        CinemaUIManager.Current.Toggles[UIOption.ToggleAspectRatio].OnValueChanged -= OnAspectRatioToggle;
        CinemaUIManager.Current.Sliders[UIOption.AspectRatioSlider].OnValueChanged -= OnAspectRatioChange;
    }
}