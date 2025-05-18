using KnowledgeBox.Auth.Features.Authentication.Queries;
using KnowledgeBox.Auth.Repositories.UserRepository;
using KnowledgeBox.Auth.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KnowledgeBox.Auth.Tests.UseCases.Authentication.Queries;

public class LoginQueryHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHashingService> _mockPasswordHashingService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly LoginQueryHandler _handler;

    public LoginQueryHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHashingService = new Mock<IPasswordHashingService>();
        _mockJwtService = new Mock<IJwtService>();
        var mockLogger = new Mock<ILogger<LoginQueryHandler>>();

        _handler = new LoginQueryHandler(
            mockLogger.Object,
            _mockUserRepository.Object,
            _mockPasswordHashingService.Object,
            _mockJwtService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessResponse()
    {
        // Arrange
        var query = new LoginQuery("validuser", "ValidPassword123!");
        var hashedPassword = "hashed_password";
        var token = "jwt_token";
        
        var user = new KnowledgeBox.Auth.Models.User
        {
            Id = Guid.NewGuid(),
            Username = query.Username,
            Email = "valid@example.com",
            PasswordHash = hashedPassword,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _mockUserRepository.Setup(r => r.GetByUsernameAsync(query.Username))
            .ReturnsAsync(user);
        _mockPasswordHashingService.Setup(s => s.VerifyPassword(query.Password, hashedPassword))
            .Returns(true);
        _mockJwtService.Setup(s => s.GenerateToken(user))
            .Returns(token);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Authentication successful", result.Message);
        Assert.Equal(token, result.Token);
        Assert.NotNull(result.User);
        Assert.Equal(user.Id, result.User.Id);
        Assert.Equal(user.Username, result.User.Username);
        
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<KnowledgeBox.Auth.Models.User>()), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnErrorResponse()
    {
        // Arrange
        var query = new LoginQuery("nonexistentuser", "Password123!");

        _mockUserRepository.Setup(r => r.GetByUsernameAsync(query.Username))
            .ReturnsAsync((KnowledgeBox.Auth.Models.User)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Invalid username or password", result.Message);
        Assert.Null(result.Token);
        Assert.Null(result.User);
        
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<KnowledgeBox.Auth.Models.User>()), Times.Never);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ShouldReturnErrorResponse()
    {
        // Arrange
        var query = new LoginQuery("validuser", "WrongPassword123!");
        var hashedPassword = "hashed_password";
        
        var user = new KnowledgeBox.Auth.Models.User
        {
            Id = Guid.NewGuid(),
            Username = query.Username,
            Email = "valid@example.com",
            PasswordHash = hashedPassword,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _mockUserRepository.Setup(r => r.GetByUsernameAsync(query.Username))
            .ReturnsAsync(user);
        _mockPasswordHashingService.Setup(s => s.VerifyPassword(query.Password, hashedPassword))
            .Returns(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Invalid username or password", result.Message);
        Assert.Null(result.Token);
        Assert.Null(result.User);
        
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<KnowledgeBox.Auth.Models.User>()), Times.Never);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
} 