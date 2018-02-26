using Modifi.Domains;
using Modifi.Mods;
using Modifi.Storage;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using CommandDotNet.Attributes;

namespace Modifi.Commands {

    [ApplicationMetadata(Name = "pack", Description = "Manages installed modpacks.")]
    internal class PackCommands {

        public void New([Option(ShortName = "f")] string filename = "pack") {
            string packFilename = Path.Combine(Environment.CurrentDirectory, filename + ".json");

            if(File.Exists(packFilename)) {
                Modifi.DefaultLogger.Error("Pack file already exists.");
                return;
            }

            Storage.Pack p = new Storage.Pack();
            Console.Write("Please enter a pack name: ");
            p.Name = Console.ReadLine();

            Console.Write("Please enter a Minecraft Version: ");
            p.MinecraftVersion = Console.ReadLine();

            p.Version = "1.0.0";
            p.SaveAs(packFilename);
        }

        public void Info() {
            Pack pack;
            ILogger log = Modifi.DefaultLogger;

            try { pack = Storage.Pack.Load(Modifi.DEFAULT_PACK_PATH); }
            catch(IOException) {
                Modifi.DefaultLogger.Error("Error loading pack, make sure one is created with {0}.", "pack init");
                return;
            }

            log.Information("{0:l}", pack.Name);
            log.Information("Built for Minecraft {0:l}", pack.MinecraftVersion);
            log.Information("");
            
            log.Information("Installed Mods:");
            foreach(string mod in pack.GetInstalledMods()) {
                ModDownloadDetails details = pack.GetDownloadDetails(mod);
                log.Information("{0:l} downloaded to {1:l}", mod.PadRight(32), details.Filename);
            }
        }

        public async Task Download([Option(ShortName = "p", LongName = "pack")] string packName = "pack") {
            try {
                using (Storage.Pack pack = Storage.Pack.Load(packName)) {
                    ILogger log = Modifi.DefaultLogger;

                    log.Information("Downloading modpack {0}.", pack.Name);
                    log.Information(Environment.NewLine);
                    
                    IEnumerable<string> requested = pack.GetRequestedMods();
                    IEnumerable<string> installed = pack.GetInstalledMods();

                    IEnumerable<string> stillNeeded = requested.Except(installed);
                    
                    foreach(string modString in stillNeeded) {
                        string domainName = ModHelper.GetDomainName(modString);
                        string modIdentifier = ModHelper.GetModIdentifier(modString);
                        string version = pack.GetRequestedVersion(modString);

                        IDomain modDomain = Modifi.DomainHandler.GetDomain(domainName);

                        if(modDomain == null) {
                            Modifi.DefaultLogger.Error("Could not download {0}; the domain handler was not found.");
                            continue;
                        }

                        IDomainHandler handler = modDomain.GetDomainHandler();
                        ModMetadata meta = await handler.GetModMetadata(pack.MinecraftVersion, modIdentifier);
                        ModVersion modVersion = await handler.GetModVersion(meta, version);
                        
                        ModDownloadDetails download = await handler.DownloadMod(modVersion, Settings.ModPath);
                        pack.MarkInstalled(modString, download);
                    }

                    await pack.Save();
                }
            }

            catch(FileNotFoundException) {
                Modifi.DefaultLogger.Error("Pack file not found. Create one first.");
            }
        }
    }
}