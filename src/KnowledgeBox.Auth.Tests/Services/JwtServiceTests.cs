using System.IdentityModel.Tokens.Jwt;
using System.Text;
using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace KnowledgeBox.Auth.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;
        private readonly string _jwtKey = "This_Is_A_Test_Key_For_JWT_That_Must_Be_At_Least_256_Bits_Or_32_Bytes_Long_For_SHA256";
        private readonly string _jwtIssuer = "TestIssuer";
        private readonly string _jwtAudience = "TestAudience";
        private readonly string _jwtDuration = "60";
        
        public JwtServiceTests()
        {
            var mockConfiguration =
                // Setup configuration mock
                new Mock<IConfiguration>();
            
            // Setup JWT configuration values
            mockConfiguration.Setup(c => c["Jwt:Key"]).Returns(_jwtKey);
            mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns(_jwtIssuer);
            mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns(_jwtAudience);
            mockConfiguration.Setup(c => c["Jwt:DurationInMinutes"]).Returns(_jwtDuration);
            
            // Initialize service with mocked configuration
            _jwtService = new JwtService(mockConfiguration.Object);
        }
        
        [Fact]
        public void GenerateToken_ShouldReturnValidJwtToken()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                CreatedAt = DateTimeOffset.UtcNow
            };
            
            // Act
            var token = _jwtService.GenerateToken(user);
            
            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            
            // Basic validation of token format without full cryptographic validation
            var tokenHandler = new JwtSecurityTokenHandler();
            Assert.True(tokenHandler.CanReadToken(token));
            
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Verify claims
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
            var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            
            Assert.NotNull(subClaim);
            Assert.NotNull(nameClaim);
            Assert.NotNull(emailClaim);
            Assert.NotNull(jtiClaim);
            Assert.Equal(user.Id.ToString(), subClaim!.Value);
            Assert.Equal(user.Username, nameClaim!.Value);
            Assert.Equal(user.Email, emailClaim!.Value);
            
            // Check token metadata
            Assert.Equal(_jwtIssuer, jwtToken.Issuer);
            Assert.Equal(_jwtAudience, jwtToken.Audiences.FirstOrDefault());
            Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
        }
        
        [Fact]
        public void GenerateToken_WithMissingConfiguration_ShouldThrowException()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com"
            };
            
            var mockConfigWithMissingValues = new Mock<IConfiguration>();
            mockConfigWithMissingValues.Setup(c => c["Jwt:Key"]).Returns((string)null);
            var serviceWithInvalidConfig = new JwtService(mockConfigWithMissingValues.Object);
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                serviceWithInvalidConfig.GenerateToken(user));
                
            Assert.Contains("JWT Key is not configured", exception.Message);
        }
        
        [Fact]
        public void GenerateToken_WithInvalidDurationFormat_ShouldUseDefaultDuration()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com"
            };
            
            var mockConfigWithInvalidDuration = new Mock<IConfiguration>();
            mockConfigWithInvalidDuration.Setup(c => c["Jwt:Key"]).Returns(_jwtKey);
            mockConfigWithInvalidDuration.Setup(c => c["Jwt:Issuer"]).Returns(_jwtIssuer);
            mockConfigWithInvalidDuration.Setup(c => c["Jwt:Audience"]).Returns(_jwtAudience);
            mockConfigWithInvalidDuration.Setup(c => c["Jwt:DurationInMinutes"]).Returns("not-an-integer");
            
            var serviceWithInvalidDuration = new JwtService(mockConfigWithInvalidDuration.Object);
            
            // Act
            var token = serviceWithInvalidDuration.GenerateToken(user);
            
            // Assert
            Assert.NotNull(token);
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Default duration is 60 minutes
            var expectedExpiry = DateTime.UtcNow.AddMinutes(60);
            var tolerance = TimeSpan.FromSeconds(5); // Allow for small timestamp differences
            
            Assert.True(Math.Abs((expectedExpiry - jwtToken.ValidTo).TotalSeconds) < tolerance.TotalSeconds);
        }
    }
} 