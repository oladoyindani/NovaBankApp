using NovaBankApp.Models;

namespace NovaBankApp.Services;

public interface IAuthenticationService
{
    bool IsValidPassword(string password);
    string HashPassword(string password);
    bool VerifyPassword(BankAccount account, string password);
}
