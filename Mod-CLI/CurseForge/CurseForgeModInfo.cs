using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.CurseForge {
    public struct CurseForgeModInfo {
        public int ID;
        public string Title;
        public string Description;
        public Dictionary<String, List<Mods.ModMetadata>> Versions;
        public Mods.ModMetadata Download;
    }
}
