using System;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PhotographyPlugin.Photography
{
    public class FirstPersonRemover : MonoBehaviour
    {
        private static bool _init;
        private static bool _uiHidden;
        private static bool _bodyHidden;
        
        private static GameObject _fpsItemHolderInstance;
        private static GameObject _crosshair;
        private static GameObject _body;
        private static UI_Apply _uiApply;

        public FirstPersonRemover(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }
        
        // Cannot init in Awake due to some elements not fully loaded in after elevator done 
        private static void Init()
        {
            _fpsItemHolderInstance = FindObjectOfType<FirstPersonItemHolder>().gameObject;
            _crosshair = FindObjectOfType<CircleCrosshair>().gameObject;
            _uiApply = FindObjectOfType<FPSCamera>().gameObject.GetComponent<UI_Apply>();
            _body = FindObjectOfType<FPSCameraHolder>().gameObject.transform.Find("FlatTransform").gameObject;
            _init = true;
        }

        public static void CycleRemoveClutter()
        {
            if (!_init) Init();
            if (!_uiHidden && !_bodyHidden)
            {
                ToggleUI();
            } else if (_uiHidden && !_bodyHidden)
            {
                ToggleBody();
            }
            else if (_uiHidden && _bodyHidden)
            {
                ToggleUI();
                ToggleBody();
            }
        }

        private static void ToggleUI()
        {
            PhotographyCore.log.LogInfo("Toggling UI Visibility");
            _uiHidden = !_uiHidden;
            
            _crosshair.active = !_uiHidden; 
            _uiApply.enabled = !_uiHidden;
        }

        private static void ToggleBody()
        {
            PhotographyCore.log.LogInfo("Toggling Body Visibility");
            _bodyHidden = !_bodyHidden;

            _body.active = !_bodyHidden;
            _fpsItemHolderInstance.active = !_bodyHidden;
        }
    }
}