using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Repositories.UserRepository;

namespace KnowledgeBox.Auth.Services;

public class AuthService(
    ILogger<AuthService> logger,
    IUserRepository userRepository,
    IPasswordHashingService passwordHashingService)
{
    public async Task<User> SignupUserAsync(UserSignupRequest request)
    {
        if (await userRepository.UsernameExistsAsync(request.Username))
        {
            throw new InvalidOperationException("Username is already taken");
        }

        if (await userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email is already registered");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHashingService.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        logger.LogInformation("New user created: {Username}", user.Username);

        return user;
    }

    public async Task<User> AuthenticateAsync(string username, string password)
    {
        var user = await userRepository.GetByUsernameAsync(username);
        
        if (user == null || !passwordHashingService.VerifyPassword(password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid username or password");
        }

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userRepository.UpdateAsync(user);
        await userRepository.SaveChangesAsync();

        logger.LogInformation("User authenticated: {Username}", user.Username);

        return user;
    }
}