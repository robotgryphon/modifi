using Modifi.Domains;
using Modifi.Mods;
using Modifi.Packs;
using Modifi.Storage;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Modifi.Commands {
    internal class PackCommandHandler {

        private enum PackAction {
            Download,
            Update,
            Info,
            Init
        }

        internal static void Handle(IEnumerable<string> arguments) {
            PackAction? action = CommandHandler.ParseCommandOption<PackAction>(arguments.First());
            if (action == null) return;


            Pack pack;
            ILogger log = Modifi.DefaultLogger;

            switch (action) {
                case PackAction.Download:
                    log.Information("Downloading modpack.");
                    DownloadPack();
                    break;

                case PackAction.Init:
                    if (!PackHelper.PackExists())
                        PackHelper.GeneratePackFile().Wait();
                    else
                        log.Error("Pack file already exists.");
                    break;

                case PackAction.Info:
                    pack = Modifi.DefaultPack;

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    log.Information("{0:l}", pack.Name);
                    log.Information("Built for Minecraft {0:l}", pack.MinecraftVersion);
                    log.Information("");
                    log.Information("Required Domains:");

                    Console.ForegroundColor = ConsoleColor.White;
                    foreach(string domain in pack.UseDomains)
                        log.Information(" - {0:l} ({1:l})", domain, Path.Combine(Settings.DomainsDirectory, domain + ".dll"));
                    break;

                default:
                    Console.Error.WriteLine("Other pack actions are not yet supported.");
                    break;
            }
        }

        private static void DownloadPack() {
            using (Pack p = Modifi.DefaultPack) {
                ILogger log = Modifi.DefaultLogger;

                log.Information("Downloading modpack.");

                IDomain curseforge = DomainHelper.LoadDomain(p, "curseforge");
                using(ModStorage storage = new ModStorage(p.Installed, curseforge)) {
                    IEnumerable<ModMetadata> mods = storage.GetAllMods();
                    foreach(ModMetadata mod in mods) {
                        log.Information("Installing: {0:l}", mod.GetName());
                    }
                }
            }
        }
    }
}