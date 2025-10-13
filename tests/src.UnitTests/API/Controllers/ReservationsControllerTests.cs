using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using src.API.Controllers;
using src.API.DTOs;
using src.Core.Interfaces.Services;
using src.UnitTests.Core.Builders;

namespace src.UnitTests.API.Controllers;

public class ReservationsControllerTests
{
    private readonly Mock<IReservationService> _mockReservationService;
    private readonly ReservationsController _controller;

    public ReservationsControllerTests()
    {
        _mockReservationService = new Mock<IReservationService>();
        var mockLogger = new Mock<ILogger<ReservationsController>>();
        _controller = new ReservationsController(_mockReservationService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetAllReservations_ReturnsOkWithReservationList()
    {
        // Arrange
        var reservationDtos = new List<ReservationDto>
        {
            ReservationBuilder.New().WithId(1).WithCustomerId(1).WithBookId(1).BuildDto(),
            ReservationBuilder.New().WithId(2).WithCustomerId(2).WithBookId(2).BuildDto()
        };

        _mockReservationService.Setup(s => s.GetAllReservationsAsync())
            .ReturnsAsync(reservationDtos);

        // Act
        var result = await _controller.GetAllReservations();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedReservations = okResult.Value.Should().BeAssignableTo<IEnumerable<ReservationDto>>().Subject;
        var reservationList = returnedReservations.ToList();
        reservationList.Should().HaveCount(2);
        reservationList[0].Id.Should().Be(1);
        reservationList[1].Id.Should().Be(2);

        _mockReservationService.Verify(s => s.GetAllReservationsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllReservations_WhenNoReservationsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        _mockReservationService.Setup(s => s.GetAllReservationsAsync())
            .ReturnsAsync(new List<ReservationDto>());

        // Act
        var result = await _controller.GetAllReservations();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedReservations = okResult.Value.Should().BeAssignableTo<IEnumerable<ReservationDto>>().Subject;
        returnedReservations.Should().BeEmpty();

        _mockReservationService.Verify(s => s.GetAllReservationsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetReservation_WithValidId_ReturnsOkWithReservation()
    {
        // Arrange
        var reservationId = 1;
        var reservationDto = ReservationBuilder.New()
            .WithId(reservationId)
            .WithCustomerId(1)
            .WithBookId(1)
            .BuildDto();

        _mockReservationService.Setup(s => s.GetReservationByIdAsync(reservationId))
            .ReturnsAsync(reservationDto);

        // Act
        var result = await _controller.GetReservation(reservationId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedReservation = okResult.Value.Should().BeOfType<ReservationDto>().Subject;
        returnedReservation.Id.Should().Be(reservationId);
        returnedReservation.CustomerId.Should().Be(1);
        returnedReservation.BookId.Should().Be(1);

        _mockReservationService.Verify(s => s.GetReservationByIdAsync(reservationId), Times.Once);
    }

    [Fact]
    public async Task GetReservation_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var reservationId = 999;
        _mockReservationService.Setup(s => s.GetReservationByIdAsync(reservationId))
            .ReturnsAsync((ReservationDto?)null);

        // Act
        var result = await _controller.GetReservation(reservationId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result.Result as NotFoundResult;
        notFoundResult!.StatusCode.Should().Be(404);

        _mockReservationService.Verify(s => s.GetReservationByIdAsync(reservationId), Times.Once);
    }

    [Fact]
    public async Task GetReservationsByCustomer_ReturnsOkWithCustomerReservations()
    {
        // Arrange
        var customerId = 1;
        var reservationDtos = new List<ReservationDto>
        {
            ReservationBuilder.New().WithId(1).WithCustomerId(customerId).WithBookId(1).BuildDto(),
            ReservationBuilder.New().WithId(3).WithCustomerId(customerId).WithBookId(3).BuildDto()
        };

        _mockReservationService.Setup(s => s.GetReservationsByCustomerIdAsync(customerId))
            .ReturnsAsync(reservationDtos);

        // Act
        var result = await _controller.GetReservationsByCustomer(customerId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedReservations = okResult.Value.Should().BeAssignableTo<IEnumerable<ReservationDto>>().Subject;
        var reservationList = returnedReservations.ToList();
        reservationList.Should().HaveCount(2);
        reservationList.All(r => r.CustomerId == customerId).Should().BeTrue();

        _mockReservationService.Verify(s => s.GetReservationsByCustomerIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task GetReservationsByBook_ReturnsOkWithBookReservations()
    {
        // Arrange
        var bookId = 1;
        var reservationDtos = new List<ReservationDto>
        {
            ReservationBuilder.New().WithId(1).WithCustomerId(1).WithBookId(bookId).BuildDto(),
            ReservationBuilder.New().WithId(2).WithCustomerId(2).WithBookId(bookId).BuildDto()
        };

        _mockReservationService.Setup(s => s.GetReservationsByBookIdAsync(bookId))
            .ReturnsAsync(reservationDtos);

        // Act
        var result = await _controller.GetReservationsByBook(bookId);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedReservations = okResult.Value.Should().BeAssignableTo<IEnumerable<ReservationDto>>().Subject;
        var reservationList = returnedReservations.ToList();
        reservationList.Should().HaveCount(2);
        reservationList.All(r => r.BookId == bookId).Should().BeTrue();

        _mockReservationService.Verify(s => s.GetReservationsByBookIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_WithValidData_ReturnsCreated()
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
            .BuildDto();

        _mockReservationService.Setup(s => s.CreateReservationAsync(createDto))
            .ReturnsAsync(createdReservation);

        // Act
        var result = await _controller.CreateReservation(createDto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(ReservationsController.GetReservation));
        createdResult.RouteValues!["id"].Should().Be(1);

        var returnedReservation = createdResult.Value.Should().BeOfType<ReservationDto>().Subject;
        returnedReservation.Id.Should().Be(1);
        returnedReservation.CustomerId.Should().Be(1);
        returnedReservation.BookId.Should().Be(1);

        _mockReservationService.Verify(s => s.CreateReservationAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var createDto = ReservationBuilder.New()
            .WithCustomerId(0)
            .WithBookId(0)
            .BuildCreateDto();

        _controller.ModelState.AddModelError("CustomerId", "CustomerId must be greater than 0");
        _controller.ModelState.AddModelError("BookId", "BookId must be greater than 0");

        // Act
        var result = await _controller.CreateReservation(createDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        badRequestResult.Value.Should().BeOfType<SerializableError>();

        _mockReservationService.Verify(s => s.CreateReservationAsync(It.IsAny<ReservationCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateReservation_WithNonExistentCustomer_ReturnsNotFound()
    {
        // Arrange
        var createDto = ReservationBuilder.New()
            .WithCustomerId(999)
            .WithBookId(1)
            .BuildCreateDto();

        _mockReservationService.Setup(s => s.CreateReservationAsync(createDto))
            .ThrowsAsync(new KeyNotFoundException("Customer with ID 999 not found."));

        // Act
        var result = await _controller.CreateReservation(createDto);

        // Assert
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);

        _mockReservationService.Verify(s => s.CreateReservationAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_WithBookUnavailable_ReturnsConflict()
    {
        // Arrange
        var createDto = ReservationBuilder.New()
            .WithCustomerId(1)
            .WithBookId(1)
            .BuildCreateDto();

        _mockReservationService.Setup(s => s.CreateReservationAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Book is not available for reservation."));

        // Act
        var result = await _controller.CreateReservation(createDto);

        // Assert
        var conflictResult = result.Result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);

        _mockReservationService.Verify(s => s.CreateReservationAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_WithInvalidArgument_ReturnsBadRequest()
    {
        // Arrange
        var createDto = ReservationBuilder.New()
            .WithCustomerId(1)
            .WithBookId(1)
            .BuildCreateDto();

        _mockReservationService.Setup(s => s.CreateReservationAsync(createDto))
            .ThrowsAsync(new ArgumentException("Invalid reservation data."));

        // Act
        var result = await _controller.CreateReservation(createDto);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);

        _mockReservationService.Verify(s => s.CreateReservationAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task CreateReservation_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var createDto = ReservationBuilder.New()
            .WithCustomerId(1)
            .WithBookId(1)
            .BuildCreateDto();

        _mockReservationService.Setup(s => s.CreateReservationAsync(createDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateReservation(createDto);

        // Assert
        var statusCodeResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        statusCodeResult.Value.Should().Be("Internal server error");

        _mockReservationService.Verify(s => s.CreateReservationAsync(createDto), Times.Once);
    }

    [Fact]
    public async Task DeleteReservation_WhenReservationExists_ReturnsNoContent()
    {
        // Arrange
        var reservationId = 1;
        _mockReservationService.Setup(s => s.DeleteReservationAsync(reservationId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteReservation(reservationId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(204);

        _mockReservationService.Verify(s => s.DeleteReservationAsync(reservationId), Times.Once);
    }

    [Fact]
    public async Task DeleteReservation_WhenReservationNotExists_ReturnsNotFound()
    {
        // Arrange
        var reservationId = 999;
        _mockReservationService.Setup(s => s.DeleteReservationAsync(reservationId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteReservation(reservationId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        var notFoundResult = result as NotFoundResult;
        notFoundResult!.StatusCode.Should().Be(404);

        _mockReservationService.Verify(s => s.DeleteReservationAsync(reservationId), Times.Once);
    }
}
