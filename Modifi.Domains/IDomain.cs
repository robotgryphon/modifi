namespace Modifi.Domains {
    public interface IDomain {

        IDomainHandler GetDomainHandler();

        string GetDomainIdentifier();

    }
}