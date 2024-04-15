

using Microsoft.AspNetCore.Identity;

namespace MyObjects.Identity
{
    public class  PasswordAuthenticationMethod : AuthenticationMethod, IAuthenticationMethod<UsernameAndPasswordCredentials>
    {
        public virtual string PasswordHash { get; protected set; }

        private readonly IPasswordHasher<UserIdentity> passwordHasher = new PasswordHasher<UserIdentity>();
        
        protected PasswordAuthenticationMethod()
        {
        }
        
        public PasswordAuthenticationMethod(UserIdentity identity, string password) : base(identity)
        {
            this.PasswordHash = this.passwordHasher.HashPassword(identity, password);
        }

        public virtual void ChangePassword(string newPassword)
        {
            this.PasswordHash = this.passwordHasher.HashPassword(this.Identity, newPassword);
        }

        public virtual Task<bool> VerifyCredentials(UsernameAndPasswordCredentials credentials)
        {
            if (this.Identity.NormalizedUsername != UserIdentity.NormalizeUsername(credentials.Username))
                return Task.FromResult(false);

            var match = this.passwordHasher.VerifyHashedPassword(this.Identity, this.PasswordHash, credentials.Password) == PasswordVerificationResult.Success;
            if (match)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}
