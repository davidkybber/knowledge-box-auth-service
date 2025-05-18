using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace KnowledgeBox.Auth.Tests.Helpers;

public class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithDatabase("testdb")
        .WithUsername("testuser")
        .WithPassword("testpassword")
        .WithCleanUp(true)
        .WithImage("postgres:16-alpine")
        .WithPortBinding(Random.Shared.Next(5433, 5500), 5432)
        .Build();
    
    public string ConnectionString => _postgresContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await InitializeDatabase();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }

    private async Task InitializeDatabase()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        // Create the schema - this simulates the migrations
        await using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS ""Users"" (
                ""Id"" UUID PRIMARY KEY,
                ""Username"" VARCHAR(100) NOT NULL,
                ""Email"" VARCHAR(100) NOT NULL,
                ""PasswordHash"" TEXT NOT NULL,
                ""FirstName"" VARCHAR(100) NULL,
                ""LastName"" VARCHAR(100) NULL,
                ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL,
                ""LastLoginAt"" TIMESTAMP WITH TIME ZONE NULL
            );

            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Username"" ON ""Users"" (""Username"");
            CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Users_Email"" ON ""Users"" (""Email"");
        ";
        
        await command.ExecuteNonQueryAsync();
    }
} 