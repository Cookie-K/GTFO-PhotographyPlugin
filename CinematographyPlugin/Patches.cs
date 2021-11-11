using System.Collections.Generic;
using ChainedPuzzles;
using HarmonyLib;
using Player;
using UnhollowerBaseLib;
using UnityEngine;

namespace CinematographyPlugin.UI
{
    [HarmonyPatch]
    public class UIPatches
    {

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
        
        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(CP_PlayerScanner), "OnSyncStateChange", typeof(eBioscanStatus),
        // typeof(float),
        // typeof(List<PlayerAgent>),
        // typeof(int),
        // typeof(bool[]),
        // typeof(bool))]
        // private static void Prefix_reduceTeamScanSizeIfPlayerInFreeCam(
        //     ref eBioscanStatus status,
        //     float progress,
        //     ref List<PlayerAgent> playersInScan,
        //     int playersMax,
        //     bool[] reqItemStatus,
        //     bool isDropinState,
        //     ref CP_PlayerScanner __instance)
        // {
        //     if (__instance.RequireAllPlayers && CinemaUIManager.IsPlayerInFreeCam() && playersInScan.Count == playersMax-1)
        //     {
        //         playersInScan.Add(PlayerManager.GetLocalPlayerAgent());
        //     }
        // }
    }
}