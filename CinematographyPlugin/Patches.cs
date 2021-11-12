using System.Collections.Generic;
using System.Linq;
using Agents;
using ChainedPuzzles;
using CinematographyPlugin.Cinematography;
using Enemies;
using HarmonyLib;
using Player;
using UnhollowerBaseLib;
using UnityEngine;

namespace CinematographyPlugin.UI
{
    [HarmonyPatch]
    public class UIPatches
    {

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), "Target", MethodType.Setter)]
        private static void Prefix_SetTarget(ref AgentTarget value)
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
            CP_Bioscan_Sync __instance) => ReduceTeamScanSizeIfPlayerInFreeCam(
            newState,
            __instance);
        
        private static void ReduceTeamScanSizeIfPlayerInFreeCam(
            pBioscanState newState,
            CP_Bioscan_Sync __instance)
        {
            var core = __instance.GetComponent<CP_Bioscan_Core>();
            if (core.m_playerScanner.RequireAllPlayers && newState.playersInScan == CinemaNetworkingManager.PlayersNotInFreeCamByName.Count)
            {
                var agentsInScanAndNotInFreeCam = new List<PlayerAgent>();
                foreach (var agent in core.m_playerAgents)
                {
                    if (!CinemaNetworkingManager.PlayersNotInFreeCamByName.ContainsKey(agent.Sync.PlayerNick))
                    {
                        agentsInScanAndNotInFreeCam.Add(agent);
                    }
                }

                if (agentsInScanAndNotInFreeCam.Count == CinemaNetworkingManager.PlayersNotInFreeCamByName.Count)
                {
                    if (PrevRequiredTeamScanIDs.Contains(__instance.GetInstanceID()))
                    {
                        CinematographyCore.log.LogInfo("Adjusting team scan to account for free cam players");
                        core.GetComponent<CP_PlayerScanner>().m_requireAllPlayers = false;
                    }
                    else
                    {
                        CinematographyCore.log.LogInfo("Reverting team scan to require all players");
                        core.GetComponent<CP_PlayerScanner>().m_requireAllPlayers = true;
                    }
                }
            }
        }
    }
}