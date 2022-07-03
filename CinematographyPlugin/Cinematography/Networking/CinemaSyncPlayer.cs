using Player;

namespace CinematographyPlugin.Cinematography.Networking
{
    public class CinemaSyncPlayer
    {
        public PlayerAgent Agent { get; }

        public bool HasPlugin { get; set; }

        public bool IsInFreeCam { get; set; }

        public bool IsInCtrlOfTime { get; set; }

        public CinemaSyncPlayer(PlayerAgent agent)
        {
            Agent = agent;
        }

        public void UpdateUIStates(CinemaNetworkingManager.CinemaPluginStateData data)
        {
            IsInFreeCam = data.StartingFreeCam || !data.StoppingFreeCam && IsInFreeCam;
            IsInCtrlOfTime = data.StartingTimeScale || !data.StoppingTimeScale && IsInCtrlOfTime;
        }

    }
}