using FluentAssertions;
using Moq;
using src.API.DTOs;
using src.Core.Entities;
using src.Core.Interfaces.Repositories;
using src.Core.Interfaces.Services;
using src.Core.Services;
using src.UnitTests.Core.Builders;

namespace src.UnitTests.Core.Services;

public class ReservationServiceTests
{
    private readonly Mock<IReservationRepository> _mockReservationRepository;
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<IBookService> _mockBookService;
    private readonly ReservationService _reservationService;

    public ReservationServiceTests()
    {
        _mockReservationRepository = new Mock<IReservationRepository>();
        _mockCustomerService = new Mock<ICustomerService>();
        _mockBookService = new Mock<IBookService>();
        _reservationService = new ReservationService(
            _mockReservationRepository.Object,
            _mockCustomerService.Object,
            _mockBookService.Object);
    }

    [Fact]
    public async Task GetReservationByIdAsync_WithValidId_ReturnsReservationDto()
    {
        // Arrange
        var reservationId = 1;
        var reservation = ReservationBuilder.New()
            .WithId(reservationId)
            .WithCustomerId(1)
            .WithBookId(1)
            .Build();

        var customerDto = new CustomerDto
        { Id = 1, FirstName = "Test", LastName = "Customer", Email = "test@example.com" };
        var bookDto = new BookDto { Id = 1, Title = "Test Book", Author = "Test Author", ISBN = "1234567890" };

        _mockReservationRepository.Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync(reservation);

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(1))
            .ReturnsAsync(customerDto);

        _mockBookService.Setup(s => s.GetBookByIdAsync(1))
            .ReturnsAsync(bookDto);

        // Act
        var result = await _reservationService.GetReservationByIdAsync(reservationId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(reservationId);
        result.CustomerId.Should().Be(1);
        result.BookId.Should().Be(1);
        result.Customer.Should().NotBeNull();
        result.Customer!.Id.Should().Be(1);
        result.Book.Should().NotBeNull();
        result.Book!.Id.Should().Be(1);

        _mockReservationRepository.Verify(r => r.GetByIdAsync(reservationId), Times.Once);
        _mockCustomerService.Verify(s => s.GetCustomerByIdAsync(1), Times.Once);
        _mockBookService.Verify(s => s.GetBookByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetReservationByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var reservationId = 999;
        _mockReservationRepository.Setup(r => r.GetByIdAsync(reservationId))
            .ReturnsAsync((Reservation?)null);

        // Act
        var result = await _reservationService.GetReservationByIdAsync(reservationId);

        // Assert
        result.Should().BeNull();
        _mockReservationRepository.Verify(r => r.GetByIdAsync(reservationId), Times.Once);
        _mockCustomerService.Verify(s => s.GetCustomerByIdAsync(It.IsAny<int>()), Times.Never);
        _mockBookService.Verify(s => s.GetBookByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAllReservationsAsync_ReturnsAllReservations()
    {
        // Arrange
        var reservations = new List<Reservation>
        {
            ReservationBuilder.New().WithId(1).WithCustomerId(1).WithBookId(1).Build(),
            ReservationBuilder.New().WithId(2).WithCustomerId(2).WithBookId(2).Build(),
            ReservationBuilder.New().WithId(3).WithCustomerId(1).WithBookId(3).Build()
        };

        var customerDto1 = new CustomerDto
        { Id = 1, FirstName = "Customer", LastName = "One", Email = "customer1@example.com" };
        var customerDto2 = new CustomerDto
        { Id = 2, FirstName = "Customer", LastName = "Two", Email = "customer2@example.com" };
        var bookDto1 = new BookDto { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1111111111" };
        var bookDto2 = new BookDto { Id = 2, Title = "Book 2", Author = "Author 2", ISBN = "2222222222" };
        var bookDto3 = new BookDto { Id = 3, Title = "Book 3", Author = "Author 3", ISBN = "3333333333" };

        _mockReservationRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(reservations);

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(1)).ReturnsAsync(customerDto1);
        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(2)).ReturnsAsync(customerDto2);
        _mockBookService.Setup(s => s.GetBookByIdAsync(1)).ReturnsAsync(bookDto1);
        _mockBookService.Setup(s => s.GetBookByIdAsync(2)).ReturnsAsync(bookDto2);
        _mockBookService.Setup(s => s.GetBookByIdAsync(3)).ReturnsAsync(bookDto3);

        // Act
        var result = await _reservationService.GetAllReservationsAsync();

        // Assert
        var reservationDtos = result.ToList();
        reservationDtos.Should().HaveCount(3);
        reservationDtos[0].Id.Should().Be(1);
        reservationDtos[1].Id.Should().Be(2);
        reservationDtos[2].Id.Should().Be(3);

        _mockReservationRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllReservationsAsync_WhenNoReservationsExist_ReturnsEmptyList()
    {
        // Arrange
        _mockReservationRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Reservation>());

        // Act
        var result = await _reservationService.GetAllReservationsAsync();

        // Assert
        result.Should().BeEmpty();
        _mockReservationRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetReservationsByCustomerIdAsync_ReturnsCustomerReservations()
    {
        // Arrange
        var customerId = 1;
        var reservations = new List<Reservation>
        {
            ReservationBuilder.New().WithId(1).WithCustomerId(1).WithBookId(1).Build(),
            ReservationBuilder.New().WithId(2).WithCustomerId(2).WithBookId(2).Build(),
            ReservationBuilder.New().WithId(3).WithCustomerId(1).WithBookId(3).Build()
        };

        var customerDto = new CustomerDto
        { Id = 1, FirstName = "Customer", LastName = "One", Email = "customer1@example.com" };
        var bookDto1 = new BookDto { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1111111111" };
        var bookDto3 = new BookDto { Id = 3, Title = "Book 3", Author = "Author 3", ISBN = "3333333333" };

        _mockReservationRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(reservations);

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(1)).ReturnsAsync(customerDto);
        _mockBookService.Setup(s => s.GetBookByIdAsync(1)).ReturnsAsync(bookDto1);
        _mockBookService.Setup(s => s.GetBookByIdAsync(3)).ReturnsAsync(bookDto3);

        // Act
        var result = await _reservationService.GetReservationsByCustomerIdAsync(customerId);

        // Assert
        var customerReservations = result.ToList();
        customerReservations.Should().HaveCount(2);
        customerReservations.All(r => r.CustomerId == customerId).Should().BeTrue();
        customerReservations.Select(r => r.Id).Should().Contain(new[] { 1, 3 });

        _mockReservationRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetReservationsByBookIdAsync_ReturnsBookReservations()
    {
        // Arrange
        var bookId = 1;
        var reservations = new List<Reservation>
        {
            ReservationBuilder.New().WithId(1).WithCustomerId(1).WithBookId(1).Build(),
            ReservationBuilder.New().WithId(2).WithCustomerId(2).WithBookId(2).Build(),
            ReservationBuilder.New().WithId(3).WithCustomerId(3).WithBookId(1).Build()
        };

        var customerDto1 = new CustomerDto
        { Id = 1, FirstName = "Customer", LastName = "One", Email = "customer1@example.com" };
        var customerDto3 = new CustomerDto
        { Id = 3, FirstName = "Customer", LastName = "Three", Email = "customer3@example.com" };
        var bookDto = new BookDto { Id = 1, Title = "Book 1", Author = "Author 1", ISBN = "1111111111" };

        _mockReservationRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(reservations);

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(1)).ReturnsAsync(customerDto1);
        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(3)).ReturnsAsync(customerDto3);
        _mockBookService.Setup(s => s.GetBookByIdAsync(1)).ReturnsAsync(bookDto);

        // Act
        var result = await _reservationService.GetReservationsByBookIdAsync(bookId);

        // Assert
        var bookReservations = result.ToList();
        bookReservations.Should().HaveCount(2);
        bookReservations.All(r => r.BookId == bookId).Should().BeTrue();
        bookReservations.Select(r => r.Id).Should().Contain(new[] { 1, 3 });

        _mockReservationRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_WithValidData_ReturnsReservationDto()
    {
        // Arrange
        var createDto = ReservationBuilder.New()
            .WithCustomerId(1)
            .WithBookId(1)
            .BuildCreateDto();

        var createdReservation = ReservationBuilder.New()
            .WithId(1)
            .WithCustomerId(1)
            .WithBookId(1)
            .Build();

        var customerDto = new CustomerDto
        { Id = 1, FirstName = "Test", LastName = "Customer", Email = "test@example.com" };
        var bookDto = new BookDto { Id = 1, Title = "Test Book", Author = "Test Author", ISBN = "1234567890" };

        _mockCustomerService.Setup(s => s.CustomerExistsAsync(1))
            .ReturnsAsync(true);

        _mockReservationRepository.Setup(r => r.CreateAsync(It.IsAny<Reservation>()))
            .ReturnsAsync(createdReservation);

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(1))
            .ReturnsAsync(customerDto);

        _mockBookService.Setup(s => s.GetBookByIdAsync(1))
            .ReturnsAsync(bookDto);

        // Act
        var result = await _reservationService.CreateReservationAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.CustomerId.Should().Be(1);
        result.BookId.Should().Be(1);
        result.Customer.Should().NotBeNull();
        result.Book.Should().NotBeNull();

        _mockCustomerService.Verify(s => s.CustomerExistsAsync(1), Times.Once);
        _mockReservationRepository.Verify(r => r.CreateAsync(It.Is<Reservation>(res =>
            res.CustomerId == 1 &&
            res.BookId == 1 &&
            res.ReservationDate <= DateTime.UtcNow &&
            res.ExpirationDate > DateTime.UtcNow)), Times.Once);
    }

    [Fact]
    public async Task CreateReservationAsync_WithNonExistentCustomer_ThrowsKeyNotFoundException()
    {
        // Arrange
        var createDto = ReservationBuilder.New()
            .WithCustomerId(999)
            .WithBookId(1)
            .BuildCreateDto();

        _mockCustomerService.Setup(s => s.CustomerExistsAsync(999))
            .ReturnsAsync(false);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _reservationService.CreateReservationAsync(createDto));

        exception.Message.Should().Contain("Customer with ID 999 not found.");
        _mockCustomerService.Verify(s => s.CustomerExistsAsync(999), Times.Once);
        _mockReservationRepository.Verify(r => r.CreateAsync(It.IsAny<Reservation>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteReservationAsync_WhenReservationExists_ReturnsTrue()
    {
        // Arrange
        var reservationId = 1;
        _mockReservationRepository.Setup(r => r.DeleteAsync(reservationId))
            .ReturnsAsync(true);

        // Act
        var result = await _reservationService.DeleteReservationAsync(reservationId);

        // Assert
        result.Should().BeTrue();
        _mockReservationRepository.Verify(r => r.DeleteAsync(reservationId), Times.Once);
    }

    [Fact]
    public async Task DeleteReservationAsync_WhenReservationNotExists_ReturnsFalse()
    {
        // Arrange
        var reservationId = 999;
        _mockReservationRepository.Setup(r => r.DeleteAsync(reservationId))
            .ReturnsAsync(false);

        // Act
        var result = await _reservationService.DeleteReservationAsync(reservationId);

        // Assert
        result.Should().BeFalse();
        _mockReservationRepository.Verify(r => r.DeleteAsync(reservationId), Times.Once);
    }

    [Fact]
    public async Task ReservationExistsAsync_WhenReservationExists_ReturnsTrue()
    {
        // Arrange
        var reservationId = 1;
        _mockReservationRepository.Setup(r => r.ExistsAsync(reservationId))
            .ReturnsAsync(true);

        // Act
        var result = await _reservationService.ReservationExistsAsync(reservationId);

        // Assert
        result.Should().BeTrue();
        _mockReservationRepository.Verify(r => r.ExistsAsync(reservationId), Times.Once);
    }

    [Fact]
    public async Task ReservationExistsAsync_WhenReservationNotExists_ReturnsFalse()
    {
        // Arrange
        var reservationId = 999;
        _mockReservationRepository.Setup(r => r.ExistsAsync(reservationId))
            .ReturnsAsync(false);

        // Act
        var result = await _reservationService.ReservationExistsAsync(reservationId);

        // Assert
        result.Should().BeFalse();
        _mockReservationRepository.Verify(r => r.ExistsAsync(reservationId), Times.Once);
    }
}
