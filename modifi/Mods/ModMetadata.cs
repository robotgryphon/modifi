using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modifi.Mods {

    /// <summary>
    /// Represents a mod as a whole.
    /// </summary>
    public abstract class ModMetadata {

        public Guid Id { get; set; }

        /// <summary>
        /// Name of the mod. Actually used for display.
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// The mod's identifier. This is a short string, such as "jei".
        /// </summary>
        public abstract string GetModIdentifier();

        /// <summary>
        /// Gets a description of the mod.
        /// </summary>
        public abstract string GetDescription();

        /// <summary>
        /// Tells whether or not the mod has a description.
        /// </summary>
        public abstract bool HasDescription();

        /// <summary>
        /// Gets the version of Minecraft this mod is running on.
        /// </summary>
        public abstract string GetMinecraftVersion();

        public ModMetadata() {
            this.Id = Guid.NewGuid();
        }

    }
}
