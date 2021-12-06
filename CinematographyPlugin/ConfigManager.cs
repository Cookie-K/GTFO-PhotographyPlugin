using System.IO;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace CinematographyPlugin
{
    public static class ConfigManager
    {
        private static readonly ConfigFile ConfigFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "CinematographyPlugin.cfg"), true);

        private static readonly ConfigEntry<KeyCode> MenuOpenClose = ConfigFile
            .Bind("Key Binds", "Menu Open Close", KeyCode.F4, "The key to open / close the plugin menu");

        private static readonly ConfigEntry<KeyCode> GoUp = ConfigFile
            .Bind("Key Binds", "Go Up", KeyCode.Space, "Key to go straight up");
        
        private static readonly ConfigEntry<KeyCode> GoDown = ConfigFile
            .Bind("Key Binds", "Go Down", KeyCode.LeftControl, "Key to go straight down");
        
        private static readonly ConfigEntry<KeyCode> SpeedUp = ConfigFile
            .Bind("Key Binds", "Speed Up", KeyCode.LeftShift, "Key to speed up movement speed (x2)");
                
        private static readonly ConfigEntry<KeyCode> SlowDown = ConfigFile
            .Bind("Key Binds", "Slow Down", KeyCode.LeftAlt, "Key to slow down movement speed (x1/2)");
        
        public static KeyCode MenuKey => MenuOpenClose.Value;
        public static KeyCode UpKey => GoUp.Value;
        public static KeyCode DownKey => GoDown.Value;
        public static KeyCode SpeedUpKey => SpeedUp.Value;
        public static KeyCode SlowDownKey => SlowDown.Value;
    }
}