using Microsoft.AspNetCore.Identity;

namespace MyObjects.Identity.Commands;

public partial class AuthenticateUser<TUser> : Command<bool> where TUser : class
{
    public string UserName { get; set; }
    public string Password { get; set; }
    
    public AuthenticateUser(string userName, string password)
    {
        this.UserName = userName;
        this.Password = password;
    }

    public partial class Handler
    {
        private readonly SignInManager<TUser> signInManager;

        public Handler(IDependencies dependencies, SignInManager<TUser> signInManager) : base(dependencies)
        {
            this.signInManager = signInManager;
        }

        public override Task<bool> Handle(AuthenticateUser<TUser> command, CancellationToken cancellationToken)
        {
            var result = this.signInManager.PasswordSignInAsync(command.UserName, command.Password, false, false);
            return Task.FromResult(result.Result.Succeeded);
        }
    }
}