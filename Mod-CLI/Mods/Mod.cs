using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.Mods {
    public enum ModStatus {
        NOT_DOWNLOADED,
        DOWNLOADED,
        IN_PROGRESS,
        NOT_FOUND
    }

    public enum ModDownloadResult {
        SUCCESS,
        ERROR_NOT_FOUND,
        ERROR_CONNECTION,
        ERROR_INVALID_FILENAME,
        ERROR_DOWNLOAD_FAILED
    }

    public class Mod : IMod {
        
        /// <summary>
        /// Name of the mod. Actually used for display.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// A unique ID for this file. May be generated.
        /// </summary>
        public String Version { get; set; }

        /// <summary>
        /// A path to the file on its server.
        /// </summary>
        public String Filename { get; set; }

        /// <summary>
        /// Where the mod is hosted. Typically this will be "curseforge".
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// The mod's identifier. This is a short string, such as "jei".
        /// </summary>
        public string ModId { get; set; }

        public string Checksum { get; set; }

        public virtual Task<ModDownloadResult> Download() {
            throw new NotImplementedException();
        }

        public virtual ModStatus GetDownloadStatus() {
            throw new NotImplementedException();
        }
    }
}
