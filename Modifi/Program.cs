using Modifi.Domains;
using Modifi.Mods;
using Modifi.Packs;
using Modifi.Storage;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// TODO: Make a disabled flag under mods

namespace Modifi {

    public class Program {

        public static void Main(string[] args) {

            // Modifi.AssureLockFiles();
            // Modifi.LoadSearchPaths();

            string[] input = args;
            
            #if DEBUG
            input = new string[] { "pack", "download" };
            #endif

            if(input.Length < 1) {
                Modifi.DefaultLogger.Error("Not sure what to do.");
                return;
            }

            Modifi.ExecuteArguments(input);
        }
    }

    
}
