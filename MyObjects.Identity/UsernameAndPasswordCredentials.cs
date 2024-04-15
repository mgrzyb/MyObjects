using NHibernate.Linq;
using System.Linq.Expressions;

namespace MyObjects.Identity
{
    public class UsernameAndPasswordCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public UsernameAndPasswordCredentials()
        {
        }

        public UsernameAndPasswordCredentials(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public Task<UserIdentity> TryResolveIdentity(ISession session)
        {
            return session.Query<UserIdentity>().SingleOrDefaultAsync(x => x.NormalizedUsername == UserIdentity.NormalizeUsername(this.Username));
        }
    }
}
