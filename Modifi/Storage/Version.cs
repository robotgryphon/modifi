using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Storage {

    /// <summary>
    /// Represents a saved modpack version, which includes the mod versions.
    /// </summary>
    public class Version {

        /// <summary>
        /// A dictionary of requested mods.
        /// The key is an md5hash of format @domain::modid.
        /// </summary>
        public IEnumerable<BaseModMetadata> Mods { get; set; }
    }
}
