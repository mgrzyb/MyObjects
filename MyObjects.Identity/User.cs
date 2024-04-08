using Microsoft.AspNetCore.Identity;

namespace MyObjects.Identity;

public class User : Entity
{

}

interface IIdentityUser
{
    string UserName { get; set; }
    string NormalizedUserName { get; set; }
}