using Modifi.Domains;
using Modifi.Mods;
using Modifi.Packs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Commands {

    public class ModCommandHandler {
        public enum ModActions {
            ADD,
            REMOVE,
            INFO,
            LIST,
            DOWNLOAD,
            VERSIONS
        }

        internal static void HandleModAction(IEnumerable<string> arguments) {

            ModActions? action = CommandHandler.ParseCommandOption<ModActions>(arguments.First());
            if (action == null) return;

            IEnumerable<string> mods = arguments.Skip(1);

            Pack pack = Modifi.DefaultPack;

            foreach (string mod in mods) {
                string domainName = ModHelper.GetDomainName(mod);
                IDomain handler = pack.GetDomain(domainName);

                if (handler == null || !(handler is IDomain))
                    throw new Exception("That domain does not have a registered handler. Aborting.");

                // TODO: Generic

                string modIdentifier = ModHelper.GetModIdentifier(mod);
                string modVersion = ModHelper.GetModVersion(mod);

                switch (action) {
                    case ModActions.ADD:
                        HandleModAdd(handler, modIdentifier, modVersion);
                        break;

                    case ModActions.REMOVE:
                        // commandHandler.HandleModRemove(handler, modIdentifier, modVersion);
                        break;

                    case ModActions.INFO:
                        // commandHandler.HandleModInformation(handler, modIdentifier, modVersion);
                        break;

                    case ModActions.VERSIONS:
                        HandleModVersions(handler, modIdentifier);
                        break;

                    case ModActions.DOWNLOAD:
                        // Do mod download, do not add to storage
                        break;

                    default:
                        throw new Exception("Invalid mod action.");
                }
            }


        }

        public static void HandleModVersions(IDomain domain, string modIdentifier) {
            IDomainHandler handler = domain.GetDomainHandler();

            IModMetadata meta = handler.GetModMetadata(Modifi.DefaultPack.MinecraftVersion, modIdentifier).Result;

            IEnumerable<IModVersion> latestVersions = handler.GetRecentVersions(meta).Result;

            // ModHelper.PrintModInformation(meta);
            foreach (IModVersion version in latestVersions) {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(version.GetModVersion());
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("] ");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(version.GetModIdentifier());

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

        public static void HandleModAdd(IDomain domain, string modIdentifier, string modVersion = null) {

            IDomainHandler handler = domain.GetDomainHandler();

            // TODO: IModVersion installed = handler.GetInstalledModVersion(modIdentifier);
            //if (installed != null) {
            //    Console.Error.WriteLine("> Error: Mod \"{0}\" is already added to the pack. Please use the upgrade command instead.", installed.GetModIdentifier());
            //    return;
            //}

            IModMetadata meta = handler.GetModMetadata(Modifi.DefaultPack.MinecraftVersion, modIdentifier).Result;

            // If the version is already specified, don't ask, just add it
            // TODO: Abstract to Storage
            //if (!String.IsNullOrEmpty(modVersion)) {
            //    IModVersion versionMeta = handler.GetModVersion(meta, modVersion).Result;
            //    if (versionMeta != null) {
            //        (domain.GetDomainHandler() as CurseforgeDomainHandler).ChangeModStatus(versionMeta, ModStatus.Requested);
            //        return;
            //    }
            //}

            // ============================================================
            // TODO: ModHelper.PrintModInformation(meta);

            Menu<IModVersion> menu = new Menu<IModVersion>();

            menu.OptionFormatter = (opt) => {
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

            IModVersion latestRelease = handler.GetRecentVersions(meta, 1, ModReleaseType.Release).Result.First();
            IModVersion latestVersion = handler.GetRecentVersions(meta, 1, ModReleaseType.Any).Result.First();

            IEnumerable<IModVersion> versions = meta.GetMostRecentVersions().Skip(1);
                // ((CurseforgeModMetadata)meta).Versions[Modifi.DefaultPack.MinecraftVersion].Skip(1);

            menu.AddItem(latestRelease);
            if (latestVersion.GetModVersion() != latestRelease.GetModVersion())
                menu.AddItem(latestVersion);

            menu.AddSpacer();
            foreach (IModVersion v in versions.Take(5)) menu.AddItem(v);


            menu.DrawMenu();

            Console.ResetColor();

            Console.WriteLine();
            IModVersion version = menu.SelectedOption;
            Console.WriteLine("Selected Version: " + version.GetModVersion());

            // TODO: handler.ChangeModStatus(version, ModStatus.Requested);
        }

        //public void HandleModRemove(IDomain domain, string modIdentifier, string modVersion = null) {
        //    CurseforgeDomainHandler handler = domain.GetDomainHandler() as CurseforgeDomainHandler;
        //    IModVersion modInformation = handler.GetInstalledModVersion(modIdentifier);
        //    if (modInformation == null) {
        //        Console.WriteLine("Error: Cannot uninstall mod; it is not installed.");
        //        return;
        //    }

        //    ModStatus status = handler.GetModStatus(modInformation);

        //    using (Pack pack = Modifi.DefaultPack) {
        //        using (ModifiVersion version = Modifi.LoadVersion(pack.Installed)) {
        //            var mods = version.FetchCollection<CurseforgeModVersion>(domain.GetDomainIdentifier());

        //            switch (status) {
        //                case ModStatus.NotInstalled:
        //                    break;

        //                case ModStatus.Requested:
        //                    // Mod has not been downloaded yet, just delete the entry
        //                    mods.Delete(x => x.ModIdentifier == modInformation.GetModIdentifier());
        //                    break;

        //                case ModStatus.Installed:
        //                    string filename = modInformation.GetFilename();
        //                    string actualFilename = Path.Combine(Settings.ModPath, filename);

        //                    bool correctChecksum = FileUtilities.ChecksumMatches(actualFilename, modInformation.GetChecksum());
        //                    if (correctChecksum) {
        //                        try {
        //                            File.Delete(actualFilename);
        //                            mods.Delete(x => x.ModIdentifier == modInformation.GetModIdentifier());
        //                        }

        //                        catch (Exception e) {
        //                            Console.Error.WriteLine("Error deleting {0}, please delete it manually.", actualFilename);
        //                            Console.Error.WriteLine(e.Message);
        //                        }
        //                    } else {
        //                        Console.WriteLine("File for {0} found at {1}, but the checksum did not match. Delete?", modInformation.GetModIdentifier(), actualFilename);
        //                        Menu<string> delete = new Menu<string>();
        //                        delete.AddItem("Delete");
        //                        delete.AddItem("Leave");

        //                        delete.DrawMenu();
        //                        switch (delete.SelectedOption.ToLower()) {
        //                            case "delete":
        //                                Console.WriteLine("Deleting file.");
        //                                File.Delete(actualFilename);
        //                                break;

        //                            case "leave":
        //                                Console.WriteLine("Leaving file in place.");
        //                                break;
        //                        }
        //                    }

        //                    break;
        //            }
        //        }
        //    }
        //}

        //public void HandleModInformation(IDomain domain, string modIdentifier, string modVersion = null) {
        //    IModMetadata meta = domain.GetDomainHandler().GetModMetadata(modIdentifier).Result;

        //    if (domain is CurseforgeDomain) {
        //        CurseforgeDomain d = domain as CurseforgeDomain;
        //        ILogger log = d.Logger;

        //        log.Information(meta.GetName());
        //        if (meta.HasDescription()) log.Information(meta.GetDescription());
        //    }
        //}

        //public ModDownloadResult? HandleModDownload(IDomain domain, string modIdentifier, string modVersion = null) {

        //    CurseforgeDomainHandler handler = domain.GetDomainHandler() as CurseforgeDomainHandler;

        //    // Fetch the mod version information from the Curseforge API
        //    try {
        //        IModMetadata meta = handler.GetModMetadata(modIdentifier).Result;
        //        IModVersion mod = handler.GetModVersion(meta, modVersion).Result;

        //        return handler.DownloadMod(mod).Result;
        //    }

        //    catch (Exception) { return null; }
        //}
    }
}
