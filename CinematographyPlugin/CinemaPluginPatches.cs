using Agents;
using ChainedPuzzles;
using CinematographyPlugin.Cinematography;
using CinematographyPlugin.Cinematography.Networking;
using CinematographyPlugin.UI;
using Enemies;
using Gear;
using HarmonyLib;
using Player;
using UnityEngine;

namespace CinematographyPlugin
{
    [HarmonyPatch]
    public class CinemaPluginPatches
    {

        public static event Action<bool> OnLocalPlayerDieOrRevive;

        private static readonly List<int> PrevRequiredTeamScanIDs = new ();
   
        [HarmonyPrefix]
        [HarmonyPatch(typeof (InputMapper), nameof(InputMapper.DoGetButton))]
        [HarmonyPatch(typeof (InputMapper), nameof(InputMapper.DoGetButtonUp))]
        [HarmonyPatch(typeof (InputMapper), nameof(InputMapper.DoGetButtonDown))]
        private static bool Prefix_DoGetButton(ref bool __result)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                return true;
            }
            __result = false;
            return false;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof (InputMapper), nameof(InputMapper.DoGetAxis))]
        private static bool Prefix_DoGetAxis(ref float __result)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                return true;
            }
            __result = 0.0f;
            return false;
        }
        
        // Force Curosr lock to none when cinema menu is open 
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Cursor), nameof(Cursor.lockState), MethodType.Setter)]
        private static void Prefix_SetLockState(ref CursorLockMode value)
        {
            if (Entry.Init && CinemaUIManager.Current.MenuOpen)
            {
                if (value != CursorLockMode.None)
                {
                    CinemaUIManager.Current.CursorLockLastMode = value;
                }
                value = CursorLockMode.None;
            }
        }
        
        // Force Curosr visible when cinema menu is open
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Cursor), nameof(Cursor.visible), MethodType.Setter)]
        private static void Prefix_setVisible(ref bool value)
        {
            if (Entry.Init && CinemaUIManager.Current.MenuOpen)
            {
                CinemaUIManager.Current.CursorLastVisible = value;
                value = true;
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.Alive), MethodType.Setter)]
        private static void Prefix_PlayerAlive(bool value, PlayerAgent __instance)
        {
            if (__instance != null && __instance.IsLocallyOwned)
            {
                OnLocalPlayerDieOrRevive?.Invoke(value);
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.Target), MethodType.Setter)]
        private static void Prefix_SetTargetDivertAwayFromCameraMan(ref AgentTarget value)
        {
            if (value == null || PlayerManager.PlayerAgentsInLevel.Count == 1) return;
            var playerAgent = value.m_agent as PlayerAgent;
            if (playerAgent == null) return;
            
            if (CinemaNetworkingManager.GetPlayersInFreeCam().Any(p => p.Sync.PlayerNick == playerAgent.Sync.PlayerNick))
            {
                value.m_agent = CinemaNetworkingManager.GetPlayersNotInFreeCam().Aggregate((currMin, pa) => pa.GetAttackersScore() < currMin.GetAttackersScore() ? pa : currMin);
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyDetection), nameof(EnemyDetection.IsTargetValid), typeof(AgentTarget))]
        private static bool Postfix_Prefix_SetTargetDivertAwayFromCameraMan(AgentTarget agentTarget)
        {
            var playerAgent = agentTarget.m_agent as PlayerAgent;
            return playerAgent == null || CinemaNetworkingManager.GetPlayersInFreeCam().All(p => p.Sync.PlayerNick != playerAgent.Sync.PlayerNick);
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerSync), nameof(PlayerSync.WantsToWieldSlot), typeof(InventorySlot), typeof(bool))]
        private static bool Postfix_Prefix_DisableWeaponChange(InventorySlot slot, bool broadcastOnly, PlayerSync __instance)
        {
            return !Entry.Init || !CinemaCamManager.Current.FreeCamEnabled() || !__instance.m_agent.IsLocallyOwned;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CP_Bioscan_Core), nameof(CP_Bioscan_Core.Update))]
        private static void Prefix_ReduceTeamScanSizeIfPlayerInFreeCam(CP_Bioscan_Core __instance)
        {
            if (!__instance.isActiveAndEnabled || __instance.m_currentPlayerCount == 0) return;
            
            if (__instance.m_playerScanner.ScanPlayersRequired == PlayerRequirement.All && CinemaNetworkingManager.GetPlayersInFreeCam().Any() && __instance.m_sync.GetCurrentState().status == eBioscanStatus.Scanning &&
                __instance.m_sync.GetCurrentState().playersInScan >= CinemaNetworkingManager.GetPlayersNotInFreeCam().Count())
            {
                CinematographyCore.log.LogInfo("Adjusting team scan to account for free cam players");
                __instance.GetComponent<CP_PlayerScanner>().m_playerRequirement = PlayerRequirement.None;
                PrevRequiredTeamScanIDs.Add(__instance.GetInstanceID());
            }
            else if (PrevRequiredTeamScanIDs.Contains(__instance.GetInstanceID()) && __instance.m_currentPlayerCount < CinemaNetworkingManager.GetPlayersNotInFreeCam().Count())
            {
                CinematographyCore.log.LogInfo("Reverting team scan to require all players");
                __instance.GetComponent<CP_PlayerScanner>().m_playerRequirement = PlayerRequirement.All;
                PrevRequiredTeamScanIDs.Remove(__instance.GetInstanceID());
            } 
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PostProcessingAdapter), nameof(PostProcessingAdapter.SetDOF), typeof(float), typeof(float), typeof(float))]
        private static bool Prefix_OverrideDoF(ref float focusDistance, ref float aperture, ref float focalLength)
        {
            return !PostProcessingController.IsDoFSet();
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Dam_PlayerDamageBase), nameof(Dam_PlayerDamageBase.OnIncomingDamage))]
        private static void Postfix_IgnoreAllDamage(ref float damage, ref bool triggerDialog, Dam_PlayerDamageBase __instance)
        {
            if (__instance.TryCast<Dam_PlayerDamageBase>() != null && CinemaCamManager.Current.InGodMode())
            {
                damage = 0;
                triggerDialog = false;
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerDialogManager), nameof(PlayerDialogManager.WantToStartDialog), typeof(uint), typeof(PlayerAgent))]
        private static bool Postfix_IgnorePlayerDialog(uint dialogID, PlayerAgent source)
        {
            return !CinemaCamManager.Current.FreeCamEnabled() || !source.IsLocallyOwned;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerDialogManager), nameof(PlayerDialogManager.WantToStartDialog), typeof(uint), typeof(int), typeof(bool), typeof(bool))]
        private static bool Postfix_IgnorePlayerDialog(
            uint dialogID,
            int playerID,
            bool overrideFilters,
            bool forced)
        {
            return !CinemaCamManager.Current.FreeCamEnabled();
        }
        
    }
}