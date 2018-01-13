using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

using RobotGryphon.Modifi.Mods;
using RobotGryphon.Modifi.Storage;

using RobotGryphon.Modifi.Domains;
using RobotGryphon.Modifi.Domains.CurseForge;
using System.Diagnostics;

namespace RobotGryphon.Modifi {
    public class Program {

        static void Main(string[] args) {

            // Modifi.AssureLockFiles();

            string[] input = args;
            
            #if DEBUG
            input = new string[] { "mods", "versions", "curseforge:jei" };
            #endif

            if(input.Length < 1) {
                Console.Error.WriteLine("Error: Not sure what to do.");
                return;
            }

            try {
                Modifi.ExecuteArguments(input);
            }

            catch(NotImplementedException) {
                Console.Error.WriteLine("Not yet implemented, sorry. Please keep an eye out for future updates!");
            }
        }
    }

    
}
