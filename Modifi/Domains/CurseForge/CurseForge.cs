using Newtonsoft.Json;
using RobotGryphon.Modifi.Domains;
using RobotGryphon.Modifi.Mods;
using RobotGryphon.Modifi.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static RobotGryphon.Modifi.Modifi;

namespace RobotGryphon.Modifi.Domains.CurseForge {
    public class CurseforgeDomainHandler : IDomainHandler {
        public const string UserAgent = "Mozilla/5.0 (X11; Linux x86_64)";

        public static Uri BaseUri { get; internal set; }
            = new Uri("https://minecraft.curseforge.net");

        public const string ApiURL = "https://api.cfwidget.com/mc-mods/minecraft";

        protected Regex FILENAME_MATCH = new Regex(@".*?/([^/]*)$");

        /// <summary>
        /// The database collection that has information on the currently-requested or installed mods.
        /// </summary>
        public const string INSTALLED_MODS_COLLECTION = "mods-curseforge";

        public static CurseforgeDomainHandler INSTANCE = new CurseforgeDomainHandler();

        public async Task<CurseforgeModMetadata> GetModInfo(IModVersion version) {

            Console.WriteLine("Fetching metadata for {0}...", version.GetModIdentifier());

            Uri api;
            if (!String.IsNullOrEmpty(version.GetModVersion()))
                api = new Uri(String.Format("{0}/{1}?version={2}", CurseforgeDomainHandler.ApiURL, version.GetModIdentifier(), version.GetModVersion()));
            else
                api = new Uri(String.Format("{0}/{1}", CurseforgeDomainHandler.ApiURL, version.GetModIdentifier()));

            try {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(api);
                req.UserAgent = CurseforgeDomainHandler.UserAgent;

                WebResponse resp = await req.GetResponseAsync();


                Stream response = resp.GetResponseStream();
                StreamReader reader = new StreamReader(response);
                String modData = await reader.ReadToEndAsync();

                CurseforgeModMetadata modInfo = JsonConvert.DeserializeObject<CurseforgeModMetadata>(modData);
                modInfo.ModIdentifier = version.GetModIdentifier();
                modInfo.RequestedVersion.ModIdentifier = version.GetModIdentifier();

                resp.Close();
                response.Close();
                reader.Close();

                return modInfo;
            }

            catch (System.Net.WebException) {
                throw new RobotGryphon.Modifi.Mods.ModDownloadException(ModDownloadResult.ERROR_CONNECTION);
            }
        }

        /// <summary>
        /// Wrapper for DomainHandler's metadata fetch.
        /// You should really just use GetModInfo instead of this if you can.
        /// </summary>
        /// <param name="version">Which mod to fetch information on</param>
        /// <returns>Mod metadata. (Curseforge version)</returns>
        async Task<IModMetadata> IDomainHandler.GetModMetadata(IModVersion version) => await GetModInfo(version);

        public string GetProjectURL(IModMetadata meta) {
            if (!(meta is CurseforgeModMetadata)) throw new Exception("Expected curseforge mod, got invalid.");
            return ((CurseforgeModMetadata)meta).URLs.Project;
        }

        void IDomainHandler.HandleModVersions(IModVersion mod) {
            Task<CurseforgeModMetadata> metaTask = GetModInfo(mod);
            CurseforgeModMetadata meta = metaTask.Result;

            IEnumerable<CurseforgeModVersion> latestVersions = (IEnumerable<CurseforgeModVersion>) FetchRecentModVersions(meta);

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

        void IDomainHandler.HandleModAdd(IModVersion mod) {

            IModMetadata meta = GetModInfo(mod).Result;

            ModHelper.PrintModInformation(meta);

            CurseforgeModMetadata meta2 = (CurseforgeModMetadata)meta;
            IEnumerable<CurseforgeModVersion> versions = meta2.Versions[Modifi.GetMinecraftVersion()];

            CurseforgeModVersion latestRelease = versions.First(x => x.Type == ModReleaseType.RELEASE);

            List<CurseforgeModVersion> versionList = new List<CurseforgeModVersion>();
            versionList.Add(latestRelease);
            versionList.Add(versions.First());

            Menu<CurseforgeModVersion> menu = new Menu<CurseforgeModVersion>();

            menu.OptionFormatter = (opt) => {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(opt.Name);
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write(" [");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(opt.FileId);
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("]");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" [{0}]", opt.Type);
                Console.WriteLine();
            };

            menu.AddItem(latestRelease);
            menu.AddItem(versions.First());
            menu.AddSpacer();

            foreach (CurseforgeModVersion v in versions.Skip(1).Take(5)) menu.AddItem(v);

            menu.DrawMenu();

            Console.ResetColor();

            Console.WriteLine();
            CurseforgeModVersion version = menu.SelectedOption;
            Console.WriteLine("Selected Version: " + version.Name);

            version.ModIdentifier = mod.GetModIdentifier();

            LiteDB.LiteDatabase db = Modifi.FetchCurrentVersion();
            var c = db.GetCollection<CurseforgeModVersion>("mods-curseforge");
            c.Insert(version);
        }

        void IDomainHandler.HandleModRemove(IModVersion mod) {
            if(!IsModInstalled(mod)) {
                Console.WriteLine("Error: Cannot uninstall mod; it is not installed.");
                return;
            }

            IModVersion modInformation = GetInstalledModVersion(mod);
            string filename = modInformation.GetFilename();
        }

        void IDomainHandler.HandleModInformation(IModVersion mod) {
            CurseforgeModMetadata meta = GetModInfo(mod).Result;

            // Print out mod information
            ModHelper.PrintModInformation(meta);
        }

        ModDownloadResult IDomainHandler.HandleModDownload(IModVersion modVersion) {
            Task<ModDownloadResult> result = DownloadMod(modVersion);
            return result.Result;
        }

        /// <summary>
        /// Download the mod using information found in Metadata.
        /// </summary>
        /// <returns></returns>
        public async Task<ModDownloadResult> DownloadMod(IModVersion version) {

            CurseforgeModVersion versionInfo;

            // If we say the mod is installed, then check the database and checksums
            if (IsModInstalled(version)) {
                // Do file check if mod is already in database
                versionInfo = (CurseforgeModVersion) this.GetInstalledModVersion(version);
                if (versionInfo != null) {
                    string installPath = Path.Combine(Settings.ModPath, versionInfo.Filename);

                    bool csMatch = FileUtilities.ChecksumMatches(installPath, versionInfo.Checksum);
                    if (csMatch) {
                        // File already downloaded, checksum matched
                        return ModDownloadResult.SUCCESS;
                    } else {
                        // File checksum did not match, delete it
                        File.Delete(installPath);
                    }
                }
            } else {
                // TODO: Make sure metadata is all there and valid for download
                if (version is CurseforgeModVersion)
                    versionInfo = version as CurseforgeModVersion;
                else {
                    versionInfo = (await GetModInfo(version)).RequestedVersion;
                }
            }

            // If we get here, the installed version was not found or corrupted
            if (versionInfo.FileId == null)
                throw new Exception("Error during download: Mod metadata has not been fetched from Curseforge yet.");

            if (!Directory.Exists(Settings.ModPath)) Directory.CreateDirectory(Settings.ModPath);

            #region Perform Mod Download
            try {
                HttpWebRequest webRequest = WebRequest.CreateHttp(new Uri(versionInfo.DownloadURL + "/file"));
                using (WebResponse r = await webRequest.GetResponseAsync()) {
                    Uri downloadUri = r.ResponseUri;

                    if (!FILENAME_MATCH.IsMatch(downloadUri.AbsoluteUri))
                        return ModDownloadResult.ERROR_INVALID_FILENAME;

                    Match m = FILENAME_MATCH.Match(downloadUri.AbsoluteUri);
                    string filename = m.Groups[1].Value;

                    if (filename.ToLowerInvariant() == "download")
                        return ModDownloadResult.ERROR_INVALID_FILENAME;

                    FileStream fs = File.OpenWrite(Path.Combine(Settings.ModPath, filename));
                    byte[] buffer = new byte[1024];
                    using (Stream s = r.GetResponseStream()) {
                        int size = s.Read(buffer, 0, buffer.Length);
                        while (size > 0) {
                            fs.Write(buffer, 0, size);
                            size = s.Read(buffer, 0, buffer.Length);
                        }

                        fs.Flush();
                        fs.Close();

                        versionInfo.Filename = filename;

                        using (var md5 = MD5.Create()) {
                            using (var stream = File.OpenRead(Path.Combine(Settings.ModPath, filename))) {
                                byte[] hash = md5.ComputeHash(stream);
                                versionInfo.Checksum = BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();
                            }
                        }

                        this.MarkInstalled(versionInfo);

                        return ModDownloadResult.SUCCESS;
                    }
                }
            }

            catch (Exception) {
                return ModDownloadResult.ERROR_DOWNLOAD_FAILED;
            }
            #endregion
        }

        protected void MarkInstalled(IModVersion versionInfo) {
            LiteDB.LiteCollection<CurseforgeModVersion> versions = Modifi.FetchCollection<CurseforgeModVersion>(INSTALLED_MODS_COLLECTION);

            if (!(versionInfo is CurseforgeModVersion))
                throw new Exception("Tried to mark a non-curseforge mod as installed under Curseforge domain.");

            // Remove existing version and replace it with the new version information
            versions.Delete(x => x.ModIdentifier == versionInfo.GetModIdentifier());
            versions.Insert(versionInfo as CurseforgeModVersion);
        }

        public IEnumerable<IModVersion> FetchRecentModVersions(IModMetadata meta) {
            string mcVersion = Modifi.GetMinecraftVersion();

            if (!(meta is CurseforgeModMetadata))
                throw new Exception("Metadata is not of Curseforge's type. Cannot fetch mod versions with this.");

            CurseforgeModMetadata metaCF = (CurseforgeModMetadata)meta;
            if (!metaCF.Versions.ContainsKey(mcVersion)) {
                // TODO: Change this to a MinecraftVersionException or something similar
                throw new Exception("Mod does not have versions for your requested Minecraft version.");
            }

            IEnumerable<CurseforgeModVersion> versions = metaCF.Versions[mcVersion];

            // TODO: Configurable number of results shown?
            IEnumerable<CurseforgeModVersion> limitedList = versions.Take(5);
            return limitedList;
        }

        public bool IsModInstalled(IModVersion mod) {
            if (!Modifi.CollectionExists(INSTALLED_MODS_COLLECTION)) return false;

            LiteDB.LiteCollection<CurseforgeModVersion> versions = Modifi.FetchCollection<CurseforgeModVersion>(INSTALLED_MODS_COLLECTION);

            // TODO: Make this smarter by also checking for a hash
            return versions.Exists(x => x.ModIdentifier == mod.GetModIdentifier());
        }

        public IModVersion GetInstalledModVersion(IModVersion mod) {
            if (!IsModInstalled(mod)) return null;

            LiteDB.LiteCollection<CurseforgeModVersion> versions = Modifi.FetchCollection<CurseforgeModVersion>(INSTALLED_MODS_COLLECTION);
            return versions.FindOne(x => x.ModIdentifier == mod.GetModIdentifier());
        }
    }
}
