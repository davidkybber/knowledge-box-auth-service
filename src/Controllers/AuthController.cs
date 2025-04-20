using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace KnowledgeBox.Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("signup")]
    [ProducesResponseType(typeof(UserSignupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserSignupResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Signup(UserSignupRequest request)
    {
        try
        {
            var user = await _authService.SignupUserAsync(request);
            var response = new UserSignupResponse
            {
                Success = true,
                Message = "User created successfully",
                User = UserDto.FromUser(user)
            };
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            var response = new UserSignupResponse
            {
                Success = false,
                Message = ex.Message
            };
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            var response = new UserSignupResponse
            {
                Success = false,
                Message = "An error occurred while creating the user"
            };
            return StatusCode(500, response);
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.AuthenticateAsync(request.Username, request.Password);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user");
            return StatusCode(500, new LoginResponse
            {
                Success = false,
                Message = "An error occurred during authentication"
            });
        }
    }
    
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        // Get the username from the claims rather than User.Identity.Name
        var username = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
        
        // In a real application, you would retrieve the user from the database
        return Ok(new 
        { 
            Message = $"You are authenticated as {username}",
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}