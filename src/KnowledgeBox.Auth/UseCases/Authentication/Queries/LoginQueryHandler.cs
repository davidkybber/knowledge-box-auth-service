using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Repositories.UserRepository;
using KnowledgeBox.Auth.Services;
using MediatR;

namespace KnowledgeBox.Auth.UseCases.Authentication.Queries;

public class LoginQueryHandler(
    ILogger<LoginQueryHandler> logger,
    IUserRepository userRepository,
    IPasswordHashingService passwordHashingService,
    IJwtService jwtService)
    : IRequestHandler<LoginQuery, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userRepository.GetByUsernameAsync(request.Username);
            
            if (user == null || !passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Invalid username or password");
            }

            user.LastLoginAt = DateTimeOffset.UtcNow;
            await userRepository.UpdateAsync(user);
            await userRepository.SaveChangesAsync();

            var token = jwtService.GenerateToken(user);

            logger.LogInformation("User authenticated: {Username}", user.Username);

            return new LoginResponse
            {
                Success = true,
                Message = "Authentication successful",
                Token = token,
                User = UserDto.FromUser(user)
            };
        }
        catch (InvalidOperationException ex)
        {
            return new LoginResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error authenticating user");
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during authentication"
            };
        }
    }
} 