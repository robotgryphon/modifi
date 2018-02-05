using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using HashidsNet;
using Modifi;

namespace Modifi.Packs {
    public abstract class PackHelper {

        public static Task<Pack> GeneratePackFile() {

            if(File.Exists(Settings.PackFile)) {
                throw new Exception("Pack file already exists.");
            }

            Console.WriteLine("Generating new pack.");

            Directory.CreateDirectory(Settings.ModifiDirectory);

            Console.Write("Please enter a pack name: ");
            String packName = Console.ReadLine();

            Console.Write("Please enter a Minecraft Version: ");
            string version = Console.ReadLine();

            Pack p = new Pack();
            p.Name = packName;
            p.Installed = ModifiVersionNumber.VERSION_1;
            p.MinecraftVersion = version;

            Directory.CreateDirectory(Settings.ModifiDirectory);
            using (StreamWriter sw = File.CreateText(Settings.PackFile)) {

                // Do the file work in the background, don't bother coming back to this afterwards
                JsonSerializer s = JsonSerializer.Create(Settings.JsonSerializer);
                try {
                    s.Serialize(sw, p);
                    Console.WriteLine("Pack file written to {0}.", Settings.PackFile);
                }

                catch (Exception) {
                    TextWriter err = Console.Error;
                    err.WriteLine("There was an error writing the pack file.");
                    err.WriteLine("Execution will continue, but the pack might not be saved correctly.");
                    err.WriteLine("If you see this, it is best to stop and check if you have the correct file permissions.");
                }   
            }

            return Task.FromResult(p);
        }

        public static bool PackExists() {
            return File.Exists(Settings.PackFile);
        }
    }
}
