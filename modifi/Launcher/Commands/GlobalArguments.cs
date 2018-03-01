using CommandDotNet;
using CommandDotNet.Attributes;

namespace Modifi.Commands {

    public class GlobalArguments : IArgumentModel {

        [Option(ShortName = "p", LongName = "pack")]
        public string packName { get; set; } = "pack";

        [Option(ShortName = "m", LongName = "mc-version", Description = "Minecraft version to pull information for.")]
        public string MinecraftVersion { get; set; }= "1.12.2";
    }
}