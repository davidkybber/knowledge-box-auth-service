using System.Linq.Expressions;
using KnowledgeBox.Auth.Database;
using KnowledgeBox.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace KnowledgeBox.Auth.Repositories;

public class Repository<T>(ApplicationDbContext context) : IRepository<T>
    where T : class
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(object id)
    {
        return await DbSet.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    public Task UpdateAsync(T entity)
    {
        DbSet.Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        if (context.Entry(entity).State == EntityState.Detached)
        {
            DbSet.Attach(entity);
        }
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
} 