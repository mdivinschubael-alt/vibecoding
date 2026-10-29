namespace PharmaGestion.Core.Security;

public interface IPasswordHasher
{
    /// <summary>
    /// Hache un mot de passe en clair avec un sel cryptographique unique et PBKDF2 (SHA-256).
    /// </summary>
    string HashPassword(string plainPassword);

    /// <summary>
    /// Vérifie un mot de passe en clair contre un hash stocké en temps constant.
    /// </summary>
    bool VerifyPassword(string plainPassword, string hashedPassword);
}
