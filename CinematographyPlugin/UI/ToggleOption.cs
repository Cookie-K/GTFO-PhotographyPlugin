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

        internal readonly TMP_Text TMP;

        public ToggleOption(GameObject root, bool initialValue, bool startActive) : base(root, OptionType.Toggle, startActive)
        {
            Toggle = root.GetComponentInChildren<Toggle>();
            Toggle.onValueChanged.AddListener((UnityAction<bool>) ChangeText);
            TMP = Toggle.transform.GetComponentInChildren<TMP_Text>();
            Toggle.isOn = initialValue;
        }
        
        public void Disable(bool state)
        {
            Toggle.enabled = !state;
            if (state)
            {
                TMP.text = $"<s>{TMP.text}</s>";
            }
            else
            {
                ChangeText(Toggle.isOn);
            }
        }

        private void ChangeText(bool state)
        {
            TMP.text = state ? "ON" : "OFF";
        }

    }
}