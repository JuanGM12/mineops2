using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using MineOps.Identity.Api.Errors;
using MineOps.Identity.Application.Auth;
using MineOps.Identity.Infrastructure.Identity;

namespace MineOps.Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService) : ControllerBase
{
    private const string InvalidCredentialsMessage = "Invalid email or password.";

    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new ErrorResponse(
                false,
                "The request is invalid.",
                ["Email and password are required."]));
        }

        if (!new EmailAddressAttribute().IsValid(request.Email.Trim()))
        {
            return BadRequest(new ErrorResponse(
                false,
                "The request is invalid.",
                ["Email must be a valid email address."]));
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new ErrorResponse(false, InvalidCredentialsMessage, []));
        }

        var roles = await userManager.GetRolesAsync(user);
        return Ok(tokenService.GenerateToken(new TokenUser(
            user.Id,
            user.Name,
            user.Email!,
            roles.ToArray())));
    }
}
