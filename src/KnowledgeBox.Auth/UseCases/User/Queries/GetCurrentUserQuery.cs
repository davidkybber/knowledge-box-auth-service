using System.Security.Claims;
using MediatR;

namespace KnowledgeBox.Auth.Features.User.Queries;

public record GetCurrentUserQuery(ClaimsPrincipal User) : IRequest<CurrentUserResponse>;

public class CurrentUserResponse
{
    public string Message { get; set; } = string.Empty;
    public IEnumerable<ClaimInfo> Claims { get; set; } = [];
}

public class ClaimInfo
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
} 