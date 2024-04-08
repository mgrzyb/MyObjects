namespace MyObjects.Identity.Commands;

public partial class AuthenticateUser<TUser>
{
    public partial class Handler : CommandHandler<AuthenticateUser<TUser>, bool>
    {
    }
}