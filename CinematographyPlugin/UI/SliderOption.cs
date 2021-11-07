using CinematographyPlugin.UI.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public class SliderOption : Option
    {
        internal Slider Slider { get; }

        private readonly float _initialValue;

        private readonly TMP_Text _valueText;

        public SliderOption(GameObject root, bool startActive, float initialValue, float minValue, float maxValue) : base(root, OptionType.Slider, startActive)
        {
            Slider = root.GetComponentInChildren<Slider>();
            _valueText = root.transform.GetChild(1).GetComponentInChildren<TMP_Text>();
            var resetButton = root.transform.GetChild(3).GetComponentInChildren<Button>();

            Slider.onValueChanged.AddListener((UnityAction<float>) UpdateSliderVal);
            resetButton.onClick.AddListener((UnityAction) OnReset);

            _initialValue = initialValue;
            Slider.value = initialValue;
            Slider.maxValue = maxValue;
            Slider.minValue = minValue;

            UpdateSliderVal(initialValue);
        }

        private void UpdateSliderVal(float value)
        {
            _valueText.text = value.ToString("0.00");
        }

        private void OnReset()
        {
            Slider.value = _initialValue;
        }

    }
}