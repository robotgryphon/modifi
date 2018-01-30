using Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modifi.Domains {

    public interface IDomainHandler {

        /// <summary>
        /// Fetches information about a mod.
        /// </summary>
        /// <param name="minecraftVersion">The version of Minecraft to fetch mod information on.</param>
        /// <param name="modIdentifier">The mod to fetch information on.</param>
        /// <returns></returns>
        Task<IModMetadata> GetModMetadata(string minecraftVersion, string modIdentifier);

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
        /// <param name="location">The location (directory) to download the mod to. If null, it defaults to the working directory.</param>
        /// <returns></returns>
        Task<ModDownloadResult> DownloadMod(IModVersion version, string location);

        /// <summary>
        /// Gets the most recent versions of a mod.
        /// </summary>
        /// <param name="metadata">The mod to fetch version information for.</param>
        /// <param name="releaseType">Filter releases to this type.</param>
        /// <param name="count">The number of versions to fetch.</param>
        /// <returns></returns>
        Task<IEnumerable<IModVersion>> GetRecentVersions(IModMetadata metadata, int count = 5, Mods.ModReleaseType releaseType = ModReleaseType.Any);

        /// <summary>
        /// Same as GetRecentVersions, but only returns a single version.
        /// </summary>
        /// <param name="metadata">The mod to fetch version information for.</param>
        /// <param name="releaseType">Filter releases to this type.</param>
        /// <returns></returns>
        Task<IModVersion> GetLatestVersion(IModMetadata metadata, ModReleaseType releaseType = ModReleaseType.Any);

        /// <summary>
        /// Gets which version of a mod is installed, if any.
        /// </summary>
        /// <param name="modIdentifier"></param>
        /// <returns></returns>
        // TODO: Extract to generic domain handler?
        // IModVersion GetInstalledModVersion(string modIdentifier);

        /// <summary>
        /// Gets the status of a particular mod version.
        /// </summary>
        /// <param name="version">The version of the mod to check status on.</param>
        /// <returns></returns>
        // TODO: Extract to generic domain handler?
        // ModStatus GetModStatus(IModVersion version);
    }
}
