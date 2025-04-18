using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Services;
using Microsoft.Extensions.Logging;

namespace KnowledgeBox.Auth.Tests.Mocks
{
    /// <summary>
    /// A mock implementation of AuthService for testing
    /// </summary>
    public class MockAuthService : AuthService
    {
        public MockAuthService(ILogger<AuthService> logger) : base(logger)
        {
        }

        /// <summary>
        /// Override to provide predefined behavior for tests
        /// </summary>
        public override async Task<bool> SignupUserAsync(UserSignupRequest request)
        {
            // For testing, we can control the output based on input
            // For example: reject empty usernames, accept everything else
            if (string.IsNullOrEmpty(request.Username))
            {
                return false;
            }
            
            if (request.Email == "invalid-email")
            {
                return false;
            }
            
            // Default to success
            return true;
        }
    }
} 