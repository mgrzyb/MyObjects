using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyObjects.Demo.Model;
using MyObjects.Functions;
using MyObjects.Identity.Commands;

namespace MyObjects.Demo.Functions.Api;

public partial class UserFunctions : HttpFunctionsBase
{
    public UserFunctions(IDependencies dependencies) : base(dependencies)
    {
    }

    [HttpPost][Route("login")]
    public async Task<OneOf<HttpOk<string>, HttpUnauthorized>> Login(LoginRequestDto body)
    {
        var success = await this.Mediator.Send(new AuthenticateUser<User>(body.Username, body.Password));
        if (success) {
            var token = new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor {
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SXkSqsKyNUyvGbnHs7ke2NCq8zQzNLW7mPmHbnZZ")), SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, body.Username) })
            });
            return HttpOk.WithValue(token);
        }
        else
        {
            return new HttpUnauthorized();
        }
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

public class LoginRequestDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}