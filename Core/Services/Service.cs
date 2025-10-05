using DotNetRestAPI.Core.Interfaces;
using DotNetRestAPI.Core.Interfaces.Services;

namespace DotNetRestAPI.Core.Services;

public class Service<T>(IRepository<T> repository, ILogger<Service<T>> logger) : IService<T>
    where T : class, IEntity
{
    public async Task<T?> GetByIdAsync(int id)
    {
        logger.LogInformation("Fetching entity of type {EntityType} with id {Id}", typeof(T).Name, id);
        return await repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        logger.LogInformation("Fetching all entities of type {EntityType}", typeof(T).Name);
        return await repository.GetAllAsync();
    }

    public async Task<T> CreateAsync(T entity)
    {
        logger.LogInformation("Creating entity of type {EntityType}", typeof(T).Name);
        return await repository.CreateAsync(entity);
    }

    public async Task<T> UpdateAsync(int id, T entity)
    {
        logger.LogInformation("Updating entity of type {EntityType} with id {Id}", typeof(T).Name, id);
        var existingEntity = await repository.GetByIdAsync(id);
        if (existingEntity == null)
            throw new ArgumentException($"Entity with id {id} not found");

        entity.Id = id;
        return await repository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        logger.LogInformation("Deleting entity of type {EntityType} with id {Id}", typeof(T).Name, id);
        return await repository.DeleteAsync(id);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        logger.LogInformation("Checking existence of entity of type {EntityType} with id {Id}", typeof(T).Name, id);
        return await repository.ExistsAsync(id);
    }
}