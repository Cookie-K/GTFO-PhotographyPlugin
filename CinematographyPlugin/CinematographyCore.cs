using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.UI;
using Dissonance;
using HarmonyLib;
using UnhollowerRuntimeLib;

namespace CinematographyPlugin
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInProcess("GTFO.exe")]
    public class CinematographyCore : BasePlugin
    {
        public const string
            NAME = "Cinematography Plugin",
            MODNAME = "Cinematography",
            AUTHOR = "Cookie_K",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "0.0.0";

        public static ManualLogSource log;

        private Harmony HarmonyPatches { get; set; }

        public override void Load()
        {
            log = Log;

            ClassInjector.RegisterTypeInIl2Cpp<UIWindow>();
            ClassInjector.RegisterTypeInIl2Cpp<CinemaUIManager>();
            ClassInjector.RegisterTypeInIl2Cpp<FreeCameraController>();
            ClassInjector.RegisterTypeInIl2Cpp<LookSmoothingController>();

            HarmonyPatches = new Harmony(GUID);
            HarmonyPatches.PatchAll();
            //
            // foreach (var method in HarmonyPatches.GetPatchedMethods())
            // {
            //     log.LogInfo("Patched method: " + method);
            // }
        }
    }
}