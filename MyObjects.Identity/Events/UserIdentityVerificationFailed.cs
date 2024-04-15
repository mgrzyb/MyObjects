namespace MyObjects.Identity.Events
{
    public class UserIdentityVerificationFailed : IDomainEvent
    {
        public UserIdentity Identity { get; }
        public string Method { get; }
        public string Reason { get; }

        public UserIdentityVerificationFailed(UserIdentity identity, string method, string reason)
        {
            Identity = identity;
            Method = method;
            Reason = reason;
        }
    }
}