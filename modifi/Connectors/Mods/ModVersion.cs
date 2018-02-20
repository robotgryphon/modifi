using System;

namespace Modifi.Mods {

    /// <summary>
    /// Represents a specific version of a mod.
    /// </summary>
    public abstract class ModVersion {

        public Guid Id { get; set; }
        public Guid MetadataId { get; set; }

        /// <summary>
        /// The location of the file on disk. (Relative path)
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Checksum for the downloaded file.
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// Returns a version-specific name or title.
        /// </summary>
        /// <returns></returns>
        public abstract string GetVersionName();

        public abstract string GetModVersion();

        public abstract ModReleaseType GetReleaseType();        
    }
}