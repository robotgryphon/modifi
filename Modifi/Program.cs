using Serilog;
using Serilog.Context;
using System;
using System.Threading.Tasks;

// TODO: Make a disabled flag under mods

namespace RobotGryphon.Modifi {

    public class Program {

        public static void Main(string[] args) {

            // Modifi.AssureLockFiles();
            Modifi.LoadSearchPaths();

            Console.WriteLine(String.Join(", ", Modifi.DomainSearchPaths));

            string[] input = args;
            
            #if DEBUG
            input = new string[] { "mods", "versions", "curseforge:jei" };
            #endif

            if(input.Length < 1) {
                Modifi.DefaultLogger.Error("Not sure what to do.");
                return;
            }

            Modifi.ExecuteArguments(input);
        }
    }

    
}
