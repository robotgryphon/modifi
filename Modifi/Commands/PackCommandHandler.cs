using RobotGryphon.Modifi.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Commands {
    internal class PackCommandHandler {

        private enum PackAction {
            Download,
            Update,
            Info
        }

        internal static void Handle(IEnumerable<string> arguments) {
            PackAction? action = CommandHandler.ParseCommandOption<PackAction>(arguments.First());
            if (action == null) return;

            Modifi.LoadPack();

            switch (action) {
                case PackAction.Download:
                    Console.WriteLine("Performing pack download.");

                    foreach (IDomainHandler handler in Modifi.INSTANCE.DomainHandlers.Values) {
                        if(handler is IDomainCommandHandler) {
                            IDomainCommandHandler commandHandler = handler as IDomainCommandHandler;
                            commandHandler.PerformPackDownload().Wait();
                        }
                    }
                    break;

                default:
                    Console.Error.WriteLine("Other pack actions are not yet supported.");
                    break;
            }
        }
    }
}