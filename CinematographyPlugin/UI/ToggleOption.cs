using System;
using CinematographyPlugin.UI.Enums;
using LibCpp2IL;
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

        private bool InitalVal;

        private readonly TMP_Text TMP;
        
        private int _nDisabled;

        public ToggleOption(GameObject root, bool initialValue, bool startActive) : base(root, OptionType.Toggle, startActive)
        {
            Toggle = root.GetComponentInChildren<Toggle>();
            Toggle.onValueChanged.AddListener((UnityAction<bool>) OnToggleChange);
            TMP = Toggle.transform.GetComponentInChildren<TMP_Text>();
            Toggle.Set(initialValue);
            OnToggleChange(initialValue);
            InitalVal = initialValue;
        }
        
        private void OnToggleChange(bool value)
        {
            TMP.text = value ? "<color=\"orange\">ON</color>" : "OFF";

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
            OnReset();
            Toggle.Set(state);
            Toggle.enabled = false;
            TMP.text = $"<s>{TMP.text}</s>";
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
            Toggle.Set(InitalVal);
            OnToggleChange(InitalVal);
        }

    }
}