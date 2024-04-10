using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Principal;
using System.Text;

namespace MyObjects.Functions;

public class PrincipalArgumentResolver : IFunctionArgumentResolver<HttpRequest>
{
    public Task<TValue> TryResolve<TValue>(string name, HttpRequest req)
    {
        if (typeof(TValue) != typeof(IPrincipal))
        {
            return Task.FromResult(default(TValue));
        }

        if (!req.Headers.TryGetValue("Authorization", out var authHeaders))
            return Task.FromResult(default(TValue));

        var authHeader = authHeaders.ToString();

        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == false)
            return Task.FromResult(default(TValue));

        authHeader = authHeader.Substring("Bearer ".Length).Trim();

        if (string.IsNullOrEmpty(authHeader))
            return Task.FromResult(default(TValue));

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(authHeader, new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SXkSqsKyNUyvGbnHs7ke2NCq8zQzNLW7mPmHbnZZ"))
            }, out var token);

            if (principal == null)
                return Task.FromResult(default(TValue));

            return Task.FromResult((TValue)(object)principal);
        }
        catch (Exception)
        {
            return Task.FromResult(default(TValue));
        }
    }
}
