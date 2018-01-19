using RobotGryphon.Modifi.Domains;
using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi {
    public abstract class ModHelper {

        public static Regex MOD_VERSION_REGEX = new Regex(@"(?<domain>[\w]+):(?<modid>[\w_\-]+)(@(?<version>[\d]+))?");

        /// <summary>
        /// Fetches a domain handler for a given mod stub string.
        /// </summary>
        /// <param name="modVersion">Mod version string, i.e. "curseforge:jei@00000000"</param>
        /// <returns>The registered domain handler for the mod, or null.</returns>
        public static IDomainHandler GetDomainHandler(string modVersion) {

            
            if(!MOD_VERSION_REGEX.IsMatch(modVersion)) {
                // TODO: Change this to a proper exception class
                throw new Exception("Invalid modid format: " + modVersion + ". Expected format of domain:modid (like Minecraft resource names).");
            }

            Match m = MOD_VERSION_REGEX.Match(modVersion);

            string domain = m.Groups["domain"].Value.ToLowerInvariant();
            if (Modifi.IsDomainRegistered(domain))
                return Modifi.GetDomainHandler(domain);

            return null;
        }

        /// <summary>
        /// Prints out some mod information in a standardized manner.
        /// </summary>
        /// <param name="meta"></param>
        public static void PrintModInformation(IModMetadata meta) {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.Write(meta.GetName());
            Console.ForegroundColor = ConsoleColor.White;

            //Console.Write(" (");
            //Console.ForegroundColor = ConsoleColor.Magenta;
            //Console.Write("v{0}", meta.GetModVersion() ?? "0");
            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine(")");

            Console.ForegroundColor = ConsoleColor.Green;
            Domains.IDomainHandler domainHandler = meta.GetDomain();
            Console.WriteLine(domainHandler.GetProjectURL(meta));

            if (meta.HasDescription()) {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(meta.GetDescription());
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("".PadLeft(Console.BufferWidth - 10, '='));
            Console.ResetColor();
        }
    }
}