using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public sealed class ToggleOption : Option
    {
        internal event Action<bool> OnValueChanged;
        
        internal Toggle Toggle { get; }

        private readonly bool _initialVal;

        private readonly TMP_Text _tmp;
        
        private int _nDisabled;

        public ToggleOption(GameObject root, bool initialValue, bool startActive) : base(root, startActive)
        {
            Toggle = root.GetComponentInChildren<Toggle>();
            Toggle.onValueChanged.AddListener((UnityAction<bool>) OnToggleChange);
            _tmp = Toggle.transform.GetComponentInChildren<TMP_Text>();
            Toggle.Set(initialValue);
            OnToggleChange(initialValue);
            _initialVal = initialValue;
        }
        
        private void OnToggleChange(bool value)
        {
            _tmp.text = value ? "<color=\"orange\">ON</color>" : "OFF";

            foreach (var (option, stateOnDisable) in StateByDisableOnSelectOptions)
            {
                if (value)
                {
                    option.Disable(stateOnDisable);
                }
                else
                {
                    option.Enable(stateOnDisable);
                }
            }
            
            foreach (var option in SubOptions)
            {
                option.OnReset();
                option.SetActive(value);
            }
            
            OnValueChanged?.Invoke(value);
        }

        public override void Disable(bool state)
        {
            _nDisabled++;
            Toggle.Set(state);
            Toggle.enabled = false;
            _tmp.text = $"<s>{_tmp.text}</s>";
            foreach (var option in SubOptions)
            {
                option.OnReset();
                option.SetActive(false);
            }
        }

        public override void Enable(bool state)
        {
            if (_nDisabled != 0 && --_nDisabled == 0)
            {
                Toggle.enabled = true;
                OnReset();
                foreach (var option in SubOptions)
                {
                    option.OnReset();
                }
            }
        }

        public override void OnReset()
        {
            Toggle.Set(_initialVal);
            OnToggleChange(_initialVal);
        }

    }
}