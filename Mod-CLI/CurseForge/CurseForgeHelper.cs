using Newtonsoft.Json;
using RobotGryphon.ModCLI.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.CurseForge {
    public static class CurseForgeHelper {

        public static async Task<CurseForgeModInfo> GetModInfo(ModVersion version) {

            Uri api;
            if (!String.IsNullOrEmpty(version.Version))
                api = new Uri(String.Format("{0}/{1}?version={2}", CurseForge.ApiURL, version.ModId, version.Version));
            else
                api = new Uri(String.Format("{0}/{1}", CurseForge.ApiURL, version.ModId));

            try {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(api);
                req.UserAgent = CurseForge.UserAgent;

                WebResponse resp = await req.GetResponseAsync();


                Stream response = resp.GetResponseStream();
                StreamReader reader = new StreamReader(response);
                String modData = await reader.ReadToEndAsync();

                CurseForgeModInfo modInfo = JsonConvert.DeserializeObject<CurseForgeModInfo>(modData);

                resp.Close();
                response.Close();
                reader.Close();

                return modInfo;
            }

            catch (System.Net.WebException) {
                throw new RobotGryphon.ModCLI.Mods.ModDownloadException(ModDownloadResult.ERROR_CONNECTION);
            }
        }

        public static async Task<ModMetadata> GetModMetadata(ModVersion version) {
            CurseForgeModInfo cfInfo = await GetModInfo(version);

            ModMetadata meta = new ModMetadata();
            meta.Name = cfInfo.Title;
            meta.DownloadURL = cfInfo.Download.DownloadURL;
            meta.Domain = "curseforge";
            meta.ModId = version.ModId;
            meta.Version = String.IsNullOrEmpty(version.Version) ? cfInfo.Download.Version : version.Version;
            meta.FileId = cfInfo.Download.FileId;
            meta.Description = cfInfo.Description;
            meta.Versions = cfInfo.Download.Versions;

            return meta;
        }
    }
}
