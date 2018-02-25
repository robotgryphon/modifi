using Modifi.Storage;
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

        public static bool DEBUG_MODE = false;

        public static string DEFAULT_PACK_PATH = Path.Combine(Environment.CurrentDirectory, "pack.json");
        
        protected DomainManager _DomainHandler;
        
        public static DomainManager DomainHandler {
            get { return INSTANCE._DomainHandler; }
            set { }
        }

        protected Modifi() {
            this._DomainHandler = new DomainManager("modifi-domains");
        }

        public static Modifi GetInstance() {
            if(INSTANCE == null) {
                INSTANCE = new Modifi();
            }

            return INSTANCE;
        }

        public static void LoadSearchPaths() {
            
            DomainManager handler = INSTANCE._DomainHandler;

            // Automatically try to find stuff in the modifi directory, current directory, and app directory
            handler.AddPath(Path.Combine(Settings.ModifiDirectory, "domains"));
            handler.AddPath(Path.Combine(Environment.CurrentDirectory, "domains"));
            handler.AddPath(Path.Combine(Settings.AppDirectory, "domains"));

            // Load domains JSON file from the app directory if it exists
            if (File.Exists(Path.Combine(Settings.AppDirectory, "domains.json"))) {
                string domainsJSON = File.ReadAllText(Path.Combine(Settings.ModifiDirectory, "domains.json"));
                IEnumerable<string> paths = JsonConvert.DeserializeObject<IEnumerable<string>>(domainsJSON);

                Regex currentDirectoryRegex = new Regex(@"{dir}\\?\/?(.*)");
                foreach(string path in paths) {
                    string path2 = path.Replace('/', Path.DirectorySeparatorChar);
                    if (currentDirectoryRegex.IsMatch(path)) {
                        string pathPart = currentDirectoryRegex.Match(path2).Groups[1].Value;
                        path2 = Path.Combine(Environment.CurrentDirectory, pathPart);
                        handler.AddPath(path2);
                    }
                }
            }
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
