using LiteDB;
using Modifi.Domains;
using Modifi.Mods;
using Modifi.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Modifi.Storage {

    internal class ModStorage : IModStorage, IDisposable {

        protected IDomain Domain;

        // TODO: Make these completely protected, only access via methods
        public LiteDB.LiteDatabase Database { get; protected set; }
        public LiteDB.LiteCollection<ModMetadata> Mods { get; protected set; }
        public LiteDB.LiteCollection<ModVersion> ModVersions { get; protected set; }

        public ModStorage(string version, IDomain domain) {
            string fullPath = Path.Combine(Settings.ModifiDirectory, version + ".db");
            this.Database = new LiteDB.LiteDatabase(fullPath);

            this.Mods = Database.GetCollection<ModMetadata>(domain.GetDomainIdentifier() + "-meta");
            this.ModVersions = Database.GetCollection<ModVersion>(domain.GetDomainIdentifier() + "-versions");

            this.Domain = domain;
        }

        public ModStorage() {
            throw new NotSupportedException();
        }

        public bool ChangeModStatus(ModMetadata meta, ModVersion version, ModStatus status) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            if(this.Database != null)
                this.Database.Dispose();

            this.Mods = null;
            this.ModVersions = null;
        }

        public IEnumerable<ModMetadata> GetAllMods() {
            return Mods.FindAll();
        }

        public ModVersion GetMod(string modIdentifier) {
            ModMetadata meta = Mods.FindOne(x => x.GetModIdentifier() == modIdentifier);
            if (meta == null) return null;

            Guid v = meta.Id;
            return ModVersions.FindOne(x => x.MetadataId == meta.Id);
        }

        public ModVersion GetMod(ModMetadata meta) {
            if (meta == null) return null;
            return GetMod(meta.GetModIdentifier());
        }

        /// <summary>
        /// Gets the installation status for a mod.
        /// </summary>
        /// <param name="meta">Mod to get status for</param>
        /// <returns></returns>
        public ModStatus GetModStatus(ModMetadata meta) {
            ModMetadata installed = this.Mods.FindOne(x => x.GetModIdentifier() == meta.GetModIdentifier());
            if (installed == null) return ModStatus.NotInstalled;

            ModVersion installedVersion = GetMod(meta);

            // If we have a request in meta, but no requested version, error.
            // Delete the meta information and let the user know to re-request the mod.
            // TODO: Make new exception for this
            if (installedVersion == null) {
                this.Mods.Delete(x => x.GetModIdentifier() == installed.GetModIdentifier());
                throw new Exception("Mod metadata was in requests, but no matching version was found with it.");
            }

            // Check filename exists - if not, mod is requested and not yet downloaded
            string filename = installedVersion.GetFilename();
            if (filename == null)
                return ModStatus.Requested;

            // Filename exists- check if properly downloaded
            string path = Path.Combine(Settings.ModPath, filename);
            if(File.Exists(path)) {
                // If our checksum matches, the mod is installed
                if (ModUtilities.ChecksumMatches(path, installedVersion.GetChecksum()))
                    return ModStatus.Installed;
                else {
                    // TODO: Should we remove the mod metadata if the checksum failed here?
                    throw new Exception("Checksum does not match version information.");
                }
            }

            return ModStatus.Requested;
        }

        public bool MarkRequested(ModMetadata meta, ModVersion version) {
            if (GetModStatus(meta) != ModStatus.NotInstalled)
                throw new Exception("Mod is already marked requested, or already installed.");

            Mods.Insert(meta);
            ModVersions.Insert(version);

            return true;
        }

        public bool MarkInstalled(ModMetadata meta, ModVersion version) {
            ModStatus status = GetModStatus(meta);
            switch(status) {
                case ModStatus.Installed:
                    throw new Exception("Mod is already marked installed.");

                case ModStatus.NotInstalled:
                    Mods.Insert(meta);
                    ModVersions.Insert(version);
                    return true;

                // Already added as requested, need to modify it
                case ModStatus.Requested:

                    return true;
            }

            return false;
        }

        public bool Delete(ModMetadata meta) {
            Mods.Delete(meta.Id);
            ModVersions.Delete(x => x.MetadataId == meta.Id);
            return true;
        }

        public ModMetadata GetMetadata(string modIdentifier) {
            return Mods.FindOne(x => x.GetModIdentifier() == modIdentifier);
        }
    }
}
