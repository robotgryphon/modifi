using Domains.Curseforge;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Modifi.Domains;
using Modifi.Mods;

namespace Modifi.Tests.DomainTester {

    [TestClass]
    public class DomainTests {

        static IDomain domain = new CurseforgeDomain();
        static IDomainHandler handler = domain.GetDomainHandler();

        private readonly string MINECRAFT_VERSION = "1.12.2";
        private readonly string MOD_IDENTIFIER = "jei";

        [TestMethod]
        public void TestMetadataFetch() {
            IModMetadata meta = handler.GetModMetadata(MINECRAFT_VERSION, MOD_IDENTIFIER).Result;

            Assert.AreEqual<string>(meta.GetModIdentifier(), MOD_IDENTIFIER);

            Assert.IsNotNull(meta.GetMinecraftVersion());

            // Should return true, JEI has a description
            Assert.IsTrue(meta.HasDescription());
        }

        [TestMethod]
        public void TestVersionFetch() {
            IModMetadata meta = handler.GetModMetadata(MINECRAFT_VERSION, MOD_IDENTIFIER).Result;

            IModVersion version = handler.GetLatestVersion(meta, ModReleaseType.Release).Result;

            Assert.IsNotNull(version.GetModVersion());
            Assert.AreEqual(ModReleaseType.Release, version.GetReleaseType());

            Assert.IsNotNull(version.GetVersionName());
        }
    }
}
