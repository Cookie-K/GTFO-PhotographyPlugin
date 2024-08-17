﻿using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using CinematographyPlugin.Util;
using GTFO.API;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

[assembly: AssemblyVersion(CinematographyPlugin.CinematographyCore.VERSION)]
[assembly: AssemblyFileVersion(CinematographyPlugin.CinematographyCore.VERSION)]
[assembly: AssemblyInformationalVersion(CinematographyPlugin.CinematographyCore.VERSION)]
[assembly: AssemblyTitle(nameof(CinematographyPlugin))]
[assembly: AssemblyProduct(nameof(CinematographyPlugin))]

namespace CinematographyPlugin
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInProcess("GTFO.exe")]
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    public class CinematographyCore : BasePlugin
    {
        public const string
            NAME = "Cinematography Plugin",
            MODNAME = "Cinematography",
            AUTHOR = "Cookie_K",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.2.7";

        public static ManualLogSource log;

        private Harmony HarmonyPatches { get; set; }

        private const string PrefabPath = "assets/ui/cinemaui.prefab";
        private static AssetBundle _bundle;

        internal static GameObject CinemaUIPrefab;

        public override void Load()
        {
            
            log = Log;

            ClassInjector.RegisterTypeInIl2Cpp<UIWindow>();
            ClassInjector.RegisterTypeInIl2Cpp<CinemaUIManager>();
            ClassInjector.RegisterTypeInIl2Cpp<CinemaCamManager>();
            ClassInjector.RegisterTypeInIl2Cpp<CinemaCamController>();
            ClassInjector.RegisterTypeInIl2Cpp<TimeScaleController>();
            ClassInjector.RegisterTypeInIl2Cpp<ScreenClutterController>();
            ClassInjector.RegisterTypeInIl2Cpp<LookSmoothingController>();
            ClassInjector.RegisterTypeInIl2Cpp<CinemaNetworkingManager>();
            ClassInjector.RegisterTypeInIl2Cpp<PostProcessingController>();
            ClassInjector.RegisterTypeInIl2Cpp<IndependentDeltaTimeManager>();
            ClassInjector.RegisterTypeInIl2Cpp<LightManager>();
            ClassInjector.RegisterTypeInIl2Cpp<AspectRatioManager>();
            ClassInjector.RegisterTypeInIl2Cpp<DimensionManager>();

            HarmonyPatches = new Harmony(GUID);
            HarmonyPatches.PatchAll();
        }

        internal static void LoadBundle()
        {
            if (_bundle != null)
                return;

            try
            {
                log.LogDebug($"Loading internal {nameof(AssetBundle)} from resources ...");

                _bundle = AssetBundle.LoadFromMemory(Properties.Resources.chinematographyplugin);

                DontDestroyAndHideFlags(_bundle);

                CinemaUIPrefab = _bundle.LoadAsset(PrefabPath).TryCast<GameObject>();

                DontDestroyAndHideFlags(CinemaUIPrefab);

                log.LogDebug("Internal bundle loaded!");
            }
            catch(Exception ex)
            {
                log.LogWarning("Internal bundle loading failed!");
                log.LogDebug($"{ex.GetType().FullName}: {ex.Message}");
                log.LogMessage($"Using GTFO-APIs {nameof(AssetAPI)} to load the prefab instead as a fallback ...");

                CinemaUIPrefab = AssetAPI.GetLoadedAsset<GameObject>(PrefabPath);

                log.LogMessage($"Done!");
            }
        }

        private static void DontDestroyAndHideFlags(UnityEngine.Object obj)
        {
            UnityEngine.Object.DontDestroyOnLoad(obj);
            obj.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontUnloadUnusedAsset;
        }
    }
}