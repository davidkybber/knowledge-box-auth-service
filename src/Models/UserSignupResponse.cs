namespace KnowledgeBox.Auth.Models;

public class UserSignupResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public User? User { get; set; }
} 