using Microsoft.Extensions.DependencyInjection;
using src.Infrastructure.Data;
using src.IntegrationTests.Infrastructure;
using src.IntegrationTests.Helpers;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace src.IntegrationTests.Base;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;
    protected readonly HttpClient HttpClient;
    protected readonly AuthenticationHelper AuthHelper;

    protected IntegrationTestBase()
    {
        _factory = new TestWebApplicationFactory();
        HttpClient = _factory.CreateClient();
        AuthHelper = new AuthenticationHelper();
    }

    protected async Task<ApplicationDbContext> GetDbContextAsync()
    {
        return await _factory.GetDbContextAsync();
    }

    protected void SetAuthenticationHeader(int adminId = 1, string username = "testadmin")
    {
        var token = AuthHelper.GenerateJwtToken(adminId, username);
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected static StringContent CreateJsonContent(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    protected static async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public virtual async Task InitializeAsync()
    {
        await _factory.CleanupDatabaseAsync();
    }

    public virtual async Task DisposeAsync()
    {
        HttpClient.Dispose();
        await _factory.DisposeAsync();
    }
}
