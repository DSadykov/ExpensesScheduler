using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using ExpensesScheduler.Authorization.Models;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ExpensesScheduler.AuthorizationService.Services;

public class JwtService(IOptions<AuthorizationSettings> options) : IJwtService
{
    private readonly string _jwtKey = options.Value.JwtKey;

    public string GenerateJwtToken(Guid userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim("id", userId.ToString())]),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
