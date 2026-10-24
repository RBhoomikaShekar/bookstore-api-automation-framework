using System.Security.Claims;
using System.Text;
using BookStore.Api.Contracts;
using BookStore.Api.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BookStore.Api.Auth;

public class TokenService(IOptions<JwtOptions> options)
{
    public TokenResponse Create(AppUser user)
    {
        var o = options.Value;
        var expires = DateTime.UtcNow.AddMinutes(o.ExpiryMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("sub", user.Id.ToString()),
                new Claim("name", user.Username),
                new Claim("role", user.Role)
            ]),
            Expires = expires,
            Issuer = o.Issuer,
            Audience = o.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Key)), SecurityAlgorithms.HmacSha256)
        };

        return new TokenResponse(new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }
}