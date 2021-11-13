using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
using ChainedPuzzles;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.UI;
using Enemies;
using HarmonyLib;
using Player;
using UnityEngine;

namespace CinematographyPlugin
{
    [HarmonyPatch]
    public class CinemaPluginPatches
    {

        public static event Action<bool> OnLocalPlayerDieOrRevive;
        
        public static event Action<CP_Bioscan_Core> OnTeamScanStartedDuringFreeCam;
        
        private static List<int> PrevRequiredTeamScanIDs = new List<int>();

        // Force Curosr lock to none when cinema menu is open 
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Cursor), "lockState", MethodType.Setter)]
        private static void Prefix_SetLockState(ref CursorLockMode value)
        {
            if (CinemaUIManager.MenuOpen)
            {
                if (value != CursorLockMode.None)
                {
                    CinemaUIManager.CursorLockLastMode = value;
                }
                value = CursorLockMode.None;
            }
        }
        
        // Force Curosr visible when cinema menu is open
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Cursor), "visible", MethodType.Setter)]
        private static void Prefix_setVisible(ref bool value)
        {
            if (CinemaUIManager.MenuOpen)
            {
                CinemaUIManager.CursorLastVisible = value;
                value = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAgent), "Alive", MethodType.Setter)]
        private static void Prefix_PlayerAlive(bool value, PlayerAgent __instance)
        {
            CinematographyCore.log.LogInfo($"{__instance.name} is alive {value}");
            if (!value && __instance.IsLocallyOwned)
            {
                OnLocalPlayerDieOrRevive?.Invoke(value);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), "Target", MethodType.Setter)]
        private static void Prefix_SetTargetDivertAwayFromCameraMan(ref AgentTarget value)
        {
            if (value == null || value.m_agent.Cast<PlayerAgent>() == null) return;
            if (CinemaNetworkingManager.PlayersInFreeCamByName.ContainsKey(value.m_agent.Cast<PlayerAgent>().Sync.PlayerNick))
            {
                value.m_agent = CinemaNetworkingManager.PlayersNotInFreeCamByName.Values.Aggregate((currMin, pa) => pa.GetAttackersScore() < currMin.GetAttackersScore() ? pa : currMin);
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CP_Bioscan_Sync), "OnStateChange",
            typeof(pBioscanState),
            typeof(pBioscanState),
            typeof(bool))]
        private static void Prefix_OnStateChange(
            pBioscanState oldState,
            pBioscanState newState,
            bool isDropinState,
            CP_Bioscan_Sync __instance) => CheckIfTeamScanStartWhilePlayerInFreeCam(
            __instance);
        
        private static void CheckIfTeamScanStartWhilePlayerInFreeCam(
            CP_Bioscan_Sync __instance)
        {
            if (CinemaNetworkingManager.PlayersNotInFreeCamByName.Count == 0) return;
            
            var core = __instance.GetComponent<CP_Bioscan_Core>();
            if (core.m_playerScanner.RequireAllPlayers && CinemaNetworkingManager.PlayersInFreeCamByName.ContainsKey(PlayerManager.GetLocalPlayerAgent().Sync.PlayerNick))
            {
                OnTeamScanStartedDuringFreeCam?.Invoke(core);
            }
        }
    }
}