using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;

namespace Modifi {
    public class Settings {
        public static string ModPath = Path.Combine(Environment.CurrentDirectory, "mods");

        /// <summary>
        /// The modifi directory inside the directory the command is run inside.
        /// </summary>
        public static String ModifiDirectory = Path.Combine(Environment.CurrentDirectory, ".modifi");

        public static String PackFile = Path.Combine(Settings.ModifiDirectory, "pack.json");

        public static JsonSerializerSettings JsonSerializer = new JsonSerializerSettings() {
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        /// <summary>
        /// The location of the modifi executable.
        /// </summary>
        public static string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
    }
}