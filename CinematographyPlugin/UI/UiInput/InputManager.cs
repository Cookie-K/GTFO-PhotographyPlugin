using UnityEngine;

namespace CinematographyPlugin.UI.UiInput
{
    public static class InputManager
    {
        private static KeyCode PosX = KeyCode.D;
        private static KeyCode NegX = KeyCode.A;
        private static KeyCode PosY = ConfigManager.UpKey;
        private static KeyCode NegY = ConfigManager.DownKey;
        private static KeyCode PosZ = KeyCode.W;
        private static KeyCode NegZ = KeyCode.S;
        private static KeyCode PosS = ConfigManager.SpeedUpKey;
        private static KeyCode NegS = ConfigManager.SlowDownKey;
        
        public static float GetAxis(AxisName axis)
        {
            switch (axis)
            {
                case AxisName.PosX:
                case AxisName.PosY:
                case AxisName.PosZ:
                case AxisName.Speed:
                    return GetKeyInput(axis);
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
                    return -UnityEngine.Input.GetAxis("MouseY");
                case AxisName.RotY:
                    return UnityEngine.Input.GetAxis("MouseX");
                case AxisName.RotZ:
                    return GetMouseButtonAxisFloat();
                case AxisName.Zoom:
                    // Invert for +ve zoom in, -ve zoom out
                    return -UnityEngine.Input.mouseScrollDelta.y;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, $"Invalid axis sent to GetMouseAxis: {axis}");
            }
        }

        private static float GetMouseButtonAxisFloat()
        {
            return Input.GetMouseButton(0) ? 1 : Input.GetMouseButton(1) ? -1 : 0;
        }

        private static float GetKeyInput(AxisName axis)
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