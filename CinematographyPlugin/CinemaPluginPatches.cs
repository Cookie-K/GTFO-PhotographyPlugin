using System;
using System.Collections.Generic;
using System.Linq;
using Agents;
using ChainedPuzzles;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.UI;
using Enemies;
using HarmonyLib;
using LibCpp2IL;
using Player;
using UnhollowerBaseLib;
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
        
        // Mirror mouse input when upside down
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LookCameraController), "MouseLookUpdate", typeof(float), typeof(float))]
        private static void Prefix_invertXonUpsideDown(ref float axisHor, float axisVer, LookCameraController __instance)
        {
            var upsideDown = Math.Sign(Vector3.Dot(__instance.transform.up, Vector3.up)) < 0;
            if (upsideDown)
            {
                axisHor *= -1;
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
        [HarmonyPatch(typeof(CP_Bioscan_Core), "Update")]
        private static void Prefix_ReduceTeamScanSizeIfPlayerInFreeCam(CP_Bioscan_Core __instance)
        {
            if (!CinemaNetworkingManager.PlayersInFreeCamByName.Any()) return;
            if (__instance.m_playerAgents.Count == CinemaNetworkingManager.PlayersNotInFreeCamByName.Count)
            {
                if (__instance.m_playerScanner.RequireAllPlayers)
                {
                    CinematographyCore.log.LogInfo("Adjusting team scan to account for free cam players");
                    __instance.GetComponent<CP_PlayerScanner>().m_requireAllPlayers = false;
                    PrevRequiredTeamScanIDs.Add(__instance.GetInstanceID());
                }   
            } else if (PrevRequiredTeamScanIDs.Contains(__instance.GetInstanceID()))
            {
                CinematographyCore.log.LogInfo("Reverting team scan to require all players");
                __instance.GetComponent<CP_PlayerScanner>().m_requireAllPlayers = true;
                PrevRequiredTeamScanIDs.Remove(__instance.GetInstanceID());
            }
        }
    }
}