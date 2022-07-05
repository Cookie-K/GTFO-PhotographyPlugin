using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI.Enums;
using GTFO.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CinematographyPlugin.UI
{
    public class CinemaUIManager : MonoBehaviour
    {
        public static CinemaUIManager Current;
        public static event Action OnUIStart;

        private static readonly KeyCode UIOpenKey = ConfigManager.MenuKey;
        private const string PrefabPath = "Assets/UI/CinemaUI.prefab";

        internal static Dictionary<UIOption, Option> Options { get; set; }
        internal static Dictionary<UIOption, ToggleOption> Toggles; 
        internal static Dictionary<UIOption, SliderOption> Sliders;

        internal static CursorLockMode CursorLockLastMode { get; set; }
        internal static bool CursorLastVisible { get; set; }
        internal static bool MenuOpen { get; set; }
        private static bool _init;

        private static TMP_Text _centerText;
        private static GameObject _centerTextWindow;
        private static GameObject _window;
        private static GameObject _cinemaUIgo;

        public void Awake()
        {
            Current = this;
            var loadedAsset = AssetAPI.GetLoadedAsset(PrefabPath);
            
            var instantiate = Instantiate(loadedAsset);
            _cinemaUIgo = instantiate.TryCast<GameObject>();
            
            if (_cinemaUIgo is null)
            {
                CinematographyCore.log.LogWarning(
                    "Cinematography plugin could not load UI prefab. Asset either not loaded or not in Bepinex/Config/Assets/AssetBundles folder");
            }
            else
            {
                // CinemaUI/Canvas/Window
                var canvas = _cinemaUIgo.transform.GetChild(0);
                _window = canvas.GetChild(0).gameObject;
                _centerTextWindow = canvas.GetChild(1).gameObject;
                _centerText = _centerTextWindow.GetComponentInChildren<TMP_Text>();
                _window.gameObject.AddComponent<UIWindow>();
                
                var resetButton = _cinemaUIgo.GetComponentInChildren<Button>();;
                resetButton.onClick.AddListener((UnityAction) OnCloseButton);
                
                Options = UIFactory.BuildOptions(_cinemaUIgo);
                Toggles = UIFactory.GetToggles(Options);
                Sliders = UIFactory.GetSliders(Options);

                _window.active = false;
                _centerTextWindow.active = false;
                _cinemaUIgo.active = true;
                _init = true;
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

        public void ShowTextOnScreen(string text)
        {
            _centerText.SetText($"[{text}]");
            _centerTextWindow.gameObject.active = true;
        }
        
        public void ShowNoTargetTextOnScreen()
        {
            _centerText.SetText("[NO TARGET]");
            _centerTextWindow.gameObject.active = true;
        }
        
        public void HideTextOnScreen()
        {
            _centerTextWindow.gameObject.active = false;
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
            _window.active = true;
            MenuOpen = true;
        }
        
        public static void CloseUI()
        {
            Cursor.lockState = CursorLockLastMode;
            Cursor.visible = CursorLastVisible;
            _window.active = false;
            MenuOpen = false;
        }

        public void OnDestroy()
        {
            CinemaNetworkingManager.OnFreeCamEnableOrDisable -= OnFreeCamEnableOrDisable;
            CinemaNetworkingManager.OnTimeScaleEnableOrDisable -= OnTimeScaleEnableOrDisable;
            CinemaPluginPatches.OnLocalPlayerDieOrRevive -= OnFreeCamEnableOrDisable;
            
            if (_cinemaUIgo != null)
            {
                Destroy(_cinemaUIgo);
            }
        }
    }
}