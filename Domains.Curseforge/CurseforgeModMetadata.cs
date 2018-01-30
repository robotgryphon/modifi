using Modifi.Domains;
using Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains.Curseforge {

    /// <summary>
    /// Used to deserialize Curseforge API information.
    /// </summary>
    public struct CurseforgeModMetadata : IModMetadata {
        public int ID;
        public string Title;
        public string Description;
        public Dictionary<String, IEnumerable<CurseforgeModVersion>> Versions;

        [Newtonsoft.Json.JsonProperty("download")]
        public CurseforgeModVersion RequestedVersion;

        public CurseForgeURLs URLs;

        public string ModIdentifier { get; internal set; }

        public string MinecraftVersion { get; internal set; }

        #region IModMetadata
        string IModMetadata.GetName() {
            return Title;
        }

        string IModMetadata.GetModIdentifier() {
            return ModIdentifier;
        }

        string IModMetadata.GetDescription() {
            return Description;
        }

        bool IModMetadata.HasDescription() {
            return !String.IsNullOrWhiteSpace(Description);
        }

        public IEnumerable<IModVersion> GetMostRecentVersions() {
            if (String.IsNullOrEmpty(this.MinecraftVersion)) 
                throw new ArgumentException("Cannot fetch versions if the Minecraft version is not specified.");

            if (Versions.ContainsKey(this.MinecraftVersion))
                return Versions[this.MinecraftVersion];

            return null;
        }

        public string GetMinecraftVersion() {
            return MinecraftVersion;
        }

        #endregion
    }

    public struct CurseForgeURLs {
        public string Project;
        public string Curseforge;
    }
}
