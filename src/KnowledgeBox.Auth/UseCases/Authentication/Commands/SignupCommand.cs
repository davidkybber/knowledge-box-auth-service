using KnowledgeBox.Auth.Models;
using MediatR;

namespace KnowledgeBox.Auth.UseCases.Authentication.Commands;

public record SignupCommand(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<UserSignupResponse>; 