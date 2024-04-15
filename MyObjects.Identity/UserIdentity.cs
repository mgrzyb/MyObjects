using MyObjects.Identity.Events;
using MyObjects.NHibernate;
using System.Net;

namespace MyObjects.Identity
{
    interface IUserIdentity 
    { 
        void OnCredentialsVerified(AuthenticationMethod method);
        void OnCredentialsVerificationFailed(AuthenticationMethod method, string reason);
    }

    public class UserIdentity : AggregateRoot, IUserIdentity
    {
        public static string NormalizeUsername(string username)
        {
            return username.ToLowerInvariant();
        }

        [Unique]
        public virtual string Username { get; protected set; }

        [Unique]
        public virtual string NormalizedUsername { get; protected set; }


        private readonly IList<AuthenticationMethod> _authenticationMethods = new List<AuthenticationMethod>();
        public virtual IEnumerable<AuthenticationMethod> AuthenticationMethods => _authenticationMethods;

        protected UserIdentity()
        {
        }

        public UserIdentity(UsernameAndPasswordCredentials credentials) : this(credentials.Username, credentials.Password)
        {
        }

        public UserIdentity(string username, string passwordHash)
        {
            this.Username = username;
            this.NormalizedUsername = NormalizeUsername(username);

            this.AddPasswordLogin(passwordHash);
        }

        public virtual async Task<bool> VerifyCredentials<TCreds>(TCreds creds)
        {
            var method = (await this.AuthenticationMethods.Initialized()).OfType<IAuthenticationMethod<TCreds>>().FirstOrDefault();
            if (method == null)
                return false;

            if (await method.VerifyCredentials(creds))
            {
                this.Publish(new UserIdentityVerified(this, method.GetType().Name));
                return true;
            }
            else
            {
                this.Publish(new UserIdentityVerificationFailed(this, method.GetType().Name, "Invalid credentials"));
                return false;
            }
        }

        public virtual PasswordAuthenticationMethod AddPasswordLogin(string password)
        {
            var login = new PasswordAuthenticationMethod(this, password);
            this._authenticationMethods.Add(login);
            return login;
        }

        void IUserIdentity.OnCredentialsVerified(AuthenticationMethod method)
        {
            this.Publish(new UserIdentityVerified(this, method.GetType().Name));
        }

        void IUserIdentity.OnCredentialsVerificationFailed(AuthenticationMethod method, string reason)
        {
            this.Publish(new UserIdentityVerificationFailed(this, method.GetType().Name, reason));
        }
    }

    public interface IAuthenticationMethod<T>
    {
        Task<bool> VerifyCredentials(T credentials);
    }

    public abstract class AuthenticationMethod : Entity
    {
        public virtual UserIdentity Identity { get; protected set; }

        protected AuthenticationMethod()
        {
        }

        protected AuthenticationMethod(UserIdentity identity)
        {
            this.Identity = identity;
        }            
    }

}
