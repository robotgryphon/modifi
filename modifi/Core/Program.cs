using Modifi.Domains;
using Modifi.Mods;
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

            Modifi.DEBUG_MODE = true;

            string[] input = args;
            
            if(Modifi.DEBUG_MODE) input = new string[] { "pack", "info" };
            if(input.Length < 1) {
                Modifi.DefaultLogger.Error("Not sure what to do. Choose from the following actions:");
                Commands.CommandHandler.ListOptions<Commands.CommandHandler.MainAction>();
                return;
            }

            // ModDownloadDetails details = new ModDownloadDetails();
            // details.Filename = "jei_1.12.2-4.8.5.147.jar";
            // details.Checksum = "b743562dac1b5334c20ac87b54c0b518";

            // Modifi.DefaultLogger.Information(details.Encrypt());

            Pack p = Pack.Load(System.IO.Path.Combine(Environment.CurrentDirectory, "test-pack.json"));
            Console.WriteLine(p.Name);
            
            // Pack p = Pack.Load(Modifi.DEFAULT_PACK_PATH);
            // Modifi.DefaultLogger.Information(p.Name);
            // foreach(string mod in p.Mods.Keys) {
            //     Modifi.DefaultLogger.Information(mod);
            //     if(p.Files.ContainsKey(mod)) {
            //         ModDownloadDetails downloadInfo = ModDownloadDetails.Decrypt(p.Files[mod]);
            //         Modifi.DefaultLogger.Information("> Downloaded to {0:l}", downloadInfo.Filename);
            //     }
            // }
            // Modifi.ExecuteArguments(input);
        }
    }

    
}
