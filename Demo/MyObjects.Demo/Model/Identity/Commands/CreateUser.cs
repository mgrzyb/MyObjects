using MyObjects.Identity;
using NHibernate.Linq;
using OneOf;
using System.Threading;
using System.Threading.Tasks;

namespace MyObjects.Demo.Model.Identity.Commands;

public enum CreateUserError
{
    UsernameAlreadyTaken

}

public class CreateUser : Command<OneOf<Reference<User>, CreateUserError>>
{
    public UsernameAndPasswordCredentials Credentials { get; }

    public CreateUser(UsernameAndPasswordCredentials credentials)
    {
        this.Credentials = credentials;
    }

    public class Handler : CommandHandler<CreateUser, OneOf<Reference<User>, CreateUserError>>
    {
        public Handler(IDependencies dependencies) : base(dependencies) { }

        public override async Task<OneOf<Reference<User>, CreateUserError>> Handle(CreateUser command, CancellationToken cancellationToken)
        {
            var usernameAlreadyTaken = await this.Session.Query<User>().AnyAsync(u => u.Identity.NormalizedUsername == UserIdentity.NormalizeUsername(command.Credentials.Username));
            if (usernameAlreadyTaken)
                return CreateUserError.UsernameAlreadyTaken;

            var identity = new UserIdentity(command.Credentials);
            await this.Session.Save(identity);

            return await this.Session.Save(new User(identity));
        }
    }
}