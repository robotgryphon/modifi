using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Mods {
    public class ModDownloadException : Exception {
        private ModDownloadResult errorType;

        public ModDownloadException(ModDownloadResult e) {
            this.errorType = e;
        }
    }
}
