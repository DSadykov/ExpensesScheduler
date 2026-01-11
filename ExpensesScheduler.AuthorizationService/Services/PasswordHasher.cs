using System.Reflection.Metadata;
using System.Security.Cryptography;

using Microsoft.AspNetCore.Identity;

namespace ExpensesScheduler.AuthorizationService.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iteration = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;
    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteration, Algorithm, HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        var parts = hashedPassword.Split('-');
        var hash = Convert.FromHexString(parts[0]);
        var salt = Convert.FromHexString(parts[1]);

        var inputHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteration, Algorithm, HashSize);

        return CryptographicOperations.FixedTimeEquals(inputHash, hash);
    }
}
