using System.Security.Cryptography;

namespace PharmaGestion.Core.Security;

/// <summary>
/// Implémentation de hachage de mot de passe sécurisée basée sur PBKDF2 avec HMAC-SHA256,
/// sel cryptographique 128-bit, 100 000 itérations et comparaison en temps constant (anti-timing attack).
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int KeySize = 32;  // 256 bits
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    private const char SegmentDelimiter = '$';

    public string HashPassword(string plainPassword)
    {
        if (string.IsNullOrEmpty(plainPassword))
        {
            throw new ArgumentException("Le mot de passe ne peut pas être vide.", nameof(plainPassword));
        }

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            plainPassword,
            salt,
            Iterations,
            Algorithm,
            KeySize);

        // Format de stockage: PBKDF2$100000${salt_base64}${hash_base64}
        return $"PBKDF2{SegmentDelimiter}{Iterations}{SegmentDelimiter}{Convert.ToBase64String(salt)}{SegmentDelimiter}{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        if (string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(hashedPassword))
        {
            return false;
        }

        string[] parts = hashedPassword.Split(SegmentDelimiter);
        if (parts.Length != 4 || parts[0] != "PBKDF2")
        {
            return false;
        }

        if (!int.TryParse(parts[1], out int iterations))
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                plainPassword,
                salt,
                iterations,
                Algorithm,
                expectedHash.Length);

            // Comparaison en temps constant pour éviter les attaques temporelles (timing attacks)
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }
}
