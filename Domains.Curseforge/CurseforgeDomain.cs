using Modifi.Domains;

namespace Domains.Curseforge {
    public class CurseforgeDomain : IDomain {

        protected CurseforgeDomainHandler Domain;

        public CurseforgeDomain() {
            this.Domain = new CurseforgeDomainHandler();
        }

        public IDomainHandler GetDomainHandler() {
            return this.Domain;
        }

        public string GetDomainIdentifier() {
            return "curseforge";
        }
    }
}
