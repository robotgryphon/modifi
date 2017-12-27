using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/**
 * Some code is adapted from Vazkii's Curseforge Modpack Downloader
 * See https://github.com/Vazkii/CMPDL for more information.
 */
namespace RobotGryphon.ModCLI.CurseForge {

    public class CurseForgeMod : Mod {
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public string Filename { get; set; }
        public string Version { get; set; }
        public string Checksum { get; set; }

        protected Regex FILENAME_MATCH = new Regex(@".*?/([^/]*)$");

        public CurseForgeModInfo ModInfo;

        public override ModStatus GetDownloadStatus() {
            string FilePath = Path.Combine(Settings.ModPath, Filename ?? "-");
            if (File.Exists(FilePath)) {
                // Perform checksum match and skip download if match
                using (var md5 = MD5.Create()) {
                    byte[] hash = md5.ComputeHash(File.OpenRead(FilePath));
                    String s = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    if (s.Equals(Checksum)) {
                        return ModStatus.DOWNLOADED;
                    }
                }
            }

            return ModStatus.NOT_DOWNLOADED;
        }

        public async Task<CurseForgeModInfo> GetModInfo() {

            if(this.ModInfo != null) {
                return this.ModInfo;
            }

            Uri api;
            if (!String.IsNullOrEmpty(this.Version))
                api = new Uri(String.Format("{0}/{1}?version={2}", CurseForge.ApiURL, this.ProjectName, this.Version));
            else
                api = new Uri(String.Format("{0}/{1}", CurseForge.ApiURL, this.ProjectName));

            try {
                HttpWebRequest req = (HttpWebRequest) WebRequest.Create(api);
                req.UserAgent = CurseForge.UserAgent;

                WebResponse resp = await req.GetResponseAsync();


                Stream response = resp.GetResponseStream();
                StreamReader reader = new StreamReader(response);
                String modData = await reader.ReadToEndAsync();

                this.ModInfo = JsonConvert.DeserializeObject<CurseForgeModInfo>(modData);

                resp.Close();
                response.Close();
                reader.Close();
            }

            catch (System.Net.WebException) {
                throw new ModDownloadException(ModDownloadResult.ERROR_CONNECTION);
            }

            return this.ModInfo;
        }

        public override async Task<ModDownloadResult> Download() {
            await GetModInfo();
            if (!Directory.Exists(Settings.ModPath)) Directory.CreateDirectory(Settings.ModPath);

            try {
                HttpWebRequest webRequest = WebRequest.CreateHttp(new Uri(ModInfo.Download.URL + "/file"));
                using (WebResponse r = await webRequest.GetResponseAsync()) {
                    Uri downloadUri = r.ResponseUri;

                    if (!FILENAME_MATCH.IsMatch(downloadUri.AbsoluteUri))
                        return ModDownloadResult.ERROR_INVALID_FILENAME;

                    Match m = FILENAME_MATCH.Match(downloadUri.AbsoluteUri);
                    String filename = m.Groups[1].Value;

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
    }
}
