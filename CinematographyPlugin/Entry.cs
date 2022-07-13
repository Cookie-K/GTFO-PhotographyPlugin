using CinematographyPlugin.Cinematography;
using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using CinematographyPlugin.Util;
using Globals;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CinematographyPlugin
{

    [HarmonyPatch]
    public class Entry
    {
        public static bool Init { get; private set; }
        private static GameObject _go;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameStateManager), "ChangeState", typeof(eGameStateName))]
        private static void Postfix_InitOrDestroyCinematographyPlugin(eGameStateName nextState)
        {
            switch (nextState)
            {
                case eGameStateName.InLevel:
                {
                    if (Global.RundownIdToLoad > 1)
                    {
                        // This plugin is only for modded rundowns with ID 1
                        return;
                    }

                    CinematographyCore.log.LogMessage("Initializing " + CinematographyCore.NAME);

                    var gameObject = new GameObject(CinematographyCore.AUTHOR + " - " + CinematographyCore.NAME);
                    gameObject.AddComponent<CinemaUIManager>();
                    gameObject.AddComponent<TimeScaleController>();
                    gameObject.AddComponent<CinemaCamManager>();
                    gameObject.AddComponent<ScreenClutterController>();
                    gameObject.AddComponent<LookSmoothingController>();
                    gameObject.AddComponent<PostProcessingController>();
                    gameObject.AddComponent<IndependentDeltaTimeManager>();
                    gameObject.AddComponent<LightManager>();
                    gameObject.AddComponent<AspectRatioManager>();
                    gameObject.AddComponent<DimensionManager>();
                    gameObject.AddComponent<CinemaNetworkingManager>().RegisterEvents();
                    Object.DontDestroyOnLoad(gameObject);

                    _go = gameObject;
                    Init = true;
                    break;
                }
                case eGameStateName.ExpeditionAbort:
                case eGameStateName.ExpeditionFail:
                case eGameStateName.ExpeditionSuccess:
                case eGameStateName.AfterLevel:
                    if (_go != null)
                    {
                        CinematographyCore.log.LogMessage("Closing " + CinematographyCore.NAME);

                        if (CinemaUIManager.Current.MenuOpen)
                        {
                            CinemaUIManager.Current.CloseUI();
                        }

                        TimeScaleController.ResetTimeScale();
                        ScreenClutterController.GetInstance().HideUI();

                        Object.Destroy(_go);
                        Init = false;
                    }

                    break;
            }
        }
    }
}