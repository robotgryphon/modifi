using System;

namespace RobotGryphon.Modifi.Mods {

    /// <summary>
    /// Represents a specific version of a mod.
    /// </summary>
    public interface IModVersion {

        string GetDomain();

        string GetModIdentifier();

        string GetModVersion();

    }

    public class ModVersionStub : IModVersion {
        public string Domain;
        public string Identifier;
        public string Version;

        public string GetDomain() {
            return Domain;
        }

        public string GetModIdentifier() {
            return Identifier;
        }

        public string GetModVersion() {
            return Version;
        }
    }
}