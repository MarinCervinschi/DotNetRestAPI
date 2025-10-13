using FluentAssertions;
using src.API.DTOs;
using src.Core.Entities;
using src.IntegrationTests.Base;
using src.IntegrationTests.Helpers;
using System.Net;
using Xunit;

namespace src.IntegrationTests.API;

public class ReservationsControllerTests : IntegrationTestBase
{
    private ApiTestHelper _apiHelper = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _apiHelper = new ApiTestHelper(this);
    }

    [Fact]
    public async Task GetAllReservations_WithAuthentication_ShouldReturnAllReservations()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var (customer, book) = await _apiHelper.CreateTestDataAsync();
        await _apiHelper.CreateTestReservationAsync(customer.Id, book.Id);

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
        await _apiHelper.SetupAuthenticationAsync();
        var (customer, book) = await _apiHelper.CreateTestDataAsync();
        var reservation = await _apiHelper.CreateTestReservationAsync(customer.Id, book.Id);

        // Act
        var response = await HttpClient.GetAsync($"/Reservations/{reservation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var returnedReservation = await DeserializeResponseAsync<ReservationDto>(response);
        returnedReservation.Should().NotBeNull();
        returnedReservation.Id.Should().Be(reservation.Id);
        returnedReservation.CustomerId.Should().Be(customer.Id);
        returnedReservation.BookId.Should().Be(book.Id);
    }

    [Fact]
    public async Task GetReservation_WithValidIdButNoAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var (customer, book) = await _apiHelper.CreateTestDataAsync();
        var reservation = await _apiHelper.CreateTestReservationAsync(customer.Id, book.Id);

        // Act
        var response = await HttpClient.GetAsync($"/Reservations/{reservation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReservation_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();

        // Act
        var response = await HttpClient.GetAsync("/Reservations/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateReservation_WithValidDataAndAuthentication_ShouldReturnCreated()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var (customer, book) = await _apiHelper.CreateTestDataAsync();

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

        // Verify location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/Reservations/{createdReservation.Id}");
    }

    [Fact]
    public async Task CreateReservation_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var (customer, book) = await _apiHelper.CreateTestDataAsync();

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
    public async Task CreateReservation_WithUnavailableBook_ShouldReturnBadRequest()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var customer = await _apiHelper.CreateTestCustomerAsync();
        var book = await _apiHelper.CreateTestBookAsync("Test Book", "Author", "1234567890", BookStatus.Unavailable);

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = customer.Id,
            BookId = book.Id
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReservation_WithInvalidCustomerId_ShouldReturnBadRequest()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var book = await _apiHelper.CreateTestBookAsync();

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = 99999,
            BookId = book.Id
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReservation_WithInvalidBookId_ShouldReturnBadRequest()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var customer = await _apiHelper.CreateTestCustomerAsync();

        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = customer.Id,
            BookId = 99999
        };

        // Act
        var response = await HttpClient.PostAsync("/Reservations", CreateJsonContent(reservationCreateDto));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelReservation_WithValidIdAndAuthentication_ShouldReturnNoContent()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();
        var (customer, book) = await _apiHelper.CreateTestDataAsync();
        var reservation = await _apiHelper.CreateTestReservationAsync(customer.Id, book.Id);

        // Act
        var response = await HttpClient.DeleteAsync($"/Reservations/{reservation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify reservation is deleted or cancelled by checking it's no longer accessible
        var getResponse = await HttpClient.GetAsync($"/Reservations/{reservation.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CancelReservation_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var (customer, book) = await _apiHelper.CreateTestDataAsync();
        var reservation = await _apiHelper.CreateTestReservationAsync(customer.Id, book.Id);

        // Act
        var response = await HttpClient.DeleteAsync($"/Reservations/{reservation.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CancelReservation_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        await _apiHelper.SetupAuthenticationAsync();

        // Act
        var response = await HttpClient.DeleteAsync("/Reservations/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}