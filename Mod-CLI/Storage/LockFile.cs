using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using RobotGryphon.ModCLI.CurseForge;
using RobotGryphon.ModCLI.Mods;

namespace RobotGryphon.ModCLI {
    public class LockFile {

        public class Domain {
            public String Name;
            public List<Uri> Servers;

            public Boolean Hosted = true;

            public Func<Mod, int, Uri> ModDownloadGenerator;

            public Domain(String key) {
                this.Name = key;
                this.Servers = new List<Uri>();

                this.ModDownloadGenerator = this.DefaultDownloadPath;
            }

            public Uri DefaultDownloadPath(Mod m, int server = 0) {
                return new Uri(this.Servers[server], m.Filename);
            }
        }

        public string Name { get; set; }
        public int ModCount {
            get {
                return this.Mods.Count;
            }

            private set { }
        }

        public Dictionary<string, Domain> PackDomains;
        public Dictionary<string, Domain> ModDomains;

        public List<Mod> Mods { get; protected set; }

        public LockFile() {
            this.PackDomains = new Dictionary<string, Domain>();
            this.ModDomains = new Dictionary<string, Domain>();
            this.Mods = new List<Mod>();

            Domain curseforge = new Domain("curseforge");
            curseforge.ModDownloadGenerator = (m, i) => {
                String u = String.Format("https://minecraft.curseforge.net/projects/{0}/files/{1}/download/file", m.Name, m.Version);
                return new Uri(u);
            };

            this.ModDomains.Add("curseforge", curseforge);
        }

        public static LockFile Parse(String path) {
            LockFileParser parser = new LockFileParser();
            parser.Parse(path).Wait();

            return parser.GetFinal();
        }
    }

    public class LockFileParser {

        System.Text.RegularExpressions.Regex MOD_LINE_CHECK = new System.Text.RegularExpressions.Regex(@"\@(?<domain>[\w]+)\/(?<name>[\w]+)");

        private enum BlockType {
            PACK_DOMAIN,
            MOD_DOMAIN,
            MOD,
            VARIABLE,
            UNKNOWN,
            EMPTY
        }

        protected LockFile parsed;
        public LockFileParser() {
            this.parsed = new LockFile();
        }

        internal LockFile GetFinal() {
            return parsed;
        }

        internal async Task Parse(string path) {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            using (FileStream fileStream = File.OpenRead(path)) {
                using (StreamReader reader = new StreamReader(fileStream)) {
                    while (!reader.EndOfStream) {
                        String line = reader.ReadLine();
                        BlockType t = BlockType.UNKNOWN;

                        if (String.IsNullOrEmpty(line)) {
                            t = BlockType.EMPTY;
                        } else {
                            switch (line[0]) {
                                case '@':
                                    if (MOD_LINE_CHECK.IsMatch(line))
                                        t = BlockType.MOD;
                                    else
                                        t = BlockType.MOD_DOMAIN;
                                    break;

                                case '$':
                                    t = BlockType.PACK_DOMAIN;
                                    break;

                                case '-':
                                    t = BlockType.VARIABLE;
                                    break;
                            }

                            WorkBlock(reader, t, line);
                        }
                    }
                }
            }
        }

        private void WorkBlock(StreamReader reader, BlockType type, String line) {
            if(type == BlockType.VARIABLE) {
                return;
            }

            switch(type) {
                case BlockType.MOD:
                    HandleModBlock(reader, line);
                    break;

                case BlockType.MOD_DOMAIN:
                    Console.WriteLine("Reached mod domain.");
                    break;

                case BlockType.PACK_DOMAIN:
                    Console.WriteLine("Reached pack domain.");
                    break;
            }

            line = reader.ReadLine();
        }

        private void HandleModBlock(StreamReader reader, String line) {
            Console.WriteLine("Reached mod.");
            Match m = this.MOD_LINE_CHECK.Match(line);

            Mod mod;
            switch (m.Groups["domain"].Value.ToLowerInvariant()) {
                case "curseforge":
                    mod = new CurseForgeMod();
                    break;

                default:
                    mod = new HostedMod();
                    break;
            }

            mod.Domain = m.Groups["domain"].Value;
            mod.Name = m.Groups["name"].Value;

            string fileLine = reader.ReadLine();

            Regex modVersionLine = new Regex(@"^\s*\[(?<fileID>[\w]+)\]\s+(?<filename>[\w\s\.\-_]+)\|?\s?(?<checksum>[\w]+)?");
            Match m2 = modVersionLine.Match(fileLine);

            mod.Version = m2.Groups["fileID"].Value;
            mod.Filename = m2.Groups["filename"].Value.Trim();
            if(m2.Groups["checksum"] != null)
                mod.Checksum = m2.Groups["checksum"].Value;

            this.parsed.Mods.Add(mod);
        }
    }
}
