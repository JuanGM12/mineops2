using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MineOps.Maintenance.IntegrationTests.Security;

internal static class TestJwtTokens
{
    private const string DefaultIssuer = "MineOps.Identity";
    private const string DefaultAudience = "MineOps.Api";
    private const string SigningKey = "MineOps-Development-Only-Signing-Key-2026-Change-Me!";

    public static string Create(
        string role,
        bool expired = false,
        string issuer = DefaultIssuer,
        string audience = DefaultAudience)
    {
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Name, $"Test {role}"),
                new Claim(JwtRegisteredClaimNames.Email, $"{role.ToLowerInvariant()}@test.local"),
                new Claim("role", role)
            ],
            notBefore: expired ? now.AddMinutes(-10) : now.AddSeconds(-5),
            expires: expired ? now.AddMinutes(-5) : now.AddMinutes(15),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string Tamper(string token)
    {
        var parts = token.Split('.');
        parts[2] = (parts[2][0] == 'A' ? 'B' : 'A') + parts[2][1..];
        return string.Join('.', parts);
    }
}
