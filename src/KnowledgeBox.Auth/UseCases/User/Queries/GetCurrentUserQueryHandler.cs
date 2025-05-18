using System.IdentityModel.Tokens.Jwt;
using MediatR;

namespace KnowledgeBox.Auth.Features.User.Queries;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public Task<CurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var username = request.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
        
        var response = new CurrentUserResponse
        {
            Message = $"You are authenticated as {username}",
            Claims = request.User.Claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value })
        };
        
        return Task.FromResult(response);
    }
} 