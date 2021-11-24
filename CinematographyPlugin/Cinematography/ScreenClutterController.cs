using System;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class ScreenClutterController : MonoBehaviour
    {
        private static ScreenClutterController _instance;
        private static bool _init;
        
        private static GameObject _body;
        private static GameObject _fpArms;
        private static GameObject _uiCrosshairLayer;
        
        private static GameObject _uiPlayerLayer;
        private static GameObject _uiInteractionLayer;
        private static GameObject _uiNavMarkerLayer;
        private static PE_FPSDamageFeedback _damageFeedback;

        public ScreenClutterController(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        public static ScreenClutterController GetInstance()
        {
            if (_init)
            {
                return _instance;
            }

            CinematographyCore.log.LogWarning("ScreenClutterManager#GetInstance called before initialized");
            return null;
        }
        
        private void Awake()
        {
            CinemaUIManager.Toggles[UIOption.ToggleUI].OnValueChanged += ToggleUIElements;
            CinemaUIManager.Toggles[UIOption.ToggleBody].OnValueChanged += ToggleBody;
            
            _uiPlayerLayer = GuiManager.PlayerLayer.Root.FindChild("PlayerLayer").gameObject;
            _uiCrosshairLayer = GuiManager.CrosshairLayer.Root.FindChild("CrosshairLayer").gameObject;
            _uiInteractionLayer = GuiManager.PlayerLayer.Root.FindChild("InteractionLayer").gameObject;
            _uiNavMarkerLayer = GuiManager.PlayerLayer.Root.FindChild("NavMarkerLayer").gameObject;

            _body = PlayerManager.GetLocalPlayerAgent().AnimatorBody.transform.parent.gameObject;
            _fpArms = PlayerManager.GetLocalPlayerAgent().FPItemHolder.gameObject;
            
            var fpsCamera = FindObjectOfType<FPSCamera>();
            _damageFeedback = fpsCamera.gameObject.GetComponent<PE_FPSDamageFeedback>();

            _instance = this;
            _init = true;
        }

        public bool IsBodyOrUiVisible()
        {
            return _body.active || _fpArms.active || _uiPlayerLayer.active;
        }

        public void HideUI()
        {
            ToggleUIElements(false);
        }
        
        public void ToggleAllScreenClutterExceptWaterMark(bool value)
        {
            ToggleBody(value);
            ToggleUIElements(value);
            ToggleScreenShake(value);
            ToggleScreenLiquids(value);
        }

        public void ToggleClientVisibility(PlayerAgent player, bool value)
        {
            player.AnimatorBody.gameObject.active = value;
            player.NavMarker.m_marker.enabled = value;
        }

        private void ToggleBody(bool value)
        {
            _body.active = value;
            _fpArms.active = value;
        }

        private void ToggleUIElements(bool value)
        {
            _uiPlayerLayer.active = value;
            _uiCrosshairLayer.active = value;
            _uiInteractionLayer.active = value;
            _uiNavMarkerLayer.active = value;
        }

        private void ToggleScreenShake(bool value)
        {
            _damageFeedback.enabled = value;
        }

        private void ToggleScreenLiquids(bool value)
        {
            ScreenLiquidManager.hasSystem = value;
        }

        private void OnDestroy()
        {
            CinemaUIManager.Toggles[UIOption.ToggleUI].OnValueChanged -= ToggleUIElements;
            CinemaUIManager.Toggles[UIOption.ToggleBody].OnValueChanged -= ToggleBody;
        }
    }
}