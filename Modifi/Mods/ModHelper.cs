using RobotGryphon.Modifi.Domains;
using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi {
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

        /// <summary>
        /// Fetches a domain handler for a given mod stub string.
        /// </summary>
        /// <param name="modVersion">Mod version string, i.e. "curseforge:jei@00000000"</param>
        /// <returns>The registered domain handler for the mod, or null.</returns>
        public static IDomainHandler GetDomainHandler(string modVersion) {

            Match m = SplitModString(modVersion);
            string domain = m.Groups["domain"].Value.ToLowerInvariant();
            if (Modifi.IsDomainRegistered(domain))
                return Modifi.GetDomainHandler(domain);

            return null;
        }

        /// <summary>
        /// Prints out some mod information in a standardized manner.
        /// </summary>
        /// <param name="meta"></param>
        public static void PrintModInformation(IModMetadata meta, bool header = false) {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine(meta.GetName());
            Console.ForegroundColor = ConsoleColor.White;

            if (meta.HasDescription()) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(meta.GetDescription());
            }

            if (header) {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("".PadLeft(Console.BufferWidth - 10, '='));
            }

            Console.ResetColor();
        }

        public static string GetModIdentifier(string mod) {
            Match m = SplitModString(mod);
            return m.Groups["modid"].Value;
        }

        public static string GetModVersion(string mod) {
            Match m = SplitModString(mod);
            if(m.Groups["version"].Length > 0)
                return m.Groups["modid"].Value;
            return null;
        }
    }
}