using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.CurseForge {
    public struct CurseForgeModInfo {
        public int ID;
        public String Title;
        public Dictionary<String, List<ModVersion>> Versions;
        public ModVersion Download;

        public struct ModVersion {
            public int ID;
            public String URL;
            public String Name;
            public String Type;
            public String Version;
        }
    }
}
