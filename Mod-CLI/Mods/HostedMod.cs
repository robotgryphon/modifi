using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.Mods {
    public class HostedMod : IMod {

        public ModMetadata Metadata;

        public Task<ModDownloadResult> Download() {
            throw new NotImplementedException();
        }

        public ModStatus GetDownloadStatus() {
            throw new NotImplementedException();
        }

        public ModMetadata GetMetadata() {
            return this.Metadata;
        }
    }
}
