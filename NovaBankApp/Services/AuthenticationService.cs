using System.Security.Cryptography;
using System.Text;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public class AuthenticationService
{
    public bool IsValidPassword(string password) => password.Length >= 4;

    public string HashPassword(string password) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    public bool VerifyPassword(BankAccount account, string password) => account.PasswordHash == HashPassword(password);
}
