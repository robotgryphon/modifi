using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static RobotGryphon.ModCLI.Mod;

namespace RobotGryphon.ModCLI {
    public class Program {
        static void Main(string[] args) {

            Run().Wait();
        }

        static async Task Run() {
            String lockData = File.ReadAllText(Environment.CurrentDirectory + "/lock.json");

            DefaultContractResolver cr = new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() };
            LockFile lf = JsonConvert.DeserializeObject<LockFile>(lockData, new JsonSerializerSettings() { ContractResolver = cr });

            Console.WriteLine(lf.Name);
            Console.WriteLine("Lockfile includes {0} curseforge mods and {1} hosted mods...", lf.Mods.Curseforge.Count, lf.Mods.Hosted.Count);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Downloading CurseForge mods...");
            foreach (CurseForge.CurseForgeMod mod in lf.Mods.Curseforge)
                await TryDownload(mod);
        }

        static async Task TryDownload(CurseForge.CurseForgeMod m) {
            Mod.ModDownloadResult dlResult = await m.Download();

            Dictionary<char, String> opts = new Dictionary<char, string>();
            char choice = '*';
            switch (dlResult) {
                case ModDownloadResult.SUCCESS:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write((m.ModInfo.Title + ":").PadRight(25) + "\t");
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
