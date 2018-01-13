using RobotGryphon.Modifi.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotGryphon.Modifi.Domains {
    public interface DomainHandler {

        void HandleModAction(Modifi.ModActions action, ModVersion mod);

        string GetProjectURL(IModMetadata meta);
    }
}
