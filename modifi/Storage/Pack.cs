using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Modifi;
using Modifi.Domains;
using Modifi.Mods;
using Newtonsoft.Json;

namespace Modifi.Storage {
    public class Pack : IDisposable {

        /// <summary>
        /// The file this pack data was loaded from.
        /// </summary>
        [JsonIgnore]
        protected string Filename;

        /// <summary>
        /// Pack name.
        /// </summary>
        public string Name;

        /// <summary>
        /// The currently-installed version.
        /// </summary>
        public string Version;

        public string MinecraftVersion;

        public Dictionary<string, string> Mods;

        /// <summary>
        /// File information for installed mods.
        /// </summary>
        public Dictionary<string, string> Files;

        public Pack() { }

        public void Dispose() {
            // Do nothing here unless necessary, it's to make code look a bit nicer if people wanna do using()
        }

        public static Pack Load(string path) {
            if (!File.Exists(path))
                throw new FileNotFoundException("Pack file does not exist. Cannot load.");

            Pack p = new Pack();
            p.Filename = path;

            // Read JSON pack file and copy data into the new pack object
            StreamReader sr = new StreamReader(File.OpenRead(path));
            StorageUtilities.PACK_SERIALIZER.Populate(new JsonTextReader(sr), p);
            sr.Close();

            return p;
        }

        public void SetFilename(string filename) {
            this.Filename = filename;
        }

        public Task Save() {
            if (String.IsNullOrEmpty(this.Filename)) {
                return Task.FromException(new Exception("Cannot save; filename not set. Use SetFilename or SaveAs instead."));
            }

            return SaveAs(this.Filename);
        }

        public Task SaveAs(string filename) {
            // Filename check - if null, try to use the built-in filename (would be set if loaded from a file)
            if (filename == null) {
                if (String.IsNullOrEmpty(this.Filename))
                    return Task.FromException(new Exception("Cannot save, need filename."));
                else
                    filename = this.Filename;
            }

            string directory = Path.GetDirectoryName(filename);
            Directory.CreateDirectory(directory);

            using(StreamWriter sw = File.CreateText(filename)) {

                // Do the file work in the background, don't bother coming back to this afterwards
                try {
                    StorageUtilities.PACK_SERIALIZER.Serialize(sw, this);
                    return Task.CompletedTask;
                } catch (Exception e) {
                    TextWriter err = Console.Error;
                    err.WriteLine("There was an error writing the pack file.");
                    err.WriteLine("Execution will continue, but the pack might not be saved correctly.");
                    err.WriteLine("If you see this, it is best to stop and check if you have the correct file permissions.");
                    return Task.FromException(e);
                }
            }
        }

        public void AddMod(string modString, string version) {
            this.Mods.Add(modString, version);
        }

        public void AddMod(IDomain domain, ModMetadata mod, ModVersion version) {
            string modString = String.Format("{0}:{1}", domain.GetDomainIdentifier(), mod.GetModIdentifier());
            this.Mods.Add(modString, version.GetModVersion());
        }

        public void RemoveMod(string modString) {
            if (this.Mods.ContainsKey(modString)) {
                this.Mods.Remove(modString);
            }
        }

    }
}