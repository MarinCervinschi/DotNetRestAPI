using DotNetRestAPI.Core.Interfaces;
using DotNetRestAPI.Core.Interfaces.Services;

namespace DotNetRestAPI.Core.Services;

public class Service<T>(IRepository<T> repository) : IService<T>
    where T : class, IEntity
{
    public async Task<T?> GetByIdAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await repository.GetAllAsync();
    }

    public async Task<T> CreateAsync(T entity)
    {
        return await repository.CreateAsync(entity);
    }

    public async Task<T> UpdateAsync(int id, T entity)
    {
        var existingEntity = await repository.GetByIdAsync(id);
        if (existingEntity == null)
            throw new ArgumentException($"Entity with id {id} not found");

        entity.Id = id;
        return await repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await repository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await repository.ExistsAsync(id);
    }
}