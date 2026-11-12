using Microsoft.AspNetCore.Mvc;
using WebApiNovaBank.Contracts;
using WebApiNovaBank.Services;

namespace WebApiNovaBank.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IJwtTokenService tokenService) : ControllerBase
{
    /// <summary>Validate credentials and issue a bearer token.</summary>
    [HttpPost("token")]
    [EndpointSummary("Obtain an access token")]
    [ProducesResponseType<AccessTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<AccessTokenResponse> CreateToken(LoginRequest request)
    {
        var user = authService.Authenticate(request.UserName, request.Password);
        return user is null ? Unauthorized() : Ok(tokenService.CreateToken(user));
    }
}
