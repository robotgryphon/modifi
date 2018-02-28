using Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modifi.Mods {
    public abstract class ModHelper {

        public static Regex MOD_VERSION_REGEX = new Regex(@"(?<domain>[\w]+):(?<modid>[\w_\-]+)(@(?<version>[\d]+))?");

        protected static Match SplitModString(string modVersion) {
            if (!MOD_VERSION_REGEX.IsMatch(modVersion)) {
                // TODO: Change this to a proper exception class
                throw new Exception("Invalid modid format: " + modVersion + ". Expected format of domain:modid (like Minecraft resource names).");
            }

            Match m = MOD_VERSION_REGEX.Match(modVersion);
            return m;
        }

        public static string GetDomainName(string mod) {
            Match m = SplitModString(mod);
            return m.Groups["domain"].Value.ToLower();
        }

        public static string GetModIdentifier(string mod) {
            Match m = SplitModString(mod);
            return m.Groups["modid"].Value;
        }

        public static string GetModVersion(string mod) {
            Match m = SplitModString(mod);
            if(m.Groups["version"].Length > 0)
                return m.Groups["version"].Value;
            return null;
        }
    }
}