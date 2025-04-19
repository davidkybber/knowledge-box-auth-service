using System.ComponentModel.DataAnnotations;

namespace KnowledgeBox.Auth.Models;

public record LoginRequest
{
    [Required]
    public required string Username { get; init; }
    
    [Required]
    public required string Password { get; init; }
} 