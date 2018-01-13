using System;

namespace RobotGryphon.Modifi.Mods {

    public struct ModVersion {
        public string Domain;
        public string ModId;
        public string Version;

        public override string ToString() {
            return string.Format("{0}:{1} [{2}]", Domain, ModId, String.IsNullOrEmpty(Version) ? "latest" : Version);
        }
    }
}