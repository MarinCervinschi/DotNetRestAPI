using DotNetRestAPI.API.DTOs;
using DotNetRestAPI.Core.Interfaces;
using DotNetRestAPI.Core.Interfaces.Services;
using DotNetRestAPI.Core.Entities;

namespace DotNetRestAPI.Core.Services;

public class CustomerService(IRepository<Customer> repository, ILogger<CustomerService> logger)
    : ICustomerService
{
    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        logger.LogInformation("Fetching customer with ID {Id}", id);
        var customer = await repository.GetByIdAsync(id);
        return customer?.ToDto();
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        logger.LogInformation("Fetching all customers");
        var customers = await repository.GetAllAsync();
        return customers.Select(customer => customer.ToDto());
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto entity)
    {
        logger.LogInformation("Creating a new customer");
        var customer = new Customer
        {
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email
        };

        var createdCustomer = await repository.CreateAsync(customer);
        return createdCustomer.ToDto();
    }

    public async Task<CustomerDto> UpdateCustomerAsync(int id, CustomerUpdateDto entity)
    {
        logger.LogInformation("Updating customer with ID {Id}", id);
        var existingCustomer = await repository.GetByIdAsync(id);
        if (existingCustomer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {id} not found.");
        }

        existingCustomer.FirstName = entity.FirstName;
        existingCustomer.LastName = entity.LastName;
        existingCustomer.Email = entity.Email;

        var updatedCustomer = await repository.UpdateAsync(existingCustomer);
        return updatedCustomer.ToDto();
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        logger.LogInformation("Deleting customer with ID {Id}", id);
        return await repository.DeleteAsync(id);
    }

    public async Task<bool> CustomerExistsAsync(int id)
    {
        return await repository.ExistsAsync(id);
    }
}

public static class CustomerExtensions
{
    public static CustomerDto ToDto(this Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            //Reservations = customer.Reservations?.Select(r => r.ToDto()).ToList() ?? new List<ReservationReadDto>()
        };
    }
}