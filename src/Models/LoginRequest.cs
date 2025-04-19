using System.ComponentModel.DataAnnotations;

namespace KnowledgeBox.Auth.Models;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = null!;
    
    [Required]
    public string Password { get; set; } = null!;
} 