using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using KnowledgeBox.Auth.UseCases.User.Queries;
using Xunit;

namespace KnowledgeBox.Auth.Tests.UseCases.User.Queries;

public class GetCurrentUserQueryHandlerTests
{
    private readonly GetCurrentUserQueryHandler _handler = new();

    [Fact]
    public async Task Handle_ShouldReturnUserInfo()
    {
        // Arrange
        var username = "testuser";
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Name, username),
            new(JwtRegisteredClaimNames.Email, "test@example.com"),
            new(JwtRegisteredClaimNames.Sub, "123456")
        };
        
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        var query = new GetCurrentUserQuery(principal);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal($"You are authenticated as {username}", result.Message);
        Assert.NotNull(result.Claims);
        Assert.Equal(3, result.Claims.Count());
        
        // Verify all claims are returned
        var resultClaims = result.Claims.ToArray();
        Assert.Equal(JwtRegisteredClaimNames.Name, resultClaims[0].Type);
        Assert.Equal(username, resultClaims[0].Value);
        Assert.Equal(JwtRegisteredClaimNames.Email, resultClaims[1].Type);
        Assert.Equal("test@example.com", resultClaims[1].Value);
        Assert.Equal(JwtRegisteredClaimNames.Sub, resultClaims[2].Type);
        Assert.Equal("123456", resultClaims[2].Value);
    }

    [Fact]
    public async Task Handle_WithNoNameClaim_ShouldStillReturnResponse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, "test@example.com"),
            new(JwtRegisteredClaimNames.Sub, "123456")
        };
        
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        
        var query = new GetCurrentUserQuery(principal);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("You are authenticated as ", result.Message);
        Assert.NotNull(result.Claims);
        Assert.Equal(2, result.Claims.Count());
    }
} 