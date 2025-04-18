using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace KnowledgeBox.Auth.Tests.Helpers
{
    /// <summary>
    /// Base class for test factories with common functionality
    /// </summary>
    public abstract class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
        where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(ConfigureServices);

            // Reduce logging noise in tests
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            });
        }

        /// <summary>
        /// Configure services for tests. Override in derived classes.
        /// </summary>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Creates a client with JSON accept header.
        /// </summary>
        public HttpClient CreateClientWithJsonAcceptHeader()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
} 