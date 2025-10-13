using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using src.Core.Entities;
using src.Core.Interfaces;
using src.Core.Interfaces.Repositories;
using src.IntegrationTests.Base;
using src.UnitTests.Core.Builders;

namespace src.IntegrationTests.Repositories;

public class ReservationRepositoryTests : IntegrationTestBase
{
    private IReservationRepository _reservationRepository = null!;
    private IRepository<Book> _bookRepository = null!;
    private IRepository<Customer> _customerRepository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _reservationRepository = Factory.Services.GetRequiredService<IReservationRepository>();
        _bookRepository = Factory.Services.GetRequiredService<IRepository<Book>>();
        _customerRepository = Factory.Services.GetRequiredService<IRepository<Customer>>();
    }

    [Fact]
    public async Task CreateAsync_WithAvailableBook_ShouldCreateReservationAndMarkBookUnavailable()
    {
        // Arrange
        var book = BookBuilder.New()
            .WithTitle("Available Book")
            .WithIsbn("1111111111111")
            .WithAvailableStatus()
            .Build();
        var createdBook = await _bookRepository.CreateAsync(book);

        var customer = CustomerBuilder.New()
            .WithFirstName("Test")
            .WithLastName("Customer")
            .WithEmail("customer@test.com")
            .Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        var reservation = ReservationBuilder.New()
            .WithBookId(createdBook.Id)
            .WithCustomerId(createdCustomer.Id)
            .Build();

        // Act
        var createdReservation = await _reservationRepository.CreateAsync(reservation);

        // Assert
        createdReservation.Should().NotBeNull();
        createdReservation.Id.Should().BeGreaterThan(0);
        createdReservation.BookId.Should().Be(createdBook.Id);
        createdReservation.CustomerId.Should().Be(createdCustomer.Id);

        // Verify book status changed to Unavailable
        var updatedBook = await _bookRepository.GetByIdAsync(createdBook.Id);
        updatedBook!.Status.Should().Be(BookStatus.Unavailable);
    }

    [Fact]
    public async Task CreateAsync_WithUnavailableBook_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var book = BookBuilder.New()
            .WithTitle("Unavailable Book")
            .WithIsbn("2222222222222")
            .WithUnavailableStatus()
            .Build();
        var createdBook = await _bookRepository.CreateAsync(book);

        var customer = CustomerBuilder.New()
            .WithFirstName("Test")
            .WithLastName("Customer")
            .WithEmail("customer2@test.com")
            .Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        var reservation = ReservationBuilder.New()
            .WithBookId(createdBook.Id)
            .WithCustomerId(createdCustomer.Id)
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _reservationRepository.CreateAsync(reservation));

        exception.Message.Should().Contain($"Book with ID {createdBook.Id} is not available for reservation");
    }

    [Fact]
    public async Task CreateAsync_WithNonExistentBook_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var customer = CustomerBuilder.New()
            .WithFirstName("Test")
            .WithLastName("Customer")
            .WithEmail("customer3@test.com")
            .Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        var reservation = ReservationBuilder.New()
            .WithBookId(99999) // Non-existent book ID
            .WithCustomerId(createdCustomer.Id)
            .Build();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _reservationRepository.CreateAsync(reservation));

        exception.Message.Should().Contain("Book with ID 99999 not found");
    }

    [Fact]
    public async Task DeleteAsync_WithExistingReservation_ShouldDeleteAndRestoreBookAvailability()
    {
        // Arrange - Create book, customer, and reservation
        var book = BookBuilder.New()
            .WithTitle("Book for Deletion Test")
            .WithIsbn("3333333333333")
            .WithAvailableStatus()
            .Build();
        var createdBook = await _bookRepository.CreateAsync(book);

        var customer = CustomerBuilder.New()
            .WithFirstName("Delete")
            .WithLastName("Test")
            .WithEmail("delete@test.com")
            .Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        var reservation = ReservationBuilder.New()
            .WithBookId(createdBook.Id)
            .WithCustomerId(createdCustomer.Id)
            .Build();
        var createdReservation = await _reservationRepository.CreateAsync(reservation);

        // Verify book is unavailable after reservation
        var bookAfterReservation = await _bookRepository.GetByIdAsync(createdBook.Id);
        bookAfterReservation!.Status.Should().Be(BookStatus.Unavailable);

        // Act
        var deleteResult = await _reservationRepository.DeleteAsync(createdReservation.Id);

        // Assert
        deleteResult.Should().BeTrue();

        // Verify reservation is deleted
        var deletedReservation = await _reservationRepository.GetByIdAsync(createdReservation.Id);
        deletedReservation.Should().BeNull();

        // Verify book status restored to Available
        var restoredBook = await _bookRepository.GetByIdAsync(createdBook.Id);
        restoredBook!.Status.Should().Be(BookStatus.Available);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentReservation_ShouldReturnFalse()
    {
        // Act
        var result = await _reservationRepository.DeleteAsync(99999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleReservations_ShouldReturnAllReservations()
    {
        // Arrange
        var book1 = BookBuilder.New().WithTitle("Book 1").WithIsbn("4444444444444").WithAvailableStatus().Build();
        var book2 = BookBuilder.New().WithTitle("Book 2").WithIsbn("5555555555555").WithAvailableStatus().Build();
        var createdBook1 = await _bookRepository.CreateAsync(book1);
        var createdBook2 = await _bookRepository.CreateAsync(book2);

        var customer = CustomerBuilder.New().WithFirstName("Multi").WithLastName("Test").WithEmail("multi@test.com")
            .Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        var reservation1 = ReservationBuilder.New().WithBookId(createdBook1.Id).WithCustomerId(createdCustomer.Id)
            .Build();
        var reservation2 = ReservationBuilder.New().WithBookId(createdBook2.Id).WithCustomerId(createdCustomer.Id)
            .Build();

        await _reservationRepository.CreateAsync(reservation1);
        await _reservationRepository.CreateAsync(reservation2);

        // Act
        var result = await _reservationRepository.GetAllAsync();

        // Assert
        var reservationList = result.ToList();
        reservationList.Should().HaveCount(c => c >= 2);
        reservationList.Should().Contain(r => r.BookId == createdBook1.Id);
        reservationList.Should().Contain(r => r.BookId == createdBook2.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithValidChanges_ShouldPersistChanges()
    {
        // Arrange
        var book = BookBuilder.New().WithTitle("Update Test Book").WithIsbn("6666666666666").WithAvailableStatus()
            .Build();
        var createdBook = await _bookRepository.CreateAsync(book);

        var customer = CustomerBuilder.New().WithFirstName("Update").WithLastName("Test").WithEmail("update@test.com")
            .Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        var reservation = ReservationBuilder.New()
            .WithBookId(createdBook.Id)
            .WithCustomerId(createdCustomer.Id)
            .Build();
        var createdReservation = await _reservationRepository.CreateAsync(reservation);

        var newExpirationDate = DateTime.UtcNow.AddDays(14);
        createdReservation.ExpirationDate = newExpirationDate;

        // Act
        var updatedReservation = await _reservationRepository.UpdateAsync(createdReservation);

        // Assert
        updatedReservation.ExpirationDate.Should().BeCloseTo(newExpirationDate, TimeSpan.FromSeconds(1));

        // Verify persistence
        var retrievedReservation = await _reservationRepository.GetByIdAsync(createdReservation.Id);
        retrievedReservation!.ExpirationDate.Should().BeCloseTo(newExpirationDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ExistsAsync_WithExistingReservation_ShouldReturnTrue()
    {
        // Arrange
        var book = BookBuilder.New().WithTitle("Exists Test Book").WithIsbn("7777777777777").WithAvailableStatus()
            .Build();
        var createdBook = await _bookRepository.CreateAsync(book);

        var customer = CustomerBuilder.New().WithFirstName("Exists").WithLastName("Test").WithEmail("exists@test.com")
            .Build();
        var createdCustomer = await _customerRepository.CreateAsync(customer);

        var reservation = ReservationBuilder.New().WithBookId(createdBook.Id).WithCustomerId(createdCustomer.Id)
            .Build();
        var createdReservation = await _reservationRepository.CreateAsync(reservation);

        // Act
        var exists = await _reservationRepository.ExistsAsync(createdReservation.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentReservation_ShouldReturnFalse()
    {
        // Act
        var exists = await _reservationRepository.ExistsAsync(99999);

        // Assert
        exists.Should().BeFalse();
    }
}
