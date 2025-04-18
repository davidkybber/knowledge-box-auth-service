using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Services;
using KnowledgeBox.Auth.Tests.Helpers;
using KnowledgeBox.Auth.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace KnowledgeBox.Auth.Tests.Controllers
{
    public class AuthControllerTests : IClassFixture<AppTestFactory>
    {
        private readonly AppTestFactory _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ITestOutputHelper _output;

        public AuthControllerTests(AppTestFactory factory, ITestOutputHelper output)
        {
            _factory = factory;
            _client = factory.CreateClientWithJsonAcceptHeader();
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _output = output;
        }

        [Fact]
        public async Task Signup_WithValidRequest_ReturnsOk()
        {
            try
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
                
                if (!string.IsNullOrEmpty(responseContent))
                {
                    var signupResponse = JsonSerializer.Deserialize<UserSignupResponse>(
                        responseContent, _jsonOptions);
                        
                    Assert.NotNull(signupResponse);
                    Assert.True(signupResponse!.Success);
                    Assert.Equal("User registered successfully", signupResponse.Message);
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Exception occurred: {ex}");
                throw;
            }
        }
        
        [Fact]
        public async Task Signup_WithEmptyUsername_ReturnsBadRequest()
        {
            try
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
            catch (Exception ex)
            {
                _output.WriteLine($"Exception occurred: {ex}");
                throw;
            }
        }
        
        [Fact]
        public async Task Signup_WithInvalidEmail_ReturnsBadRequest()
        {
            try
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
            catch (Exception ex)
            {
                _output.WriteLine($"Exception occurred: {ex}");
                throw;
            }
        }
        
        // Example of a test with a custom factory that uses a mock
        // Uncomment and use when needed
        /*
        [Fact]
        public async Task Signup_WithMockAuthService_ReturnsExpectedResult()
        {
            // Create a custom factory with a mock service
            var customFactory = new TestWebApplicationFactory();
            customFactory.ConfigureServices(services =>
            {
                // Replace the real service with a mock
                services.Remove(services.SingleOrDefault(
                    d => d.ServiceType == typeof(AuthService)));
                services.AddScoped<AuthService>(provider => 
                    new MockAuthService(provider.GetRequiredService<ILogger<AuthService>>()));
            });
            
            var client = customFactory.CreateClientWithJsonAcceptHeader();
            
            // Continue with test using the client with mocked service
            // ...
        }
        */
    }
} 