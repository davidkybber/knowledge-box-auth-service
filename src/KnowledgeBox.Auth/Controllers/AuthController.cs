using KnowledgeBox.Auth.Models;
using KnowledgeBox.Auth.UseCases.Authentication.Commands;
using KnowledgeBox.Auth.UseCases.Authentication.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBox.Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(IMediator mediator, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("signup")]
    [ProducesResponseType(typeof(UserSignupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserSignupResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Signup(UserSignupRequest request)
    {
        var command = new SignupCommand(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName ?? string.Empty,
            request.LastName ?? string.Empty);
            
        var response = await mediator.Send(command);
        
        if (!response.Success)
        {
            return BadRequest(response);
        }
        
        return Ok(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var query = new LoginQuery(request.Username, request.Password);
        var response = await mediator.Send(query);
        
        if (!response.Success)
        {
            return Unauthorized(response);
        }
        
        return Ok(response);
    }
}