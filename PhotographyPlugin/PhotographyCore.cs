using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using Dissonance;
using HarmonyLib;
using PhotographyPlugin.Photography;
using PhotographyPlugin.UserInput;
using UnhollowerRuntimeLib;

namespace PhotographyPlugin
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInProcess("GTFO.exe")]
    public class PhotographyCore : BasePlugin
    {
        public const string
            NAME = "Photography Plugin",
            MODNAME = "Photography",
            AUTHOR = "Cookie_K",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "0.0.0";

        public static ManualLogSource log;

        private Harmony HarmonyPatches { get; set; }

        public override void Load()
        {
            log = Log;

            ClassInjector.RegisterTypeInIl2Cpp<InputController>();
            ClassInjector.RegisterTypeInIl2Cpp<FirstPersonRemover>();
            ClassInjector.RegisterTypeInIl2Cpp<FreeCameraController>();

            HarmonyPatches = new Harmony(GUID);
            HarmonyPatches.PatchAll();
            
            foreach (var method in HarmonyPatches.GetPatchedMethods())
            {
                log.LogInfo("Patched method: " + method);
            }
        }
    }
}