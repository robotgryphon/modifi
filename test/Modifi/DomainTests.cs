using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Modifi.Domains;
using Xunit;

namespace Modifi.Tests {

    /*
        Please be advised that most of these tests are designed to run using the standard Curseforge
        domain handler DLL. They run under the assumption that the test runner has the DLL inside the
        output directory.

        Most of these tests will be marked with [Ignore] so the builds will pass. Please run them manually
        if you are changing the domain loading code.

        - TS

     */

    public class DomainTests {

        public void TestDomainLoad() {

            DomainManager manager = new DomainManager("test-manager", true);
            Task<IDomain> tryLoadDomain = manager.LoadDomain("curseforge");
            
            // If it faulted, either the user forgot to add the domain handler for
            // curseforge, or the code actually failed to load the domain. Either
            // way, it isn't good.
            if(tryLoadDomain.IsFaulted) {
                Assert.False(false, tryLoadDomain.Exception.Message);

            }
            
            IDomain curseforge = tryLoadDomain.Result;
            Assert.NotNull(curseforge);
        }
    }
}