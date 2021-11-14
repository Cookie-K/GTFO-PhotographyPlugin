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
        
        public static event Action<Vector3> OnTeamScanStartedDuringFreeCam;
        
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
        [HarmonyPatch(typeof(CP_PlayerScanner), "StartScan")]
        private static void Prefix_OnStateChange(
            CP_PlayerScanner __instance) => CheckIfTeamScanStartWhilePlayerInFreeCam(
            __instance);
        
        private static void CheckIfTeamScanStartWhilePlayerInFreeCam(
            CP_PlayerScanner __instance)
        {
            if (CinemaNetworkingManager.PlayersNotInFreeCamByName.Count == 0) return;
            
            if (__instance.RequireAllPlayers && CinemaNetworkingManager.PlayersInFreeCamByName.ContainsKey(PlayerManager.GetLocalPlayerAgent().Sync.PlayerNick))
            {
                OnTeamScanStartedDuringFreeCam?.Invoke(__instance.transform.position);
            }
        }
    }
}