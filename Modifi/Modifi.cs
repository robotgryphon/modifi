using Modifi.Packs;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;

namespace RobotGryphon.Modifi {

    public class Modifi {

        public static Pack DefaultPack {
            get {
                return LoadOrGetDefaultPack();
            }

            private set { }
        }

        protected static Pack _DefaultPack;

        public static IEnumerable<string> DomainSearchPaths {
            get;
            protected set;
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

        public static ILogger DefaultLogger = new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message}{NewLine}", theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
            .CreateLogger();

        public static Serilog.ILogger CreateLogger(string id) {
            ILogger log = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Id}] {Message}{NewLine}")
                .Enrich.WithProperty("Id", id)
                .CreateLogger();

            return log;
        }
        /// <summary>
        /// Loads the pack from the Modifi directory.
        /// </summary>
        /// <exception cref="IOException"></exception>
        public static Pack LoadOrGetDefaultPack() {
            if (Modifi._DefaultPack != null) return _DefaultPack;

            if(!Directory.Exists(Settings.ModifiDirectory)) {
                Console.Error.WriteLine("Error: Modifi directory not found. Please run pack init first.");
                throw new IOException();
            }

            if(!File.Exists(Settings.PackFile)) {
                Console.Error.WriteLine("Error: Modifi pack file not found. Please run pack init command first.");
                throw new IOException();
            }

            JsonSerializer s = JsonSerializer.Create(Settings.JsonSerializer);

            Pack p = new Pack();

            // Read JSON pack file and copy data into the new pack object
            StreamReader sr = new StreamReader(File.OpenRead(Settings.PackFile));
            s.Populate(new JsonTextReader(sr), p);
            sr.Close();

            // Set cache object for quick fetch
            Modifi._DefaultPack = p;
            return p;
        }

        internal static void ExecuteArguments(string[] input) {
            Commands.CommandHandler.ExecuteArguments(input);
        }
    }
}
