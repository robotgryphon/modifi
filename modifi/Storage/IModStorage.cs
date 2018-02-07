using Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Text;

namespace Modifi.Storage {

    /// <summary>
    /// Represents storage for a set of domains.
    /// Handles domain mod installations.
    /// </summary>
    public interface IModStorage : IDisposable {

        IEnumerable<ModMetadata> GetAllMods();

        /// <summary>
        /// Marks a mod as requested, so the pack can download it when ready.
        /// </summary>
        /// <param name="meta">The mod to request.</param>
        /// <param name="version">The version to request.</param>
        /// <returns>True if successfully requested, false otherwise.</returns>
        bool MarkRequested(ModMetadata meta, ModVersion version);

        bool MarkInstalled(ModMetadata meta, ModVersion version, ModDownloadResult downloadDetails);

        /// <summary>
        /// Removes a mod from the database, making it "uninstalled".
        /// This does not remove the file from the filesystem.
        /// </summary>
        /// <param name="meta">The mod to remove information for.</param>
        /// <returns></returns>
        bool Delete(ModMetadata meta);

        /// <summary>
        /// Gets the install information for a mod.
        /// </summary>
        /// <param name="modIdentifier">The modid to fetch metadata for.</param>
        /// <returns></returns>
        ModMetadata GetMetadata(string modIdentifier);

        /// <summary>
        /// Gets version information for a mod, if it is part of the pack.
        /// </summary>
        /// <param name="modIdentifier">The mod to request metadata for.</param>
        /// <returns>Mod information if it is part of the pack, otherwise null.</returns>
        ModVersion GetMod(string modIdentifier);

        /// <summary>
        /// Gets version information for a mod, if it is part of the pack.
        /// </summary>
        /// <param name="meta">Metadata for the mod.</param>
        /// <returns>Mod information if it is part of the pack, otherwise null.</returns>
        ModVersion GetMod(ModMetadata meta);

        ModStatus GetModStatus(ModMetadata meta);
    }
}
