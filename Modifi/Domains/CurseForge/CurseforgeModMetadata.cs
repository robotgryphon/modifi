using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Domains.CurseForge {

    /// <summary>
    /// Used to deserialize Curseforge API information.
    /// </summary>
    public struct CurseforgeModMetadata : Mods.IModMetadata {
        public int ID;
        public string Title;
        public string Description;
        public Dictionary<String, IEnumerable<CurseforgeModVersion>> Versions;

        [Newtonsoft.Json.JsonProperty("download")]
        public CurseforgeModVersion RequestedVersion;

        public CurseForgeURLs URLs;

        public string ModIdentifier { get; internal set; }

        #region IModMetadata
        string IModMetadata.GetName() {
            return Title;
        }

        IDomainHandler IModMetadata.GetDomain() {
            return Modifi.GetDomainHandler("curseforge");
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

        #endregion
    }

    public struct CurseForgeURLs {
        public string Project;
        public string Curseforge;
    }
}
