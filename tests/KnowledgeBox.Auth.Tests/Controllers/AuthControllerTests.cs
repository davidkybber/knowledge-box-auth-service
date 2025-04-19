using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using KnowledgeBox.Auth.Database;
using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace KnowledgeBox.Auth.Tests.Controllers
{
    public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly ITestOutputHelper _output;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public AuthControllerTests(TestWebApplicationFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClientWithJsonAcceptHeader();
            _output = output;
        }

        [Fact]
        public async Task Signup_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var signupRequest = new UserSignupRequest
            {
                Username = "testuser",
                Password = "Password123!",
                Email = "test@example.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Auth/signup", signupRequest);
            
            _output.WriteLine($"Response status code: {response.StatusCode}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response content: {responseContent}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var signupResponse = JsonSerializer.Deserialize<UserSignupResponse>(
                responseContent, _jsonOptions);
            
            Assert.NotNull(signupResponse);
            Assert.True(signupResponse!.Success);
            Assert.NotNull(signupResponse.User);
            Assert.Equal(signupRequest.Username, signupResponse.User!.Username);
            Assert.Equal(signupRequest.Email, signupResponse.User.Email);
        }
        
        [Fact]
        public async Task Signup_WithEmptyUsername_ReturnsBadRequest()
        {
            // Arrange
            var signupRequest = new UserSignupRequest
            {
                Username = "",
                Password = "Password123!",
                Email = "test@example.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Auth/signup", signupRequest);
            _output.WriteLine($"Response status code: {response.StatusCode}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response content: {responseContent}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task Signup_WithInvalidEmail_ReturnsBadRequest()
        {
            // Arrange
            var signupRequest = new UserSignupRequest
            {
                Username = "testuser",
                Password = "Password123!",
                Email = "invalid-email"  // Invalid email format
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Auth/signup", signupRequest);
            _output.WriteLine($"Response status code: {response.StatusCode}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response content: {responseContent}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Signup_WithValidUsernamePassword_CreatesNewUser()
        {
            // Arrange
            var signupRequest = new UserSignupRequest
            {
                Username = "createdUser",
                Password = "Password123!",
                Email = "createdUser@example.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/Auth/signup", signupRequest);
            _output.WriteLine($"Response status code: {response.StatusCode}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response content: {responseContent}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Verify the user exists in the database
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await dbContext.Users.FirstOrDefaultAsync(u => 
                u.Username.ToLower() == signupRequest.Username.ToLower());
            
            Assert.NotNull(user);
            Assert.Equal(signupRequest.Email, user.Email);
        }
        
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            // Arrange - Create a user first
            var username = "loginuser";
            var password = "Password123!";
            var email = "loginuser@example.com";
            
            var signupRequest = new UserSignupRequest
            {
                Username = username,
                Password = password,
                Email = email
            };
            
            await _client.PostAsJsonAsync("/Auth/signup", signupRequest);
            
            // Act - Attempt to login
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };
            
            var response = await _client.PostAsJsonAsync("/Auth/login", loginRequest);
            _output.WriteLine($"Response status code: {response.StatusCode}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response content: {responseContent}");
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Just check if we can parse the response to dictionary
            var responseObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                responseContent, _jsonOptions);
            
            Assert.NotNull(responseObj);
            Assert.True(responseObj!.ContainsKey("success"));
            Assert.True(responseObj.ContainsKey("message"));
            Assert.True(responseObj.ContainsKey("user"));
        }
        
        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                Username = "nonexistentuser",
                Password = "wrongpassword"
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/Auth/login", loginRequest);
            _output.WriteLine($"Response status code: {response.StatusCode}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"Response content: {responseContent}");
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
} 