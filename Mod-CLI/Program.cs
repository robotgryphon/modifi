using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using RobotGryphon.ModCLI.Mods;
using RobotGryphon.ModCLI.Storage;
using static RobotGryphon.ModCLI.Mods.Mod;

namespace RobotGryphon.ModCLI {
    public class Program {

        public static JsonSerializerSettings settings = new JsonSerializerSettings() {
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        static void Main(string[] args) {

            Run().Wait();
        }

        static async Task Run() {

            if(!Directory.Exists(Settings.ModifiDirectory) || !File.Exists(Settings.PackFile)) {
                // File not found, generate new pack.
                GeneratePack();
            }

            JsonSerializer s = JsonSerializer.Create(settings);
            StreamReader sr = new StreamReader(File.OpenRead(Settings.PackFile));
            Pack p = s.Deserialize<Pack>(new JsonTextReader(sr));

            if(!File.Exists(Path.Combine(Settings.ModifiDirectory, p.Installed + ".json")))
                GenerateVersionFile(p.Installed);

            String latestVersionMods = File.ReadAllText(Path.Combine(Settings.ModifiDirectory, p.Installed + ".json"));
            Storage.Version latestVersion = JsonConvert.DeserializeObject<Storage.Version>(latestVersionMods);


            Console.WriteLine("Version includes {0} domains and {1} mods...", 0, latestVersion.Mods.Count);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Downloading mods...");

            sr.Close();

            // foreach (Mod mod in lf.Mods) await TryDownload(mod);
        }

        private static void GeneratePack() {
            Console.WriteLine("Generating new pack.");

            Directory.CreateDirectory(Settings.ModifiDirectory);

            Console.Write("Please enter a pack name: ");
            String packName = Console.ReadLine();

            Pack p = new Pack();
            p.Name = packName;
            p.Installed = "1.0.0";

            using(StreamWriter sw = File.CreateText(Settings.PackFile)) {
                JsonSerializer s = JsonSerializer.Create(settings);
                s.Serialize(sw, p);

                Console.WriteLine("Pack file written to {0}.", Settings.PackFile);
            }
        }

        private static void GenerateVersionFile(String version = "1.0.0") {
            Storage.Version v = new Storage.Version();
            v.Mods = new Dictionary<string, Mod>();

            String versionFile = Path.Combine(Settings.ModifiDirectory, version + ".json");
            using (StreamWriter sw = File.CreateText(versionFile)) {
                JsonSerializer s = JsonSerializer.Create(settings);
                s.Serialize(sw, v);

                Console.WriteLine("Version file created at {0}.", versionFile);
            }
        }

        static async Task TryDownload(Mod m) {
            ModDownloadResult dlResult = await m.Download();

            Dictionary<char, String> opts = new Dictionary<char, string>();
            char choice = '*';
            switch (dlResult) {
                case ModDownloadResult.SUCCESS:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write((m.Name + ":").PadRight(25) + "\t");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Mod downloaded successfully.");
                    return;

                case ModDownloadResult.ERROR_INVALID_FILENAME:
                case ModDownloadResult.ERROR_CONNECTION:
                    Console.WriteLine("\tThere was an error downloading the mod: INVALID_FILENAME or ERROR_CONNECTION");
                    Console.WriteLine("\tThis may be a result of the download site refusing to connect or being down;");
                    choice = ConsoleMenu.Create()
                        .AddOption('r', "Retry the Download", 0)
                        .AddOption('s', "Skip this mod", 0)
                        .Display(2);

                    switch(choice) {
                        case 'r':
                            // RETRY DOWNLOAD
                            Console.WriteLine("Retrying download.");
                            await TryDownload(m);
                            break;

                        case 's':
                            Console.WriteLine("Continuing without mod.");
                            break;
                    }

                    Console.WriteLine(Environment.NewLine + Environment.NewLine);
                    return;

                default:
                    Console.WriteLine("\tThere was an error fetching the mod: UNKNOWN_ERROR");
                    Console.WriteLine("\tPlease manually add the file into the install directory.");
                    return;
            }
        }
    }

    
}
