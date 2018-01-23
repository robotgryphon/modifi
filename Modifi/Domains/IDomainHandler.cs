using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Domains {

    public interface IDomainCommandHandler {

        void HandleModAdd(string modIdentifier, string modVersion = null);

        void HandleModRemove(string modIdentifier, string modVersion = null);

        void HandleModInformation(string modIdentifier, string modVersion = null);

        void HandleModVersions(string modIdentifier);

        ModDownloadResult? HandleModDownload(string modIdentifier, string modVersion = null);

        Task PerformPackDownload();
    }

    public interface IDomainHandler {

        /// <summary>
        /// Fetches information about a mod.
        /// </summary>
        /// <param name="modIdentifier">The mod to fetch information on.</param>
        /// <returns></returns>
        Task<IModMetadata> GetModMetadata(string modIdentifier);

        /// <summary>
        /// Fetches a specific version of a mod from the mod's metadata.
        /// </summary>
        /// <param name="metadata">Mod to fetch version from.</param>
        /// <param name="version">The version of the mod to fetch.</param>
        /// <returns></returns>
        Task<IModVersion> GetModVersion(IModMetadata metadata, string version);

        /// <summary>
        /// Downloads a specific version of a mod.
        /// </summary>
        /// <param name="version">The version to download.</param>
        /// <returns></returns>
        Task<ModDownloadResult> DownloadMod(IModVersion version);

        /// <summary>
        /// Gets which version of a mod is installed, if any.
        /// </summary>
        /// <param name="modIdentifier"></param>
        /// <returns></returns>
        IModVersion GetInstalledModVersion(string modIdentifier);

        /// <summary>
        /// Gets the status of a particular mod version.
        /// </summary>
        /// <param name="version">The version of the mod to check status on.</param>
        /// <returns></returns>
        ModStatus GetModStatus(IModVersion version);
    }
}
