namespace WebApplication.Misc
{
    //These Dependencies are used as test dependencies for the 
    //MyIoc to inject for the myIocWebApp test only.

    public interface ILocator
    {
        string Location { get; }
    }
    public interface IRealm
    {
        string RealmType { get; }
    }
    public class LDAPRealm : IRealm
    {
        ILocator locator;

        public string RealmType { get => "LDAP"; }

        public LDAPRealm(ILocator locator)
        {
            this.locator = locator;
        }
    }
    public class LDAPLocator : ILocator
    {
        private string location;
        public string Location { get => location; }
        
        public LDAPLocator()
        {
            this.location = "ldap://default";
        }
    }
}