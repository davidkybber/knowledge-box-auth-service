using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Services;
using Microsoft.AspNetCore.Mvc;

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
                User = user
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
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await _authService.AuthenticateAsync(request.Username, request.Password);
            return Ok(new { Success = true, Message = "Authentication successful", User = user });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating user");
            return StatusCode(500, new { Success = false, Message = "An error occurred during authentication" });
        }
    }
}