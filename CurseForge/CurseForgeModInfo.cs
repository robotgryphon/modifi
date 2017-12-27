using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.CurseForge {
    public class CurseForgeModInfo {
        public int ID { get; set; }
        public String Title { get; set; }
        public Dictionary<String, List<ModVersion>> Versions { get; set; }
        public ModVersion Download { get; set; }

        public class ModVersion {
            public int ID { get; set; }
            public String URL { get; set; }
            public String Name { get; set; }
            public String Type { get; set; }
            public String Version { get; set; }
        }
    }
}
