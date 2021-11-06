using HarmonyLib;
using PhotographyPlugin.UserInput;
using UnityEngine;

namespace PhotographyPlugin
{
    [HarmonyPatch]
    public class UIPatches
    {

        // Force Curosr lock to none when cinema menu is open 
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Cursor), "lockState", MethodType.Setter)]
        private static void Prefix_setLockState(ref CursorLockMode value)
        {
            if (CinemaUIManager.menuOpen)
            {
                CinemaUIManager.cursorLastMode = value;
                value = CursorLockMode.None;
            }
        }
        
        // Force Curosr visible when cinema menu is open
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Cursor), "visible", MethodType.Setter)]
        private static void Prefix_setVisible(ref bool value)
        {
            if (CinemaUIManager.menuOpen)
            {
                CinemaUIManager.cursorLastVisible = value;
                value = true;
            }
        }
    }
}