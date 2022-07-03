using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public sealed class SliderOption : Option
    {
        internal event Action<float> OnValueChanged;
        private Slider Slider { get; }

        private readonly float _initialValue;

        private readonly TMP_Text _valueText;

        private int _nDisabled;

        public SliderOption(GameObject root, bool startActive, float initialValue, float minValue, float maxValue) : base(root, startActive)
        {
            Slider = root.GetComponentInChildren<Slider>();
            _valueText = root.transform.GetChild(1).GetComponentInChildren<TMP_Text>();
            var resetButton = root.transform.GetChild(3).GetComponentInChildren<Button>();

            Slider.onValueChanged.AddListener((UnityAction<float>) OnSliderChange);
            resetButton.onClick.AddListener((UnityAction) OnReset);

            Slider.maxValue = maxValue;
            Slider.minValue = minValue;
            Slider.Set(initialValue);
            _initialValue = initialValue;

            OnSliderChange(initialValue);
        }

        public void OnSliderChange(float value)
        {
            _valueText.text = value.ToString(Mathf.Abs(value) < 100 ? "0.00" : "0.0");
            OnValueChanged?.Invoke(value);
            Slider.value = value;
        }

        public override void Disable(bool state)
        {
            _nDisabled++;
            OnReset();
            Slider.enabled = false;
            Slider.interactable = false;
        }

        public override void Enable(bool state)
        {
            if (--_nDisabled == 0)
            {
                Slider.enabled = true;
                Slider.interactable = true;
                OnReset();
            }
        }

        public override void OnReset()
        {
            Slider.Set(_initialValue);
            OnSliderChange(_initialValue);
        }

    }
}