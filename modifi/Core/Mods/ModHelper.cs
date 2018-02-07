using Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Modifi {
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

        internal static string GetDomainName(string mod) {
            Match m = SplitModString(mod);
            return m.Groups["domain"].Value.ToLower();
        }

        /// <summary>
        /// Prints out some mod information in a standardized manner.
        /// </summary>
        /// <param name="meta"></param>
        public static void PrintModInformation(ModMetadata meta, bool header = false) {
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