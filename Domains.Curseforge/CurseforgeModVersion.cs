using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

using Modifi.Mods;
using Modifi.Domains;

namespace Domains.Curseforge {
    public class CurseforgeModVersion : ModVersion {

        public string Name { get; set; }

        public string Version { get; set; }

        [Newtonsoft.Json.JsonProperty("versions")]
        public string[] MinecraftVersions { get; set; }

        /// <summary>
        /// The location of the mod on disk, if it is downloaded.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The type of release the mod is.
        /// </summary>
        public ModReleaseType Type { get; set; }

        [Newtonsoft.Json.JsonProperty("url")]
        public string DownloadURL { get; set; }

        [Newtonsoft.Json.JsonProperty("id")]
        public string FileId { get; set; }

        /// <summary>
        /// An MD5 hash of the file to ensure it downloaded correctly.
        /// </summary>
        public string Checksum { get; internal set; }

        public override ModReleaseType GetReleaseType() {
            return Type;
        }

        public override string GetVersionName() {
            return Name;
        }

        public override string GetModVersion() {
            return FileId;
        }

        public override string GetFilename() {
            return Filename;
        }

        public override string GetChecksum() {
            return Checksum;
        }
    }
}
