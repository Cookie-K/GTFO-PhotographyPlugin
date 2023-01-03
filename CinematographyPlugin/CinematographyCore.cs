using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using CinematographyPlugin.Util;
using Globals;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

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
            VERSION = "1.2.4";

        public static ManualLogSource log;

        private Harmony HarmonyPatches { get; set; }

        public override void Load()
        {
             
            if (Global.RundownIdToLoad > 1)
            {
                // This plugin is only for modded rundowns with ID 1
                return;
            }
            
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
    }
}