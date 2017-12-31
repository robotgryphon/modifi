using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.Storage {
    public struct Pack {

        public string Name;

        /// <summary>
        /// The currently-installed version.
        /// </summary>
        public string Installed;

        public Dictionary<String, Domain> Domains;

        public string MinecraftVersion;
    }
}
