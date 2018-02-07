using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modifi.Commands {
    internal class CommandHandler {

        public enum MainAction {
            Mods,
            Pack,
            Help
        }

        /// <summary>
        /// Given a set of arguments, execute the things that need to happen.
        /// </summary>
        /// <param name="input"></param>
        public static void ExecuteArguments(string[] input) {
            if (input.Length == 0) throw new ArgumentException("Nothing to do");

            MainAction? action = ParseCommandOption<MainAction>(input[0]);
            if (action == null) action = MainAction.Help;

            IEnumerable<string> arguments = input.Skip(1);

            switch (action) {
                case MainAction.Mods:
                    ModCommandHandler.HandleModAction(arguments);
                    break;

                case MainAction.Pack:
                    PackCommandHandler.Handle(arguments);
                    break;

                case MainAction.Help:
                    CommandHandler.ListOptions<MainAction>();
                    break;
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

        internal static void ListOptions<T>() where T : struct {
            string[] names = Enum.GetNames(typeof(T));
            Modifi.DefaultLogger.Information("Available actions: {0:l}", String.Join(", ", names));
        }
    }
}
