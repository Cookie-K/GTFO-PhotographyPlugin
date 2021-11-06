using CinematographyPlugin.UI.Enums;
using UnityEngine;

namespace CinematographyPlugin.UI
{
    public abstract class Option
    {
        private GameObject Root { get; }
        private OptionType OptionType { get; }
        private bool StartActive { get; }
        private string Name { get; }

        protected Option(GameObject root, OptionType optionType, bool startActive)
        {
            Root = root;
            OptionType = optionType;
            StartActive = startActive;
            Name = root.name;

            root.active = startActive;
        }

        public void SetActive(bool state)
        {
            Root.active = state;
        }
    }
}