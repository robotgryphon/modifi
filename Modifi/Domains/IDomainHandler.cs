using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Domains {
    public interface IDomainHandler {

        void HandleModAdd(IModVersion mod);

        void HandleModRemove(IModVersion mod);

        void HandleModInformation(IModVersion mod);

        void HandleModVersions(IModVersion mod);

        string GetProjectURL(IModMetadata meta);

        ModDownloadResult HandleModDownload(IModVersion modVersion);

        Task<IModMetadata> GetModMetadata(IModVersion version);

        IModVersion GetInstalledModVersion(IModVersion version);

        bool IsModInstalled(IModVersion version);
    }
}
