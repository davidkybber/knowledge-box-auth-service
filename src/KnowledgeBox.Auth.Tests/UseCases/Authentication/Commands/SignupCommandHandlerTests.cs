using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Repositories.UserRepository;
using KnowledgeBox.Auth.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using KnowledgeBox.Auth.UseCases.Authentication.Commands;
using Xunit;

namespace KnowledgeBox.Auth.Tests.Features.Authentication.Commands;

public class SignupCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IPasswordHashingService> _mockPasswordHashingService;
    private readonly Mock<ILogger<SignupCommandHandler>> _mockLogger;
    private readonly SignupCommandHandler _handler;

    public SignupCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockPasswordHashingService = new Mock<IPasswordHashingService>();
        _mockLogger = new Mock<ILogger<SignupCommandHandler>>();

        _handler = new SignupCommandHandler(
            _mockLogger.Object,
            _mockUserRepository.Object,
            _mockPasswordHashingService.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var command = new SignupCommand(
            Username: "testuser",
            Email: "test@example.com",
            Password: "Password123!",
            FirstName: "Test",
            LastName: "User");

        var hashedPassword = "hashed_password";

        _mockUserRepository.Setup(r => r.UsernameExistsAsync(command.Username))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.EmailExistsAsync(command.Email))
            .ReturnsAsync(false);
        _mockPasswordHashingService.Setup(s => s.HashPassword(command.Password))
            .Returns(hashedPassword);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("User created successfully", result.Message);
        Assert.NotNull(result.User);
        Assert.Equal(command.Username, result.User.Username);
        Assert.Equal(command.Email, result.User.Email);
        Assert.Equal(command.FirstName, result.User.FirstName);
        Assert.Equal(command.LastName, result.User.LastName);

        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<KnowledgeBox.Auth.Models.User>()), Times.Once);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingUsername_ShouldReturnErrorResponse()
    {
        // Arrange
        var command = new SignupCommand(
            Username: "existinguser",
            Email: "test@example.com",
            Password: "Password123!",
            FirstName: "Test",
            LastName: "User");

        _mockUserRepository.Setup(r => r.UsernameExistsAsync(command.Username))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Username is already taken", result.Message);
        Assert.Null(result.User);
        
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<KnowledgeBox.Auth.Models.User>()), Times.Never);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnErrorResponse()
    {
        // Arrange
        var command = new SignupCommand(
            Username: "newuser",
            Email: "existing@example.com",
            Password: "Password123!",
            FirstName: "Test",
            LastName: "User");

        _mockUserRepository.Setup(r => r.UsernameExistsAsync(command.Username))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(r => r.EmailExistsAsync(command.Email))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Email is already registered", result.Message);
        Assert.Null(result.User);
        
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<KnowledgeBox.Auth.Models.User>()), Times.Never);
        _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
} 