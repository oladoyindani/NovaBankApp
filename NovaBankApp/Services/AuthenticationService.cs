using System.Security.Cryptography;
using NovaBankApp.Models;

namespace NovaBankApp.Services;

public sealed class AuthenticationService : IAuthenticationService
{
    private const int PasswordHashIterations = 600_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const string HashPrefix = "pbkdf2-sha256";

    public bool IsValidPassword(string password) => password.Length >= 12;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            PasswordHashIterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return $"{HashPrefix}${PasswordHashIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(BankAccount account, string password)
    {
        var parts = account.PasswordHash.Split('$');
        if (parts.Length != 4
            || parts[0] != HashPrefix
            || !int.TryParse(parts[1], out var iterations)
            || iterations is < 100_000 or > PasswordHashIterations)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            if (salt.Length != SaltSize || expectedHash.Length != HashSize)
            {
                return false;
            }

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
