using FluentAssertions;
using src.API.DTOs;
using src.IntegrationTests.Base;
using src.IntegrationTests.Helpers;
using src.UnitTests.Core.Builders;
using System.Net;
using Xunit;

namespace src.IntegrationTests.API;

public class CustomersControllerTests : IntegrationTestBase
{
    private ApiTestHelper _apiHelper = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _apiHelper = new ApiTestHelper(this);
    }

    [Fact]
    public async Task GetAllCustomers_WithAuthentication_ShouldReturnAllCustomers()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        await _apiHelper.CreateMultipleTestCustomersAsync();

        // Act
        var response = await HttpClient.GetAsync("/Customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var customers = await DeserializeResponseAsync<IEnumerable<CustomerDto>>(response);
        customers.Should().NotBeNull();
        var customersList = customers!.ToList();
        customersList.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAllCustomers_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await HttpClient.GetAsync("/Customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomer_WithValidIdAndAuthentication_ShouldReturnCustomer()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var customer = await _apiHelper.CreateTestCustomerAsync("John", "Doe", "john@test.com");

        // Act
        var response = await HttpClient.GetAsync($"/Customers/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedCustomer = await DeserializeResponseAsync<CustomerDto>(response);
        returnedCustomer.Should().NotBeNull();
        returnedCustomer!.Id.Should().Be(customer.Id);
        returnedCustomer.FirstName.Should().Be("John");
        returnedCustomer.LastName.Should().Be("Doe");
        returnedCustomer.Email.Should().Be("john@test.com");
    }

    [Fact]
    public async Task GetCustomer_WithValidIdButNoAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var customer = await _apiHelper.CreateTestCustomerAsync();

        // Act
        var response = await HttpClient.GetAsync($"/Customers/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();

        // Act
        var response = await HttpClient.GetAsync("/Customers/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCustomer_WithValidDataAndAuthentication_ShouldReturnCreated()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var customerCreateDto = CustomerBuilder.New()
            .WithFirstName("New")
            .WithLastName("Customer")
            .WithEmail("new.customer@test.com")
            .BuildCreateDto();

        // Act
        var response = await HttpClient.PostAsync("/Customers", CreateJsonContent(customerCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdCustomer = await DeserializeResponseAsync<CustomerDto>(response);
        createdCustomer.Should().NotBeNull();
        createdCustomer!.FirstName.Should().Be("New");
        createdCustomer.LastName.Should().Be("Customer");
        createdCustomer.Email.Should().Be("new.customer@test.com");

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/Customers/{createdCustomer.Id}");
    }

    [Fact]
    public async Task CreateCustomer_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var customerCreateDto = CustomerBuilder.New().BuildCreateDto();

        // Act
        var response = await HttpClient.PostAsync("/Customers", CreateJsonContent(customerCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCustomer_WithValidIdAndAuthentication_ShouldReturnNoContent()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var customer = await _apiHelper.CreateTestCustomerAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"/Customers/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify customer is actually deleted
        var getResponse = await HttpClient.GetAsync($"/Customers/{customer.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomer_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var customer = await _apiHelper.CreateTestCustomerAsync();

        // Act
        var response = await HttpClient.DeleteAsync($"/Customers/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();

        // Act
        var response = await HttpClient.DeleteAsync("/Customers/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateCustomer_WithInvalidFirstName_ShouldReturnBadRequest(string firstName)
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();

        var customerCreateDto = CustomerBuilder.New()
            .WithFirstName(firstName)
            .WithLastName("ValidLastName")
            .WithEmail("valid@test.com")
            .BuildCreateDto();

        // Act
        var response = await HttpClient.PostAsync("/Customers", CreateJsonContent(customerCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@test.com")]
    [InlineData("user@")]
    [InlineData("")]
    public async Task CreateCustomer_WithInvalidEmail_ShouldReturnBadRequest(string email)
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();

        var customerCreateDto = CustomerBuilder.New()
            .WithFirstName("ValidFirstName")
            .WithLastName("ValidLastName")
            .WithEmail(email)
            .BuildCreateDto();

        // Act
        var response = await HttpClient.PostAsync("/Customers", CreateJsonContent(customerCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}