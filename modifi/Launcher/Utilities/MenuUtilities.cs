using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Modifi.Domains;
using Modifi.Mods;

namespace Modifi.Utilities {
    public static class MenuUtilities {

        public static Menu<ModVersion> CreateModVersionMenu(IDomainHandler handler, ModMetadata meta) {
            Menu<ModVersion> menu = new Menu<ModVersion>();

            menu.OptionFormatter = (opt)=> {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(opt.GetVersionName());
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write(" [");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(opt.GetModVersion());
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("]");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(" [{0}]", opt.GetReleaseType());
                Console.WriteLine();
            };

            ModVersion latestRelease = handler.GetRecentVersions(meta, 1, ModReleaseType.Release).Result.First();
            ModVersion latestVersion = handler.GetRecentVersions(meta, 1, ModReleaseType.Any).Result.First();

            IEnumerable<ModVersion> versions = handler.GetRecentVersions(meta, 6, ModReleaseType.Any).Result.Skip(1);

            menu.AddItem(latestRelease);
            if (latestVersion.GetModVersion()!= latestRelease.GetModVersion())
                menu.AddItem(latestVersion);

            menu.AddSpacer();
            foreach (ModVersion v in versions.Take(5))menu.AddItem(v);

            return menu;
        }


    }
}
