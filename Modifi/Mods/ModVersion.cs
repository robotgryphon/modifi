using RobotGryphon.Modifi.Domains;
using System;

namespace RobotGryphon.Modifi.Mods {

    /// <summary>
    /// Represents a specific version of a mod.
    /// </summary>
    public interface IModVersion {

        IDomainHandler GetDomain();

        string GetModIdentifier();

        string GetModVersion();

        /// <summary>
        /// Gets the location of the mod file on disk.
        /// </summary>
        /// <returns></returns>
        string GetFilename();
    }
}