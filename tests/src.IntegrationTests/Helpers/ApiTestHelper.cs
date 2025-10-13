using Microsoft.Extensions.DependencyInjection;
using src.API.DTOs;
using src.Core.Entities;
using src.Core.Interfaces;
using src.Core.Interfaces.Repositories;
using src.IntegrationTests.Base;
using src.UnitTests.Core.Builders;
using System.Net.Http.Headers;

namespace src.IntegrationTests.Helpers;

/// <summary>
/// Helper class for API integration tests with common functionality
/// Eliminates code duplication across different API test classes
/// </summary>
public class ApiTestHelper
{
    private readonly IntegrationTestBase _testBase;
    private readonly IAdminRepository _adminRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Book> _bookRepository;
    private readonly IReservationRepository _reservationRepository;

    public ApiTestHelper(IntegrationTestBase testBase)
    {
        _testBase = testBase;

        // Use reflection to access protected Factory field
        var factoryField = typeof(IntegrationTestBase).GetField("Factory",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var factory =
            factoryField?.GetValue(testBase) as Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>;

        if (factory == null)
            throw new InvalidOperationException("Could not access Factory from IntegrationTestBase");

        _adminRepository = factory.Services.GetRequiredService<IAdminRepository>();
        _customerRepository = factory.Services.GetRequiredService<IRepository<Customer>>();
        _bookRepository = factory.Services.GetRequiredService<IRepository<Book>>();
        _reservationRepository = factory.Services.GetRequiredService<IReservationRepository>();
    }

    /// <summary>
    /// Sets up authentication by creating an admin and logging in via HTTP API
    /// </summary>
    /// <param name="username">Admin username (default: "testadmin")</param>
    /// <param name="password">Admin password (default: "password123")</param>
    /// <returns>The created admin entity and login response</returns>
    public async Task<(Admin admin, LoginResponseDto loginResponse)> SetupAuthenticationAsync(
        string username = "testadmin",
        string password = "password123")
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var admin = AdminBuilder.New()
            .WithUsername(username)
            .WithPasswordHash(passwordHash)
            .Build();

        var createdAdmin = await _adminRepository.CreateAsync(admin);

        var loginRequest = new AdminLoginDto
        {
            Username = username,
            Password = password
        };

        // Use reflection to access protected methods
        var httpClientProperty = typeof(IntegrationTestBase).GetProperty("HttpClient",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var httpClient = httpClientProperty?.GetValue(_testBase) as HttpClient;

        var createJsonContentMethod = typeof(IntegrationTestBase).GetMethod("CreateJsonContent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var jsonContent = createJsonContentMethod?.Invoke(_testBase, new object[] { loginRequest }) as StringContent;

        var loginResponse = await httpClient!.PostAsync("/Auth/login", jsonContent);

        var deserializeMethod = typeof(IntegrationTestBase).GetMethod("DeserializeResponseAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var genericMethod = deserializeMethod?.MakeGenericMethod(typeof(LoginResponseDto));
        var task = genericMethod?.Invoke(_testBase, new object[] { loginResponse }) as Task<LoginResponseDto>;
        var loginResult = await task!;

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        return (createdAdmin, loginResult);
    }

    /// <summary>
    /// Sets up authentication using the legacy SetAuthenticationHeader method
    /// </summary>
    /// <param name="username">Admin username (default: "testadmin")</param>
    /// <returns>The created admin entity</returns>
    public async Task<Admin> SetupAuthenticationLegacyAsync(string username = "testadmin")
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password123");
        var admin = AdminBuilder.New()
            .WithUsername(username)
            .WithPasswordHash(passwordHash)
            .Build();

        var createdAdmin = await _adminRepository.CreateAsync(admin);

        // Use reflection to access protected method
        var setAuthMethod = typeof(IntegrationTestBase).GetMethod("SetAuthenticationHeader",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        setAuthMethod?.Invoke(_testBase, new object[] { createdAdmin.Id, createdAdmin.Username });

        return createdAdmin;
    }

    /// <summary>
    /// Clears authentication header from HttpClient
    /// </summary>
    public void ClearAuthentication()
    {
        var httpClientProperty = typeof(IntegrationTestBase).GetProperty("HttpClient",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var httpClient = httpClientProperty?.GetValue(_testBase) as HttpClient;
        httpClient!.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Creates test data for customer and book entities
    /// </summary>
    /// <param name="customerFirstName">Customer first name (default: "Test")</param>
    /// <param name="customerLastName">Customer last name (default: "Customer")</param>
    /// <param name="customerEmail">Customer email (default: "test@example.com")</param>
    /// <param name="bookTitle">Book title (default: "Test Book")</param>
    /// <param name="bookAuthor">Book author (default: "Test Author")</param>
    /// <param name="bookIsbn">Book ISBN (default: "1234567890")</param>
    /// <param name="bookStatus">Book status (default: Available)</param>
    /// <returns>Tuple with created customer and book</returns>
    public async Task<(Customer customer, Book book)> CreateTestDataAsync(
        string customerFirstName = "Test",
        string customerLastName = "Customer",
        string customerEmail = "test@example.com",
        string bookTitle = "Test Book",
        string bookAuthor = "Test Author",
        string bookIsbn = "1234567890",
        BookStatus bookStatus = BookStatus.Available)
    {
        var customer = CustomerBuilder.New()
            .WithFirstName(customerFirstName)
            .WithLastName(customerLastName)
            .WithEmail(customerEmail)
            .Build();

        var book = BookBuilder.New()
            .WithTitle(bookTitle)
            .WithAuthor(bookAuthor)
            .WithIsbn(bookIsbn)
            .WithStatus(bookStatus)
            .Build();

        var createdCustomer = await _customerRepository.CreateAsync(customer);
        var createdBook = await _bookRepository.CreateAsync(book);

        return (createdCustomer, createdBook);
    }

    /// <summary>
    /// Creates a test customer entity
    /// </summary>
    /// <param name="firstName">Customer first name (default: "Test")</param>
    /// <param name="lastName">Customer last name (default: "Customer")</param>
    /// <param name="email">Customer email (default: "test@example.com")</param>
    /// <returns>Created customer entity</returns>
    public async Task<Customer> CreateTestCustomerAsync(
        string firstName = "Test",
        string lastName = "Customer",
        string email = "test@example.com")
    {
        var customer = CustomerBuilder.New()
            .WithFirstName(firstName)
            .WithLastName(lastName)
            .WithEmail(email)
            .Build();

        return await _customerRepository.CreateAsync(customer);
    }

    /// <summary>
    /// Creates a test book entity
    /// </summary>
    /// <param name="title">Book title (default: "Test Book")</param>
    /// <param name="author">Book author (default: "Test Author")</param>
    /// <param name="isbn">Book ISBN (default: "1234567890")</param>
    /// <param name="status">Book status (default: Available)</param>
    /// <returns>Created book entity</returns>
    public async Task<Book> CreateTestBookAsync(
        string title = "Test Book",
        string author = "Test Author",
        string isbn = "1234567890",
        BookStatus status = BookStatus.Available)
    {
        var book = BookBuilder.New()
            .WithTitle(title)
            .WithAuthor(author)
            .WithIsbn(isbn)
            .WithStatus(status)
            .Build();

        return await _bookRepository.CreateAsync(book);
    }

    /// <summary>
    /// Creates a test reservation via repository (bypasses API)
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="bookId">Book ID</param>
    /// <returns>Created reservation entity</returns>
    public async Task<Reservation> CreateTestReservationAsync(int customerId, int bookId)
    {
        var reservation = ReservationBuilder.New()
            .WithCustomerId(customerId)
            .WithBookId(bookId)
            .Build();

        return await _reservationRepository.CreateAsync(reservation);
    }

    /// <summary>
    /// Creates a test reservation via HTTP API (full integration)
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="bookId">Book ID</param>
    /// <returns>Created reservation DTO from API response</returns>
    public async Task<ReservationDto> CreateTestReservationViaApiAsync(int customerId, int bookId)
    {
        var reservationCreateDto = new ReservationCreateDto
        {
            CustomerId = customerId,
            BookId = bookId
        };

        // Use reflection to access protected methods
        var httpClientProperty = typeof(IntegrationTestBase).GetProperty("HttpClient",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var httpClient = httpClientProperty?.GetValue(_testBase) as HttpClient;

        var createJsonContentMethod = typeof(IntegrationTestBase).GetMethod("CreateJsonContent",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var jsonContent =
            createJsonContentMethod?.Invoke(_testBase, new object[] { reservationCreateDto }) as StringContent;

        var response = await httpClient!.PostAsync("/Reservations", jsonContent);
        response.EnsureSuccessStatusCode();

        var deserializeMethod = typeof(IntegrationTestBase).GetMethod("DeserializeResponseAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var genericMethod = deserializeMethod?.MakeGenericMethod(typeof(ReservationDto));
        var task = genericMethod?.Invoke(_testBase, new object[] { response }) as Task<ReservationDto>;
        return await task!;
    }

    /// <summary>
    /// Creates multiple test books with different properties for search tests
    /// </summary>
    /// <returns>List of created books</returns>
    public async Task<List<Book>> CreateMultipleTestBooksAsync()
    {
        var books = new List<Book>
        {
            await CreateTestBookAsync("C# Programming", "Author 1", "1111111111"),
            await CreateTestBookAsync("Java Programming", "Author 2", "2222222222"),
            await CreateTestBookAsync("Python Guide", "Author 3", "3333333333", BookStatus.Unavailable),
            await CreateTestBookAsync("Book by John Smith", "John Smith", "4444444444"),
            await CreateTestBookAsync("Another Book", "Jane Doe", "5555555555")
        };

        return books;
    }

    /// <summary>
    /// Creates multiple test customers for list tests
    /// </summary>
    /// <returns>List of created customers</returns>
    public async Task<List<Customer>> CreateMultipleTestCustomersAsync()
    {
        var customers = new List<Customer>
        {
            await CreateTestCustomerAsync("John", "Doe", "john@test.com"),
            await CreateTestCustomerAsync("Jane", "Smith", "jane@test.com"),
            await CreateTestCustomerAsync("Bob", "Johnson", "bob@test.com")
        };

        return customers;
    }

    /// <summary>
    /// Verifies that an HTTP response has the expected status code with detailed error info
    /// </summary>
    /// <param name="response">HTTP response</param>
    /// <param name="expectedStatusCode">Expected status code</param>
    /// <param name="customMessage">Custom error message</param>
    public static async Task AssertStatusCodeAsync(HttpResponseMessage response,
        System.Net.HttpStatusCode expectedStatusCode, string? customMessage = null)
    {
        if (response.StatusCode != expectedStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var message = customMessage ??
                          $"Expected {expectedStatusCode} but got {response.StatusCode}";
            throw new Xunit.Sdk.XunitException(
                $"{message}. Response content: {responseContent}");
        }
    }

    /// <summary>
    /// Creates a CustomerCreateDto with default valid values
    /// </summary>
    /// <param name="firstName">First name (default: "Test")</param>
    /// <param name="lastName">Last name (default: "Customer")</param>
    /// <param name="email">Email (default: "test@example.com")</param>
    /// <returns>CustomerCreateDto instance</returns>
    public static CustomerCreateDto CreateValidCustomerDto(
        string firstName = "Test",
        string lastName = "Customer",
        string email = "test@example.com")
    {
        return new CustomerCreateDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };
    }

    /// <summary>
    /// Creates a BookCreateDto with default valid values
    /// </summary>
    /// <param name="title">Book title (default: "Test Book")</param>
    /// <param name="author">Book author (default: "Test Author")</param>
    /// <param name="isbn">Book ISBN (default: "1234567890")</param>
    /// <param name="status">Book status (default: Available)</param>
    /// <returns>BookCreateDto instance</returns>
    public static BookCreateDto CreateValidBookDto(
        string title = "Test Book",
        string author = "Test Author",
        string isbn = "1234567890",
        BookStatus status = BookStatus.Available)
    {
        return new BookCreateDto
        {
            Title = title,
            Author = author,
            ISBN = isbn,
            Status = status
        };
    }

    /// <summary>
    /// Creates a ReservationCreateDto with specified customer and book IDs
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="bookId">Book ID</param>
    /// <returns>ReservationCreateDto instance</returns>
    public static ReservationCreateDto CreateValidReservationDto(int customerId, int bookId)
    {
        return new ReservationCreateDto
        {
            CustomerId = customerId,
            BookId = bookId
        };
    }

    /// <summary>
    /// Waits for database synchronization (useful after HTTP operations)
    /// </summary>
    /// <param name="delayMs">Delay in milliseconds (default: 100)</param>
    public static async Task WaitForDatabaseSyncAsync(int delayMs = 100)
    {
        await Task.Delay(delayMs);
    }

    /// <summary>
    /// Gets the repository instances for direct access when needed
    /// </summary>
    public (IAdminRepository adminRepo, IRepository<Customer> customerRepo,
        IRepository<Book> bookRepo, IReservationRepository reservationRepo) GetRepositories()
    {
        return (_adminRepository, _customerRepository, _bookRepository, _reservationRepository);
    }
}