using DotNetRestAPI.API.DTOs;
using DotNetRestAPI.Core.Interfaces;

namespace DotNetRestAPI.Core.Interfaces.Services;

public interface ICustomerService
{
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto entity);
    Task<CustomerDto> UpdateCustomerAsync(int id, CustomerUpdateDto entity);
    Task<bool> DeleteCustomerAsync(int id);
    Task<bool> CustomerExistsAsync(int id);
}