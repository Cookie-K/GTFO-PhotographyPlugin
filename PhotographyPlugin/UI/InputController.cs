using System;
using PhotographyPlugin.Photography;
using UnityEngine;

namespace PhotographyPlugin.UserInput
{
    public class InputController : MonoBehaviour
    {
        private const KeyCode HideUi = KeyCode.U;
        private const KeyCode FreeCamera = KeyCode.F2;

                
        public InputController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }
        
        public void Update()
        {
            if (Input.GetKeyDown(HideUi))
            {
                FirstPersonRemover.CycleRemoveClutter();
            }
            
            if (Input.GetKeyDown(FreeCamera))
            {
                FreeCameraController.ToggleNoClip();
            }
        }
    }
}