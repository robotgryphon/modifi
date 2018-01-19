using Newtonsoft.Json;
using RobotGryphon.Modifi.Domains;
using RobotGryphon.Modifi.Mods;
using RobotGryphon.Modifi.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        protected struct ModVersionStub : IModVersion {
            private IDomainHandler Domain;
            private string ModIdentifier;
            private string ModVersion;

            public static ModVersionStub Create(string modVersionString) {
                ModVersionStub stub = new ModVersionStub();
                stub.Domain = ModHelper.GetDomainHandler(modVersionString);

                if(ModHelper.MOD_VERSION_REGEX.IsMatch(modVersionString)) {
                    Match m = ModHelper.MOD_VERSION_REGEX.Match(modVersionString);
                    stub.ModIdentifier = m.Groups["modid"].Value;
                    stub.ModVersion = m.Groups["version"].Value;
                } else {
                    stub.ModIdentifier = "unknown";
                    stub.ModVersion = "latest";
                }
                
                return stub;
            }

            IDomainHandler IModVersion.GetDomain() {
                return Domain;
            }

            string IModVersion.GetModIdentifier() {
                return ModIdentifier;
            }

            string IModVersion.GetModVersion() {
                return ModVersion;
            }

            string IModVersion.GetFilename() {
                throw new NotSupportedException("Error: Do not support filenames on mod stubs, this is for telling other code about a " +
                    "passed-in version from the command line.");
            }
        }

        #region Properties
        /// <summary>
        /// The current pack in the working directory.
        /// </summary>
        public Storage.Pack Pack;

        /// <summary>
        /// The current version of the pack.
        /// </summary>
        public LiteDB.LiteDatabase VersionDatabase;

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

        public static bool IsDomainRegistered(string domain) {
            return INSTANCE.DomainHandlers.ContainsKey(domain.ToLowerInvariant());
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

        public static LiteDB.LiteDatabase FetchCurrentVersion() {
            return INSTANCE.VersionDatabase;
        }

        public static LiteDB.LiteCollection<T> FetchCollection<T>(string collectionName) {
            LiteDB.LiteDatabase db = FetchCurrentVersion();
            if(!db.CollectionExists(collectionName)) return null;

            return db.GetCollection<T>(collectionName);
        }

        public static void LoadVersion(string version = "1.0.0") {
            string path = Path.Combine(Settings.ModifiDirectory, version + ".db");

            UnloadVersion();
            INSTANCE.VersionDatabase = new LiteDB.LiteDatabase(path);
        }

        internal static void UnloadVersion() {
            if(INSTANCE.VersionDatabase != null) INSTANCE.VersionDatabase.Dispose();
            INSTANCE.VersionDatabase = null;
        }

        #region Command Line Arguments
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
                IDomainHandler handler = ModHelper.GetDomainHandler(mod);
                if (handler == null)
                    throw new Exception("That domain does not have a registered handler. Aborting.");

                ModVersionStub modVersion = ModVersionStub.Create(mod);
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
        #endregion
    }
}
