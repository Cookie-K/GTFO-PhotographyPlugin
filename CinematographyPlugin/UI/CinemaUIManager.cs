using System;
using System.Collections.Generic;
using BepInEx;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI.Enums;
using GTFO.API;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    [BepInDependency("dev.gtfomodding.gtfo-api")]
    public class CinemaUIManager : MonoBehaviour
    {
        public static event Action OnUIStart;
        
        private const string PrefabPath = "Assets/UI/CinemaUI.prefab";
        private const KeyCode UIOpenKey = KeyCode.F4;
       
        internal static Dictionary<UIOption, Option> Options { get; set; }
        internal static Dictionary<UIOption, ToggleOption> Toggles; 
        internal static Dictionary<UIOption, SliderOption> Sliders; 

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
                
                Options = UIFactory.BuildOptions(_cinemaUIgo);
                Toggles = UIFactory.GetToggles(Options);
                Sliders = UIFactory.GetSliders(Options);
            }
        }

        public void Start()
        {
            CinemaNetworkingManager.OnFreeCamEnableOrDisable += OnFreeCamEnableOrDisable;
            CinemaNetworkingManager.OnTimeScaleEnableOrDisable += OnTimeScaleEnableOrDisable;
            CinemaPluginPatches.OnLocalPlayerDieOrRevive += OnFreeCamEnableOrDisable;
            
            OnUIStart?.Invoke();
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
        
        private void OnFreeCamEnableOrDisable(bool enable)
        {
            var option = Toggles[UIOption.ToggleFreeCamera];
            if (enable)
            {
                option.Enable(option.Toggle.isOn);
            }
            else
            {
                option.Disable(false);
            }
        }
        
        private void OnTimeScaleEnableOrDisable(bool enable)
        {
            var option = Toggles[UIOption.ToggleTimeScale];
            if (enable)
            {
                option.Enable(option.Toggle.isOn);
            }
            else
            {
                option.Disable(false);
            }
        }

        public void OpenUI()
        {
            if (!CinemaNetworkingManager.AssertAllPlayersHasPlugin()) return;

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
            CinemaNetworkingManager.OnFreeCamEnableOrDisable -= OnFreeCamEnableOrDisable;
            CinemaNetworkingManager.OnTimeScaleEnableOrDisable -= OnTimeScaleEnableOrDisable;
            CinemaPluginPatches.OnLocalPlayerDieOrRevive -= OnFreeCamEnableOrDisable;
            
            if (MenuOpen)
            {
                CloseUI();
            }

            TimeScaleController.ResetTimeScale();
            ScreenClutterManager.GetInstance().HideUI();

            Destroy(_cinemaUIgo);
        }
    }
}