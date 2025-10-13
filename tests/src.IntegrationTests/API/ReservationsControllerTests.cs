using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using src.API.DTOs;
using src.Core.Entities;
using src.Core.Interfaces;
using src.Core.Interfaces.Repositories;
using src.IntegrationTests.Base;
using src.UnitTests.Core.Builders;
using System.Net;

namespace src.IntegrationTests.API;

public class ReservationsControllerTests : IntegrationTestBase
{
    private IReservationRepository _reservationRepository = null!;
    private IRepository<Customer> _customerRepository = null!;
    private IRepository<Book> _bookRepository = null!;
    private IAdminRepository _adminRepository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _reservationRepository = Factory.Services.GetRequiredService<IReservationRepository>();
        _customerRepository = Factory.Services.GetRequiredService<IRepository<Customer>>();
        _bookRepository = Factory.Services.GetRequiredService<IRepository<Book>>();
        _adminRepository = Factory.Services.GetRequiredService<IAdminRepository>();
    }

    private async Task SetupAuthenticationAsync()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var admin = AdminBuilder.New()
            .WithUsername("testadmin")
            .WithPasswordHash(passwordHash)
            .Build();

        await _adminRepository.CreateAsync(admin);

        var loginRequest = new AdminLoginDto
        {
            Username = "testadmin",
            Password = "password123"
        };

        var loginResponse = await HttpClient.PostAsync("/Auth/login", CreateJsonContent(loginRequest));
        var loginResult = await DeserializeResponseAsync<LoginResponseDto>(loginResponse);

        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.Token);
    }

    private async Task<(Customer customer, Book book)> CreateTestDataAsync()
    {
        var customer = CustomerBuilder.New()
            .WithFirstName("Test")
            .WithLastName("Customer")
            .WithEmail("test@example.com")
            .Build();

        var book = BookBuilder.New()
            .WithTitle("Test Book")
            .WithAuthor("Test Author")
            .WithIsbn("1234567890")
            .WithStatus(BookStatus.Available)
            .Build();

        var createdCustomer = await _customerRepository.CreateAsync(customer);
        var createdBook = await _bookRepository.CreateAsync(book);

        return (createdCustomer, createdBook);
    }

    [Fact]
    public async Task GetAllReservations_WithAuthentication_ShouldReturnAllReservations()
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (customer, book) = await CreateTestDataAsync();

        var reservation = ReservationBuilder.New()
            .WithCustomerId(customer.Id)
            .WithBookId(book.Id)
            .Build();

        await _reservationRepository.CreateAsync(reservation);

        // Act
        var response = await HttpClient.GetAsync("/Reservations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reservations = await DeserializeResponseAsync<IEnumerable<ReservationDto>>(response);
        reservations.Should().NotBeNull();
        var reservationsList = reservations!.ToList();
        reservationsList.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetAllReservations_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await HttpClient.GetAsync("/Reservations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReservation_WithValidIdAndAuthentication_ShouldReturnReservation()
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (customer, book) = await CreateTestDataAsync();

        var reservation = ReservationBuilder.New()
            .WithCustomerId(customer.Id)
            .WithBookId(book.Id)
            .Build();

        var createdReservation = await _reservationRepository.CreateAsync(reservation);

        // Act
        var response = await HttpClient.GetAsync($"/Reservations/{createdReservation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedReservation = await DeserializeResponseAsync<ReservationDto>(response);
        returnedReservation.Should().NotBeNull();
        returnedReservation.Id.Should().Be(createdReservation.Id);
        returnedReservation.CustomerId.Should().Be(customer.Id);
        returnedReservation.BookId.Should().Be(book.Id);
    }

    [Fact]
    public async Task GetReservation_WithValidIdButNoAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var (customer, book) = await CreateTestDataAsync();
        var reservation = ReservationBuilder.New()
            .WithCustomerId(customer.Id)
            .WithBookId(book.Id)
            .Build();
        var createdReservation = await _reservationRepository.CreateAsync(reservation);

        // Act
        var response = await HttpClient.GetAsync($"/Reservations/{createdReservation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReservation_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await SetupAuthenticationAsync();

        // Act
        var response = await HttpClient.GetAsync("/Reservations/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReservationsByCustomer_WithValidCustomerId_ShouldReturnCustomerReservations()
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (customer, book) = await CreateTestDataAsync();

        var reservation = ReservationBuilder.New()
            .WithCustomerId(customer.Id)
            .WithBookId(book.Id)
            .Build();

        await _reservationRepository.CreateAsync(reservation);

        // Act
        var response = await HttpClient.GetAsync($"/Reservations/customer/{customer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reservations = await DeserializeResponseAsync<IEnumerable<ReservationDto>>(response);
        if (reservations != null)
        {
            var reservationDtos = reservations as ReservationDto[] ?? reservations.ToArray();
            reservationDtos.Should().NotBeNull();
            var reservationsList = reservationDtos!.ToList();
            reservationsList.Should().HaveCount(1);
            reservationsList.First().CustomerId.Should().Be(customer.Id);
        }
    }

    [Fact]
    public async Task GetReservationsByBook_WithValidBookId_ShouldReturnBookReservations()
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (customer, book) = await CreateTestDataAsync();

        var reservation = ReservationBuilder.New()
            .WithCustomerId(customer.Id)
            .WithBookId(book.Id)
            .Build();

        await _reservationRepository.CreateAsync(reservation);

        // Act
        var response = await HttpClient.GetAsync($"/Reservations/book/{book.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var reservations = await DeserializeResponseAsync<IEnumerable<ReservationDto>>(response);
        if (reservations != null)
        {
            var reservationDtos = reservations as ReservationDto[] ?? reservations.ToArray();
            reservationDtos.Should().NotBeNull();
            var reservationsList = reservationDtos!.ToList();
            reservationsList.Should().HaveCount(1);
            reservationsList.First().BookId.Should().Be(book.Id);
        }
    }

    [Fact]
    public async Task CreateReservation_WithValidDataAndAuthentication_ShouldReturnCreatedAndUpdateBookStatus()
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (customer, book) = await CreateTestDataAsync();

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = customer.Id,
            BookId = book.Id
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdReservation = await DeserializeResponseAsync<ReservationDto>(response);
        createdReservation.Should().NotBeNull();
        createdReservation.CustomerId.Should().Be(customer.Id);
        createdReservation.BookId.Should().Be(book.Id);
        createdReservation.ReservationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        createdReservation.ExpirationDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/Reservations/{createdReservation.Id}");

        // Wait a moment for database synchronization and verify book status via HTTP API instead of direct repository access
        await Task.Delay(100);
        var bookResponse = await HttpClient.GetAsync($"/Books/{book.Id}");
        bookResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedBookDto = await DeserializeResponseAsync<BookDto>(bookResponse);
        updatedBookDto!.Status.Should().Be(BookStatus.Unavailable);
    }

    [Fact]
    public async Task CreateReservation_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var (customer, book) = await CreateTestDataAsync();

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = customer.Id,
            BookId = book.Id
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateReservation_WithInvalidCustomerId_ShouldReturnNotFound()
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (_, book) = await CreateTestDataAsync();

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = 99999, // Non-existent customer
            BookId = book.Id
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReservation_WithInvalidBookId_ShouldReturnNotFound()
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (customer, _) = await CreateTestDataAsync();

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = customer.Id,
            BookId = 99999 // Non-existent book
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReservation_WithUnavailableBook_ShouldReturnConflict()
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (customer, book) = await CreateTestDataAsync();

        // Make book unavailable
        book.Status = BookStatus.Unavailable;
        await _bookRepository.UpdateAsync(book);

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = customer.Id,
            BookId = book.Id
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateReservation_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await SetupAuthenticationAsync();

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = 0, // Invalid - zero value
            BookId = 0 // Invalid - zero value
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteReservation_WithValidIdAndAuthentication_ShouldReturnNoContentAndRestoreBookStatus()
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (customer, book) = await CreateTestDataAsync();

        // Create reservation via HTTP API (this will automatically set book to Unavailable)
        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = customer.Id,
            BookId = book.Id
        };

        var createResponse = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdReservation = await DeserializeResponseAsync<ReservationDto>(createResponse);

        // Act
        var response = await HttpClient.DeleteAsync($"/Reservations/{createdReservation!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify reservation is actually deleted
        var getResponse = await HttpClient.GetAsync($"/Reservations/{createdReservation.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify book status is restored to Available via HTTP API for consistency
        await Task.Delay(100); // Small delay for database synchronization
        var bookResponse = await HttpClient.GetAsync($"/Books/{book.Id}");
        bookResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedBookDto = await DeserializeResponseAsync<BookDto>(bookResponse);
        updatedBookDto!.Status.Should().Be(BookStatus.Available);
    }

    [Fact]
    public async Task DeleteReservation_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var (customer, book) = await CreateTestDataAsync();
        var reservation = ReservationBuilder.New()
            .WithCustomerId(customer.Id)
            .WithBookId(book.Id)
            .Build();
        var createdReservation = await _reservationRepository.CreateAsync(reservation);

        // Act
        var response = await HttpClient.DeleteAsync($"/Reservations/{createdReservation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteReservation_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await SetupAuthenticationAsync();

        // Act
        var response = await HttpClient.DeleteAsync("/Reservations/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task CreateReservation_WithInvalidCustomerIdValues_ShouldReturnBadRequest(int customerId)
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (_, book) = await CreateTestDataAsync();

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = customerId,
            BookId = book.Id
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task CreateReservation_WithInvalidBookIdValues_ShouldReturnBadRequest(int bookId)
    {
        // Arrange
        await SetupAuthenticationAsync();
        var (customer, _) = await CreateTestDataAsync();

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = customer.Id,
            BookId = bookId
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}