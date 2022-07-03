using System.Runtime.InteropServices;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using GTFO.API;
using HarmonyLib;
using Player;
using UnityEngine;

namespace CinematographyPlugin.Cinematography.Networking
{
    public class CinemaNetworkingManager : MonoBehaviour
    {
        public static event Action<float> OnTimeScaleChangedByOtherPlayer;
        public static event Action<PlayerAgent, bool> OnOtherPlayerEnterExitFreeCam;
        public static event Action<bool> OnFreeCamEnableOrDisable;
        public static event Action<bool> OnTimeScaleEnableOrDisable;

        private static readonly Dictionary<string, CinemaSyncPlayer> PlayersByName = new ();

        private const string SyncCinemaStateEvent = "Sync_CinemaPlugin_States";
        private const string SyncCinemaAlterTimeScaleEvent = "Sync_CinemaPlugin_Time_Scale";
        private const string CinemaPingEvent = "Sync_CinemaPlugin_Ping";

        private static bool _prevCanUseFreeCam;
        private static bool _prevCanUseTimeScale;
        private static bool _canUseFreeCam = true;
        private static bool _canUseTimeScale = true;
        
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
        }
                
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CinemaPluginTimeScaleData {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string PlayerName;

            public double TimeScale;
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CinemaPluginPingData {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string PlayerName;
        }
        
        public void RegisterEvents()
        {
            if (!NetworkAPI.IsEventRegistered(SyncCinemaStateEvent))
            {
                NetworkAPI.RegisterEvent<CinemaPluginStateData>(SyncCinemaStateEvent,  (senderId, packet) => 
                {
                    UpdateStatesAndTriggerEvents(packet);
                });
            }
            
            if (!NetworkAPI.IsEventRegistered(SyncCinemaAlterTimeScaleEvent))
            {
                NetworkAPI.RegisterEvent<CinemaPluginTimeScaleData>(SyncCinemaAlterTimeScaleEvent,  (senderId, packet) => 
                {
                    OnTimeScaleSetByOtherPlayer(packet);
                });
            }
            
            if (!NetworkAPI.IsEventRegistered(CinemaPingEvent))
            {
                NetworkAPI.RegisterEvent<CinemaPluginPingData>(CinemaPingEvent,  (senderId, packet) => 
                {
                    OnPing(packet);
                });
            }
        }

        private void Awake()
        {
            CinemaUIManager.OnUIStart += Ping;
            CinemaUIManager.Toggles[UIOption.ToggleFreeCamera].OnValueChanged += SyncLocalPlayerEnterExitFreeCam;
            CinemaUIManager.Toggles[UIOption.ToggleTimeScale].OnValueChanged += SyncLocalPlayerEnterExitTimeScale;
            CinemaUIManager.Sliders[UIOption.TimeScaleSlider].OnValueChanged += SyncLocalPlayerAlterTimeScale;
        }

        private void Start()
        {
            UpdatePlayersList();
        }

        public static IEnumerable<PlayerAgent> GetPlayersNotInFreeCam()
        {
            return PlayersByName.Values.Where(p => !p.IsInFreeCam).Select(p => p.Agent).ToList();
        }
        
        public static IEnumerable<PlayerAgent> GetPlayersInFreeCam()
        {
            return PlayersByName.Values.Where(p => p.IsInFreeCam).Select(p => p.Agent).ToList();
        }

        public static bool AssertAllPlayersHasPlugin()
        {
            var assertAll = PlayersByName.Values.All(p => p.HasPlugin);
            
            if (!assertAll)
            {
                var names = PlayersByName.Values.Where(p => !p.HasPlugin).Join(p => p.Agent.Sync.PlayerNick);
                var msg = new[] {
                    "Cinematography plugin cannot be started.",
                    "All must have the plugin installed to continue.",
                    "The players that do not have the plugin are:",
                    names
                };
                
                CinematographyCore.log.LogInfo(msg.Join());
                
                foreach (var s in msg)
                {
                    PlayerChatManager.WantToSentTextMessage(PlayerManager.GetLocalPlayerAgent(), s);
                }
            }
            
            return assertAll;
        }
        
        private static void OnPing(CinemaPluginPingData data)
        {
            UpdatePlayersList();
            PlayersByName[data.PlayerName].HasPlugin = true;
        }

        private static void UpdateStatesAndTriggerEvents(CinemaPluginStateData stateData)
        {
            var agent = GetAgentFromName(stateData.PlayerName);
            if (agent == null) return;

            var playerName = agent.Sync.PlayerNick;
            UpdatePlayersList();
            
            if (stateData.StartingFreeCam && !PlayersByName[playerName].IsInFreeCam)
            {
                CinematographyCore.log.LogInfo($"{stateData.PlayerName} is starting free cam");
                OnOtherPlayerEnterExitFreeCam?.Invoke(agent, true);
            }
            if (stateData.StoppingFreeCam && PlayersByName[playerName].IsInFreeCam)
            {
                CinematographyCore.log.LogInfo($"{stateData.PlayerName} is stopping free cam");
                OnOtherPlayerEnterExitFreeCam?.Invoke(agent, false);
            }
            if (stateData.StartingTimeScale && !PlayersByName[playerName].IsInCtrlOfTime)
            {
                CinematographyCore.log.LogInfo($"{stateData.PlayerName} is starting to change time scale");
            }
            if (stateData.StoppingTimeScale && PlayersByName[playerName].IsInCtrlOfTime)
            {
                CinematographyCore.log.LogInfo($"{stateData.PlayerName} has stopped altering time scale");
            }
            
            PlayersByName[playerName].UpdateUIStates(stateData);

            var nPlayersInLvl = PlayerManager.PlayerAgentsInLevel.Count;
            var localPlayerAgent = PlayerManager.GetLocalPlayerAgent();
            var isAlone = nPlayersInLvl == 1;
            var nOtherPlayersInFreeCam = PlayersByName.Values.Count(p => p.IsInFreeCam && p.Agent.Sync.PlayerNick != localPlayerAgent.Sync.PlayerNick);
            var anotherPlayerInCtrlOfTime = PlayersByName.Values.Any(p => p.IsInCtrlOfTime && p.Agent.Sync.PlayerNick != localPlayerAgent.Sync.PlayerNick);
            var isEveryOneElseInFreeCam = nOtherPlayersInFreeCam == nPlayersInLvl - 1;

            _canUseFreeCam = isAlone || !isEveryOneElseInFreeCam;
            _canUseTimeScale = !anotherPlayerInCtrlOfTime;

            if (_canUseFreeCam != _prevCanUseFreeCam)
            {
                CinematographyCore.log.LogInfo(isEveryOneElseInFreeCam
                    ? "Free cam disabled: everyone else in lobby is in free cam"
                    : "Free cam enabled: not everyone else in lobby is in free cam");
                OnFreeCamEnableOrDisable?.Invoke(_canUseFreeCam);
            }
            
            if (_canUseTimeScale != _prevCanUseTimeScale)
            {
                CinematographyCore.log.LogInfo(anotherPlayerInCtrlOfTime
                    ? "Time scale control disabled: another player is changing time"
                    : "Time scale control enabled: other player has stopped changing time");
                OnTimeScaleEnableOrDisable?.Invoke(_canUseTimeScale);
            }

            _prevCanUseFreeCam = _canUseFreeCam;
            _prevCanUseTimeScale = _canUseTimeScale;
        }

        private static void Ping()
        {
            UpdatePlayersList();
            var playerName = PlayerManager.GetLocalPlayerAgent().Sync.PlayerNick;
            var data = new CinemaPluginPingData { PlayerName = playerName };
            PlayersByName[data.PlayerName].HasPlugin = true;
            NetworkAPI.InvokeEvent(CinemaPingEvent, data);
        }

        private static void OnTimeScaleSetByOtherPlayer(CinemaPluginTimeScaleData data)
        {
            if (PlayersByName.ContainsKey(data.PlayerName) && PlayersByName[data.PlayerName].IsInCtrlOfTime)
            {
                OnTimeScaleChangedByOtherPlayer?.Invoke((float) data.TimeScale);
            }
        }

        private static void SyncLocalPlayerEnterExitFreeCam(bool enteringFreeCam)
        {
            var playerName = PlayerManager.GetLocalPlayerAgent().Sync.PlayerNick;
            var data = new CinemaPluginStateData
            {
                PlayerName = playerName,
                StartingFreeCam = enteringFreeCam,
                StoppingFreeCam = !enteringFreeCam
            };

            PlayersByName[playerName].IsInFreeCam = enteringFreeCam;
            // CinematographyCore.log.LogInfo($"{data.PlayerName} broadcasting free cam {enteringFreeCam}");
            NetworkAPI.InvokeEvent(SyncCinemaStateEvent, data);
        }

        private static void SyncLocalPlayerEnterExitTimeScale(bool alteringTimeScale)
        {
            var playerName = PlayerManager.GetLocalPlayerAgent().Sync.PlayerNick;
            var data = new CinemaPluginStateData
            {
                PlayerName = playerName,
                StartingTimeScale = alteringTimeScale,
                StoppingTimeScale = !alteringTimeScale
            };
            
            PlayersByName[playerName].IsInCtrlOfTime = alteringTimeScale;
            // CinematographyCore.log.LogInfo($"{data.PlayerName} broadcasting time scale {alteringTimeScale}");
            NetworkAPI.InvokeEvent(SyncCinemaStateEvent, data);
        }

        private static void SyncLocalPlayerAlterTimeScale(float timeScale)
        {
            var data = new CinemaPluginTimeScaleData
            {
                PlayerName = PlayerManager.GetLocalPlayerAgent().Sync.PlayerNick, TimeScale = timeScale
            };
            NetworkAPI.InvokeEvent(SyncCinemaAlterTimeScaleEvent, data);
        }

        private static void UpdatePlayersList()
        {
            var agentsNames = new List<string>();
            foreach (var agent in PlayerManager.PlayerAgentsInLevel)
            {
                if (agent.gameObject.GetComponent<PlayerAIBot>() != null) continue;
                
                var agentName = agent.Sync.PlayerNick;
                agentsNames.Add(agentName);
                if (!PlayersByName.ContainsKey(agentName))
                {
                    CinematographyCore.log.LogDebug($"Adding {agentName} to active players list");

                    PlayersByName.Add(agentName, new CinemaSyncPlayer(agent));
                }
            }
            
            var playersNoLongerInLobby = PlayersByName.Keys.Where(playerName => !agentsNames.Contains(playerName)).ToList();
            foreach (var playerName in playersNoLongerInLobby)
            {
                PlayersByName.Remove(playerName);
            }
        }

        private static PlayerAgent GetAgentFromName(string playerName)
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

        private void OnDestroy()
        {
            CinemaUIManager.OnUIStart -= Ping;
            CinemaUIManager.Toggles[UIOption.ToggleFreeCamera].OnValueChanged -= SyncLocalPlayerEnterExitFreeCam;
            CinemaUIManager.Toggles[UIOption.ToggleTimeScale].OnValueChanged -= SyncLocalPlayerEnterExitTimeScale;
            CinemaUIManager.Sliders[UIOption.TimeScaleSlider].OnValueChanged -= SyncLocalPlayerAlterTimeScale;
            
            PlayersByName.Clear();

            _prevCanUseFreeCam = false;
            _prevCanUseTimeScale = false;
            _canUseFreeCam = true;
            _canUseTimeScale = true;
        }
    }
}