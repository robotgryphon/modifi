using System;
using System.IO;

namespace RobotGryphon.ModCLI {
    internal class Settings {
        internal static string ModPath = Path.Combine(Environment.CurrentDirectory, "mods");

        public static String ModifiDirectory = Path.Combine(Environment.CurrentDirectory, ".modifi");

        public static String PackFile = Path.Combine(Settings.ModifiDirectory, "pack.json");
    }
}