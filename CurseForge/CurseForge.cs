using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.ModCLI.CurseForge {
    public class CurseForge {
        public const string UserAgent = "Mozilla/5.0 (X11; Linux x86_64)";

        public static Uri BaseUri { get; internal set; }
            = new Uri("https://minecraft.curseforge.net");

        public const string ApiURL = "https://api.cfwidget.com/mc-mods/minecraft";
    }
}
