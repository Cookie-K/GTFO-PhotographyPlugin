using UnityEngine;

namespace CinematographyPlugin.UI
{
    public abstract class Option
    {
        public string Name { get; }
        protected GameObject Root { get; }
        private readonly bool _activeWhenParentOff;

        internal List<Option> SubOptions { get; } = new ();

        internal Dictionary<Option, bool> StateByDisableOnSelectOptions { get; } = new ();

        protected Option(GameObject root, bool startActive, bool activeWhenParentOff = false)
        {
            Root = root;
            Name = root.name;
            _activeWhenParentOff = activeWhenParentOff;

            root.active = startActive;
        }

        public void SetActive(bool state)
        {
            OnSetActive(state != _activeWhenParentOff);
            Root.active = state != _activeWhenParentOff;
        }

        public abstract void Disable(bool state);
        public abstract void Enable(bool state);
        public abstract void OnReset();
        public abstract void SetPreviousValue();
        public abstract void OnSetActive(bool state);
    }
}