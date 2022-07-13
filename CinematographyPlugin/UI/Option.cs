using UnityEngine;

namespace CinematographyPlugin.UI
{
    public abstract class Option
    {
        protected GameObject Go { get; }
        protected readonly bool ActiveWhenParentOff;

        internal List<Option> SubOptions { get; } = new ();

        internal Dictionary<Option, bool> StateByDisableOnSelectOptions { get; } = new ();

        protected Option(GameObject go, bool startActive, bool activeWhenParentOff = false)
        {
            Go = go;
            ActiveWhenParentOff = activeWhenParentOff;

            go.active = startActive;
        }

        public void SetActive(bool state)
        {
            OnSetActive(state != ActiveWhenParentOff);
            Go.active = state != ActiveWhenParentOff;
        }

        public abstract void Disable(bool state);
        public abstract void Enable(bool state);
        public abstract void OnReset();
        public abstract void SetPreviousValue();
        public abstract void OnSetActive(bool state);
    }
}