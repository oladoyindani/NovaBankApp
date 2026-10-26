using System.Security.Cryptography;
using System.Text;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

/// <summary>Validates credentials and hashes passwords before they are stored in memory.</summary>
public class AuthenticationService
{
    /// <summary>Requires a simple minimum password length for this educational console app.</summary>
    public bool IsValidPassword(string password) => password.Length >= 4;

    /// <summary>Converts a password to a SHA-256 hash so plaintext is not stored on the account.</summary>
    public string HashPassword(string password) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

    /// <summary>Checks a login or transaction-confirmation password against an account.</summary>
    public bool VerifyPassword(BankAccount account, string password) => account.PasswordHash == HashPassword(password);
}
