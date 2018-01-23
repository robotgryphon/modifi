using RobotGryphon.Modifi.Mods;
using RobotGryphon.Modifi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Domains.CurseForge {
    public partial class CurseforgeDomainHandler : IDomainCommandHandler {

        public void HandleModVersions(string modIdentifier) {
            IModMetadata meta = GetModMetadata(modIdentifier).Result;

            IEnumerable<CurseforgeModVersion> latestVersions = (IEnumerable<CurseforgeModVersion>)FetchRecentModVersions(meta);

            ModHelper.PrintModInformation(meta);
            foreach (CurseforgeModVersion version in latestVersions) {
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(version.FileId);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("] ");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(version.Name);

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" (");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(version.Type);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(")");
                Console.ResetColor();
                Console.WriteLine();
            }
        }

        public void HandleModAdd(string modIdentifier, string modVersion = null) {

            IModVersion installed = GetInstalledModVersion(modIdentifier);
            if (installed != null) {
                Console.Error.WriteLine("> Error: Mod \"{0}\" is already added to the pack. Please use the upgrade command instead.", installed.GetModIdentifier());
                return;
            }

            IModMetadata meta = GetModMetadata(modIdentifier).Result;

            // If the version is already specified, don't ask, just add it
            if (!String.IsNullOrEmpty(modVersion)) {
                IModVersion versionMeta = GetModVersion(meta, modVersion).Result;
                if (versionMeta != null) {
                    ChangeModStatus(versionMeta, ModStatus.Requested);
                    return;
                }
            }

            // ============================================================
            ModHelper.PrintModInformation(meta);

            Menu<IModVersion> menu = new Menu<IModVersion>();

            menu.OptionFormatter = (opt) => {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write((opt as CurseforgeModVersion).Name);
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write(" [");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(opt.GetModVersion());
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("]");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" [{0}]", (opt as CurseforgeModVersion).Type);
                Console.WriteLine();
            };

            IModVersion latestRelease = GetMostRecentModVersion(meta, ModReleaseType.Release).Result;
            IModVersion latestVersion = GetMostRecentModVersion(meta, ModReleaseType.Any).Result;

            IEnumerable<IModVersion> versions = ((CurseforgeModMetadata)meta).Versions[Modifi.GetMinecraftVersion()].Skip(1);

            menu.AddItem(latestRelease);
            if (latestVersion.GetModVersion() != latestRelease.GetModVersion())
                menu.AddItem(latestVersion);

            menu.AddSpacer();
            foreach (CurseforgeModVersion v in versions.Take(5)) menu.AddItem(v);


            menu.DrawMenu();

            Console.ResetColor();

            Console.WriteLine();
            CurseforgeModVersion version = (CurseforgeModVersion)menu.SelectedOption;
            Console.WriteLine("Selected Version: " + version.Name);

            version.ModIdentifier = modIdentifier;

            ChangeModStatus(version, ModStatus.Requested);
        }

        public void HandleModRemove(string modIdentifier, string modVersion = null) {

            IModVersion modInformation = GetInstalledModVersion(modIdentifier);
            if (modInformation == null) {
                Console.WriteLine("Error: Cannot uninstall mod; it is not installed.");
                return;
            }

            ModStatus status = GetModStatus(modInformation);
            var mods = Modifi.CurrentVersion.FetchCollection<CurseforgeModVersion>(INSTALLED_MODS_COLLECTION);
            switch (status) {
                case ModStatus.NotInstalled:
                    break;

                case ModStatus.Requested:
                    // Mod has not been downloaded yet, just delete the entry
                    mods.Delete(x => x.ModIdentifier == modInformation.GetModIdentifier());
                    break;

                case ModStatus.Installed:
                    string filename = modInformation.GetFilename();
                    string actualFilename = Path.Combine(Settings.ModPath, filename);

                    bool correctChecksum = FileUtilities.ChecksumMatches(actualFilename, modInformation.GetChecksum());
                    if (correctChecksum) {
                        try {
                            File.Delete(actualFilename);
                            mods.Delete(x => x.ModIdentifier == modInformation.GetModIdentifier());
                        }

                        catch (Exception e) {
                            Console.Error.WriteLine("Error deleting {0}, please delete it manually.", actualFilename);
                            Console.Error.WriteLine(e.Message);
                        }
                    } else {
                        Console.WriteLine("File for {0} found at {1}, but the checksum did not match. Delete?", modInformation.GetModIdentifier(), actualFilename);
                        Menu<string> delete = new Menu<string>();
                        delete.AddItem("Delete");
                        delete.AddItem("Leave");

                        delete.DrawMenu();
                        switch (delete.SelectedOption.ToLower()) {
                            case "delete":
                                Console.WriteLine("Deleting file.");
                                File.Delete(actualFilename);
                                break;

                            case "leave":
                                Console.WriteLine("Leaving file in place.");
                                break;
                        }
                    }

                    break;
            }
        }

        public void HandleModInformation(string modIdentifier, string modVersion = null) {
            IModMetadata meta = GetModMetadata(modIdentifier).Result;

            // Print out mod information
            ModHelper.PrintModInformation(meta);
        }

        public ModDownloadResult? HandleModDownload(string modIdentifier, string modVersion = null) {

            // Fetch the mod version information from the Curseforge API
            try {
                IModMetadata meta = GetModMetadata(modIdentifier).Result;
                IModVersion mod = GetModVersion(meta, modVersion).Result;

                return DownloadMod(mod).Result;
            }

            catch(Exception) { return null; }
        }

        public async Task PerformPackDownload() {

            LiteDB.LiteCollection<CurseforgeModVersion> mods = Modifi.CurrentVersion.FetchCollection<CurseforgeModVersion>(INSTALLED_MODS_COLLECTION);
            IEnumerable<CurseforgeModVersion> requested = mods.FindAll();

            foreach(CurseforgeModVersion mod in requested) {

                ModStatus status = GetModStatus(mod);
                Console.WriteLine();
                switch (status) {
                    case ModStatus.Requested:    
                        try {
                            ModDownloadResult result = await DownloadMod(mod);
                            mod.Checksum = result.Checksum;
                            mod.Filename = result.Filename;
                            mod.Status = ModStatus.Installed;

                            mods.Update(mod);
                        }

                        catch (ModDownloadException e) {
                            Console.Error.WriteLine("Error downloading {0}: {1}", mod.ModIdentifier, e.Message);
                        }

                        break;

                    case ModStatus.Installed:
                        Console.WriteLine("> Already installed {0}, skipping.", mod.ModIdentifier);
                        break;
                }
            }
        }
    }
}
