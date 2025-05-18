using KnowledgeBox.Auth.Database;
using KnowledgeBox.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBox.Auth.Repositories.UserRepository;

public class UserRepository(ApplicationDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await DbSet.AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await DbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task CreateUserAsync(User user)
    {
        await DbSet.AddAsync(user);
    }
} 