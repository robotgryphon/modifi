using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RobotGryphon.ModCLI.CurseForge;

namespace RobotGryphon.ModCLI {
    public class LockFile {
        public string Name { get; set; }
        public string Author { get; set; }
        public DateTime Generated { get; set; }

        public ModListing Mods { get; set; }

        public class ModListing {
            public List<Mod> Hosted { get; set; }
            public List<CurseForgeMod> Curseforge { get; set; }
        }
    }
}
