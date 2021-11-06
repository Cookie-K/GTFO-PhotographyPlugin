using CinematographyPlugin.UI.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public class ToggleOption : Option
    {
        internal Toggle Toggle { get; }

        public ToggleOption(GameObject root, bool initialValue, bool startActive) : base(root, OptionType.Toggle, startActive)
        {
            Toggle = root.GetComponentInChildren<Toggle>();
            Toggle.onValueChanged.AddListener((UnityAction<bool>) ChangeText);
            Toggle.isOn = initialValue;
        }

        public bool GetValue()
        {
            return Toggle.isOn;
        }

        private void ChangeText(bool state)
        {
            var tmp = Toggle.transform.GetComponentInChildren<TMP_Text>();
            tmp.text = state ? "ON" : "OFF";
        }
        
    }
}