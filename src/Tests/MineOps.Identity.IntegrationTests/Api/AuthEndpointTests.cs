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
    [InlineData("admin@mineops.local", "MineOps Administrator", "Administrator")]
    [InlineData("planner@mineops.local", "MineOps Planner", "Planner")]
    [InlineData("maintenance@mineops.local", "Maintenance Supervisor", "MaintenanceSupervisor")]
    [InlineData("operator@mineops.local", "MineOps Operator", "Operator")]
    public async Task ValidLoginReturnsSignedTokenWithExpectedClaims(
        string email,
        string expectedName,
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
        Assert.Equal(expectedName, principal.FindFirstValue(JwtRegisteredClaimNames.Name));
        Assert.False(string.IsNullOrWhiteSpace(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)));
        var userClaimTypes = jwt.Claims
            .Where(claim => claim.Type is JwtRegisteredClaimNames.Sub
                or JwtRegisteredClaimNames.Name
                or JwtRegisteredClaimNames.Email
                or "role")
            .Select(claim => claim.Type)
            .OrderBy(type => type);
        Assert.Equal(["email", "name", "role", "sub"], userClaimTypes);
        Assert.Equal(expectedRole, Assert.Single(principal.FindAll("role")).Value);
        Assert.Equal(login.ExpiresAt.ToUnixTimeSeconds(), jwt.Payload.Expiration.GetValueOrDefault());
        Assert.True(login.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("", IdentityApiFixture.Password)]
    [InlineData("invalid-email", IdentityApiFixture.Password)]
    [InlineData("planner@mineops.local", "")]
    public async Task InvalidLoginRequestReturnsBadRequest(string email, string password)
    {
        var response = await fixture.Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task LoginEmailComparisonIsCaseInsensitive()
    {
        var response = await fixture.Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("PLANNER@MINEOPS.LOCAL", IdentityApiFixture.Password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
