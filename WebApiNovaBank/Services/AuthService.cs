using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using NovaBankApp.Services;

namespace WebApiNovaBank.Services;

public sealed class AuthService(
    IAccountService accountService,
    IAuthenticationService authenticationService,
    IOptions<AdminCredentialsOptions> adminOptions) : IAuthService
{
    public AuthenticatedUser? Authenticate(string userName, string password)
    {
        var admin = adminOptions.Value;
        if (FixedTimeEquals(userName, admin.UserName) && FixedTimeEquals(password, admin.Password))
        {
            return new AuthenticatedUser(admin.UserName, admin.UserName, "Admin");
        }

        var account = accountService.GetByCustomerName(userName);
        return account is not null && authenticationService.VerifyPassword(account, password)
            ? new AuthenticatedUser(account.AccountNumber, account.CustomerName, "Customer")
            : null;
    }

    private static bool FixedTimeEquals(string supplied, string expected)
    {
        if (string.IsNullOrEmpty(expected))
        {
            return false;
        }

        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        return suppliedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
    }
}
