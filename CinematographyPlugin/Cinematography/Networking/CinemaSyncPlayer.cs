using System;
using Player;

namespace CinematographyPlugin.Cinematography.Networking
{
    public class CinemaSyncPlayer
    {
        public PlayerAgent Agent { get; }

        public bool IsInFreeCam { get; set; }

        public bool IsInCtrlOfTime { get; set; }

        public CinemaSyncPlayer(PlayerAgent agent)
        {
            Agent = agent;
        }

        public void UpdateStates(CinemaNetworkingManager.CinemaPluginStateData data)
        {
            IsInFreeCam = data.StartingFreeCam;
            IsInCtrlOfTime = data.StartingTimeScale;
        }

    }
}