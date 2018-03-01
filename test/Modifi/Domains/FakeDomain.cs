using Modifi.Domains;

namespace Modifi.Tests.Domains {

    public class FakeDomain : IDomain {
        
        protected FakeDomainHandler Handler;

        public FakeDomain() {
            this.Handler = new FakeDomainHandler();
        }

        public IDomainHandler GetDomainHandler() {
            return Handler;
        }

        public string GetDomainIdentifier() {
            return "fake";
        }
    }
}