using BepInEx.Unity.IL2CPP.Utils.Collections;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using CinematographyPlugin.Util;
using HarmonyLib;
using Player;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CinematographyPlugin
{

    [HarmonyPatch]
    public class Entry
    {
        public static bool Init { get; private set; }
        private static GameObject _go;

        private static IEnumerator InitCoroutine()
        {
            // Wait for local player agent to properly populate in case of late joins.
            while (PlayerManager.GetLocalPlayerAgent()?.Owner == null)
            {
                yield return null;
            }

            if (GameStateManager.CurrentStateName != eGameStateName.InLevel)
                yield break;

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
            gameObject.AddComponent<CinemaNetworkingManager>();
            Object.DontDestroyOnLoad(gameObject);

            _go = gameObject;
            Init = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameStateManager), "ChangeState", typeof(eGameStateName))]
        private static void Postfix_InitOrDestroyCinematographyPlugin(eGameStateName nextState)
        {
            CinematographyCore.log.LogMessage("Next state " + nextState);

            switch (nextState)
            {
                case eGameStateName.Offline:
                    CinematographyCore.LoadBundle();
                    CinemaNetworkingManager.RegisterEvents();
                    break;
                case eGameStateName.InLevel:
                    CoroutineManager.StartCoroutine(InitCoroutine().WrapToIl2Cpp());
                    break;
                case eGameStateName.ExpeditionAbort:
                case eGameStateName.ExpeditionFail:
                case eGameStateName.ExpeditionSuccess:
                case eGameStateName.AfterLevel:
                    if (_go != null)
                    {
                        CinematographyCore.log.LogMessage("Closing " + CinematographyCore.NAME);

                        if (CinemaCamManager.Current.FreeCamEnabled())
                        {
                            CinemaCamManager.Current.EnableOrDisableCinemaCam(false);
                        }

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