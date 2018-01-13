using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Mods {

    public interface IModMetadata {

        /// <summary>
        /// Name of the mod. Actually used for display.
        /// </summary>
        string GetName();

        /// <summary>
        /// A unique ID for this file. May be generated.
        /// </summary>
        string GetModVersion();

        /// <summary>
        /// Where the mod is hosted. Typically this will be "curseforge".
        /// </summary>
        string GetDomain();

        /// <summary>
        /// The mod's identifier. This is a short string, such as "jei".
        /// </summary>
        string GetModId();

        string GetDescription();
        bool HasDescription();
    }

    public class BaseModMetadata : IModMetadata {

        string Name { get; set; }
        string ModId { get; set; }
        string ModVersion { get; set; }

        public string GetDescription() {
            return "";
        }

        public string GetDomain() {
            throw new NotImplementedException();
        }

        public string GetModId() {
            return ModId;
        }

        public string GetModVersion() {
            return ModVersion;
        }

        public string GetName() {
            return Name;
        }

        public bool HasDescription() {
            return false;
        }
    }
}
