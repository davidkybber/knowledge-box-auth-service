using System.ComponentModel.DataAnnotations;

namespace KnowledgeBox.Auth.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Username { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = null!;
    
    [Required]
    public string PasswordHash { get; set; } = null!;
    
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? LastLoginAt { get; set; }
}