using System;
using System.Collections.Generic;
using System.Text;

namespace Modifi.Mods
{
    public enum ModReleaseType {
        Alpha,
        Beta,
        Release,
        Any
    }

    public enum ModStatus {

        /// <summary>
        /// Mod is not requested for a pack.
        /// </summary>
        NotInstalled,

        /// <summary>
        /// Mod is required for the pack.
        /// </summary>
        Requested,

        /// <summary>
        /// Mod is downloaded and installed to mods directory.
        /// </summary>
        Installed,

        /// <summary>
        /// Mod is requested or downloaded, but manually disabled.
        /// </summary>
        Disabled
    }
}
