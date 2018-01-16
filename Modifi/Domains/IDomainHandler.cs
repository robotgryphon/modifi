using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Domains {
    public interface IDomainHandler {

        void HandleModAdd(ModVersion mod);

        void HandleModRemove(ModVersion mod);

        void HandleModInformation(ModVersion mod);

        void HandleModVersions(ModVersion mod);

        string GetProjectURL(IModMetadata meta);

        ModDownloadResult HandleModDownload(ModVersion modVersion);
    }
}
