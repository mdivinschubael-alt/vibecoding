using PharmaGestion.Core.Security;
using Xunit;

namespace PharmaGestion.Tests;

public class PasswordSecurityTests
{
    private readonly IPasswordHasher _hasher = new PasswordHasher();

    [Fact]
    public void HashPassword_ShouldGenerateSecureFormat_WithSaltAndIterations()
    {
        string rawPassword = "MonMotDePassePharmacie2026!";
        string hash = _hasher.HashPassword(rawPassword);

        Assert.NotNull(hash);
        Assert.StartsWith("PBKDF2$", hash);
        
        string[] parts = hash.Split('$');
        Assert.Equal(4, parts.Length);
        Assert.Equal("100000", parts[1]); // 100 000 itérations PBKDF2
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        string password = "Admin123!";
        string hash = _hasher.HashPassword(password);

        bool isValid = _hasher.VerifyPassword(password, hash);
        Assert.True(isValid);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        string password = "Admin123!";
        string hash = _hasher.HashPassword(password);

        bool isValid = _hasher.VerifyPassword("MauvaisMotDePasse", hash);
        Assert.False(isValid);
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashesForSamePassword_DueToUniqueSalts()
    {
        string password = "SecretPassword!";
        string hash1 = _hasher.HashPassword(password);
        string hash2 = _hasher.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
        Assert.True(_hasher.VerifyPassword(password, hash1));
        Assert.True(_hasher.VerifyPassword(password, hash2));
    }
}
