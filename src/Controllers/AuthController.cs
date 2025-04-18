using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBox.Auth.Controllers
{
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
        public async Task<IActionResult> Signup([FromBody] UserSignupRequest request)
        {
            var result = await _authService.SignupUserAsync(request);
            
            if (result)
            {
                return Ok(new UserSignupResponse 
                { 
                    Success = true, 
                    Message = "User registered successfully" 
                });
            }
            
            return BadRequest(new UserSignupResponse 
            { 
                Success = false, 
                Message = "User registration failed" 
            });
        }
    }
} 