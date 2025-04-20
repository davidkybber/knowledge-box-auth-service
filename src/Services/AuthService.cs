using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Repositories.UserRepository;

namespace KnowledgeBox.Auth.Services;

public class AuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtService _jwtService;

    public AuthService(
        ILogger<AuthService> logger,
        IUserRepository userRepository,
        IPasswordHashingService passwordHashingService,
        IJwtService jwtService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
        _jwtService = jwtService;
    }

    public async Task<User> SignupUserAsync(UserSignupRequest request)
    {
        if (await _userRepository.UsernameExistsAsync(request.Username))
        {
            throw new InvalidOperationException("Username is already taken");
        }

        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email is already registered");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHashingService.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _logger.LogInformation("New user created: {Username}", user.Username);

        return user;
    }

    public async Task<LoginResponse> AuthenticateAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        
        if (user == null || !_passwordHashingService.VerifyPassword(password, user.PasswordHash))
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
}