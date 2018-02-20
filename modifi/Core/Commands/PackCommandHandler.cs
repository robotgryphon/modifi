using Modifi.Domains;
using Modifi.Mods;
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
            Help,
            Info,
            New
        }

        internal static void Handle(IEnumerable<string> arguments) {
            if(arguments.Count() == 0) {
                CommandHandler.ListOptions<PackAction>();
                return;
            }

            PackAction? action = CommandHandler.ParseCommandOption<PackAction>(arguments.First());
            if (action == null) action = PackAction.Help;

            Pack pack;
            ILogger log = Modifi.DefaultLogger;

            switch (action) {
                case PackAction.Download:
                    DownloadPack();
                    break;

                case PackAction.New:
                    if (!File.Exists(Modifi.DEFAULT_PACK_PATH)) {
                        
                        Pack p = new Pack();
                        Console.Write("Please enter a pack name: ");
                        p.Name = Console.ReadLine();

                        Console.Write("Please enter a Minecraft Version: ");
                        p.MinecraftVersion = Console.ReadLine();

                        p.Version = "1.0.0";
                        p.SaveAs(Modifi.DEFAULT_PACK_PATH);
                    } else
                        log.Error("Pack file already exists.");
                    break;

                case PackAction.Info:
                    try { pack = Pack.Load(Modifi.DEFAULT_PACK_PATH); }
                    catch(IOException) {
                        Modifi.DefaultLogger.Error("Error loading pack, make sure one is created with {0}.", "pack init");
                        return;
                    }

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    log.Information("{0:l}", pack.Name);
                    log.Information("Built for Minecraft {0:l}", pack.MinecraftVersion);
                    log.Information("");
                    log.Information("Required Domains:");

                    Console.ForegroundColor = ConsoleColor.White;
                    break;

                case PackAction.Help:
                default:
                    CommandHandler.ListOptions<PackAction>();
                    break;
            }
        }

        private static void DownloadPack() {
            try {
                using (Pack p = Pack.Load(Modifi.DEFAULT_PACK_PATH)) {
                    ILogger log = Modifi.DefaultLogger;

                    log.Information("Downloading modpack.");
                    log.Information(Environment.NewLine);

                    IDomain curseforge;
                    try {
                        bool loaded = Modifi.GetInstance().IsDomainLoaded("curseforge");
                        if(loaded) curseforge = Modifi.GetInstance().GetDomain("curseforge");
                        else curseforge = Modifi.GetInstance().LoadDomain("curseforge");
                    }

                    catch(DllNotFoundException) {
                        log.Error("Cannot install mods; curseforge domain handler not found.");
                        return;
                    }
                    
                    IDomainHandler handler = curseforge.GetDomainHandler();
                    using(ModStorage storage = new ModStorage(p.Version, curseforge)) {
                        IEnumerable<ModMetadata> mods = storage.GetAllMods();
                        foreach(ModMetadata mod in mods) {
                            log.Information("Installing: {0:l}", mod.GetName());

                            ModStatus status;
                            try { status = storage.GetModStatus(mod); }
                            catch(Exception) {
                                log.Error("Error: Mod marked installed but checksum did not match.");
                                log.Error("Please use the remove command and re-add it, or download the version manually.");
                                continue;
                            }

                            switch(status) {
                                case ModStatus.Installed:
                                    log.Information("Skipping, already installed.");
                                    log.Information(Environment.NewLine);
                                    continue;

                                case ModStatus.Requested:
                                    ModVersion version = storage.GetMod(mod);
                                    log.Information("Requested Version: {0:l} ({1})", version.GetVersionName(), version.GetModVersion());
                                    try {
                                        ModDownloadDetails result = handler.DownloadMod(version, Settings.ModPath).Result;
                                        storage.MarkInstalled(mod, version, result);

                                        log.Information("Downloaded to {0}.", result.Filename);
                                        log.Information(Environment.NewLine);
                                    }

                                    catch(Exception e) {
                                        log.Error(e.Message);
                                        log.Error(Environment.NewLine);
                                    }

                                    break;
                            }
                        }
                    }
                }
            }

            catch(FileNotFoundException) {
                Modifi.DefaultLogger.Error("Pack file not found. Create one first.");
            }
        }
    }
}