using System;

namespace Modifi.Mods {

    /// <summary>
    /// Represents a specific version of a mod.
    /// </summary>
    public interface IModVersion {

        /// <summary>
        /// Returns a version-specific name or title.
        /// </summary>
        /// <returns></returns>
        string GetVersionName();

        string GetModIdentifier();

        string GetModVersion();

        /// <summary>
        /// Gets the location of the mod file on disk.
        /// </summary>
        /// <returns></returns>
        string GetFilename();
        string GetChecksum();

        ModReleaseType GetReleaseType();
        
    }
}