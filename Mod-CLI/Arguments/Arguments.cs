using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;

namespace RobotGryphon.ModCLI {
    [Verb("mods", HelpText = "Manages installed mods")]
    public class ModOptions {

        [Value(0, HelpText = "add, remove, info")]
        public string Switch { get; set; }

        [Value(1)]
        public IEnumerable<string> ModList { get; set; }
    }

    // Verb Mods
    /*
     * --add @domain::modid
     * --add-mods @d::1 @d::2 .. @d::n
     * 
     * --remove @domain::modid
     * --remove-mods @d::1 .. @d::n
     */

    // Verb Domains
    [Verb("domains", HelpText = "Manages domains where mods can be installed from.")]
    public class DomainOptions { }

    // Verb Packs
    [Verb("packs", HelpText = "Manages the pack.")]
    public class PackOptions { }
}
