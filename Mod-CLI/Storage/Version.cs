using RobotGryphon.ModCLI.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.Storage {

    /// <summary>
    /// Represents a saved modpack.
    /// </summary>
    public class Version {

        /// <summary>
        /// A dictionary of requested mods.
        /// The key is an md5hash of format @domain::modid.
        /// </summary>
        public Dictionary<String, ModMetadata> Mods { get; set; }
    }
}
