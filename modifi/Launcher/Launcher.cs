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
            AppRunner<Commands.Base> r = new AppRunner<Commands.Base>(new CommandDotNet.Models.AppSettings() {
                Case = CommandDotNet.Models.Case.KebabCase
            });            

            return r.Run(args);
        }
    }

    
}
