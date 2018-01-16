using Newtonsoft.Json;
using RobotGryphon.Modifi.Domains;
using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static RobotGryphon.Modifi.Modifi;

namespace RobotGryphon.Modifi.Domains.CurseForge {
    public class CurseForge : IDomainHandler {
        public const string UserAgent = "Mozilla/5.0 (X11; Linux x86_64)";

        public static Uri BaseUri { get; internal set; }
            = new Uri("https://minecraft.curseforge.net");

        public const string ApiURL = "https://api.cfwidget.com/mc-mods/minecraft";

        public static CurseForge INSTANCE = new CurseForge();

        protected CurseforgeModHelper ModHelper;

        private CurseForge() {
            ModHelper = new CurseforgeModHelper();
        }

        public static async Task<CurseforgeModMetadata> GetModInfo(ModVersion version) {

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

                CurseforgeModMetadata modInfo = JsonConvert.DeserializeObject<CurseforgeModMetadata>(modData);

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
        public static async Task<IModMetadata> GetModMetadata(ModVersion version) => await GetModInfo(version);

        public string GetProjectURL(IModMetadata meta) {
            if (!(meta is CurseforgeModMetadata)) throw new Exception("Expected curseforge mod, got invalid.");
            return ((CurseforgeModMetadata)meta).URLs.Project;
        }

        public void HandleModVersions(ModVersion mod) {
            Task<CurseforgeModMetadata> metaTask = GetModInfo(mod);
            CurseforgeModMetadata meta = metaTask.Result;
            string mcVersion = Modifi.GetMinecraftVersion();

            if (!meta.Versions.ContainsKey(mcVersion)) {
                Console.Error.WriteLine("Error: Mod not supported for Minecraft version {0}.", mcVersion);
                return;
            }

            IEnumerable<CurseforgeModVersion> versions = meta.Versions[mcVersion];

            // TODO: Configurable number of results shown?
            IEnumerable<CurseforgeModVersion> limitedList = versions.Take(5);

            ModHelper.PrintModInformation(meta);
            foreach (CurseforgeModVersion version in limitedList) {
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

        public void HandleModAdd(ModVersion mod) {
            throw new NotImplementedException();
        }

        public void HandleModRemove(ModVersion mod) {
            throw new NotImplementedException();
        }

        public void HandleModInformation(ModVersion mod) {
            Task<CurseforgeModMetadata> meta = GetModInfo(mod);
            CurseforgeModMetadata metaData = meta.Result;

            // Print out mod information
            ModHelper.PrintModInformation(metaData);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine("URL: {0}", metaData.RequestedVersion.DownloadURL);
            Console.WriteLine("File ID: {0}", metaData.RequestedVersion.FileId);
        }

        public ModDownloadResult HandleModDownload(ModVersion modVersion) {
            Task<CurseforgeModMetadata> meta = CurseForge.GetModInfo(modVersion);
            CurseforgeModMetadata meta2 = meta.Result;

            Task<ModDownloadResult> result = ModHelper.DownloadMod(meta2);
            return result.Result;
        }
    }
}
