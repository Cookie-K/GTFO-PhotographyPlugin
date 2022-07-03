using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace CinematographyPlugin
{
    public static class ConfigManager
    {
        private static readonly ConfigFile ConfigFile = new (Path.Combine(Paths.ConfigPath, "CinematographyPlugin.cfg"), true);

        private static readonly ConfigEntry<KeyCode> MenuOpenClose = ConfigFile
            .Bind("Key Binds", "Menu Open Close", KeyCode.F4, "The key to open / close the plugin menu");

        private static readonly ConfigEntry<bool> UseAlpha = ConfigFile
            .Bind("Key Binds", "Use Alphanumeric keys for Time Controls", false, 
                "The time scale percentages are bound to the numpad keys by default, change this to false to use the alpha numeric keys in stead " +
                "(must be in free cam as to not conflict with weapon changes in first person)");
        
        private static readonly ConfigEntry<KeyCode> GoUp = ConfigFile
            .Bind("Key Binds", "Go Up", KeyCode.Space, "Key to go straight up");
        
        private static readonly ConfigEntry<KeyCode> GoDown = ConfigFile
            .Bind("Key Binds", "Go Down", KeyCode.LeftControl, "Key to go straight down");
        
        private static readonly ConfigEntry<KeyCode> SpeedUp = ConfigFile
            .Bind("Key Binds", "Speed Up", KeyCode.LeftShift, "Key to speed up movement speed (x2)");
                
        private static readonly ConfigEntry<KeyCode> SlowDown = ConfigFile
            .Bind("Key Binds", "Slow Down", KeyCode.LeftAlt, "Key to slow down movement speed (x1/2)");
        
        private static readonly ConfigEntry<KeyCode> TimeInc = ConfigFile
            .Bind("Key Binds", "Time Increment", KeyCode.E, "Speedup time (only work while in free cam)");
        
        private static readonly ConfigEntry<KeyCode> TimeDec = ConfigFile
            .Bind("Key Binds", "Time Decrement", KeyCode.Q, "Slowdown time (only work while in free cam)");
        
        private static readonly ConfigEntry<KeyCode> TimePausePlay = ConfigFile
            .Bind("Key Binds", "Time Pause/Play", KeyCode.R, "Pause/play time (only work while in free cam)");
        
        public static KeyCode MenuKey => MenuOpenClose.Value;
        public static KeyCode UpKey => GoUp.Value;
        public static KeyCode DownKey => GoDown.Value;
        public static KeyCode SpeedUpKey => SpeedUp.Value;
        public static KeyCode SlowDownKey => SlowDown.Value;
        public static KeyCode TimeIncKey => TimeInc.Value;
        public static KeyCode TimeDecKey => TimeDec.Value;
        public static KeyCode TimePausePlayKey => TimePausePlay.Value;
    }
}