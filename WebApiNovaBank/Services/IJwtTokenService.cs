using WebApiNovaBank.Contracts;

namespace WebApiNovaBank.Services;

public interface IJwtTokenService
{
    AccessTokenResponse CreateToken(AuthenticatedUser user);
}
