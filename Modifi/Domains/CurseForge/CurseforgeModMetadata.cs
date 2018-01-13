using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Domains.CurseForge {
    public struct CurseforgeModMetadata : Mods.IModMetadata {
        public int ID;
        public string Title;
        public string Description;
        public Dictionary<String, IEnumerable<CurseforgeModVersion>> Versions;

        [Newtonsoft.Json.JsonProperty("download")]
        public CurseforgeModVersion RequestedVersion;

        public CurseForgeURLs URLs;

        #region IModMetadata
        public string GetName() {
            return Title;
        }

        public string GetModVersion() {
            return RequestedVersion.FileId;
        }

        public string GetDomain() {
            return "curseforge";
        }

        public string GetModId() {
            return RequestedVersion.ModID;
        }

        public string GetDescription() {
            return Description;
        }

        public bool HasDescription() {
            return !String.IsNullOrWhiteSpace(Description);
        }

        #endregion
    }

    public struct CurseForgeURLs {
        public string Project;
        public string Curseforge;
    }

    public class CurseforgeModVersion {
        
        public string Name { get; set; }
        public string Version { get; set; }

        [Newtonsoft.Json.JsonProperty("versions")]
        public string[] MinecraftVersions { get; set; }

        public string ModID { get; set; }

        /// <summary>
        /// The type of release the mod is.
        /// </summary>
        public ModReleaseType Type { get; set; }

        [Newtonsoft.Json.JsonProperty("url")]
        public string DownloadURL { get; set; }

        [Newtonsoft.Json.JsonProperty("id")]
        public string FileId { get; set; }
    }
}
