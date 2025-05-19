using KnowledgeBox.Auth.Models;
using MediatR;

namespace KnowledgeBox.Auth.UseCases.Authentication.Queries;

public record LoginQuery(string Username, string Password) : IRequest<LoginResponse>; 