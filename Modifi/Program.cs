using System;
using System.Threading.Tasks;

// TODO: Make a disabled flag under mods

namespace RobotGryphon.Modifi {

    public class Program {

        public static void Main(string[] args) {

            // Modifi.AssureLockFiles();

            string[] input = args;
            
            #if DEBUG
            input = new string[] { "pack", "download" };
            #endif

            if(input.Length < 1) {
                Console.Error.WriteLine("Error: Not sure what to do.");
                return;
            }

            try {
                Modifi.ExecuteArguments(input);
                Modifi.INSTANCE.Dispose();
            }

            catch (NotImplementedException e) {
                Console.Error.WriteLine("There was an error with your request.");
                Console.Error.WriteLine(e.Message);

                Console.Error.WriteLine();
                Console.Error.WriteLine("Please pass this along to the Modifi developers:");
                Console.Error.WriteLine(e);
            }
        }
    }

    
}
