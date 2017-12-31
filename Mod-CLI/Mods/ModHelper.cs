using RobotGryphon.ModCLI.Mods;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI {
    internal class ModHelper {

        public static ModVersion SplitDomainAndID(string modid) {

            Regex r = new Regex(@"@(?<domain>[\w]+)::(?<modid>[\w_\-]+)");
            if(!r.IsMatch(modid)) {
                // TODO: Change this to a proper exception class
                throw new Exception("Invalid modid format: " + modid + ". Expected format of @domain::modid.");
            }

            Match m = r.Match(modid);

            ModVersion i = new ModVersion();
            i.Domain = m.Groups["domain"].Value;
            i.ModId = m.Groups["modid"].Value;

            return i;
        }

        /// <summary>
        /// Gets information about a mod.
        /// </summary>
        /// <param name="modid">The mod (domain and id) to fetch information for.</param>
        public async static Task<ModMetadata> GetModInfo(string modid) {
            ModVersion id = SplitDomainAndID(modid);

            string domain = id.Domain;

            if(domain.ToLowerInvariant() != "curseforge") {
                throw new NotSupportedException("Custom domains are not yet supported.");
            }

            return await CurseForge.CurseForgeHelper.GetModMetadata(id);
        }
    }
}