using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI {
    public abstract class Mod {

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

        public abstract ModStatus GetDownloadStatus();

        /// <summary>
        /// Makes an attempt to fetch the mod from its repository.
        /// </summary>
        /// <returns>Status on how the download went.</returns>
        public abstract Task<ModDownloadResult> Download();
    }
}
