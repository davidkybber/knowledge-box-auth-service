using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace KnowledgeBox.Auth.Tests.IntegrationTests
{
    public class JwtAuthenticationFlowTests(TestWebApplicationFactory factory, ITestOutputHelper output)
        : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client = factory.CreateClientWithJsonAcceptHeader();
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        [Fact]
        public async Task CompleteAuthenticationFlow_ShouldSucceed()
        {
            // Step 1: Create a user
            var username = "flowtest";
            var password = "FlowTest123!";
            var email = "flowtest@example.com";
            
            var signupRequest = new UserSignupRequest
            {
                Username = username,
                Password = password,
                Email = email,
                FirstName = "Flow",
                LastName = "Test"
            };

            var signupResponse = await _client.PostAsJsonAsync("/Auth/signup", signupRequest);
            output.WriteLine($"Signup response: {signupResponse.StatusCode}");
            
            Assert.Equal(HttpStatusCode.OK, signupResponse.StatusCode);
            
            // Step 2: Login to get JWT token
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var loginResponse = await _client.PostAsJsonAsync("/Auth/login", loginRequest);
            output.WriteLine($"Login response: {loginResponse.StatusCode}");
            
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
            
            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            output.WriteLine($"Login content: {loginContent}");
            
            var loginResult = JsonSerializer.Deserialize<LoginResponse>(loginContent, _jsonOptions);
            Assert.NotNull(loginResult);
            Assert.NotNull(loginResult!.Token);
            Assert.True(loginResult.Success);
            
            // Step 3: Parse and validate JWT token structure
            var token = loginResult.Token!;
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Make sure the token is in the correct format
            Assert.True(tokenHandler.CanReadToken(token));
            
            var jwtToken = tokenHandler.ReadJwtToken(token);
            output.WriteLine($"JWT token issued at: {jwtToken.IssuedAt}, expires: {jwtToken.ValidTo}");
            
            // Validate token claims - checking the format without cryptographic validation
            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
            output.WriteLine($"Token claims: {string.Join(", ", claims.Select(c => $"{c.Key}={c.Value}"))}");
            
            // Check specific claims
            Assert.True(claims.ContainsKey(JwtRegisteredClaimNames.Name));
            Assert.True(claims.ContainsKey(JwtRegisteredClaimNames.Email));
            Assert.True(claims.ContainsKey(JwtRegisteredClaimNames.Sub)); // Subject (user ID)
            Assert.True(claims.ContainsKey(JwtRegisteredClaimNames.Jti)); // JWT ID (unique identifier)
            
            Assert.Equal(username, claims[JwtRegisteredClaimNames.Name]);
            Assert.Equal(email, claims[JwtRegisteredClaimNames.Email]);
            
            // Step 4: Access protected endpoint with token
            var authorizedClient = factory.CreateClientWithJsonAcceptHeader();
            authorizedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var meResponse = await authorizedClient.GetAsync("/Auth/me");
            output.WriteLine($"Me endpoint response: {meResponse.StatusCode}");
            
            Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
            
            var meContent = await meResponse.Content.ReadAsStringAsync();
            output.WriteLine($"Me endpoint content: {meContent}");
            
            var meResult = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(meContent, _jsonOptions);
            Assert.NotNull(meResult);
            Assert.True(meResult!.ContainsKey("message"));
            
            // Step 5: Try accessing protected endpoint without token
            var unauthorizedResponse = await _client.GetAsync("/Auth/me");
            output.WriteLine($"Unauthorized response: {unauthorizedResponse.StatusCode}");
            
            Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);
        }
    }
} 