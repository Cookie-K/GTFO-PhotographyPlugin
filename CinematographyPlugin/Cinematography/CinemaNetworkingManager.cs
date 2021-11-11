using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CinematographyPlugin.UI;
using Nidhogg.Managers;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Cinematography
{
    public class CinemaNetworkingManager : MonoBehaviour
    {
        public static event Action<float> OnTimeScaleChangedByOtherPlayer;
        
        public static event Action<PlayerAgent, bool> OnOtherPlayerEnterExitFreeCam;
        
        public static event Action<bool> OnFreeCamEnableOrDisable;
        
        public static event Action<bool> OnTimeScaleEnableOrDisable;

        public static readonly Dictionary<string, PlayerAgent> PlayersInFreeCamByName = new Dictionary<string, PlayerAgent>();
        
        public static readonly Dictionary<string, PlayerAgent> PlayersNotInFreeCamByName = new Dictionary<string, PlayerAgent>();
        
        public static readonly Dictionary<string, PlayerAgent> PlayersInTimeScaleByName = new Dictionary<string, PlayerAgent>();
        
        public static readonly Dictionary<string, PlayerAgent> PlayersNotInTimeScaleByName = new Dictionary<string, PlayerAgent>();

        private const string SyncCinemaStateEventName = "Sync_CinemaPlugin_States";
        
        private const string SyncCinemaTimeScaleEventName = "Sync_CinemaPlugin_Time_Scale";

        private static bool _prevCanUseFreeCam = true;
        
        private static bool _prevCanUseTimeScale = true;
        
        private static bool _canUseFreeCam;

        private static bool _canUseTimeScale;
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CinemaPluginStateData {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string PlayerName;

            [MarshalAs(UnmanagedType.Bool)]
            public bool StartingFreeCam;
            
            [MarshalAs(UnmanagedType.Bool)]
            public bool StoppingFreeCam;
            
            [MarshalAs(UnmanagedType.Bool)]
            public bool StartingTimeScale;
            
            [MarshalAs(UnmanagedType.Bool)]
            public bool StoppingTimeScale;
            
            [MarshalAs(UnmanagedType.Bool)]
            public bool TimeScaleChange;
            
            [MarshalAs(UnmanagedType.R8)]
            public float TimeScale;
        }
        
                
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CinemaPluginTimeScaleData {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string PlayerName;

            [MarshalAs(UnmanagedType.R8)]
            public float TimeScale;
        }
        
        public CinemaNetworkingManager(IntPtr intPtr) : base(intPtr)
        {
            // For Il2CppAssemblyUnhollower
        }

        private void Start()
        {
            if (!NetworkingManager.IsEventRegistered(SyncCinemaStateEventName))
            {
                NetworkingManager.RegisterEvent<CinemaPluginStateData>(SyncCinemaStateEventName,  (senderId, packet) => 
                {
                    UpdateStates(packet);
                });
            }
            
            if (!NetworkingManager.IsEventRegistered(SyncCinemaTimeScaleEventName))
            {
                NetworkingManager.RegisterEvent<CinemaPluginTimeScaleData>(SyncCinemaTimeScaleEventName,  (senderId, packet) => 
                {
                    OnTimeScaleSetByOtherPlayer(packet);
                });
            }
            
            foreach (var agent in PlayerManager.PlayerAgentsInLevel)
            {
                PlayersNotInFreeCamByName.Add(agent.Sync.PlayerNick, agent);
                PlayersNotInTimeScaleByName.Add(agent.Sync.PlayerNick, agent);
            }
        }

        private void UpdateStates(CinemaPluginStateData stateData)
        {
            if (stateData.StartingFreeCam)
            {
                CinematographyCore.log.LogInfo($"{stateData.PlayerName} is starting free cam");
                var agent = GetAgentFromName(stateData.PlayerName);
                if (agent != null)
                {
                    PlayersInFreeCamByName.Add(stateData.PlayerName, agent);
                    PlayersNotInFreeCamByName.Remove(stateData.PlayerName);
                    OnOtherPlayerEnterExitFreeCam?.Invoke(agent, true);
                }
            }
            if (stateData.StoppingFreeCam)
            {
                CinematographyCore.log.LogInfo($"{stateData.PlayerName} is stopping free cam");
                var agent = GetAgentFromName(stateData.PlayerName);
                if (agent != null)
                {
                    PlayersInFreeCamByName.Remove(stateData.PlayerName);
                    PlayersNotInFreeCamByName.Add(stateData.PlayerName, agent);
                    OnOtherPlayerEnterExitFreeCam?.Invoke(agent, false);
                }
            }
            if (stateData.StartingTimeScale)
            {
                CinematographyCore.log.LogInfo($"{stateData.PlayerName} is starting to change time scale");
                var agent = GetAgentFromName(stateData.PlayerName);
                if (agent != null)
                {
                    PlayersInTimeScaleByName.Add(stateData.PlayerName, agent);
                    PlayersNotInTimeScaleByName.Remove(stateData.PlayerName);
                }
            }
            if (stateData.StoppingTimeScale)
            {
                CinematographyCore.log.LogInfo($"{stateData.PlayerName} has stopped altering time scale");
                var agent = GetAgentFromName(stateData.PlayerName);
                if (agent != null)
                {
                    PlayersInTimeScaleByName.Remove(stateData.PlayerName);
                    PlayersNotInTimeScaleByName.Add(stateData.PlayerName, agent);
                }
            }

            var playersInLvl = PlayerManager.PlayerAgentsInLevel._size;
            _canUseFreeCam = playersInLvl == 1 || playersInLvl - PlayersInFreeCamByName.Count > 1;
            _canUseTimeScale = PlayersInTimeScaleByName.Count == 0;

            if (_canUseFreeCam != _prevCanUseFreeCam)
            {
                CinematographyCore.log.LogInfo($"Free cam control state change, can use free cam: {_canUseFreeCam}");
                OnFreeCamEnableOrDisable?.Invoke(_canUseFreeCam);
            }
            
            if (_canUseTimeScale != _prevCanUseTimeScale)
            {
                CinematographyCore.log.LogInfo($"Time scale control state change, can use time scale: {_canUseTimeScale}");
                OnTimeScaleEnableOrDisable?.Invoke(_canUseTimeScale);
            }

            if (stateData.TimeScaleChange)
            {
                OnTimeScaleChangedByOtherPlayer?.Invoke(stateData.TimeScale);
            }

            _prevCanUseFreeCam = _canUseFreeCam;
            _prevCanUseTimeScale = _canUseTimeScale;
        }

        private void OnTimeScaleSetByOtherPlayer(CinemaPluginTimeScaleData data)
        {
            if (PlayersInTimeScaleByName.ContainsKey(data.PlayerName))
            {
                OnTimeScaleChangedByOtherPlayer?.Invoke(data.TimeScale);
            }
        }

        private PlayerAgent GetAgentFromName(string playerName)
        {
            foreach (var agent in PlayerManager.PlayerAgentsInLevel)
            {
                if (agent.Sync.PlayerNick == playerName)
                {
                    return agent;
                }
            }
            CinematographyCore.log.LogWarning($"Could not find player agent with the name {playerName}");
            return null;
        }
        
    }
}