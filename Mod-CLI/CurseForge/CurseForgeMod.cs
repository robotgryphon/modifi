using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using RobotGryphon.ModCLI.Mods;

/**
 * Some code is adapted from Vazkii's Curseforge Modpack Downloader
 * See https://github.com/Vazkii/CMPDL for more information.
 */
namespace RobotGryphon.ModCLI.CurseForge {

    public class CurseForgeMod : IMod {

        /// <summary>
        /// The curseforge id of the mod (ie: jei)
        /// </summary>
        public string ProjectId { get; set; }

        protected Regex FILENAME_MATCH = new Regex(@".*?/([^/]*)$");

        public ModMetadata Metadata;
        public CurseForgeModInfo ModInfo;

        public CurseForgeMod() { }

        public ModStatus GetDownloadStatus() {
            string FilePathLocal = Path.Combine(Settings.ModPath, Metadata.Filename ?? "-");
            if (File.Exists(FilePathLocal)) {
                // Perform checksum match and skip download if match
                using (var md5 = MD5.Create()) {
                    byte[] hash = md5.ComputeHash(File.OpenRead(FilePathLocal));
                    String s = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    if (s.Equals(Metadata.Checksum)) {
                        return ModStatus.DOWNLOADED;
                    }
                }
            }

            return ModStatus.NOT_DOWNLOADED;
        }

        /// <summary>
        /// Quick wrapper around the helper class.
        /// Returns the stored mod information if it has it.
        /// </summary>
        /// <returns></returns>
        public async Task<CurseForgeModInfo> GetModInfo() {
            if (String.IsNullOrEmpty(this.ModInfo.Title)) {
                this.ModInfo = await CurseForgeHelper.GetModInfo(new ModVersion() {
                    ModId = Metadata.ModId,
                    Version = Metadata.Version
                });

                this.Metadata = this.ModInfo.Download;
            }

            return this.ModInfo;
        }


        /// <summary>
        /// Download the mod using information found in Metadata.
        /// </summary>
        /// <returns></returns>
        public async Task<ModDownloadResult> Download() {
            await GetModInfo();
            if (!Directory.Exists(Settings.ModPath)) Directory.CreateDirectory(Settings.ModPath);

            try {
                HttpWebRequest webRequest = WebRequest.CreateHttp(new Uri(Metadata.DownloadURL + "/file"));
                using (WebResponse r = await webRequest.GetResponseAsync()) {
                    Uri downloadUri = r.ResponseUri;

                    if (!FILENAME_MATCH.IsMatch(downloadUri.AbsoluteUri))
                        return ModDownloadResult.ERROR_INVALID_FILENAME;

                    Match m = FILENAME_MATCH.Match(downloadUri.AbsoluteUri);
                    this.Metadata.Filename = m.Groups[1].Value;

                    if (Metadata.Filename.ToLowerInvariant() == "download")
                        return ModDownloadResult.ERROR_INVALID_FILENAME;

                    FileStream fs = File.OpenWrite(Path.Combine(Settings.ModPath, Metadata.Filename));
                    byte[] buffer = new byte[1024];
                    using (Stream s = r.GetResponseStream()) {
                        int size = s.Read(buffer, 0, buffer.Length);
                        while (size > 0) {
                            fs.Write(buffer, 0, size);
                            size = s.Read(buffer, 0, buffer.Length);
                        }

                        fs.Flush();
                        fs.Close();

                        return ModDownloadResult.SUCCESS;
                    }
                }
            }

            catch(Exception) {
                return ModDownloadResult.ERROR_DOWNLOAD_FAILED;
            }
        }

        public ModMetadata GetMetadata() {
            return this.Metadata;
        }
    }
}
