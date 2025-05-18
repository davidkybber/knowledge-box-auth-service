using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Repositories.UserRepository;
using KnowledgeBox.Auth.Services;
using MediatR;

namespace KnowledgeBox.Auth.Features.Authentication.Queries;

public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResponse>
{
    private readonly ILogger<LoginQueryHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtService _jwtService;

    public LoginQueryHandler(
        ILogger<LoginQueryHandler> logger,
        IUserRepository userRepository,
        IPasswordHashingService passwordHashingService,
        IJwtService jwtService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            
            if (user == null || !_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Invalid username or password");
            }

            user.LastLoginAt = DateTimeOffset.UtcNow;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            _logger.LogInformation("User authenticated: {Username}", user.Username);

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
            _logger.LogError(ex, "Error authenticating user");
            return new LoginResponse
            {
                Success = false,
                Message = "An error occurred during authentication"
            };
        }
    }
} 