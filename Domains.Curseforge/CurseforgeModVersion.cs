using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

using Modifi.Mods;
using Modifi.Domains;

namespace Domains.Curseforge {
    public class CurseforgeModVersion : IModVersion {

        public Guid Id { get; set; }

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
        /// Mod identifier (i.e. jei)
        /// </summary>
        public string ModIdentifier { get; internal set; }


        /// <summary>
        /// An MD5 hash of the file to ensure it downloaded correctly.
        /// </summary>
        public string Checksum { get; internal set; }

        public ModStatus Status { get; internal set; }

        public ModReleaseType GetReleaseType() {
            return Type;
        }

        public string GetVersionName() {
            return Name;
        }

        string IModVersion.GetChecksum() {
            return Checksum;
        }

        string IModVersion.GetFilename() {
            return Filename;
        }

        string IModVersion.GetModIdentifier() {
            return ModIdentifier;
        }

        string IModVersion.GetModVersion() {
            return FileId;
        }
    }
}
