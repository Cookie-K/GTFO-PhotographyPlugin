using UnityEngine;

namespace CinematographyPlugin.UI.UiInput
{
    public static class InputManager
    {
        private static readonly KeyCode PosX = KeyCode.D;
        private static readonly KeyCode NegX = KeyCode.A;
        private static readonly KeyCode PosY = ConfigManager.UpKey;
        private static readonly KeyCode NegY = ConfigManager.DownKey;
        private static readonly KeyCode PosZ = KeyCode.W;
        private static readonly KeyCode NegZ = KeyCode.S;
        private static readonly KeyCode PosS = ConfigManager.SpeedUpKey;
        private static readonly KeyCode NegS = ConfigManager.SlowDownKey;

        private static readonly KeyCode TimeInc01P = KeyCode.UpArrow;
        private static readonly KeyCode TimeDec01P = KeyCode.DownArrow;
        private static readonly KeyCode TimeInc10P = KeyCode.RightArrow;
        private static readonly KeyCode TimeDec10P = KeyCode.LeftArrow;

        private static float _lastTimeHeld;
        private static float _timeCheckInterval = 0.1f;
        
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

        public static bool GetReset()
        {
            return Input.GetMouseButtonDown(2);
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

        public static float GetTimeScaleInput()
        {
            var newTimeDelta = 0f;

            // Check for time change input at a frame independent interval 
            if (Time.realtimeSinceStartup - _lastTimeHeld < _timeCheckInterval)
            {
                return newTimeDelta;
            }
           
            if (Input.GetKey(TimeInc01P))
            {
                newTimeDelta = 0.01f;
            }

            if (Input.GetKey(TimeDec01P))
            {
                newTimeDelta = -0.01f;
            }
            
            if (Input.GetKey(TimeInc10P))
            {
                newTimeDelta = 0.10f;
            }

            if (Input.GetKey(TimeDec10P))
            {
                newTimeDelta = -0.10f;
            }

            if (newTimeDelta != 0f) 
            {
                _lastTimeHeld = Time.realtimeSinceStartup;
            }
            
            return newTimeDelta;
        }
    }
}