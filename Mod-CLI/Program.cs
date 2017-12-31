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

using CommandLine;

namespace RobotGryphon.ModCLI {
    public class Program {

        public static JsonSerializerSettings settings = new JsonSerializerSettings() {
            ContractResolver = new DefaultContractResolver() {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public static Pack pack;
        public static Storage.Version version;

        static void Main(string[] args) {

            // Tries to add the latest version of JEI and JourneyMap
            // "modifi mods -a @curseforge::jei @curseforge::journeymap
            string[] argsDebugMods = new string[] {
                "mods", "info", "@curseforge::jei", "@curseforge::journeymap", "@curseforge::waila"
            };

            string[] argsDebugHelp = new string[] { "help" };

            string[] argsDebug = argsDebugMods;

            Tuple<Pack, Storage.Version> t = AssurePackAndVersionFiles();
            pack = t.Item1; version = t.Item2;

            Parser.Default.ParseArguments<ModOptions, DomainOptions, PackOptions>(argsDebug)
                .WithParsed<ModOptions>(DoModManagement);
        }

        private static int NotYetImplemented(Object dop) {
            Console.Error.WriteLine("Error: This is not yet implemented.");
            return 1;
        }

        private static void DoModManagement(ModOptions opts) {
            Console.WriteLine("Switch: {0}", opts.Switch);
            Console.WriteLine("Affecting mods: {0}", String.Join(", ", opts.ModList));

            IEnumerable<string> mods = opts.ModList;

            // TODO: Try and use a second verb for nice outputs and help stuff?
            switch(opts.Switch.ToLowerInvariant()) {
                case "add":
                    // Adding mods

                    break;

                case "remove":
                case "rem":

                    break;

                case "info":
                case "i":
                    foreach(string modid in mods) {
                        Console.WriteLine();
                        Task<ModMetadata> t = ModHelper.GetModInfo(modid);
                        ModMetadata meta = t.Result;

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(meta.Name);
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(meta.Description);

                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Version: {0}\t\tFile ID: {1}", meta.Version, meta.FileId);

                        if(!meta.Versions.Contains(pack.MinecraftVersion)) {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Not compatible with your pack version.");
                        }
                    }

                    Console.ResetColor();

                    break;

                default:
                    Console.Error.WriteLine("Invalid switch, please use { add, remove, or info }.");
                    break;
            }
        }

        private static void ThrowError(IEnumerable<Error> err) {
            Console.Error.WriteLine("Error parsing the given arguments. Please run the help command and check your arguments.");
        }

        static Tuple<Storage.Pack, Storage.Version> AssurePackAndVersionFiles() {

            // Generate new pack file if the modifi directory was not found, or if pack file not found
            if(!Directory.Exists(Settings.ModifiDirectory) || !File.Exists(Settings.PackFile)) GeneratePack();

            // Now deserialize the pack file
            JsonSerializer s = JsonSerializer.Create(settings);
            StreamReader sr = new StreamReader(File.OpenRead(Settings.PackFile));
            Pack p = s.Deserialize<Pack>(new JsonTextReader(sr));

            // Generate new version file if not found
            String versionFile = Path.Combine(Settings.ModifiDirectory, p.Installed + ".json");
            if (!File.Exists(versionFile)) GenerateVersionFile(p.Installed);

            // Deserialize requested version file.
            StreamReader vr = new StreamReader(File.OpenRead(versionFile));
            Storage.Version version = s.Deserialize<Storage.Version>(new JsonTextReader(vr));

            sr.Close();

            return Tuple.Create(p, version);
        }

        private static void GeneratePack() {
            Console.WriteLine("Generating new pack.");

            Directory.CreateDirectory(Settings.ModifiDirectory);

            Console.Write("Please enter a pack name: ");
            String packName = Console.ReadLine();

            Console.Write("Please enter a Minecraft Version: ");
            string version = Console.ReadLine();

            Pack p = new Pack();
            p.Name = packName;
            p.Installed = "1.0.0";
            p.MinecraftVersion = version;

            using(StreamWriter sw = File.CreateText(Settings.PackFile)) {
                JsonSerializer s = JsonSerializer.Create(settings);
                s.Serialize(sw, p);

                Console.WriteLine("Pack file written to {0}.", Settings.PackFile);
            }
        }

        private static void GenerateVersionFile(String version = "1.0.0") {
            Storage.Version v = new Storage.Version();
            v.Mods = new Dictionary<string, ModMetadata>();

            String versionFile = Path.Combine(Settings.ModifiDirectory, version + ".json");
            using (StreamWriter sw = File.CreateText(versionFile)) {
                JsonSerializer s = JsonSerializer.Create(settings);
                s.Serialize(sw, v);

                Console.WriteLine("Version file created at {0}.", versionFile);
            }
        }

        static async Task TryDownload(IMod m) {
            ModDownloadResult dlResult = await m.Download();

            Dictionary<char, String> opts = new Dictionary<char, string>();
            char choice = '*';
            switch (dlResult) {
                case ModDownloadResult.SUCCESS:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write((m.GetMetadata().Name + ":").PadRight(25) + "\t");
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
