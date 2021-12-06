using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
using ChainedPuzzles;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.Cinematography.Networking;
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

        private static readonly List<int> PrevRequiredTeamScanIDs = new List<int>();
        
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
            if (__instance.IsLocallyOwned)
            {
                // CinematographyCore.log.LogInfo($"{__instance.name} is alive {value}");
                OnLocalPlayerDieOrRevive?.Invoke(value);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), "Target", MethodType.Setter)]
        private static void Prefix_SetTargetDivertAwayFromCameraMan(ref AgentTarget value)
        {
            if (value == null || PlayerManager.PlayerAgentsInLevel.Count == 1) return;
            var playerAgent = value.m_agent.TryCast<PlayerAgent>();
            if (playerAgent == null) return;
            
            if (CinemaNetworkingManager.GetPlayersInFreeCam().Any(p => p.Sync.PlayerNick == playerAgent.Sync.PlayerNick))
            {
                value.m_agent = CinemaNetworkingManager.GetPlayersNotInFreeCam().Aggregate((currMin, pa) => pa.GetAttackersScore() < currMin.GetAttackersScore() ? pa : currMin);
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CP_Bioscan_Core), "Update")]
        private static void Prefix_ReduceTeamScanSizeIfPlayerInFreeCam(CP_Bioscan_Core __instance)
        {
            if (!__instance.isActiveAndEnabled || __instance.m_currentPlayerCount == 0) return;
            
            if (__instance.m_playerScanner.RequireAllPlayers && CinemaNetworkingManager.GetPlayersInFreeCam().Any() && __instance.m_sync.GetCurrentState().status == eBioscanStatus.Scanning &&
                __instance.m_sync.GetCurrentState().playersInScan >= CinemaNetworkingManager.GetPlayersNotInFreeCam().Count())
            {
                CinematographyCore.log.LogInfo("Adjusting team scan to account for free cam players");
                __instance.GetComponent<CP_PlayerScanner>().m_requireAllPlayers = false;
                PrevRequiredTeamScanIDs.Add(__instance.GetInstanceID());
            }
            else if (PrevRequiredTeamScanIDs.Contains(__instance.GetInstanceID()) && __instance.m_currentPlayerCount < CinemaNetworkingManager.GetPlayersNotInFreeCam().Count())
            {
                CinematographyCore.log.LogInfo("Reverting team scan to require all players");
                __instance.GetComponent<CP_PlayerScanner>().m_requireAllPlayers = true;
                PrevRequiredTeamScanIDs.Remove(__instance.GetInstanceID());
            } 
        }
    }
}