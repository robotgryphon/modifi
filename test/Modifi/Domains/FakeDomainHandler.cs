using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Modifi.Domains;
using Modifi.Mods;

namespace Modifi.Tests.Domains {

    public class FakeDomainHandler : IDomainHandler {

        List<FakeModVersion> FakeModVersions = new List<FakeModVersion>();
            
        public FakeDomainHandler() {
            FakeModVersions.Add(new FakeModVersion("1.0.0", ModReleaseType.Release));
            FakeModVersions.Add(new FakeModVersion("1.1.0", ModReleaseType.Beta));
            FakeModVersions.Add(new FakeModVersion("2.0.0", ModReleaseType.Release));
            FakeModVersions.Add(new FakeModVersion("2.1.0", ModReleaseType.Release));
            FakeModVersions.Add(new FakeModVersion("2.2.1", ModReleaseType.Alpha));
        }

        public Task<ModDownloadDetails> DownloadMod(ModVersion version, string path) {
            string dirPath = new DirectoryInfo(path).Name;
            if(!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }

            string filename = Path.Combine(dirPath, version.GetModVersion() + ".mod");
            using(TextWriter tw = new StreamWriter(File.OpenWrite(filename))) {
                Storage.StorageUtilities.PACK_SERIALIZER.Serialize(tw, version);

                ModDownloadDetails details = new ModDownloadDetails() {
                    Filename = filename,
                    Checksum = ModUtilities.GetFileChecksum(filename)
                };

                return Task.FromResult(details);
            }
        }

        public Task<ModVersion> GetLatestVersion(ModMetadata metadata, ModReleaseType releaseType = ModReleaseType.Any) {
            FakeModVersion last;
            if(releaseType == ModReleaseType.Any)
                last = FakeModVersions.Last();
            else
                last = FakeModVersions.Last(v => v.ReleaseType == releaseType);

            return Task.FromResult<ModVersion>(last);
        }

        public Task<ModMetadata> GetModMetadata(string minecraftVersion, string modIdentifier) {
            FakeModMetadata meta = new FakeModMetadata(modIdentifier);
            meta.MinecraftVersion = minecraftVersion;

            return Task.FromResult<ModMetadata>(meta);
        }

        public Task<ModVersion> GetModVersion(ModMetadata metadata, string version) {
            FakeModVersion mVersion = new FakeModVersion(version);
            
            return Task.FromResult<ModVersion>(mVersion);
        }

        public Task<IEnumerable<ModVersion>> GetRecentVersions(ModMetadata metadata, int count = 5, ModReleaseType releaseType = ModReleaseType.Any) {
            IEnumerable<FakeModVersion> mods = FakeModVersions;
            if(releaseType != ModReleaseType.Any)
                mods = mods.Where(x => x.ReleaseType == releaseType);

            mods = mods.Take(count);
            return Task.FromResult<IEnumerable<ModVersion>>(mods);
        }
    }
}