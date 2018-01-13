using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;

namespace RobotGryphon.Modifi {
    internal class Settings {
        internal static string ModPath = Path.Combine(Environment.CurrentDirectory, "mods");

        public static String ModifiDirectory = Path.Combine(Environment.CurrentDirectory, ".modifi");

        public static String PackFile = Path.Combine(Settings.ModifiDirectory, "pack.json");

        public static JsonSerializerSettings JsonSerializer = new JsonSerializerSettings() {
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };
    }
}