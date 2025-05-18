using KnowledgeBox.Auth.Database;
using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Repositories.UserRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace KnowledgeBox.Auth.Tests.Repositories;

public class UserRepositoryTests
{
    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserDb_{Guid.NewGuid()}")
            .Options;

        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            await context.Users.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var repository = new UserRepository(context);
            var user = await repository.GetByUsernameAsync("testuser");

            // Assert
            Assert.NotNull(user);
            Assert.Equal("testuser", user.Username);
            Assert.Equal("test@example.com", user.Email);
        }
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserDb_{Guid.NewGuid()}")
            .Options;

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var repository = new UserRepository(context);
            var user = await repository.GetByUsernameAsync("nonexistentuser");

            // Assert
            Assert.Null(user);
        }
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserDb_{Guid.NewGuid()}")
            .Options;

        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            await context.Users.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var repository = new UserRepository(context);
            var user = await repository.GetByEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(user);
            Assert.Equal("testuser", user.Username);
            Assert.Equal("test@example.com", user.Email);
        }
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnTrue_WhenUsernameExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserDb_{Guid.NewGuid()}")
            .Options;

        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            await context.Users.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var repository = new UserRepository(context);
            var exists = await repository.UsernameExistsAsync("testuser");

            // Assert
            Assert.True(exists);
        }
    }

    [Fact]
    public async Task UsernameExistsAsync_ShouldReturnFalse_WhenUsernameDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserDb_{Guid.NewGuid()}")
            .Options;

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var repository = new UserRepository(context);
            var exists = await repository.UsernameExistsAsync("nonexistentuser");

            // Assert
            Assert.False(exists);
        }
    }

    [Fact]
    public async Task EmailExistsAsync_ShouldReturnTrue_WhenEmailExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserDb_{Guid.NewGuid()}")
            .Options;

        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            await context.Users.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var repository = new UserRepository(context);
            var exists = await repository.EmailExistsAsync("test@example.com");

            // Assert
            Assert.True(exists);
        }
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserDb_{Guid.NewGuid()}")
            .Options;
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "newuser",
            Email = "new@example.com",
            PasswordHash = "hashedpassword",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var repository = new UserRepository(context);
            await repository.AddAsync(user);
            await repository.SaveChangesAsync();
        }

        // Assert
        using (var context = new ApplicationDbContext(options))
        {
            var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.NotNull(savedUser);
            Assert.Equal("newuser", savedUser.Username);
            Assert.Equal("new@example.com", savedUser.Email);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUserInDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"UserDb_{Guid.NewGuid()}")
            .Options;

        // Seed the database
        using (var context = new ApplicationDbContext(options))
        {
            await context.Users.AddAsync(new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                CreatedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = new ApplicationDbContext(options))
        {
            var repository = new UserRepository(context);
            var user = await context.Users.FindAsync(userId);
            
            // Update the user
            user.Email = "updated@example.com";
            user.LastLoginAt = DateTimeOffset.UtcNow;
            
            await repository.UpdateAsync(user);
            await repository.SaveChangesAsync();
        }

        // Assert
        using (var context = new ApplicationDbContext(options))
        {
            var updatedUser = await context.Users.FindAsync(userId);
            Assert.NotNull(updatedUser);
            Assert.Equal("updated@example.com", updatedUser.Email);
            Assert.NotNull(updatedUser.LastLoginAt);
        }
    }
} 