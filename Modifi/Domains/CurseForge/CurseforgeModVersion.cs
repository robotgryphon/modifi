using LiteDB;
using RobotGryphon.Modifi.Mods;
using System;

namespace RobotGryphon.Modifi.Domains.CurseForge {
    public class CurseforgeModVersion : IModVersion {

        public Guid Id { get; set; }

        [BsonIgnore]
        public string Name { get; set; }

        [BsonIgnore]
        public string Version { get; set; }

        [Newtonsoft.Json.JsonProperty("versions")]
        [BsonIgnore]
        public string[] MinecraftVersions { get; set; }

        /// <summary>
        /// The location of the mod on disk, if it is downloaded.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// The type of release the mod is.
        /// </summary>
        [BsonIgnore]
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

        IDomainHandler IModVersion.GetDomain() {
            return Modifi.GetDomainHandler("curseforge");
        }

        string IModVersion.GetFilename() {
            return Filename;
        }

        string IModVersion.GetModIdentifier() {
            return ModIdentifier;
        }

        string IModVersion.GetModVersion() {
            return Version;
        }
    }
}
