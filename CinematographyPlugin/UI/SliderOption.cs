using CinematographyPlugin.UI.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public class SliderOption : Option
    {
        private Slider _slider;

        public SliderOption(GameObject root, bool startActive, float initialValue, float maxValue, float minValue) : base(root, OptionType.Slider, startActive)
        {
            _slider = root.GetComponentInChildren<Slider>();
            _slider.value = initialValue;
            _slider.maxValue = maxValue;
            _slider.minValue = minValue;
        }

        public float GetValue()
        {
            return _slider.value;
        }
    }
}