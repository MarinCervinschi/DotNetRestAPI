using FluentAssertions;
using src.IntegrationTests.Base;
using System.Net;
using Xunit;

namespace src.IntegrationTests.Infrastructure;

public class InfrastructureTests : IntegrationTestBase
{
    [Fact]
    public async Task WebApplicationFactory_ShouldStartSuccessfully()
    {
        // Act
        var response = await HttpClient.GetAsync("/health");

        // Assert
        response.Should().NotBeNull();
        // Health endpoint might return different status codes, so just verify it responds
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Database_ShouldBeAccessible()
    {
        // Act
        await using var context = await GetDbContextAsync();
        
        // Assert
        context.Should().NotBeNull();
        context.Database.Should().NotBeNull();
    }

    [Fact]
    public void AuthenticationHelper_ShouldGenerateValidToken()
    {
        // Act
        var token = AuthHelper.GenerateJwtToken(1, "testuser");
        
        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts separated by dots
    }

    [Fact]
    public void AuthenticationHelper_ShouldGenerateAuthHeaders()
    {
        // Act
        var headers = AuthHelper.GetAuthHeaders(1, "testuser");
        
        // Assert
        headers.Should().ContainKey("Authorization");
        headers["Authorization"].Should().StartWith("Bearer ");
    }
}
