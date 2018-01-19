using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using RobotGryphon.Modifi.Mods;
using System.Collections.Generic;
using System.Linq;

namespace RobotGryphon.Modifi.Domains.CurseForge {

    public class CurseforgeModHelper : ModHelper {

        protected Regex FILENAME_MATCH = new Regex(@".*?/([^/]*)$");

        /// <summary>
        /// The database collection that has information on the currently-requested or installed mods.
        /// </summary>
        public const string INSTALLED_MODS_COLLECTION = "mods-curseforge";

        /// <summary>
        /// Download the mod using information found in Metadata.
        /// </summary>
        /// <returns></returns>
        public override async Task<ModDownloadResult> DownloadMod(IModVersion version) {

            CurseforgeModVersion versionInfo;

            // If we say the mod is installed, then check the database and checksums
            if (IsModInstalled(version)) {
                // Do file check if mod is already in database
                versionInfo = (CurseforgeModVersion) this.GetInstalledModVersion(version);
                string installPath = Path.Combine(Settings.ModPath, versionInfo.Filename);

                if(File.Exists(installPath)) {
                    // Check checksum to see if mod downloaded already
                    using(var md5 = MD5.Create()) {
                        using (var fileStream = File.OpenRead(installPath)) {
                            byte[] hash = md5.ComputeHash(fileStream);
                            string hashString = BitConverter.ToString(hash).Replace("-", String.Empty).ToLower();

                            if (hashString == versionInfo.Checksum) {
                                Console.WriteLine("Mod {0} is already downloaded, checksum matched. Skipping.", version.GetModIdentifier());
                                return ModDownloadResult.SUCCESS;
                            }
                        }
                    }
                }
            } else {
                if(version is CurseforgeModVersion)
                    versionInfo = version as CurseforgeModVersion;
                else {
                    versionInfo = CurseForge.GetModInfo(version).Result.RequestedVersion;
                }
            }

            // If we get here, the installed version was not found or corrupted
            if (versionInfo.FileId == null)
                throw new Exception("Error during download: Mod metadata has not been fetched from Curseforge yet.");

            if (!Directory.Exists(Settings.ModPath)) Directory.CreateDirectory(Settings.ModPath);

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

                        using(var md5 = MD5.Create()) {
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

            catch(Exception) {
                return ModDownloadResult.ERROR_DOWNLOAD_FAILED;
            }
        }

        public void MarkInstalled(IModVersion versionInfo) {
            LiteDB.LiteCollection<CurseforgeModVersion> versions = Modifi.FetchCollection<CurseforgeModVersion>(INSTALLED_MODS_COLLECTION);

            if (!(versionInfo is CurseforgeModVersion))
                throw new Exception("Tried to mark a non-curseforge mod as installed under Curseforge domain.");

            // Remove existing version and replace it with the new version information
            versions.Delete(x => x.ModIdentifier == versionInfo.GetModIdentifier());
            versions.Insert(versionInfo as CurseforgeModVersion);
        }

        public override IEnumerable<IModVersion> FetchRecentModVersions(IModMetadata meta) {
            string mcVersion = Modifi.GetMinecraftVersion();

            if (!(meta is CurseforgeModMetadata))
                throw new Exception("Metadata is not of Curseforge's type. Cannot fetch mod versions with this.");

            CurseforgeModMetadata metaCF = (CurseforgeModMetadata) meta;
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
