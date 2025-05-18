using KnowledgeBox.Auth.Models;
using MediatR;

namespace KnowledgeBox.Auth.Features.Authentication.Queries;

public record LoginQuery(string Username, string Password) : IRequest<LoginResponse>; 