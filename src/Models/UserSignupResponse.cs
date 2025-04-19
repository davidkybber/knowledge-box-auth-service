namespace KnowledgeBox.Auth.Models;

public record UserSignupResponse
{
    public required bool Success { get; init; }
    public string? Message { get; init; }
    public User? User { get; init; }
} 