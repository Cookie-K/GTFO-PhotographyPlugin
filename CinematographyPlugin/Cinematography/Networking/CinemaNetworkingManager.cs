using System.Runtime.InteropServices;
using CinematographyPlugin.UI;
using CinematographyPlugin.UI.Enums;
using GTFO.API;
using HarmonyLib;
using Player;
using SNetwork;
using UnityEngine;

namespace CinematographyPlugin.Cinematography.Networking
{
    public class CinemaNetworkingManager : MonoBehaviour
    {
        public static event Action<float> OnTimeScaleChangedByOtherPlayer;
        public static event Action<PlayerAgent, bool> OnOtherPlayerEnterExitFreeCam;
        // public static event Action<bool> OnFreeCamEnableOrDisable;
        public static event Action<bool> OnTimeScaleEnableOrDisable;

        private static readonly Dictionary<ulong, CinemaSyncPlayer> SyncPlayersById = new ();

        private const string SyncCinemaStateEvent = "Sync_CinemaPlugin_States_v2";
        private const string SyncCinemaAlterTimeScaleEvent = "Sync_CinemaPlugin_Time_Scale_v2";
        private const string CinemaPingEvent = "Sync_CinemaPlugin_Ping_v2";

        private static bool _actOnUpdates = false;
        private static bool _prevCanUseFreeCam;
        private static bool _prevCanUseTimeScale;
        private static bool _canUseFreeCam = true;
        private static bool _canUseTimeScale = true;

        private static ulong LocalPlayerId => PlayerManager.GetLocalPlayerAgent()?.Owner?.Lookup ?? 0;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CinemaPluginStateData {
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
            public double TimeScale;
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CinemaPluginPingData {
            [MarshalAs(UnmanagedType.Bool)]
            public bool HasPlugin;
        }

        public static void RegisterEvents()
        {
            if (!NetworkAPI.IsEventRegistered(SyncCinemaStateEvent))
            {
                NetworkAPI.RegisterEvent<CinemaPluginStateData>(SyncCinemaStateEvent,  (senderId, packet) => 
                {
                    UpdateStatesAndTriggerEvents(senderId, packet);
                });
            }
            
            if (!NetworkAPI.IsEventRegistered(SyncCinemaAlterTimeScaleEvent))
            {
                NetworkAPI.RegisterEvent<CinemaPluginTimeScaleData>(SyncCinemaAlterTimeScaleEvent,  (senderId, packet) => 
                {
                    OnTimeScaleSetByOtherPlayer(senderId, packet);
                });
            }
            
            if (!NetworkAPI.IsEventRegistered(CinemaPingEvent))
            {
                NetworkAPI.RegisterEvent<CinemaPluginPingData>(CinemaPingEvent,  (senderId, packet) => 
                {
                    OnPing(senderId, packet);
                });
            }
        }

        private void Awake()
        {
            CinemaUIManager.OnUIStart += OnUIStart;
            CinemaUIManager.Current.Toggles[UIOption.ToggleFreeCamera].OnValueChanged += SyncLocalPlayerEnterExitFreeCam;
            CinemaUIManager.Current.Toggles[UIOption.ToggleTimeScale].OnValueChanged += SyncLocalPlayerEnterExitTimeScale;
            CinemaUIManager.Current.Sliders[UIOption.TimeScaleSlider].OnValueChanged += SyncLocalPlayerAlterTimeScale;
        }

        private void Start()
        {
            UpdatePlayersList();
        }

        public static IEnumerable<PlayerAgent> GetPlayersNotInFreeCam()
        {
            return SyncPlayersById.Values.Where(p => !p.IsInFreeCam).Select(p => p.Agent).ToList();
        }
        
        public static IEnumerable<PlayerAgent> GetPlayersInFreeCam()
        {
            return SyncPlayersById.Values.Where(p => p.IsInFreeCam).Select(p => p.Agent).ToList();
        }

        public static bool AssertAllPlayersHasPlugin()
        {
            UpdatePlayersList();

            var assertAll = SyncPlayersById.Values.All(p => p.IsBot || p.HasPlugin);
            
            foreach(var syncPlayerKvp in SyncPlayersById)
            {
                CinematographyCore.log.LogDebug($"{syncPlayerKvp.Key}: {syncPlayerKvp.Value}");
            }

            if (!assertAll)
            {
                var names = SyncPlayersById.Values.Where(p => !p.HasPlugin).Join(p => p.Agent.Sync.PlayerNick);
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
        
        private static void OnPing(ulong senderId, CinemaPluginPingData data)
        {
            UpdatePlayersList();
            SyncPlayersById[senderId].HasPlugin = true;

            if (!SNet.TryGetPlayer(senderId, out var joinedPlayer))
            {
                return;
            }

            SyncLocalDataTo(joinedPlayer);

            if (!SNet.IsMaster)
                return;

            var timeScaleData = new CinemaPluginTimeScaleData
            {
                TimeScale = Time.timeScale
            };

            NetworkAPI.InvokeEvent(SyncCinemaAlterTimeScaleEvent, timeScaleData, joinedPlayer);
        }

        private static void SyncLocalDataTo(SNet_Player joinedPlayer)
        {
            var localSync = SyncPlayersById[LocalPlayerId];

            var data = new CinemaPluginStateData
            {
                StartingFreeCam = localSync.IsInFreeCam,
                StartingTimeScale = localSync.IsInCtrlOfTime,
                StoppingFreeCam = false,
                StoppingTimeScale = false,
            };

            NetworkAPI.InvokeEvent(SyncCinemaStateEvent, data, joinedPlayer);
        }

        private static void UpdateStatesAndTriggerEvents(ulong senderId, CinemaPluginStateData stateData)
        {
            var agent = GetAgentFromId(senderId);
            
            if (agent == null)
                return;

            var playerName = agent.Sync.PlayerNick;
            UpdatePlayersList();
            SyncPlayersById[senderId].HasPlugin = true;

            if (stateData.StartingFreeCam && !SyncPlayersById[senderId].IsInFreeCam)
            {
                CinematographyCore.log.LogInfo($"{playerName} is starting free cam");
                if (_actOnUpdates)
                    OnOtherPlayerEnterExitFreeCam?.Invoke(agent, true);
            }
            if (stateData.StoppingFreeCam && SyncPlayersById[senderId].IsInFreeCam)
            {
                CinematographyCore.log.LogInfo($"{playerName} is stopping free cam");
                if (_actOnUpdates)
                    OnOtherPlayerEnterExitFreeCam?.Invoke(agent, false);
            }
            if (stateData.StartingTimeScale && !SyncPlayersById[senderId].IsInCtrlOfTime)
            {
                CinematographyCore.log.LogInfo($"{playerName} is starting to change time scale");
            }
            if (stateData.StoppingTimeScale && SyncPlayersById[senderId].IsInCtrlOfTime)
            {
                CinematographyCore.log.LogInfo($"{playerName} has stopped altering time scale");
            }

            SyncPlayersById[senderId].UpdateUIStates(stateData);

            var anotherPlayerInCtrlOfTime = SyncPlayersById.Values.Any(p => p.IsInCtrlOfTime && p.Agent.Owner.Lookup != LocalPlayerId);
            _canUseTimeScale = !anotherPlayerInCtrlOfTime;

            // Commenting this out since it's probably fine that everyone can use free cam mode
            
            // var nPlayersInLvl = PlayerManager.PlayerAgentsInLevel.Count;
            // var isAlone = nPlayersInLvl == 1;
            // var nOtherPlayersInFreeCam = SyncPlayersByName.Values.Count(p => p.IsInFreeCam && p.Agent.Sync.PlayerNick != localPlayerAgent.Sync.PlayerNick);
            // var isEveryOneElseInFreeCam = nOtherPlayersInFreeCam == nPlayersInLvl - 1;
            // _canUseFreeCam = isAlone || !isEveryOneElseInFreeCam;
            
            // if (_canUseFreeCam != _prevCanUseFreeCam)
            // {
            //     CinematographyCore.log.LogInfo(isEveryOneElseInFreeCam
            //         ? "Free cam disabled: everyone else in lobby is in free cam"
            //         : "Free cam enabled: not everyone else in lobby is in free cam");
            //     OnFreeCamEnableOrDisable?.Invoke(_canUseFreeCam);
            // }
            
            if (_canUseTimeScale != _prevCanUseTimeScale)
            {
                CinematographyCore.log.LogInfo(anotherPlayerInCtrlOfTime
                    ? "Time scale control disabled: another player is changing time"
                    : "Time scale control enabled: other player has stopped changing time");
                if (_actOnUpdates)
                    OnTimeScaleEnableOrDisable?.Invoke(_canUseTimeScale);
            }

            _prevCanUseFreeCam = _canUseFreeCam;
            _prevCanUseTimeScale = _canUseTimeScale;
        }

        private static void OnUIStart()
        {
            _actOnUpdates = true;

            SendPing();
        }

        private static void SendPing()
        {
            UpdatePlayersList();
            var data = new CinemaPluginPingData { HasPlugin = true };
            SyncPlayersById[LocalPlayerId].HasPlugin = true;
            NetworkAPI.InvokeEvent(CinemaPingEvent, data);
        }

        private static void OnTimeScaleSetByOtherPlayer(ulong senderId, CinemaPluginTimeScaleData data)
        {
            if (!_actOnUpdates)
                return;

            if (SyncPlayersById.ContainsKey(senderId) && SyncPlayersById[senderId].IsInCtrlOfTime)
            {
                OnTimeScaleChangedByOtherPlayer?.Invoke((float) data.TimeScale);
            }
        }

        private static void SyncLocalPlayerEnterExitFreeCam(bool enteringFreeCam)
        {
            var data = new CinemaPluginStateData
            {
                StartingFreeCam = enteringFreeCam,
                StoppingFreeCam = !enteringFreeCam
            };

            SyncPlayersById[LocalPlayerId].IsInFreeCam = enteringFreeCam;
            // CinematographyCore.log.LogInfo($"{data.PlayerName} broadcasting free cam {enteringFreeCam}");
            NetworkAPI.InvokeEvent(SyncCinemaStateEvent, data);
        }

        private static void SyncLocalPlayerEnterExitTimeScale(bool alteringTimeScale)
        {
            var data = new CinemaPluginStateData
            {
                StartingTimeScale = alteringTimeScale,
                StoppingTimeScale = !alteringTimeScale
            };

            SyncPlayersById[LocalPlayerId].IsInCtrlOfTime = alteringTimeScale;
            // CinematographyCore.log.LogInfo($"{data.PlayerName} broadcasting time scale {alteringTimeScale}");
            NetworkAPI.InvokeEvent(SyncCinemaStateEvent, data);
        }

        private static void SyncLocalPlayerAlterTimeScale(float timeScale)
        {
            var data = new CinemaPluginTimeScaleData
            {
                TimeScale = timeScale
            };
            NetworkAPI.InvokeEvent(SyncCinemaAlterTimeScaleEvent, data);
        }

        private static IEnumerable<PlayerAgent> GetPlayers()
        {
            if (SNet.Lobby == null)
                yield break;

            foreach(var player in SNet.Lobby.Players)
            {
                if (player == null || !player.IsInSlot || !player.HasPlayerAgent)
                    continue;

                yield return player.PlayerAgent.TryCast<PlayerAgent>();
            }
        }

        private static void UpdatePlayersList()
        {
            var agentLookupIds = new List<ulong>();

            foreach (var agent in GetPlayers())
            {
                var isBot = agent.gameObject.GetComponent<PlayerAIBot>() != null;
                var agentLookup = agent.Owner.Lookup;
                agentLookupIds.Add(agentLookup);
                if (!SyncPlayersById.ContainsKey(agentLookup))
                {
                    CinematographyCore.log.LogDebug($"Adding {agentLookup} to active players list");
                    SyncPlayersById.Add(agentLookup, new CinemaSyncPlayer(agent, isBot));
                } else if (SyncPlayersById[agentLookup].Agent == null)
                {
                    // It's possible for the agent to be destroyed on disconnects 
                    SyncPlayersById.Remove(agentLookup);
                    SyncPlayersById.Add(agentLookup, new CinemaSyncPlayer(agent, isBot));
                }
            }
            
            var playersNoLongerInLobby = SyncPlayersById.Keys.Where(playerId => !agentLookupIds.Contains(playerId)).ToList();
            foreach (var playerId in playersNoLongerInLobby)
            {
                SyncPlayersById.Remove(playerId);
            }
        }

        private static PlayerAgent GetAgentFromId(ulong senderId)
        {
            if (!SNet.TryGetPlayer(senderId, out var player))
            {
                CinematographyCore.log.LogWarning($"Could not find player agent with id {senderId}");
                return null;
            }

            if(!player.HasPlayerAgent)
            {
                CinematographyCore.log.LogWarning($"Player with id {senderId} ({player.NickName}) does not have a {nameof(PlayerAgent)} yet!");
                return null;
            }

            return player.PlayerAgent.TryCast<PlayerAgent>();
        }

        private void OnDestroy()
        {
            CinemaUIManager.OnUIStart -= OnUIStart;
            CinemaUIManager.Current.Toggles[UIOption.ToggleFreeCamera].OnValueChanged -= SyncLocalPlayerEnterExitFreeCam;
            CinemaUIManager.Current.Toggles[UIOption.ToggleTimeScale].OnValueChanged -= SyncLocalPlayerEnterExitTimeScale;
            CinemaUIManager.Current.Sliders[UIOption.TimeScaleSlider].OnValueChanged -= SyncLocalPlayerAlterTimeScale;

            _actOnUpdates = false;

            _prevCanUseFreeCam = false;
            _prevCanUseTimeScale = false;
            _canUseFreeCam = true;
            _canUseTimeScale = true;
        }
    }
}