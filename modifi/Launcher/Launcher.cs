using Modifi.Domains;
using Modifi.Mods;
using Modifi.Storage;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// TODO: Make a disabled flag under mods

using CommandDotNet;

namespace Modifi {

    public class Launcher {

        public static int Main(string[] args) {

            string[] input = args;
            
            Modifi.DEBUG_MODE = true;
            if(Modifi.DEBUG_MODE) input = new string[] { "mods", "download", "curseforge:jei", "--debug-mode" };

            AppRunner<Commands.Base> r = new AppRunner<Commands.Base>(new CommandDotNet.Models.AppSettings() {
                Case = CommandDotNet.Models.Case.KebabCase
            });            

            return r.Run(input);
        }
    }

    
}
