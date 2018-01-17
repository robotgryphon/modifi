using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi {
    public abstract class ModHelper {

        public static IModVersion SplitDomainAndID(string modid) {

            Regex r = new Regex(@"(?<domain>[\w]+):(?<modid>[\w_\-]+)(@(?<version>[\d]+))?");
            if(!r.IsMatch(modid)) {
                // TODO: Change this to a proper exception class
                throw new Exception("Invalid modid format: " + modid + ". Expected format of domain:modid (like Minecraft resource names).");
            }

            Match m = r.Match(modid);

            return new ModVersionStub {
                Domain = m.Groups["domain"].Value,
                Identifier = m.Groups["modid"].Value,
                Version = m.Groups["version"].Value
            };
        }

        /// <summary>
        /// Prints out some mod information in a standardized manner.
        /// </summary>
        /// <param name="meta"></param>
        public virtual void PrintModInformation(IModMetadata meta) {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.Write(meta.GetName());
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" (");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("v{0}", meta.GetModVersion() ?? "0");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(")");

            Console.ForegroundColor = ConsoleColor.Green;
            Domains.IDomainHandler domainHandler = Modifi.GetDomainHandler(meta.GetDomain());
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

        /// <summary>
        /// Downloads a mod from a domain, given a set of defined metadata.
        /// </summary>
        /// <param name="meta">The mod's metadata. The domain should handle conversion to what it needs.</param>
        /// <returns>A result on the download.</returns>
        public abstract Task<ModDownloadResult> DownloadMod(IModMetadata meta);


        public abstract IEnumerable<IModVersion> FetchRecentModVersions(IModMetadata meta);
    }
}