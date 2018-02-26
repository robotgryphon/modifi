using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandDotNet;
using CommandDotNet.Attributes;
using Modifi.Domains;
using Modifi.Mods;
using Modifi.Storage;
using Serilog;

namespace Modifi.Commands {

    public class ModArguments : IArgumentModel {

        [Argument(Description = "Mod strings in the format 'domain:modid[@version]'. For example, curseforge:jei@100000.")]
        public List<string> modStrings {
            get;
            set;
        }

        [Option(ShortName = "p", LongName = "pack")]
        public string packName { get; set; } = "pack";
    }

    [ApplicationMetadata(Name = "mods", Description = "Manages mods in the pack.")]
    public class ModsCommand {

        /// <summary>
        /// Handles the mods versions {modid} command.
        /// </summary>
        /// <param name="domain">Domain handler to use for lookup.</param>
        /// <param name="modIdentifier">Mod to lookup versions for.</param>
        [ApplicationMetadata(Description = "Gets the latest versions of a mod.")]
        public void Versions(
            List<string> modStrings, 
            [Option(Description = "Minecraft version to pull information for.", LongName = "mc-version", ShortName = "m")] string mcversion = "1.12.2", 
            [Option(ShortName = "c", LongName = "count")] int num = 5) {

            if (modStrings == null || modStrings.Count == 0) {
                Modifi.DefaultLogger.Error("Nothing to do; no mods were defined. Missing an argument?");
                return;
            }

            foreach (string modString in modStrings) {
                string domainName = ModHelper.GetDomainName(modString);
                string modIdentifier = ModHelper.GetModIdentifier(modString);
                string modVersion = ModHelper.GetModVersion(modString);

                IDomain domain = Modifi.DomainHandler.GetDomain(domainName);

                // TODO: Show error here?
                if (domain == null) return;
                IDomainHandler handler = domain.GetDomainHandler();

                ModMetadata meta = handler.GetModMetadata(mcversion, modIdentifier).Result;

                IEnumerable<ModVersion> latestVersions = handler.GetRecentVersions(meta, num).Result;

                // ModHelper.PrintModInformation(meta);
                foreach (ModVersion version in latestVersions) {
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(version.GetModVersion());
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("] ");

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(version.GetVersionName());

                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" (");
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(version.GetReleaseType());
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(")");
                    Console.ResetColor();
                    Console.WriteLine();
                }
            }
        }

        [ApplicationMetadata(Description = "Adds mods to the modpack.")]
        public void Add(ModArguments arguments) {

            if (arguments.modStrings == null || arguments.modStrings.Count == 0) {
                Modifi.DefaultLogger.Error("Nothing to do; no mods were defined. Missing an argument?");
                return;
            }

            Storage.Pack pack = Storage.Pack.Load(arguments.packName);
            foreach (string modString in arguments.modStrings) {
                string domainName = ModHelper.GetDomainName(modString);
                string modIdentifier = ModHelper.GetModIdentifier(modString);
                string modVersion = ModHelper.GetModVersion(modString);

                IDomain domain = Modifi.DomainHandler.GetDomain(domainName);

                // TODO: Show error here?
                if (domain == null)return;
                IDomainHandler handler = domain.GetDomainHandler();

                ModMetadata meta = handler.GetModMetadata(pack.MinecraftVersion, modIdentifier).Result;

                // Check mod installation status, error out if already requested/installed
                ModStatus status = pack.GetModStatus(modString);
                switch (status) {
                    case ModStatus.Requested:
                    case ModStatus.Installed:
                        Modifi.DefaultLogger.Error("Mod {0} is already marked as requested, or already installed.", meta.GetName());
                        return;

                    case ModStatus.Disabled:
                        Modifi.DefaultLogger.Information("Mod {0} is marked at disabled. Please either delete it, or re-enable it.");
                        return;
                }

                // If the version is already specified, don't ask, just add it
                if (!String.IsNullOrEmpty(modVersion)) {
                    ModVersion v = handler.GetModVersion(meta, modVersion).Result;
                    pack.AddMod(domain, meta, v);
                    pack.Save();
                }

                // ============================================================
                // TODO: ModHelper.PrintModInformation(meta);

                Menu<ModVersion> menu = new Menu<ModVersion>();

                menu.OptionFormatter = (opt)=> {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(opt.GetVersionName());
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write(" [");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(opt.GetModVersion());
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.Write("]");

                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(" [{0}]", opt.GetReleaseType());
                    Console.WriteLine();
                };

                ModVersion latestRelease = handler.GetRecentVersions(meta, 1, ModReleaseType.Release).Result.First();
                ModVersion latestVersion = handler.GetRecentVersions(meta, 1, ModReleaseType.Any).Result.First();

                IEnumerable<ModVersion> versions = handler.GetRecentVersions(meta, 6, ModReleaseType.Any).Result.Skip(1);

                menu.AddItem(latestRelease);
                if (latestVersion.GetModVersion()!= latestRelease.GetModVersion())
                    menu.AddItem(latestVersion);

                menu.AddSpacer();
                foreach (ModVersion v in versions.Take(5))menu.AddItem(v);

                menu.DrawMenu();

                Console.ResetColor();

                Console.WriteLine();
                ModVersion version = menu.SelectedOption;
                Console.WriteLine("Selected Version: " + version.GetModVersion());

                // Mark the mod as requested in the pack
                pack.AddMod(domain, meta, version);
                pack.Save();
            }

            pack.Dispose();
        }
        
        [ApplicationMetadata(Description = "Removes mods from the modpack.")]
        public void Remove(ModArguments arguments) {

            if (arguments.modStrings == null || arguments.modStrings.Count == 0) {
                Modifi.DefaultLogger.Error("Nothing to do; no mods were defined. Missing an argument?");
                return;
            }

            Storage.Pack pack = Storage.Pack.Load(arguments.packName);
            foreach (string modString in arguments.modStrings) {
                string domainName = ModHelper.GetDomainName(modString);
                string modIdentifier = ModHelper.GetModIdentifier(modString);
                string modVersion = ModHelper.GetModVersion(modString);

                IDomain handler = Modifi.DomainHandler.GetDomain(domainName);
                ModMetadata meta = handler.GetDomainHandler()
                    .GetModMetadata(pack.MinecraftVersion, modIdentifier).Result;

                ModStatus status = pack.GetModStatus(modString);
                switch (status) {
                    case ModStatus.NotInstalled:
                        Modifi.DefaultLogger.Error("Cannot uninstall {0}; it is not installed.", meta.GetName());
                        return;

                    case ModStatus.Requested:
                        Modifi.DefaultLogger.Information("Removing {0}...", meta.GetName());
                        Modifi.DefaultLogger.Information("Done.");
                        return;

                    case ModStatus.Installed:
                        Modifi.DefaultLogger.Information("Removing {0} and deleting files...", meta.GetName());

                        ModDownloadDetails dlInfo = pack.GetDownloadDetails(modString);
                        string filePath = Path.Combine(Settings.ModPath, dlInfo.Filename);
                        bool correctChecksum = ModUtilities.ChecksumMatches(filePath, dlInfo.Checksum);
                        if (correctChecksum) {
                            try {
                                File.Delete(filePath);
                            } catch (Exception e) {
                                Modifi.DefaultLogger.Error("Error deleting {0}, please delete it manually.", filePath);
                                Modifi.DefaultLogger.Error(e.Message);
                            }
                        } else {
                            Modifi.DefaultLogger.Information("File for {0} found at {1}, but the checksum did not match. Delete?", meta.GetName(), filePath);
                            Menu<string> delete = new Menu<string>();
                            delete.AddItem("Delete");
                            delete.AddItem("Leave");

                            delete.DrawMenu();
                            switch (delete.SelectedOption.ToLower()) {
                                case "delete":
                                    File.Delete(filePath);
                                    Modifi.DefaultLogger.Information("File deleted.");
                                    break;

                                case "leave":
                                    Modifi.DefaultLogger.Information("File left in place.");
                                    break;
                            }
                        }

                        break;
                }

                pack.RemoveMod(modString);
                pack.Dispose();
            }
        }

        [ApplicationMetadata(Description = "Fetches information on mods.")]
        public void Info(
            List<string> modStrings,
            [Option(ShortName = "m", LongName = "mc-version", Description = "Minecraft version to pull information for.")]
            string mcversion = "1.12.2") {

            if (modStrings == null || modStrings.Count == 0) {
                Modifi.DefaultLogger.Error("Nothing to do; no mods were defined. Missing an argument?");
                return;
            }

            foreach (string modString in modStrings) {
                string domainName = ModHelper.GetDomainName(modString);
                string modIdentifier = ModHelper.GetModIdentifier(modString);
                string modVersion = ModHelper.GetModVersion(modString);

                IDomain domain = Modifi.DomainHandler.GetDomain(domainName);

                // TODO: Show error here?
                if (domain == null) return;
                IDomainHandler handler = domain.GetDomainHandler();

                ILogger log = Modifi.DefaultLogger;

                Task<ModMetadata> meta = handler.GetModMetadata(mcversion, modIdentifier);
                if(meta.IsCompletedSuccessfully) {
                    ModMetadata metadata = meta.Result;
                    log.Information(metadata.GetName());
                    if (metadata.HasDescription())
                        log.Information(metadata.GetDescription());
                }
            }
        }

        [ApplicationMetadata(Description = "Downloads mods directly to the mod directory, without adding them to the pack.")]
        public Task Download(
            List<string> modStrings,
            [Option(ShortName = "m", LongName = "mc-version", Description = "Minecraft version to pull information for.")]
            string mcversion = "1.12.2") {

            if (modStrings == null || modStrings.Count == 0) {
                Modifi.DefaultLogger.Error("Nothing to do; no mods were defined. Missing an argument?");
                return Task.CompletedTask;
            }

            foreach (string modString in modStrings) {
                string domainName = ModHelper.GetDomainName(modString);
                string modIdentifier = ModHelper.GetModIdentifier(modString);
                string modVersion = ModHelper.GetModVersion(modString);

                IDomain domain = Modifi.DomainHandler.GetDomain(domainName);

                // TODO: Show error here?
                if (domain == null)return Task.CompletedTask;
                IDomainHandler handler = domain.GetDomainHandler();

                // Fetch the mod version information from the domain API
                try {
                    ModMetadata meta = handler.GetModMetadata(mcversion, modIdentifier).Result;
                    ModVersion mod = handler.GetModVersion(meta, modVersion).Result;

                    handler.DownloadMod(mod, Settings.ModPath);
                } catch (Exception e) { 
                    return Task.FromException(e); 
                }
            }

            return Task.CompletedTask;
        }
    }
}