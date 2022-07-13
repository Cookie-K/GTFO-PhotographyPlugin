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
        
        private bool _prevValueSet;
        
        private float _prevValue;

        private int _nDisabled;

        public SliderOption(GameObject go, bool startActive, float initialValue, float minValue, float maxValue) : base(go, startActive)
        {
            Slider = go.GetComponentInChildren<Slider>();
            _valueText = go.transform.GetChild(1).GetComponentInChildren<TMP_Text>();
            var resetButton = go.transform.GetChild(3).GetComponentInChildren<Button>();

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
                if (_prevValueSet)
                {
                    SetPreviousValue();
                }
                else
                {
                    OnReset();
                }
            }
        }

        public override void OnReset()
        {
            SetSliderValue(_initialValue);
        }
        
        public override void SetPreviousValue()
        {
            SetSliderValue(_prevValueSet ? _prevValue : _initialValue);
        }

        public override void OnSetActive(bool state)
        {
            if (state || !Go.active) return;
            
            _prevValue = Slider.value;
            _prevValueSet = true;
        }
        
        private void SetSliderValue(float value)
        {
            Slider.Set(value);
            OnSliderChange(value);
        }

    }
}