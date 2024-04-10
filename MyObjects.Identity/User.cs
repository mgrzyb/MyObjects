using Microsoft.AspNetCore.Identity;

namespace MyObjects.Identity;

public interface IIdentityUser
{
    string UserName { get; set; }
    string NormalizedUserName { get; set; }
}