using Newtonsoft.Json;
using RobotGryphon.Modifi.Domains;
using RobotGryphon.Modifi.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi {

    enum ActionType {
        MODS,
        DOMAINS,
        INIT,
        PACK,
        HELP,
        DOWNLOAD
    }

    public class Modifi {

        #region Properties
        /// <summary>
        /// The current pack in the working directory.
        /// </summary>
        public Storage.Pack Pack;

        /// <summary>
        /// The current version of the pack.
        /// </summary>
        public Storage.Version version;

        public Dictionary<string, IDomainHandler> DomainHandlers;

        private static Modifi _INSTANCE;
        public static Modifi INSTANCE {
            get {
                if (_INSTANCE != null) return _INSTANCE;
                _INSTANCE = new Modifi();
                return _INSTANCE;
            }

            set { }
        }

        public bool PackLoaded { get; private set; }
        #endregion

        private Modifi() {

            PackLoaded = false;

            // Instantiate the domain handlers, make sure the built-in curseforge handler is added
            DomainHandlers = new Dictionary<string, IDomainHandler> {
                { "curseforge", Domains.CurseForge.CurseForge.INSTANCE }
            };
        }

        /// <summary>
        /// Quick means of getting a domain handler from a pack.
        /// If the domain is not registered, null will be returned.
        /// </summary>
        /// <param name="v">The domain handler to fetch.</param>
        /// <returns></returns>
        internal static IDomainHandler GetDomainHandler(string v) {
            if (!INSTANCE.PackLoaded) LoadPack();

            if (INSTANCE.DomainHandlers.ContainsKey(v.ToLower()))
                return INSTANCE.DomainHandlers[v.ToLower()];

            return null;
        }

        /// <summary>
        /// Assures that there's a lock file for both the pack and the pack's specified version.
        /// If a file does not exist, it asks to create it.
        /// </summary>
        public static async void AssureLockFiles() {

            JsonSerializer s = JsonSerializer.Create(Settings.JsonSerializer);

            // If the modifi directory wasn't found, or the pack file does not exist, generate it
            if (!Directory.Exists(Settings.ModifiDirectory) || !File.Exists(Settings.PackFile)) {
                INSTANCE.Pack = await PackHelper.GeneratePackFile();
            } else {
                LoadPack();
            }

            // Generate new version file if not found
            String versionFile = Path.Combine(Settings.ModifiDirectory, INSTANCE.Pack.Installed + ".json");
            if (!File.Exists(versionFile)) {
                INSTANCE.version = await VersionHelper.GenerateVersionFile(INSTANCE.Pack.Installed);
            } else {
                // Deserialize requested version file.
                StreamReader vr = new StreamReader(File.OpenRead(versionFile));
                INSTANCE.version = s.Deserialize<Storage.Version>(new JsonTextReader(vr));
                vr.Close();                
            }
        }

        /// <summary>
        /// Wrapper to quickly check the pack and get the pack's requested Minecraft version.
        /// </summary>
        /// <returns></returns>
        public static string GetMinecraftVersion() {
            if (!INSTANCE.PackLoaded) LoadPack();

            if(String.IsNullOrEmpty(INSTANCE.Pack.MinecraftVersion)) {
                // invalid version

                throw new Exception("Invalid pack version.");
            }

            return INSTANCE.Pack.MinecraftVersion;
        }

        public static void LoadPack() {
            if (INSTANCE.PackLoaded) return;

            JsonSerializer s = JsonSerializer.Create(Settings.JsonSerializer);
            StreamReader sr = new StreamReader(File.OpenRead(Settings.PackFile));
            INSTANCE.Pack = s.Deserialize<Pack>(new JsonTextReader(sr));
            INSTANCE.PackLoaded = true;
            sr.Close();
        }

        /// <summary>
        /// Given a set of arguments, execute the things that need to happen.
        /// </summary>
        /// <param name="input"></param>
        public static void ExecuteArguments(string[] input) {
            if(input.Length == 0) throw new ArgumentException("Nothing to do");
            
            switch(Enum.Parse(typeof(ActionType), input[0].ToUpperInvariant())) {
                case ActionType.MODS:
                    HandleModAction(input);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public enum ModActions {
            // Unknown action- leave this as first so it can be skipped in available actions
            INVALID,

            ADD,
            REMOVE,
            INFO,
            LIST,
            DOWNLOAD,
            VERSIONS
        }

        private static void HandleModAction(string[] input) {
            IEnumerable<string> arguments = input.Skip(1);
            ModActions action;
            try { action = (ModActions) Enum.Parse(typeof(ModActions), arguments.First().ToUpperInvariant()); }
            catch(Exception) { action = ModActions.INVALID;  }

            IEnumerable<string> mods = arguments.Skip(1);

            foreach (string mod in mods) {
                Mods.ModVersion modVersion = ModHelper.SplitDomainAndID(mod);

                if (!INSTANCE.DomainHandlers.ContainsKey(modVersion.Domain.ToLower())) {
                    throw new NotImplementedException("Custom domains are not yet supported.");
                }

                IDomainHandler handler = INSTANCE.DomainHandlers[modVersion.Domain.ToLower()];
                switch (action) {

                    case ModActions.INVALID:
                        IEnumerable<string> actions = Enum.GetNames(typeof(ModActions)).Skip(1);
                        Console.Error.WriteLine("Invalid mod action, choose from: {0}", String.Join(", ", actions));
                        break;

                    case ModActions.ADD:
                        handler.HandleModAdd(modVersion);
                        break;

                    case ModActions.REMOVE:
                        handler.HandleModRemove(modVersion);
                        break;

                    case ModActions.INFO:
                        handler.HandleModInformation(modVersion);
                        break;

                    case ModActions.VERSIONS:
                        handler.HandleModVersions(modVersion);
                        break;

                    case ModActions.DOWNLOAD:
                        handler.HandleModDownload(modVersion);
                        break;

                    default:
                        throw new Exception("Invalid mod action.");
                }
            }
        }
    }
}
