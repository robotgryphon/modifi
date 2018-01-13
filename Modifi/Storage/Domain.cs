using System.Collections.Generic;

namespace RobotGryphon.Modifi.Storage {

    /// <summary>
    /// Represents a set of servers where mods can be stored at.
    /// Mods are attempted to be fetched from the first server it can access.
    /// </summary>
    public struct Domain {
        public IEnumerable<string> Servers;
    }
}