using System;
using System.Collections.Generic;
using CinematographyPlugin.UI.Enums;
using Gear;
using GTFO.API;
using Player;
using SNetwork;
using ToggleUIPlugin.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public class CinemaUIManager : MonoBehaviour
    {
        private const string PrefabPath = "Assets/UI/CinemaUI.prefab";
        private const KeyCode UIOpenKey = KeyCode.F4;
        private const KeyCode Test1 = KeyCode.F5;
        private const KeyCode Test2 = KeyCode.F6;
        internal static Dictionary<UIOption, Option> Options { get; set; }

        internal static CursorLockMode CursorLockLastMode { get; set; }
        internal static bool CursorLastVisible { get; set; }
        internal static bool MenuOpen { get; set; }
        private static bool _init;

        private GameObject _cinemaUIgo;

        public CinemaUIManager(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        public void Awake()
        {
            var loadedAsset = AssetAPI.GetLoadedAsset(PrefabPath);
            _cinemaUIgo = Instantiate(loadedAsset).TryCast<GameObject>();
            if (_cinemaUIgo is null)
            {
                CinematographyCore.log.LogWarning(
                    "Cinematography plugin could not load UI prefab. Asset either not loaded or not in Bepinex/Config folder");
            }
            else
            {
                _init = true;
                _cinemaUIgo.active = false;
                // CinemaUI/Canvas/Window
                var canvas = _cinemaUIgo.transform.GetChild(0);
                var window = canvas.GetChild(0);
                window.gameObject.AddComponent<UIWindow>();
                
                var resetButton = _cinemaUIgo.GetComponentInChildren<Button>();;
                resetButton.onClick.AddListener((UnityAction) OnCloseButton);
                
                Options = UIUtils.BuildOptions(_cinemaUIgo);

                ((ToggleOption) Options[UIOption.ToggleUI]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnUIToggle);
                ((ToggleOption) Options[UIOption.ToggleBody]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnBodyToggle);
                ((ToggleOption) Options[UIOption.ToggleFreeCamera]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnFreeCameraToggle);
                ((ToggleOption) Options[UIOption.ToggleLookSmoothing]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnLookSmoothingToggle);
                ((ToggleOption) Options[UIOption.ToggleCameraTilt]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnCameraTiltToggle);
                ((ToggleOption) Options[UIOption.ToggleTimeScale]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnTimeScaleToggle);
            }
        }
        
        public void Update()
        {
            if (!_init) return;
            if (Input.GetKeyDown(UIOpenKey))
            {
                if (MenuOpen)
                {
                    CloseUI();
                }
                else
                {
                    OpenUI();
                }
            }
        }

        public void OnCloseButton()
        {
            CloseUI();
        }
        
        public void OnUIToggle(bool state)
        {
            if (state)
            {
                ToggleUIManager.ShowUI();
            }
            else
            {
                ToggleUIManager.HideUI();
            }
        }
        
        public void OnBodyToggle(bool state)
        {
            if (state)
            {
                ToggleUIManager.ShowBody();
            }
            else
            {
                ToggleUIManager.HideBody();
            }
        }

        public void OnFreeCameraToggle(bool state)
        {
            OnBodyToggle(!state);
            ((ToggleOption) Options[UIOption.ToggleBody]).Toggle.isOn = !state;
            ((ToggleOption) Options[UIOption.ToggleBody]).Disable(state);
            
            Options[UIOption.MovementSpeedSlider].SetActive(state);
            Options[UIOption.MovementSmoothingSlider].SetActive(state);
        }
        
        public void OnLookSmoothingToggle(bool state)
        {
            Options[UIOption.LookSmoothingSlider].SetActive(state);
        }
        
        public void OnCameraTiltToggle(bool state)
        {
            Options[UIOption.CameraTiltSlider].SetActive(state);
        }
        
        public void OnTimeScaleToggle(bool state)
        {
            Options[UIOption.TimeScaleSlider].SetActive(state);
        }
        
        public void OpenUI()
        {
            CursorLockLastMode = Cursor.lockState;
            CursorLastVisible = Cursor.visible;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _cinemaUIgo.active = true;
            MenuOpen = true;
        }
        
        public void CloseUI()
        {
            Cursor.lockState = CursorLockLastMode;
            Cursor.visible = CursorLastVisible;
            _cinemaUIgo.active = false;
            MenuOpen = false;
        }

        public void OnDestroy()
        {
            if (MenuOpen)
            {
                CloseUI();
            }
            
            Destroy(_cinemaUIgo);
        }
    }
}