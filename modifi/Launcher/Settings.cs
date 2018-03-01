using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;

namespace Modifi {
    public class Settings {
        public static string ModPath = Path.Combine(Environment.CurrentDirectory, "mods");

        /// <summary>
        /// The location of the modifi executable.
        /// </summary>
        public static string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
    }
}