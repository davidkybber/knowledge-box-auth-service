using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Repositories.UserRepository;
using KnowledgeBox.Auth.Services;
using MediatR;

namespace KnowledgeBox.Auth.Features.Authentication.Commands;

public class SignupCommandHandler(
    ILogger<SignupCommandHandler> logger,
    IUserRepository userRepository,
    IPasswordHashingService passwordHashingService)
    : IRequestHandler<SignupCommand, UserSignupResponse>
{
    public async Task<UserSignupResponse> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await userRepository.UsernameExistsAsync(request.Username))
            {
                throw new InvalidOperationException("Username is already taken");
            }

            if (await userRepository.EmailExistsAsync(request.Email))
            {
                throw new InvalidOperationException("Email is already registered");
            }

            var user = new Models.User
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

            return new UserSignupResponse
            {
                Success = true,
                Message = "User created successfully",
                User = UserDto.FromUser(user)
            };
        }
        catch (InvalidOperationException ex)
        {
            return new UserSignupResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return new UserSignupResponse
            {
                Success = false,
                Message = "An error occurred while creating the user"
            };
        }
    }
} 