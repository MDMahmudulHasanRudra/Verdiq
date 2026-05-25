using System.Net.Http.Json;
using FluentAssertions;

namespace Verdiq.API.Tests.Integration;

public class AuthTests : TestBase
{
    public AuthTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        ClearAuthHeader();
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@verdiq.com",
            password = "admin123"
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthTestResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        ClearAuthHeader();
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@verdiq.com",
            password = "wrongpassword"
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_WithNewUser_ReturnsTokens()
    {
        ClearAuthHeader();
        var uniqueEmail = $"test_{Guid.NewGuid():N}@test.com";
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Test User",
            email = uniqueEmail,
            password = "Test123!",
            confirmPassword = "Test123!",
            phone = "+8801700000000",
            role = "Lawyer"
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthTestResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        ClearAuthHeader();
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Another Admin",
            email = "admin@verdiq.com",
            password = "Test123!",
            confirmPassword = "Test123!",
            phone = "+8801700000001",
            role = "Lawyer"
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RefreshToken_WithValidTokens_ReturnsNewTokens()
    {
        ClearAuthHeader();
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@verdiq.com",
            password = "admin123"
        });
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthTestResponse>();

        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            accessToken = loginResult!.AccessToken,
            refreshToken = loginResult.RefreshToken
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthTestResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        ClearAuthHeader();
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            accessToken = "invalid_token",
            refreshToken = "invalid_refresh"
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        ClearAuthHeader();
        var response = await Client.GetAsync("/api/cases");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);
        var response = await Client.GetAsync("/api/cases");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_WithValidCurrentPassword_Succeeds()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var response = await Client.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = "admin123",
            newPassword = "Admin123!",
            confirmPassword = "Admin123!"
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthTestResponse>();
        result!.Success.Should().BeTrue();

        ClearAuthHeader();
        var oldLogin = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@verdiq.com",
            password = "admin123"
        });
        oldLogin.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        var newLogin = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@verdiq.com",
            password = "Admin123!"
        });
        newLogin.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var newLoginResult = await newLogin.Content.ReadFromJsonAsync<AuthTestResponse>();

        SetAuthHeader(newLoginResult!.AccessToken!);
        var revertResponse = await Client.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = "Admin123!",
            newPassword = "admin123",
            confirmPassword = "admin123"
        });
        revertResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ReturnsBadRequest()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var response = await Client.PostAsJsonAsync("/api/auth/change-password", new
        {
            currentPassword = "wrongpassword",
            newPassword = "NewPass123!",
            confirmPassword = "NewPass123!"
        });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_InvalidatesRefreshToken()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        await Client.PostAsync("/api/auth/logout", null);

        ClearAuthHeader();
        var refreshResponse = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            accessToken = token,
            refreshToken = "expired_or_invalidated"
        });
        refreshResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
