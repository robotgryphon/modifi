using Newtonsoft.Json;
using Modifi.Domains;
using Modifi.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Domains.Curseforge {

    /// <summary>
    /// Handles mods and packs from curseforge.com
    /// </summary>
    public class CurseforgeDomainHandler : IDomainHandler {
        private const string UserAgent = "Mozilla/5.0 (X11; Linux x86_64)";

        private static Uri BaseUri { get; set; }
            = new Uri("https://minecraft.curseforge.net");

        private const string ApiURL = "https://api.cfwidget.com/mc-mods/minecraft";

        protected Regex FILENAME_MATCH = new Regex(@".*?/([^/]*)$");

        /// <summary>
        /// The database collection that has information on the currently-requested or installed mods.
        /// </summary>
        public const string INSTALLED_MODS_COLLECTION = "curseforge";

        public async Task<ModMetadata> GetModMetadata(string minecraftVersion, string modIdentifier) {
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
                modInfo.MinecraftVersion = minecraftVersion;

                resp.Close();
                response.Close();
                reader.Close();

                return modInfo;
            }

            catch (System.Net.WebException we) {
                HttpStatusCode status = (we.Response as HttpWebResponse).StatusCode;
                if (status == HttpStatusCode.NotFound) {
                    Console.Error.WriteLine("Could not find that mod. Check your input and try again.");
                    return null;
                }
            }

            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
            }

            return null;
        }

        public Task<ModVersion> GetModVersion(ModMetadata metadata, string version) {
            if (metadata == null) {
                throw new Exception("Metadata is not defined; cannot get mod versions with it.");
            }

            if (!(metadata is CurseforgeModMetadata))
                throw new Exception("Metadata needs to be a curseforge metadata object.");

            CurseforgeModMetadata cfMeta = metadata as CurseforgeModMetadata;

            bool hasVersion = cfMeta.Versions.ContainsKey(cfMeta.MinecraftVersion);
            if (!hasVersion)
                throw new Exception("Mod does not have versions for the requested Minecraft version.");

            IEnumerable<ModVersion> versions = cfMeta.Versions[cfMeta.MinecraftVersion];
            if (version == null) return Task.FromResult(versions.First());

            ModVersion mv = versions.First(v => v.GetModVersion() == version);
            return Task.FromResult(mv);
        }

        /// <summary>
        /// Download a specific version of a mod.
        /// </summary>
        /// <exception cref="ModDownloadException"></exception>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        public async Task<ModDownloadDetails> DownloadMod(ModVersion version, string directory) {

            CurseforgeModVersion versionInfo;
            if (version is CurseforgeModVersion)
                versionInfo = version as CurseforgeModVersion;
            else {
                throw new ArgumentException("Version provided is not a curseforge mod version, cannot continue.");
            }

            // If we get here, the installed version was not found or corrupted
            if (versionInfo.FileId == null)
                throw new ModDownloadException("Error during download: Mod metadata has not been fetched from Curseforge yet.");

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

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

                    string finalFilename = Path.Combine(directory, filename);

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


                        ModDownloadDetails result = new ModDownloadDetails();
                        result.Filename = filename;
                        result.Checksum = ModUtilities.GetFileChecksum(finalFilename);

                        return result;
                    }
                }
            }

            catch (Exception) {
                throw;
            }
            #endregion
        }

        public Task<IEnumerable<ModVersion>> GetRecentVersions(ModMetadata meta, int count = 5, ModReleaseType releaseType = ModReleaseType.Any) {
            if (!(meta is CurseforgeModMetadata))
                throw new Exception("Metadata is not of Curseforge's type. Cannot fetch mod versions with this.");

            CurseforgeModMetadata metaCF = (CurseforgeModMetadata)meta;
            if (String.IsNullOrEmpty(metaCF.MinecraftVersion))
                throw new Exception("Minecraft version was not set on metadata, cannot properly fetch versions.");

            if (!metaCF.Versions.ContainsKey(metaCF.MinecraftVersion))
                return null;

            IEnumerable<ModVersion> versions = metaCF.Versions[metaCF.MinecraftVersion];

            if(releaseType != ModReleaseType.Any) {
                versions = versions.Where(v => v.GetReleaseType() == releaseType);
            }

            IEnumerable<ModVersion> limitedList = versions.Take(count);
            foreach(ModVersion mv in limitedList)
                mv.MetadataId = meta.Id;

            return Task.FromResult(limitedList);
        }

        public Task<ModVersion> GetLatestVersion(ModMetadata meta, ModReleaseType releaseType = ModReleaseType.Any) {
            if (!(meta is CurseforgeModMetadata))
                throw new Exception("Metadata is not of Curseforge's type. Cannot fetch mod versions with this.");

            IEnumerable<ModVersion> versions = GetRecentVersions(meta, 1, releaseType).Result;

            return Task.FromResult(versions.First());
        }
    }
}
