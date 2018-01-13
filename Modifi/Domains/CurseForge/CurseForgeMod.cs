using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using RobotGryphon.Modifi.Mods;

namespace RobotGryphon.Modifi.Domains.CurseForge {

    public class CurseForgeMod : IMod {

        /// <summary>
        /// The curseforge id of the mod (ie: jei)
        /// </summary>
        public string ProjectId { get; set; }

        protected Regex FILENAME_MATCH = new Regex(@".*?/([^/]*)$");

        public CurseforgeModMetadata Metadata;

        public CurseForgeMod() { }


        /// <summary>
        /// Download the mod using information found in Metadata.
        /// </summary>
        /// <returns></returns>
        public async Task<ModDownloadResult> Download() {

            if (this.Metadata.RequestedVersion.FileId == null)
                throw new Exception("Error during download: Mod metadata has not been fetched from Curseforge yet.");

            if (!Directory.Exists(Settings.ModPath)) Directory.CreateDirectory(Settings.ModPath);

            try {
                HttpWebRequest webRequest = WebRequest.CreateHttp(new Uri(Metadata.RequestedVersion.DownloadURL + "/file"));
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

                        return ModDownloadResult.SUCCESS;
                    }
                }
            }

            catch(Exception) {
                return ModDownloadResult.ERROR_DOWNLOAD_FAILED;
            }
        }

        public IModMetadata GetMetadata() {
            return Metadata;
        }

        public string GetName() {
            return this.Metadata.Title ?? "Unknown Curseforge Mod";
        }
    }
}
