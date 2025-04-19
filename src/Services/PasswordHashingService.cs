using System.Security.Cryptography;

namespace KnowledgeBox.Auth.Services;

public interface IPasswordHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHashingService : IPasswordHashingService
{
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32; // 256 bit
    private const int Iterations = 100000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    private const char Delimiter = ':';

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize);

        return string.Join(
            Delimiter,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash),
            Iterations,
            Algorithm
        );
    }

    public bool VerifyPassword(string password, string hashString)
    {
        var parts = hashString.Split(Delimiter);
        if (parts.Length != 4)
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[0]);
        var hash = Convert.FromBase64String(parts[1]);
        var iterations = int.Parse(parts[2]);
        var algorithm = new HashAlgorithmName(parts[3]);

        var newHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            algorithm,
            hash.Length);

        return CryptographicOperations.FixedTimeEquals(hash, newHash);
    }
} 