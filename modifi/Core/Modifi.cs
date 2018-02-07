using Modifi.Packs;
using Modifi.Domains;
using Newtonsoft.Json;
using Serilog;
using Serilog.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace Modifi {

    public class Modifi {

        protected static Modifi INSTANCE;

        protected Dictionary<string, IDomain> LoadedDomains;

        public static IEnumerable<string> DomainSearchPaths {
            get;
            protected set;
        }

        public static bool DEBUG_MODE = false;

        protected Modifi() {
            this.LoadedDomains = new Dictionary<string, IDomain>();
        }

        public static Modifi GetInstance() {
            if(INSTANCE == null) {
                INSTANCE = new Modifi();
            }

            return INSTANCE;
        }

        public bool IsDomainLoaded(string domain) {
            return LoadedDomains.ContainsKey(domain);
        }
        
        /// <summary>
        /// Loads a domain and adds it to a pack's loaded domain list.        /// </summary>
        /// <param name="pack"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public IDomain LoadDomain(string domain) {

            if (LoadedDomains.ContainsKey(domain)) return LoadedDomains[domain];

            // Perform domain search
            bool domainFound = false;
            string domainPath = null;

            if (Modifi.DomainSearchPaths == null)
                Modifi.LoadSearchPaths();

            foreach (string path in Modifi.DomainSearchPaths) {
                string pathCheck = Path.Combine(path, domain + ".dll");

                Modifi.DefaultLogger.Debug("Trying to load domain handler from {0}...", pathCheck);
                if(File.Exists(pathCheck)) {
                    domainPath = pathCheck;
                    domainFound = true;
                    break;
                }
            }

            if (!domainFound)
                throw new DllNotFoundException("Cannot find the domain DLL.");

            Assembly cfAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(domainPath);
            Type controller = cfAssembly.ExportedTypes.First(t => t.GetInterfaces().Contains(typeof(IDomain)));

            if(controller == null) {
                throw new Exception("Cannot find the domain handler inside domain assembly.");
            }

            try {
                IDomain domainInstance = Activator.CreateInstance(controller) as IDomain;
                LoadedDomains.Add(domain, domainInstance);
                return domainInstance;
            }

            catch(Exception e) {
                Modifi.DefaultLogger.Error("Error loading domain handler from {0}:", domainPath);
                Modifi.DefaultLogger.Error(e.Message);
                return null;
            }
            
        }

        public IDomain GetDomain(string id) {
            if (LoadedDomains.ContainsKey(id)) return LoadedDomains[id];
            try {

                ConsoleColor old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Gray;
                Modifi.DefaultLogger.Information("Domain {0:l} not loaded, loading it now.", id);
                Console.ForegroundColor = old;

                IDomain domain = LoadDomain(id);
                return domain;
            }

            catch(Exception e) {
                Modifi.DefaultLogger.Error(e.Message);
                return null;
            }
        }

        public static void LoadSearchPaths() {
            List<string> searchPaths = new List<string>();
            searchPaths.Add(Path.Combine(Settings.ModifiDirectory, "domains"));
            searchPaths.Add(Settings.DomainsDirectory);

            // Load JSON settings file
            if (File.Exists(Path.Combine(Settings.ModifiDirectory, "domains.json"))) {
                string domainsJSON = File.ReadAllText(Path.Combine(Settings.ModifiDirectory, "domains.json"));
                IEnumerable<string> paths = JsonConvert.DeserializeObject<IEnumerable<string>>(domainsJSON);

                Regex currentDirectoryRegex = new Regex(@"{dir}\\?\/?(.*)");
                foreach(string path in paths) {
                    string path2 = path.Replace('/', Path.DirectorySeparatorChar);
                    if (currentDirectoryRegex.IsMatch(path)) {
                        string pathPart = currentDirectoryRegex.Match(path2).Groups[1].Value;
                        path2 = Path.Combine(Environment.CurrentDirectory, pathPart);
                    }

                    searchPaths.Add(path2);
                }
            }

            DomainSearchPaths = searchPaths;
        }

        public static ILogger DefaultLogger = GenerateDefaultLogger();

        private static ILogger GenerateDefaultLogger() {
            string template = "{Message}{NewLine}";
            
            if(Modifi.DEBUG_MODE)
                template = "[{Level}] " + template;

            LoggerConfiguration c = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: template, theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code);

            if(Modifi.DEBUG_MODE)
                c.MinimumLevel.Debug();

            return c.CreateLogger();
        }

        public static Serilog.ILogger CreateLogger(string id) {
            ILogger log = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Id}] {Message}{NewLine}")
                .Enrich.WithProperty("Id", id)
                .CreateLogger();

            return log;
        }

        internal static void ExecuteArguments(string[] input) {
            Commands.CommandHandler.ExecuteArguments(input);
        }
    }
}
