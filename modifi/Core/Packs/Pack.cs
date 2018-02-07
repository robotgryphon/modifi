using System;
using System.IO;
using System.Collections.Generic;
using Modifi.Domains;
using Modifi.Mods;
using Newtonsoft.Json;

namespace Modifi.Packs {
    public class Pack : IDisposable {

        public string Name;

        /// <summary>
        /// The currently-installed version.
        /// </summary>
        public string Version;
    
        public string MinecraftVersion;

        public Dictionary<string, string> Mods;

        /// <summary>
        /// File information for installed mods.
        /// </summary>
        public Dictionary<string, string> Files;

        public Pack() {
            
        }

        public static ModDownloadDetails ParseDownloadEntry(string entry) {
            byte[] actual = Convert.FromBase64String(entry);
            MemoryStream stream = new MemoryStream(actual);
            BinaryReader b = new BinaryReader(stream);

            ModDownloadDetails details = new ModDownloadDetails();
            details.Filename = b.ReadString();
            details.Checksum = b.ReadString();
            
            return details;
        }

        public void Dispose() {
            // Do nothing here unless necessary, it's to make code look a bit nicer if people wanna do using()
        }
    }
}
