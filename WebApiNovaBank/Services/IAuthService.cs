namespace WebApiNovaBank.Services;

public interface IAuthService
{
    AuthenticatedUser? Authenticate(string userName, string password);
}
