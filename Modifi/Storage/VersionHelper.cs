using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Storage {

    public abstract class VersionHelper {

        public static async Task<Storage.Version> GenerateVersionFile(string version = "1.0.0") {
            Storage.Version v = new Storage.Version {
                Mods = new Mods.BaseModMetadata[0]
            };

            String versionFile = Path.Combine(Settings.ModifiDirectory, version + ".json");
            using (StreamWriter sw = File.CreateText(versionFile)) {

                // Do the file work in the background, don't bother coming back to this afterwards
                JsonSerializer s = JsonSerializer.Create(Settings.JsonSerializer);
                await Task.Run(() => s.Serialize(sw, v))
                    .ContinueWith(t => {
                        TextWriter err = Console.Error;
                        err.WriteLine("There was an error writing out the new version file at {0}", versionFile);
                        err.WriteLine("The program will continue execution, but it is likely things will not be saved correctly.");
                        err.WriteLine("Please make sure you have the correct file permissions and try again.");

                        err.WriteLine();
                        err.WriteLine(t.Exception);
                    }, TaskContinuationOptions.OnlyOnFaulted);

                Console.WriteLine("Version file created at {0}.", versionFile);
            }

            return v;
        }
    }
}
