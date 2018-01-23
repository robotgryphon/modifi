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

    /// <summary>
    /// Handles mods and packs from curseforge.com
    /// </summary>
    public partial class CurseforgeDomainHandler : IDomainHandler {
        private const string UserAgent = "Mozilla/5.0 (X11; Linux x86_64)";

        private static Uri BaseUri { get; set; }
            = new Uri("https://minecraft.curseforge.net");

        private const string ApiURL = "https://api.cfwidget.com/mc-mods/minecraft";

        protected Regex FILENAME_MATCH = new Regex(@".*?/([^/]*)$");

        /// <summary>
        /// The database collection that has information on the currently-requested or installed mods.
        /// </summary>
        public const string INSTALLED_MODS_COLLECTION = "curseforge";

        public static CurseforgeDomainHandler INSTANCE = new CurseforgeDomainHandler();

        public async Task<IModMetadata> GetModMetadata(string modIdentifier) {
            Console.WriteLine("Fetching metadata for {0}...", modIdentifier);

            Uri api = new Uri(String.Format("{0}/{1}", CurseforgeDomainHandler.ApiURL, modIdentifier));

            // if (!String.IsNullOrEmpty(modVersion)) 
            // api = new Uri(String.Format("{0}/{1}?version={2}", CurseforgeDomainHandler.ApiURL, modIdentifier, modVersion));

            try {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(api);
                req.UserAgent = CurseforgeDomainHandler.UserAgent;

                WebResponse resp = await req.GetResponseAsync();


                Stream response = resp.GetResponseStream();
                StreamReader reader = new StreamReader(response);
                String modData = await reader.ReadToEndAsync();

                CurseforgeModMetadata modInfo = JsonConvert.DeserializeObject<CurseforgeModMetadata>(modData);
                modInfo.ModIdentifier = modIdentifier;
                modInfo.RequestedVersion.ModIdentifier = modIdentifier;

                resp.Close();
                response.Close();
                reader.Close();

                return modInfo;
            }

            catch (System.Net.WebException we) {
                HttpStatusCode status = (we.Response as HttpWebResponse).StatusCode;
                if(status == HttpStatusCode.NotFound) {
                    Console.Error.WriteLine("Could not find that mod. Check your input and try again.");
                    return null;
                }
            }

            catch(Exception e) {
                Console.Error.WriteLine(e.Message);
            }

            return null;
        }

        public Task<IModVersion> GetModVersion(IModMetadata metadata, string version) {
            if(metadata == null) {
                throw new Exception("Metadata is not defined; cannot get mod versions with it.");
            }

            if(metadata is CurseforgeModMetadata) {
                CurseforgeModMetadata meta = (CurseforgeModMetadata) metadata;
                IEnumerable<CurseforgeModVersion> versions = meta.Versions[Modifi.GetMinecraftVersion()];

                IModVersion versionMeta;
                if (version == null)
                    versionMeta = versions.First();
                else
                    versionMeta = versions.First(v => v.FileId == version);

                return Task.FromResult<IModVersion>(versionMeta);
            } else {
                throw new FormatException("Metadata is not of the Curseforge type");
            }
        }

        /// <summary>
        /// Download a specific version of a mod.
        /// </summary>
        /// <exception cref="ModDownloadException"></exception>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        public async Task<ModDownloadResult> DownloadMod(IModVersion version) {

            Console.WriteLine("> Downloading {0}...", version.GetModIdentifier());

            // If mod is already installed, return the appropriate response
            if (GetModStatus(version) == ModStatus.Installed)
                throw new ModDownloadException("Mod is already downloaded.");

            CurseforgeModVersion versionInfo;
            if (version is CurseforgeModVersion)
                versionInfo = version as CurseforgeModVersion;
            else {
                IModMetadata modMeta = await GetModMetadata(version.GetModIdentifier());
                versionInfo = GetModVersion(modMeta, version.GetModVersion()).Result as CurseforgeModVersion;
            }

            // If we get here, the installed version was not found or corrupted
            if (versionInfo.FileId == null)
                throw new ModDownloadException("Error during download: Mod metadata has not been fetched from Curseforge yet.");

            if (!Directory.Exists(Settings.ModPath)) Directory.CreateDirectory(Settings.ModPath);

            #region Perform Mod Download
            try {
                HttpWebRequest webRequest = WebRequest.CreateHttp(new Uri(versionInfo.DownloadURL + "/file"));
                using (WebResponse r = await webRequest.GetResponseAsync()) {
                    Uri downloadUri = r.ResponseUri;

                    if (!FILENAME_MATCH.IsMatch(downloadUri.AbsoluteUri))
                        throw new ModDownloadException("The provided download URL does not appear to be valid.");

                    Match m = FILENAME_MATCH.Match(downloadUri.AbsoluteUri);
                    string filename = m.Groups[1].Value;

                    if (filename.ToLowerInvariant() == "download")
                        throw new ModDownloadException("Bad filename for a mod from Curseforge; got 'download' instead of filename.");

                    string finalFilename = Path.Combine(Settings.ModPath, filename);

                    if (File.Exists(finalFilename)) File.Delete(finalFilename);

                    FileStream fs = File.OpenWrite(finalFilename);
                    byte[] buffer = new byte[1024];
                    using (Stream s = r.GetResponseStream()) {
                        int size = s.Read(buffer, 0, buffer.Length);
                        while (size > 0) {
                            fs.Write(buffer, 0, size);
                            size = s.Read(buffer, 0, buffer.Length);
                        }

                        fs.Flush();
                        fs.Close();


                        ModDownloadResult result = new ModDownloadResult();
                        result.Filename = filename;
                        result.Checksum = FileUtilities.GetFileChecksum(Path.Combine(Settings.ModPath, filename));

                        Console.WriteLine("> Downloaded.");

                        return result;
                    }
                }
            }

            catch (Exception) {
                throw;
            }
            #endregion
        }

        public void ChangeModStatus(IModVersion versionInfo, ModStatus status) {
            LiteDB.LiteCollection<CurseforgeModVersion> versions = Modifi.CurrentVersion.FetchCollection<CurseforgeModVersion>(INSTALLED_MODS_COLLECTION);

            if (!(versionInfo is CurseforgeModVersion))
                throw new Exception("Tried to change status of a non-curseforge mod.");

            // Remove existing version and replace it with the new version information
            CurseforgeModVersion oldVersion = versions.FindOne(x => x.ModIdentifier == versionInfo.GetModIdentifier());

            if (oldVersion == null) oldVersion = versionInfo as CurseforgeModVersion;
            oldVersion.Status = status;

            versions.Upsert(oldVersion);
        }

        public IEnumerable<IModVersion> FetchRecentModVersions(IModMetadata meta, int count = 5) {
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
            IEnumerable<CurseforgeModVersion> limitedList = versions.Take(count);
            return limitedList;
        }

        public ModStatus GetModStatus(IModVersion mod) {
            if (!Modifi.CurrentVersion.CollectionExists(INSTALLED_MODS_COLLECTION)) return ModStatus.NotInstalled;

            LiteDB.LiteCollection<CurseforgeModVersion> versions = Modifi.CurrentVersion.FetchCollection<CurseforgeModVersion>(INSTALLED_MODS_COLLECTION);

            CurseforgeModVersion version = versions.FindOne(x =>
                (x.ModIdentifier == mod.GetModIdentifier()) &&
                (x.FileId == mod.GetModVersion())
            );

            if (version == null) return ModStatus.NotInstalled;

            if(String.IsNullOrEmpty(version.Filename)) return ModStatus.Requested;

            // Mod is supposedly installed, check if checksum matches
            string installPath = Path.Combine(Settings.ModPath, version.Filename);

            bool csMatch = FileUtilities.ChecksumMatches(installPath, version.Checksum);
            if (csMatch) {
                // File already downloaded, checksum matched
                return ModStatus.Installed;
            } else {
                // File checksum did not match, delete it and set filename to null in database
                File.Delete(installPath);
                version.Filename = null;
                versions.Update(version);
                return ModStatus.Requested;
            }
        }

        public IModVersion GetInstalledModVersion(string modIdentifier) {
            LiteDB.LiteCollection<CurseforgeModVersion> versions = Modifi.CurrentVersion.FetchCollection<CurseforgeModVersion>(INSTALLED_MODS_COLLECTION);
            return versions.FindOne(x => x.ModIdentifier == modIdentifier);
        }

        public Task<IModVersion> GetMostRecentModVersion(IModMetadata meta, ModReleaseType releaseType = ModReleaseType.Any) {
            if(meta is CurseforgeModMetadata) {
                CurseforgeModMetadata metadata = (CurseforgeModMetadata) meta;
                IEnumerable<CurseforgeModVersion> versions = metadata.Versions[Modifi.GetMinecraftVersion()];

                if (releaseType == ModReleaseType.Any)
                    return Task.FromResult<IModVersion>(versions.First());
                else
                    return Task.FromResult<IModVersion>(versions.First(mv => mv.Type == releaseType));
            } else {
                throw new FormatException("Meta needs to be of type CurseforgeModMetadata.");
            }
        }
    }
}
