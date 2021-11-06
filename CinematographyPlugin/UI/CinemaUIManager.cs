using System;
using FluffyUnderware.DevTools.Extensions;
using GTFO.API;
using MultiplayerWithBindingsExample;
using PhotographyPlugin.Photography;
using TMPro;
using ToggleUIPlugin.Managers;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;
using PlayerManager = Player.PlayerManager;

namespace PhotographyPlugin.UserInput
{
    public class CinemaUIManager : MonoBehaviour
    {
        private const KeyCode HideUI = KeyCode.F1;
        private const KeyCode HideBody = KeyCode.F2;
        private const KeyCode FreeCamera = KeyCode.F3;
        private const KeyCode UITest = KeyCode.F4;

        public static CursorLockMode cursorLastMode;
        public static bool cursorLastVisible;
        public static bool menuOpen;
        private UnityEngine.Object go;

        public CinemaUIManager(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }
        
        public void Update()
        {
            if (Input.GetKeyDown(HideUI))
            {
                ToggleUIManager.ToggleUI();
            }
            
            if (Input.GetKeyDown(HideBody))
            {
                ToggleUIManager.ToggleUI();
            }
            
            if (Input.GetKeyDown(FreeCamera))
            {
                FreeCameraController.ToggleNoClip();
            }
            
            if (Input.GetKeyDown(UITest))
            {
                if (!menuOpen)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    var loadedAsset = AssetAPI.GetLoadedAsset("Assets/UI/CinemaUI.prefab");
                    go = Instantiate(loadedAsset);
                    menuOpen = true;
                }
                else
                {
                    Cursor.lockState = cursorLastMode;
                    Cursor.visible = cursorLastVisible;
                    Destroy(go);
                    go = null;
                    menuOpen = false;
                }
            }
        }
    }
}