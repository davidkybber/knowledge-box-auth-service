using System.IdentityModel.Tokens.Jwt;
using KnowledgeBox.Auth.UseCases.User.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBox.Auth.Controllers;

[ApiController]
[Route("[controller]")]
public class MeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var query = new GetCurrentUserQuery(User);
        var response = await mediator.Send(query);
        
        return Ok(response);
    }
}