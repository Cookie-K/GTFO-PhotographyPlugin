using System;
using System.Collections.Generic;
using CinematographyPlugin.UI.Enums;
using GTFO.API;
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
                ((ToggleOption) Options[UIOption.ToggleDynamicRoll]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnDynamicRollToggle);
                ((ToggleOption) Options[UIOption.ToggleCameraRoll]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnCameraRollToggle);
                ((ToggleOption) Options[UIOption.ToggleTimeScale]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnTimeScaleToggle);
                ((ToggleOption) Options[UIOption.ToggleFoV]).Toggle.onValueChanged.AddListener((UnityAction<bool>) OnFoVToggle);
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
            ((ToggleOption) Options[UIOption.ToggleBody]).Toggle.Set(!state);
            ((ToggleOption) Options[UIOption.ToggleBody]).Disable(state);

            var movSpeedSlider = Options[UIOption.MovementSpeedSlider];
            var movSmoothing = Options[UIOption.MovementSmoothingSlider];
            var dynRoll = Options[UIOption.ToggleDynamicRoll];
            var miCtrl = Options[UIOption.ToggleMouseIndependentCtrl];
            
            movSpeedSlider.SetActive(state);
            movSmoothing.SetActive(state);
            dynRoll.SetActive(state);
            miCtrl.SetActive(state);
            
            ResetIfClose(state, movSpeedSlider);
            ResetIfClose(state, movSmoothing);
            ResetIfClose(state, dynRoll);
            ResetIfClose(state, miCtrl);
        }

        public void OnDynamicRollToggle(bool state)
        {
            var rollToggle = ((ToggleOption) Options[UIOption.ToggleCameraRoll]);
            var rollSlider = ((SliderOption) Options[UIOption.CameraRollSlider]);

            var dynRoll = Options[UIOption.ToggleDynamicRoll];
            var dynRollSlider = Options[UIOption.DynamicRollIntensitySlider];
            
            rollToggle.Toggle.Set(state);
            rollToggle.Disable(state);
            rollSlider.SetActive(!state);
            rollSlider.Disable(state);
            
            dynRoll.SetActive(state);
            dynRollSlider.SetActive(state);

            ResetIfClose(state, rollToggle);
            ResetIfClose(state, rollSlider);
            ResetIfClose(state, dynRoll);
            ResetIfClose(state, dynRollSlider);
        }
        
        public void OnLookSmoothingToggle(bool state)
        {
            var option = Options[UIOption.LookSmoothingSlider];
            option.SetActive(state);
            ResetIfClose(state, option);
        }
        
        public void OnCameraRollToggle(bool state)
        {
            var option = Options[UIOption.CameraRollSlider];
            option.SetActive(state);
            ResetIfClose(state, option);
        }
        
        public void OnTimeScaleToggle(bool state)
        {
            var option = Options[UIOption.TimeScaleSlider];
            option.SetActive(state);
            ResetIfClose(state, option);
        }
        
        public void OnFoVToggle(bool state)
        {
            var option = Options[UIOption.FoVSlider];
            option.SetActive(state);
            ResetIfClose(state, option);
        }

        private void ResetIfClose(bool state, Option option)
        {
            if (!state)
            {
                option.OnReset();
            }
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