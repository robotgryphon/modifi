using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Modifi.Domains;
using Modifi.Mods;
using Modifi.Tests.Domains;
using Xunit;

namespace Modifi.Tests.Domains {

    public class DomainTests {

        /// <summary>
        /// If you have a domain DLL, please run this one manually for now.
        /// </summary>
        // public void TestDomainLoad() {
        //     DomainManager manager = new DomainManager("test-manager", true);
        //     Task<IDomain> tryLoadDomain = manager.LoadDomain("{domain-dll-name}");
        //     // If it faulted, either the user forgot to add the domain handler, 
        //     // or the code actually failed to load the domain. Either
        //     // way, it isn't good.
        //     if(tryLoadDomain.IsFaulted) {
        //         Assert.False(false, tryLoadDomain.Exception.Message);
        //     }
        //     IDomain domain = tryLoadDomain.Result;
        //     Assert.NotNull(domain);
        // }

        [Fact]
        public async Task TestModLookup() {
            IDomain domain = new FakeDomain();
            IDomainHandler handler = domain.GetDomainHandler();

            ModMetadata meta = await handler.GetModMetadata("1.12.2", "test-mod");

            Assert.NotNull(meta);
        }
    }
}