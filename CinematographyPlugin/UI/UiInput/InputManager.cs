using CinematographyPlugin.Util;
using UnityEngine;

namespace CinematographyPlugin.UI.UiInput
{
    public static class InputManager
    {
        private const float TimeCheckIntervalStart = 0.1f;
        private const float TimeScaleSmallDelta = 0.01f;

        private static readonly KeyCode PosX = KeyCode.D;
        private static readonly KeyCode NegX = KeyCode.A;
        private static readonly KeyCode PosY = ConfigManager.UpKey;
        private static readonly KeyCode NegY = ConfigManager.DownKey;
        private static readonly KeyCode PosZ = KeyCode.W;
        private static readonly KeyCode NegZ = KeyCode.S;
        private static readonly KeyCode PosS = ConfigManager.SpeedUpKey;
        private static readonly KeyCode NegS = ConfigManager.SlowDownKey;

        private static readonly KeyCode TimeInc = ConfigManager.TimeIncKey;
        private static readonly KeyCode TimeDec = ConfigManager.TimeDecKey;
        private static readonly KeyCode TimePausePlay = ConfigManager.TimePausePlayKey;

        private static float _lastTimeHeld;
        private static float _timeCheckInterval = TimeCheckIntervalStart;

        public static float GetAxis(AxisName axis)
        {
            switch (axis)
            {
                case AxisName.PosX:
                case AxisName.PosY:
                case AxisName.PosZ:
                case AxisName.Speed:
                    return GetAxisKeyInput(axis);
                case AxisName.RotX:
                case AxisName.RotY:
                case AxisName.RotZ:
                case AxisName.Zoom:
                    return GetMouseInput(axis);
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Invalid axis sent to GetAxis: {axis}");
            }
        }

        public static bool GetMiddleMouse()
        {
            return Input.GetMouseButtonDown(2);
        }
        
        public static float GetTimeScaleInput()
        {
            var newTimeDelta = 0f;

            // Check for time change input at a frame independent interval 
            if (Time.realtimeSinceStartup - _lastTimeHeld < _timeCheckInterval)
            {
                _timeCheckInterval = Mathf.MoveTowards(_timeCheckInterval, 0, IndependentDeltaTimeManager.GetDeltaTime());
                return newTimeDelta;
            }
            
            if (Input.GetKey(TimeInc))
            {
                newTimeDelta = TimeScaleSmallDelta;
            }

            if (Input.GetKey(TimeDec))
            {
                newTimeDelta = -TimeScaleSmallDelta;
            }

            if (newTimeDelta != 0f) 
            {
                _lastTimeHeld = Time.realtimeSinceStartup;
            }
            else
            {
                _timeCheckInterval = TimeCheckIntervalStart;
            }
            
            return newTimeDelta;
        }
        
        public static bool GetTimeScalePausePlay()
        {
            return Input.GetKeyDown(TimePausePlay);
        }

        private static float GetMouseInput(AxisName axis)
        {
            switch (axis)
            {
                case AxisName.RotX:
                    // Invert Y for +ve up, -ve down
                    return -Input.GetAxis("MouseY");
                case AxisName.RotY:
                    return Input.GetAxis("MouseX");
                case AxisName.RotZ:
                    return GetMouseButtonAxisFloat();
                case AxisName.Zoom:
                    // Invert for +ve zoom in, -ve zoom out
                    return -Input.mouseScrollDelta.y;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Invalid axis sent to GetMouseAxis: {axis}");
            }
        }

        private static float GetMouseButtonAxisFloat()
        {
            return Input.GetMouseButton(0) ? 1 : Input.GetMouseButton(1) ? -1 : 0;
        }
        
        private static float GetAxisKeyInput(AxisName axis)
        {
            switch (axis)
            {
                case AxisName.PosX:
                    return Input.GetKey(PosX) ? 1 : Input.GetKey(NegX) ? -1 : 0;
                case AxisName.PosY:
                    return Input.GetKey(PosY) ? 1 : Input.GetKey(NegY) ? -1 : 0;
                case AxisName.PosZ:
                    return Input.GetKey(PosZ) ? 1 : Input.GetKey(NegZ) ? -1 : 0;
                case AxisName.Speed:
                    return Input.GetKey(PosS) ? 1 : Input.GetKey(NegS) ? -1 : 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Invalid axis sent to GetKeyAxis: {axis}");
            }
        }

    }
}