using Modifi.Domains;
using Modifi.Packs;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Commands {
    internal class PackCommandHandler {

        private enum PackAction {
            Download,
            Update,
            Info,
            Init
        }

        internal static void Handle(IEnumerable<string> arguments) {
            PackAction? action = CommandHandler.ParseCommandOption<PackAction>(arguments.First());
            if (action == null) return;


            Pack pack;
            ILogger log = Modifi.DefaultLogger;

            switch (action) {
                case PackAction.Download:
                    log.Information("Downloading modpack.");
                    pack = Modifi.DefaultPack;

                    // TODO: Pack download request
                    //foreach (IDomain handler in pack.GetRequiredDomains()) {
                    //    if(handler is IDomainModCommandHandler) {
                    //        IDomainModCommandHandler commandHandler = handler.GetModCommandHandler() as IDomainModCommandHandler;
                    //        commandHandler.PerformPackDownload(handler).Wait();
                    //    }
                    //}
                    break;

                case PackAction.Init:
                    if (!PackHelper.PackExists())
                        PackHelper.GeneratePackFile().Wait();
                    else
                        log.Error("Pack file already exists.");
                    break;

                case PackAction.Info:
                    pack = Modifi.DefaultPack;

                    Console.ForegroundColor = ConsoleColor.Magenta;
                    log.Information("{0:l}", pack.Name);
                    log.Information("Built for Minecraft {0:l}", pack.MinecraftVersion);
                    log.Information("");
                    log.Information("Required Domains:");

                    Console.ForegroundColor = ConsoleColor.White;
                    foreach(string domain in pack.UseDomains)
                        log.Information(" - {0:l} ({1:l})", domain, Path.Combine(Settings.DomainsDirectory, domain + ".dll"));
                    break;

                default:
                    Console.Error.WriteLine("Other pack actions are not yet supported.");
                    break;
            }
        }
    }
}