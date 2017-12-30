using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.Mods {
    public class HostedMod : Mod {
        public override Task<ModDownloadResult> Download() {
            throw new NotImplementedException();
        }

        public override ModStatus GetDownloadStatus() {
            throw new NotImplementedException();
        }
    }
}
