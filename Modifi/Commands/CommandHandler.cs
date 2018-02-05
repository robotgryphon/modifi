using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modifi.Commands {
    internal class CommandHandler {

        enum ActionType {
            MODS,
            DOMAINS,
            INIT,
            PACK,
            HELP,
            DOWNLOAD
        }

        /// <summary>
        /// Given a set of arguments, execute the things that need to happen.
        /// </summary>
        /// <param name="input"></param>
        public static void ExecuteArguments(string[] input) {
            if (input.Length == 0) throw new ArgumentException("Nothing to do");

            ActionType? action = ParseCommandOption<ActionType>(input[0]);
            if (action == null) return;

            IEnumerable<string> arguments = input.Skip(1);

            switch (action) {
                case ActionType.MODS:
                    ModCommandHandler.HandleModAction(arguments);
                    break;

                case ActionType.PACK:
                    PackCommandHandler.Handle(arguments);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Tries to get an enum value from a string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <exception cref="FormatException">Thrown if the option does not exist in the enumeration.</exception>
        /// <returns></returns>
        internal static T? ParseCommandOption<T>(string input) where T : struct {
            input = input.Replace('-', '_');
            T action;
            if (!Enum.TryParse(input, true, out action)) {
                IEnumerable<string> actions = Enum.GetNames(typeof(T));
                Console.Error.WriteLine("Invalid action, choose from: {0}", String.Join(", ", actions));
                return null;
            }

            return action;
        }
    }
}
