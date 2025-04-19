using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace KnowledgeBox.Auth.Tests.Helpers;

/// <summary>
/// Factory for integration tests against the application
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
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
    /// Configure services for tests. Can be extended in tests if needed.
    /// </summary>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Default implementation does nothing
        // You can add test-specific service configuration here
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