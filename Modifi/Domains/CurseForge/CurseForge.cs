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

        public static async Task<CurseforgeModMetadata> GetModInfo(IModVersion version) {

            Console.WriteLine("Fetching metadata for {0}...", version.GetModIdentifier());

            Uri api;
            if (!String.IsNullOrEmpty(version.GetModVersion()))
                api = new Uri(String.Format("{0}/{1}?version={2}", CurseForge.ApiURL, version.GetModIdentifier(), version.GetModVersion()));
            else
                api = new Uri(String.Format("{0}/{1}", CurseForge.ApiURL, version.GetModIdentifier()));

            try {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(api);
                req.UserAgent = CurseForge.UserAgent;

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
        public static async Task<IModMetadata> GetModMetadata(IModVersion version) => await GetModInfo(version);

        public string GetProjectURL(IModMetadata meta) {
            if (!(meta is CurseforgeModMetadata)) throw new Exception("Expected curseforge mod, got invalid.");
            return ((CurseforgeModMetadata)meta).URLs.Project;
        }

        public void HandleModVersions(IModVersion mod) {
            Task<CurseforgeModMetadata> metaTask = GetModInfo(mod);
            CurseforgeModMetadata meta = metaTask.Result;

            IEnumerable<CurseforgeModVersion> latestVersions = (IEnumerable<CurseforgeModVersion>) ModHelper.FetchRecentModVersions(meta);

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

        public void HandleModAdd(IModVersion mod) {

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

        public void HandleModRemove(IModVersion mod) {
            throw new NotImplementedException();
        }

        public void HandleModInformation(IModVersion mod) {
            CurseforgeModMetadata meta = GetModInfo(mod).Result;

            // Print out mod information
            ModHelper.PrintModInformation(meta);
        }

        public ModDownloadResult HandleModDownload(IModVersion modVersion) {
            CurseforgeModMetadata meta = CurseForge.GetModInfo(modVersion).Result;

            Task<ModDownloadResult> result = ModHelper.DownloadMod(meta);
            return result.Result;
        }
    }
}
