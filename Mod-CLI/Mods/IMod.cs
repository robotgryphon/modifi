using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.Mods {
    public interface IMod {
        ModStatus GetDownloadStatus();

        /// <summary>
        /// Makes an attempt to fetch the mod from its repository.
        /// </summary>
        /// <returns>Status on how the download went.</returns>
        Task<ModDownloadResult> Download();
    }
}
