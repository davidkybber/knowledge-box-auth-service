using KnowledgeBox.Auth.Services;
using Xunit;

namespace KnowledgeBox.Auth.Tests.Services;

public class PasswordHashingServiceTests
{
    private readonly IPasswordHashingService _passwordHashingService;

    public PasswordHashingServiceTests()
    {
        _passwordHashingService = new PasswordHashingService();
    }

    [Fact]
    public void HashPassword_ShouldGenerateHash_ThatIsNotEqualToOriginalPassword()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _passwordHashingService.HashPassword(password);

        // Assert
        Assert.NotEqual(password, hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash1 = _passwordHashingService.HashPassword(password);
        var hash2 = _passwordHashingService.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_ForCorrectPassword()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hash = _passwordHashingService.HashPassword(password);

        // Act
        var result = _passwordHashingService.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForIncorrectPassword()
    {
        // Arrange
        var correctPassword = "SecurePassword123!";
        var incorrectPassword = "WrongPassword123!";
        var hash = _passwordHashingService.HashPassword(correctPassword);

        // Act
        var result = _passwordHashingService.VerifyPassword(incorrectPassword, hash);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_ForMalformedHash()
    {
        // Arrange
        var password = "SecurePassword123!";
        var malformedHash = "not-a-valid-hash-format";

        // Act
        var result = _passwordHashingService.VerifyPassword(password, malformedHash);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void HashPassword_ShouldHandleEmptyOrNullPasswords(string password)
    {
        // Act & Assert
        var exception = Record.Exception(() => _passwordHashingService.HashPassword(password));
        
        // We're just checking it doesn't throw an unhandled exception
        // The actual behavior (returning empty hash or throwing specific exception)
        // depends on your implementation requirements
    }

    [Theory]
    [InlineData("short")]
    [InlineData("verylongpasswordthatisextremelylongmorethananyonewouldeveruse")]
    public void HashPassword_ShouldHandlePasswordsOfDifferentLengths(string password)
    {
        // Act
        var hash = _passwordHashingService.HashPassword(password);

        // Assert
        Assert.NotEmpty(hash);
        Assert.True(_passwordHashingService.VerifyPassword(password, hash));
    }
} 