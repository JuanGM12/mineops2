namespace MineOps.Identity.Application.Auth;

public interface ITokenService
{
    LoginResponse GenerateToken(TokenUser user);
}
