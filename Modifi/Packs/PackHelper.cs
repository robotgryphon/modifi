using Newtonsoft.Json;
using RobotGryphon.Modifi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using HashidsNet;

namespace RobotGryphon.Modifi.Packs {
    public abstract class PackHelper {

        public static async Task<Pack> GeneratePackFile() {
            Console.WriteLine("Generating new pack.");

            Directory.CreateDirectory(Settings.ModifiDirectory);

            Console.Write("Please enter a pack name: ");
            String packName = Console.ReadLine();

            Console.Write("Please enter a Minecraft Version: ");
            string version = Console.ReadLine();

            Pack p = new Pack();
            p.Name = packName;
            p.Installed = ModifiVersion.VERSION_1;
            p.MinecraftVersion = version;

            using (StreamWriter sw = File.CreateText(Settings.PackFile)) {

                // Do the file work in the background, don't bother coming back to this afterwards
                JsonSerializer s = JsonSerializer.Create(Settings.JsonSerializer);
                await Task.Run(() => s.Serialize(sw, p))
                    .ContinueWith(t => {
                        TextWriter err = Console.Error;
                        err.WriteLine("There was an error writing the pack file.");
                        err.WriteLine("Execution will continue, but the pack might not be saved correctly.");
                        err.WriteLine("If you see this, it is best to stop and check if you have the correct file permissions.");

                        err.WriteLine();

                        err.WriteLine(t.Exception);
                    }, TaskContinuationOptions.OnlyOnFaulted);

                Console.WriteLine("Pack file written to {0}.", Settings.PackFile);
            }

            return p;
        }
    }
}
