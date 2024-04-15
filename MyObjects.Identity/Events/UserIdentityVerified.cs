namespace MyObjects.Identity.Events
{
    public class UserIdentityVerified : IDomainEvent
    {
        public UserIdentity Identity { get; }
        public string Method { get; }

        public UserIdentityVerified(UserIdentity identity, string method)
        {
            Identity = identity;
            Method = method;
        }

    }
}