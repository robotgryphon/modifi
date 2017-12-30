using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.Storage {
    public struct Pack {
        public String Name;

        /// <summary>
        /// The currently-installed version.
        /// </summary>
        public String Installed;

        public Dictionary<String, Domain> Domains;
    }
}
