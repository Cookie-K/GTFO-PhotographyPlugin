using GTFO.API;
using HarmonyLib;
using PhotographyPlugin.Photography;
using PhotographyPlugin.UserInput;
using UnityEngine;

namespace PhotographyPlugin
{
    
    [HarmonyPatch]
    public class Entry
    {
        private static GameObject _go;

        [HarmonyPatch(typeof(GameStateManager), "ChangeState", typeof(eGameStateName))]
        public static void Postfix(eGameStateName nextState) => AddWeaponRandomizerComponents(nextState);

        private static void AddWeaponRandomizerComponents(eGameStateName state)
        {
            switch (state)
            {
                case eGameStateName.InLevel:
                {
                    PhotographyCore.log.LogMessage("Initializing " + PhotographyCore.NAME);

                    var gameObject = new GameObject(PhotographyCore.AUTHOR + " - " + PhotographyCore.NAME);
                    gameObject.AddComponent<CinemaUIManager>();
                    gameObject.AddComponent<FreeCameraController>();
                    Object.DontDestroyOnLoad(gameObject);

                    _go = gameObject;
                    break;
                }
                case eGameStateName.AfterLevel:
                    PhotographyCore.log.LogMessage("Closing " + PhotographyCore.NAME);
                    Object.Destroy(_go);
                    break;
            }
        }
    }
}