using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Mods {
    public interface IMod {

        /// <summary>
        /// Makes an attempt to fetch the mod from its repository.
        /// </summary>
        /// <returns>Status on how the download went.</returns>
        Task<ModDownloadResult> Download();

        IModMetadata GetMetadata();
        string GetName();
    }
}
