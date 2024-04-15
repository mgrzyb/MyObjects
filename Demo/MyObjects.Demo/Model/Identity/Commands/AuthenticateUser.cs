using MyObjects.Identity;
using NHibernate.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyObjects.Demo.Model.Identity.Commands;

public class AuthenticateUser : Command<Reference<User>?>
{
    public UsernameAndPasswordCredentials Credentials { get; }

    public AuthenticateUser(UsernameAndPasswordCredentials credentials)
    {
        this.Credentials = credentials;
    }

    public class Handler : CommandHandler<AuthenticateUser, Reference<User>?>
    {

        public Handler(IDependencies dependencies) : base(dependencies) { }

        public override async Task<Reference<User>?> Handle(AuthenticateUser command, CancellationToken cancellationToken)
        {
            var identity = await command.Credentials.TryResolveIdentity(this.Session);
            if (identity == null)
                return null;

            var result = await identity.VerifyCredentials(command.Credentials);
            if (!result)
                return null;

            var user = await this.Session.Query<User>().SingleOrDefaultAsync(x => x.Identity == identity);
            if (user == null)
                return null;

            return user.GetReference();
        }
    }
}
