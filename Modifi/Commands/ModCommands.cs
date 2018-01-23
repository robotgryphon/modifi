using RobotGryphon.Modifi.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Commands {

    public class ModCommandHandler {
        public enum ModActions {
            ADD,
            REMOVE,
            INFO,
            LIST,
            DOWNLOAD,
            VERSIONS
        }

        internal static void HandleModAction(IEnumerable<string> arguments) {

            ModActions? action = CommandHandler.ParseCommandOption<ModActions>(arguments.First());
            if (action == null) return;

            IEnumerable<string> mods = arguments.Skip(1);

            foreach (string mod in mods) {
                IDomainHandler handler = ModHelper.GetDomainHandler(mod);
                if (handler == null || !(handler is IDomainCommandHandler))
                    throw new Exception("That domain does not have a registered handler. Aborting.");

                IDomainCommandHandler commandHandler = handler as IDomainCommandHandler;

                string modIdentifier = ModHelper.GetModIdentifier(mod);
                string modVersion = ModHelper.GetModVersion(mod);

                switch (action) {
                    case ModActions.ADD:
                        commandHandler.HandleModAdd(modIdentifier, modVersion);
                        break;

                    case ModActions.REMOVE:
                        commandHandler.HandleModRemove(modIdentifier, modVersion);
                        break;

                    case ModActions.INFO:
                        commandHandler.HandleModInformation(modIdentifier, modVersion);
                        break;

                    case ModActions.VERSIONS:
                        commandHandler.HandleModVersions(modIdentifier);
                        break;

                    case ModActions.DOWNLOAD:
                        try { commandHandler.HandleModDownload(modIdentifier, modVersion); }
                        catch (Exception) { }
                        break;

                    default:
                        throw new Exception("Invalid mod action.");
                }
            }
        }
    }
}
