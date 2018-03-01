using CommandDotNet.Attributes;

namespace Modifi.Commands {

    internal class Base {

        [SubCommand]
        public PackCommands Pack { get; set; }

        [SubCommand]
        public ModsCommand Mods { get; set; }
    }
}