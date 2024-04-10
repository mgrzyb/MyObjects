using MyObjects.Identity;

namespace MyObjects.Demo.Model;

public class User : AggregateRoot, IIdentityUser
{
    public virtual string UserName { get; set; }
    public virtual string NormalizedUserName { get; set; }
}