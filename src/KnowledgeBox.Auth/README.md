# Database Migrations

This service uses Entity Framework Core for database access and migrations.

## Creating Migrations

To create a new migration after model changes, run:

```bash
dotnet ef migrations add <MigrationName> -o Database/Migrations
```

For example:

```bash
dotnet ef migrations add InitialCreate -o Database/Migrations
```

## Applying Migrations Manually

To apply migrations manually:

```bash
dotnet ef database update
```

## Production Deployment

The application automatically applies pending migrations at startup. This is handled in the `Program.cs` file:

```csharp
// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}
```

## Additional Commands

- List all migrations: `dotnet ef migrations list`
- Remove the last migration: `dotnet ef migrations remove`
- Generate SQL script for migrations: `dotnet ef migrations script`

## Connection String

The database connection string is defined in `appsettings.json` and `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=knowledgebox;Username=postgres;Password=postgres"
}
```

For production, override this setting using environment variables:

```
ConnectionStrings__DefaultConnection
``` 