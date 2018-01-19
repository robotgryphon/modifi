using RobotGryphon.Modifi.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Mods {

    /// <summary>
    /// Represents a mod as a whole.
    /// </summary>
    public interface IModMetadata {

        /// <summary>
        /// Name of the mod. Actually used for display.
        /// </summary>
        string GetName();

        /// <summary>
        /// Where the mod is hosted. Typically this will be "curseforge".
        /// </summary>
        IDomainHandler GetDomain();

        /// <summary>
        /// The mod's identifier. This is a short string, such as "jei".
        /// </summary>
        string GetModIdentifier();

        /// <summary>
        /// Gets a description of the mod.
        /// </summary>
        /// <returns></returns>
        string GetDescription();

        /// <summary>
        /// Tells whether or not the mod has a description.
        /// </summary>
        /// <returns></returns>
        bool HasDescription();
    }
}
