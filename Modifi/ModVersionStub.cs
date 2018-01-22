using RobotGryphon.Modifi.Domains;
using RobotGryphon.Modifi.Mods;
using System;
using System.Text.RegularExpressions;

namespace RobotGryphon.Modifi {



    public partial class Modifi {
        protected struct ModVersionStub : IModVersion {
            private IDomainHandler Domain;
            private string ModIdentifier;
            private string ModVersion;

            public static ModVersionStub Create(string modVersionString) {
                ModVersionStub stub = new ModVersionStub();
                stub.Domain = ModHelper.GetDomainHandler(modVersionString);

                if(ModHelper.MOD_VERSION_REGEX.IsMatch(modVersionString)) {
                    Match m = ModHelper.MOD_VERSION_REGEX.Match(modVersionString);
                    stub.ModIdentifier = m.Groups["modid"].Value;
                    stub.ModVersion = m.Groups["version"].Value;
                } else {
                    stub.ModIdentifier = "unknown";
                    stub.ModVersion = "latest";
                }
                
                return stub;
            }

            IDomainHandler IModVersion.GetDomain() {
                return Domain;
            }

            string IModVersion.GetModIdentifier() {
                return ModIdentifier;
            }

            string IModVersion.GetModVersion() {
                return ModVersion;
            }

            string IModVersion.GetFilename() {
                throw new NotSupportedException("Error: Do not support filenames on mod stubs, this is for telling other code about a " +
                    "passed-in version from the command line.");
            }
        }
    }
}
