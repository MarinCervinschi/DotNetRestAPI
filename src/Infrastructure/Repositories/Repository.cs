using Microsoft.EntityFrameworkCore;
using src.Core.Interfaces;
using src.Infrastructure.Data;

namespace src.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly ApplicationDbContext Context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        _dbSet = Context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;

        var entry = Context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _dbSet.Attach(entity);
        }

        _dbSet.Remove(entity);
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }
}