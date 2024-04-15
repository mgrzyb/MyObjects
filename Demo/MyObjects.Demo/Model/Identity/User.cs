using MyObjects.Identity;

namespace MyObjects.Demo.Model.Identity;

public class User : AggregateRoot
{
    public virtual UserIdentity Identity { get; protected set; }

    protected User()
    {
    }

    public User(MyObjects.Identity.UserIdentity identity)
    {
        this.Identity = identity;
    }
    
}