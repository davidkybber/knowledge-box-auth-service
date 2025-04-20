using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Repositories.UserRepository;
using KnowledgeBox.Auth.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace KnowledgeBox.Auth.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHashingService> _mockPasswordHashingService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHashingService = new Mock<IPasswordHashingService>();
        _mockLogger = new Mock<ILogger<AuthService>>();

        _authService = new AuthService(
            _mockLogger.Object,
            _mockUserRepository.Object,
            _mockPasswordHashingService.Object);
    }

    [Fact]
    public async Task SignupUserAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var request = new UserSignupRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        var hashedPassword = "hashed_password";

        _mockUserRepository.Setup(r => r.UsernameExistsAsync(request.Username))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email))
            .ReturnsAsync(false);
        _mockPasswordHashingService.Setup(s => s.HashPassword(request.Password))
            .Returns(hashedPassword);

        // Act
        var result = await _authService.SignupUserAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Username, result.Username);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(hashedPassword, result.PasswordHash);
        Assert.Equal(request.FirstName, result.FirstName);
        Assert.Equal(request.LastName, result.LastName);

        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SignupUserAsync_WithExistingUsername_ShouldThrowException()
    {
        // Arrange
        var request = new UserSignupRequest
        {
            Username = "existinguser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        _mockUserRepository.Setup(r => r.UsernameExistsAsync(request.Username))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.SignupUserAsync(request));
        
        Assert.Contains("Username is already taken", exception.Message);
        
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task SignupUserAsync_WithExistingEmail_ShouldThrowException()
    {
        // Arrange
        var request = new UserSignupRequest
        {
            Username = "newuser",
            Email = "existing@example.com",
            Password = "Password123!"
        };

        _mockUserRepository.Setup(r => r.UsernameExistsAsync(request.Username))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.EmailExistsAsync(request.Email))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.SignupUserAsync(request));
        
        Assert.Contains("Email is already registered", exception.Message);
        
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        var username = "validuser";
        var password = "ValidPassword123!";
        var hashedPassword = "hashed_password";
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = "valid@example.com",
            PasswordHash = hashedPassword,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _mockUserRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync(user);
        _mockPasswordHashingService.Setup(s => s.VerifyPassword(password, hashedPassword))
            .Returns(true);

        // Act
        var result = await _authService.AuthenticateAsync(username, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Username, result.Username);
        Assert.NotNull(result.LastLoginAt);
        
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentUser_ShouldThrowException()
    {
        // Arrange
        var username = "nonexistentuser";
        var password = "Password123!";

        _mockUserRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync((User)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.AuthenticateAsync(username, password));
        
        Assert.Contains("Invalid username or password", exception.Message);
        
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_WithIncorrectPassword_ShouldThrowException()
    {
        // Arrange
        var username = "validuser";
        var password = "WrongPassword123!";
        var hashedPassword = "hashed_password";
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = "valid@example.com",
            PasswordHash = hashedPassword,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _mockUserRepository.Setup(r => r.GetByUsernameAsync(username))
            .ReturnsAsync(user);
        _mockPasswordHashingService.Setup(s => s.VerifyPassword(password, hashedPassword))
            .Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.AuthenticateAsync(username, password));
        
        Assert.Contains("Invalid username or password", exception.Message);
        
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
} 