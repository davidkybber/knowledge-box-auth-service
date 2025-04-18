using Microsoft.Extensions.DependencyInjection;

namespace KnowledgeBox.Auth.Tests.Helpers
{
    /// <summary>
    /// Test factory for integration tests against the application.
    /// </summary>
    public class AppTestFactory : TestWebApplicationFactory<Program>
    {
        /// <summary>
        /// Override to configure services for tests if needed.
        /// </summary>
        protected override void ConfigureServices(IServiceCollection services)
        {
            // You can add test-specific service configuration here
        }
    }
} 