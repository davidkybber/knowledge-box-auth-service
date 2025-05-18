using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBox.Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class MeController: ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var username = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
        
        return Ok(new 
        { 
            Message = $"You are authenticated as {username}",
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}