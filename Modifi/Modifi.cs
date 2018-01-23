using Newtonsoft.Json;
using RobotGryphon.Modifi.Domains;
using RobotGryphon.Modifi.Packs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi {   

    public class Modifi : IDisposable {

        #region Properties
        /// <summary>
        /// The current pack in the working directory.
        /// </summary>
        private Pack Pack;
        private string InstalledVersionString;

        protected VersionFile _Version;
        public static VersionFile CurrentVersion {
            get { return INSTANCE._Version; }
            private set { }
        }

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
                { "curseforge", Domains.CurseForge.CurseforgeDomainHandler.INSTANCE }
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
            INSTANCE.InstalledVersionString = INSTANCE.Pack.Installed;
            sr.Close();

            INSTANCE._Version = new VersionFile(INSTANCE.InstalledVersionString);
        }

        public static string GetPackName() {
            if (!INSTANCE.PackLoaded) return "<No Pack Loaded>";
            return INSTANCE.Pack.Name;
        }

        public static ModifiVersion GetInstalledVersion() {
            return ModifiVersion.FromHash(INSTANCE.InstalledVersionString);
        }

        internal static void ExecuteArguments(string[] input) {
            Commands.CommandHandler.ExecuteArguments(input);
        }

        public void Dispose() {
            if(_Version != null) ((IDisposable)_Version).Dispose();
        }
    }
}
