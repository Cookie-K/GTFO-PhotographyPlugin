using MS.Internal.Xml.XPath;
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

        private readonly bool _initialValue;

        private readonly TMP_Text _tmp;
        
        private bool _prevValueSet;
        
        private bool _prevValue;
        
        private int _nDisabled;

        public ToggleOption(GameObject go, bool initialValue, bool startActive, bool activeWhenParentOff = false) : base(go, startActive, activeWhenParentOff)
        {
            Toggle = go.GetComponentInChildren<Toggle>();
            Toggle.onValueChanged.AddListener((UnityAction<bool>) OnToggleChange);
            _tmp = Toggle.transform.GetComponentInChildren<TMP_Text>();
            Toggle.Set(initialValue);
            OnToggleChange(initialValue);
            _initialValue = initialValue;
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
                option.SetActive(value);

                if (value)
                {
                    option.SetPreviousValue();   
                }
                else
                {
                    option.OnReset();
                }
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
                
                if (_prevValueSet)
                {
                    SetPreviousValue();
                    foreach (var option in SubOptions)
                    {
                        option.SetPreviousValue();
                    }
                }
                else
                {
                    OnReset();
                    foreach (var option in SubOptions)
                    {
                        option.OnReset();
                    }
                }
            }
        }

        public override void OnReset()
        {
            SetToggleValue(_initialValue);
        }
        
        public override void SetPreviousValue()
        {
            SetToggleValue(_prevValueSet ? _prevValue : _initialValue);
        }

        public override void OnSetActive(bool state)
        {
            if (state || !Go.active) return;

            _prevValue = Toggle.isOn;
            _prevValueSet = true;
            
            foreach (var option in SubOptions)
            {
                option.OnSetActive(false);
            }
        }

        private void SetToggleValue(bool value)
        {
            Toggle.Set(value);
            OnToggleChange(value);
        }

    }
}