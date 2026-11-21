using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MineOps.Identity.Application.Auth;

namespace MineOps.Identity.IntegrationTests.Api;

public sealed class AuthEndpointTests(IdentityApiFixture fixture)
    : IClassFixture<IdentityApiFixture>
{
    [Fact]
    public async Task HealthReturnsOkWhenDatabaseIsAvailable()
    {
        var response = await fixture.Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("admin@mineops.local", "Administrator")]
    [InlineData("planner@mineops.local", "Planner")]
    [InlineData("maintenance@mineops.local", "MaintenanceSupervisor")]
    [InlineData("operator@mineops.local", "Operator")]
    public async Task ValidLoginReturnsSignedTokenWithExpectedClaims(
        string email,
        string expectedRole)
    {
        var response = await fixture.Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, IdentityApiFixture.Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.Equal("Bearer", login.TokenType);

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = handler.ValidateToken(login.AccessToken, ValidationParameters(), out var validatedToken);
        var jwt = Assert.IsType<JwtSecurityToken>(validatedToken);

        Assert.Equal("MineOps.Identity", jwt.Issuer);
        Assert.Contains("MineOps.Api", jwt.Audiences);
        Assert.Equal(email, principal.FindFirstValue(JwtRegisteredClaimNames.Email));
        Assert.False(string.IsNullOrWhiteSpace(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)));
        Assert.False(string.IsNullOrWhiteSpace(principal.FindFirstValue(JwtRegisteredClaimNames.Name)));
        Assert.Contains(principal.FindAll("role"), claim => claim.Value == expectedRole);
        Assert.Equal(login.ExpiresAt.ToUnixTimeSeconds(), new DateTimeOffset(jwt.ValidTo).ToUnixTimeSeconds());
        Assert.True(login.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task InvalidPasswordAndUnknownEmailReturnSameUnauthorizedResponse()
    {
        var wrongPassword = await fixture.Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("planner@mineops.local", "WrongPassword2026!"));
        var unknownUser = await fixture.Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("unknown@mineops.local", IdentityApiFixture.Password));

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);
        Assert.Equal(
            await wrongPassword.Content.ReadAsStringAsync(),
            await unknownUser.Content.ReadAsStringAsync());
    }

    private static TokenValidationParameters ValidationParameters() => new()
    {
        ValidateIssuer = true,
        ValidIssuer = "MineOps.Identity",
        ValidateAudience = true,
        ValidAudience = "MineOps.Api",
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(IdentityApiFixture.SigningKey)),
        ClockSkew = TimeSpan.Zero,
        NameClaimType = JwtRegisteredClaimNames.Name,
        RoleClaimType = "role"
    };
}
