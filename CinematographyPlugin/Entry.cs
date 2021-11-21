﻿using CinematographyPlugin.Cinematography;
using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using GTFO.API;
using HarmonyLib;
using ToggleUIPlugin.Managers;
using UnityEngine;

namespace CinematographyPlugin
{
    
    [HarmonyPatch]
    public class Entry
    {
        private static GameObject _go;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameStateManager), "ChangeState", typeof(eGameStateName))]
        private static void Postfix_InitOrDestroyCinematographyPlugin(eGameStateName nextState)
        {
            switch (nextState)
            {
                case eGameStateName.InLevel:
                {
                    CinematographyCore.log.LogMessage("Initializing " + CinematographyCore.NAME);

                    var gameObject = new GameObject(CinematographyCore.AUTHOR + " - " + CinematographyCore.NAME);
                    gameObject.AddComponent<CinemaUIManager>();
                    gameObject.AddComponent<TimeScaleController>();
                    gameObject.AddComponent<CinemaCamManager>();
                    gameObject.AddComponent<ScreenClutterManager>();
                    gameObject.AddComponent<LookSmoothingController>();
                    gameObject.AddComponent<CinemaNetworkingManager>();
                    gameObject.AddComponent<PostProcessingController>();
                    Object.DontDestroyOnLoad(gameObject);

                    _go = gameObject;
                    break;
                }
                case eGameStateName.ExpeditionAbort:
                case eGameStateName.ExpeditionFail:
                case eGameStateName.ExpeditionSuccess:
                case eGameStateName.AfterLevel:
                    CinematographyCore.log.LogMessage("Closing " + CinematographyCore.NAME);
                    Object.Destroy(_go);
                    break;
            }
        }
    }
}