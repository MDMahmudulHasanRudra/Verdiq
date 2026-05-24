using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Verdiq.Infrastructure.Data;

namespace Verdiq.API.Tests.Integration;

public abstract class TestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected TestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected async Task<string> GetAdminTokenAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@verdiq.com",
            password = "admin123"
        });

        var result = await response.Content.ReadFromJsonAsync<AuthTestResponse>();
        return result?.AccessToken ?? "";
    }

    protected async Task<string> GetLawyerTokenAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "lawyer@verdiq.com",
            password = "lawyer123"
        });

        var result = await response.Content.ReadFromJsonAsync<AuthTestResponse>();
        return result?.AccessToken ?? "";
    }

    protected void SetAuthHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected void ClearAuthHeader()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }
}

public class AuthTestResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class ApiTestResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public class PagedTestResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}
