using System;

namespace Modifi.Mods {

    /// <summary>
    /// Represents a specific version of a mod.
    /// </summary>
    public abstract class ModVersion {

        public Guid Id { get; set; }
        public Guid MetadataId { get; set; }

        /// <summary>
        /// Returns a version-specific name or title.
        /// </summary>
        /// <returns></returns>
        public abstract string GetVersionName();

        public abstract string GetModVersion();

        /// <summary>
        /// Gets the location of the mod file on disk.
        /// </summary>
        /// <returns></returns>
        public abstract string GetFilename();

        /// <summary>
        /// Gets a checksum for the mod, to check if it has downloaded correctly.
        /// </summary>
        /// <returns></returns>
        public abstract string GetChecksum();

        public abstract ModReleaseType GetReleaseType();        
    }
}