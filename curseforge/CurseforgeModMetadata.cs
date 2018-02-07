using Modifi.Domains;
using Modifi.Mods;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains.Curseforge {

    /// <summary>
    /// Used to deserialize Curseforge API information.
    /// </summary>
    public class CurseforgeModMetadata : ModMetadata {

        #region Properties and Fields
        /// <summary>
        /// Curseforge Project identifier.
        /// </summary>
        [JsonProperty("id")]
        public int ProjectId { get; protected set; }

        /// <summary>
        /// The name of the mod on Curseforge.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Project description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// All the available versions for the mod.
        /// </summary>
        public Dictionary<String, IEnumerable<CurseforgeModVersion>> Versions;

        [JsonProperty("download")]
        public CurseforgeModVersion RequestedVersion;

        public CurseForgeURLs URLs { get; internal set; }

        public string ModIdentifier { get; internal set; }

        public string MinecraftVersion { get; internal set; }
        #endregion

        public CurseforgeModMetadata() : base() { }

        public override string GetName() {
            return Title;
        }

        public override string GetModIdentifier() {
            return ModIdentifier;
        }

        public override string GetDescription() {
            return Description;
        }

        public override bool HasDescription() {
            return !String.IsNullOrWhiteSpace(Description);
        }

        public override string GetMinecraftVersion() {
            return MinecraftVersion;
        }
    }

    public struct CurseForgeURLs {
        public string Project;
        public string Curseforge;
    }
}
