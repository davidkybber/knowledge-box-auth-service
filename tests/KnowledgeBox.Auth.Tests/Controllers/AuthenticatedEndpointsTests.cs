using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace KnowledgeBox.Auth.Tests.Controllers
{
    public class AuthenticatedEndpointsTests(TestWebApplicationFactory factory, ITestOutputHelper output)
        : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClientWithJsonAcceptHeader();
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        private async Task<string> SignupAndLoginUser(string username, string password, string email)
        {
            // Create a user
            var signupRequest = new UserSignupRequest
            {
                Username = username,
                Password = password,
                Email = email
            };

            await _client.PostAsJsonAsync("/Auth/signup", signupRequest);

            // Login to get token
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var loginResponse = await _client.PostAsJsonAsync("/Auth/login", loginRequest);
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<LoginResponse>(loginContent, _jsonOptions);

            return loginResult!.Token!;
        }

        [Fact]
        public async Task MeEndpoint_WithValidToken_ReturnsUserInfo()
        {
            // Arrange
            var username = "authtestuser";
            var password = "AuthTest123!";
            var email = "authtest@example.com";

            var token = await SignupAndLoginUser(username, password, email);
            
            // Use a new client with the authorization header
            var authorizedClient = factory.CreateClientWithJsonAcceptHeader();
            authorizedClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await authorizedClient.GetAsync("/Auth/me");
            output.WriteLine($"Response status code: {response.StatusCode}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            output.WriteLine($"Response content: {responseContent}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var responseObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(
                responseContent, _jsonOptions);
            
            Assert.NotNull(responseObj);
            Assert.True(responseObj!.ContainsKey("message"));
            Assert.True(responseObj.ContainsKey("claims"));
            
            // Check user identity in the message
            var message = responseObj["message"].GetString();
            Assert.Contains(username, message!);
        }

        [Fact]
        public async Task MeEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/Auth/me");
            output.WriteLine($"Response status code: {response.StatusCode}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task MeEndpoint_WithInvalidToken_ReturnsUnauthorized()
        {
            // Arrange
            var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            
            var authorizedClient = factory.CreateClientWithJsonAcceptHeader();
            authorizedClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", invalidToken);

            // Act
            var response = await authorizedClient.GetAsync("/Auth/me");
            output.WriteLine($"Response status code: {response.StatusCode}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
    
} 