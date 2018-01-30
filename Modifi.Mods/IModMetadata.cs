using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modifi.Mods {

    /// <summary>
    /// Represents a mod as a whole.
    /// </summary>
    public interface IModMetadata {

        /// <summary>
        /// Name of the mod. Actually used for display.
        /// </summary>
        string GetName();

        /// <summary>
        /// The mod's identifier. This is a short string, such as "jei".
        /// </summary>
        string GetModIdentifier();

        /// <summary>
        /// Gets a description of the mod.
        /// </summary>
        string GetDescription();

        /// <summary>
        /// Tells whether or not the mod has a description.
        /// </summary>
        bool HasDescription();

        /// <summary>
        /// Gets the version of Minecraft this mod is running on.
        /// </summary>
        string GetMinecraftVersion();

        /// <summary>
        /// Gets the most recent versions of the mod.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IModVersion> GetMostRecentVersions();
    }
}
