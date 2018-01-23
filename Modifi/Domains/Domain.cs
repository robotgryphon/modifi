using System.Collections.Generic;

namespace RobotGryphon.Modifi.Domains {

    /// <summary>
    /// Represents a set of servers where mods can be stored at.
    /// Mods are attempted to be fetched from the first server it can access.
    /// </summary>
    public struct Domain {
        public IEnumerable<string> Servers;
    }
}