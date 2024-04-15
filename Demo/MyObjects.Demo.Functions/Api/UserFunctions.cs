using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyObjects.Demo.Model.Identity;
using MyObjects.Demo.Model.Identity.Commands;
using MyObjects.Functions;
using MyObjects.Identity;

namespace MyObjects.Demo.Functions.Api;

public partial class UserFunctions : HttpFunctionsBase
{
    public UserFunctions(IDependencies dependencies) : base(dependencies)
    {
    }

    [HttpPost]
    [Route("api/users")]
    public async Task<OneOf<HttpOk<string>, HttpUnauthorized>> Register(UsernameAndPasswordCredentials body)
    {
        var createUserResult = await this.Mediator.Send(new CreateUser(body));

        return createUserResult.Match<OneOf<HttpOk<string>, HttpUnauthorized>>(
            userRef =>
            {
                var user = this.Session.Resolve(userRef).Result;
                var token = new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
                {
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SXkSqsKyNUyvGbnHs7ke2NCq8zQzNLW7mPmHbnZZ")), SecurityAlgorithms.HmacSha256),
                    Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Identity.Username) })
                });

                return HttpOk.WithValue(token);
            },
            error =>
            {
                return new HttpUnauthorized();
            }
        );
    }

    [HttpPost][Route("login")]
    public async Task<OneOf<HttpOk<string>, HttpUnauthorized>> Login(UsernameAndPasswordCredentials body)
    {
        var userRef = await this.Mediator.Send(new AuthenticateUser(body));
        if (userRef == null)
            return new HttpUnauthorized();

        var user = await this.Session.Resolve(userRef);

        var token = new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor {
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SXkSqsKyNUyvGbnHs7ke2NCq8zQzNLW7mPmHbnZZ")), SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Identity.Username) })
        });

        return HttpOk.WithValue(token);
    }

    [HttpGet][Route("auth-test")]
    public async Task<OneOf<HttpOk<string>, HttpUnauthorized>> AuthTest(IPrincipal principal)
    {
        if (principal?.Identity?.IsAuthenticated == true)
        {
            return HttpOk.WithValue($"You are authenticated as {principal.Identity.Name}");
        }

        return new HttpUnauthorized();
    }
}