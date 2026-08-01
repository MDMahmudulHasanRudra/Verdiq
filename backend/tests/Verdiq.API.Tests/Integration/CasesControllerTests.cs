using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Verdiq.API.Tests.Integration;

public class CasesControllerTests : TestBase
{
    public CasesControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<Guid> CreateTestClientAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/clients", new
        {
            fullName = "Test Client",
            email = $"client_{Guid.NewGuid():N}@test.com",
            phone = "+8801700000001",
            address = "Test Address"
        });

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Create client failed ({response.StatusCode}): {body}");
        }

        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        return doc!.RootElement.GetProperty("data").GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task GetCases_ReturnsPagedResponse()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var response = await Client.GetAsync("/api/cases?page=1&pageSize=10");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        doc.Should().NotBeNull();
        doc!.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task CreateCase_WithValidData_ReturnsCreated()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var clientId = await CreateTestClientAsync();
        var response = await Client.PostAsJsonAsync("/api/cases", new
        {
            title = "Test Case",
            caseType = "Civil",
            courtName = "Dhaka District Court",
            clientIds = new[] { clientId },
            description = "A test case description",
            priority = "medium"
        });

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException($"Create case failed ({response.StatusCode}): {body}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task GetCaseById_WithExistingId_ReturnsCase()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var clientId = await CreateTestClientAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/cases", new
        {
            title = "Case To Find",
            caseType = "Criminal",
            courtName = "Dhaka District Court",
            clientIds = new[] { clientId },
            description = "Searchable case",
            priority = "high"
        });

        var createBody = await createResponse.Content.ReadAsStringAsync();
        if (!createResponse.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException($"Create case failed ({createResponse.StatusCode}): {createBody}");

        var createDoc = JsonDocument.Parse(createBody);
        var data = createDoc.RootElement.GetProperty("data");
        var idProp = data.GetProperty("id");
        Guid caseId;
        if (idProp.ValueKind == JsonValueKind.String)
            caseId = idProp.GetGuid();
        else
            caseId = Guid.Parse(idProp.GetRawText());

        var response = await Client.GetAsync($"/api/cases/{caseId}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        doc!.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetProperty("title").GetString().Should().Be("Case To Find");
    }

    [Fact]
    public async Task GetCaseById_WithNonExistentId_ReturnsNotFound()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var response = await Client.GetAsync($"/api/cases/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCase_WithValidData_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var clientId = await CreateTestClientAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/cases", new
        {
            title = "Original Title",
            caseType = "Family",
            courtName = "Dhaka District Court",
            clientIds = new[] { clientId },
            description = "Original description",
            priority = "low"
        });
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createBody);
        var idProp = createDoc.RootElement.GetProperty("data").GetProperty("id");
        var caseId = idProp.ValueKind == JsonValueKind.String ? idProp.GetGuid() : Guid.Parse(idProp.GetRawText());

        var response = await Client.PutAsJsonAsync($"/api/cases/{caseId}", new
        {
            title = "Updated Title",
            caseType = "Family",
            courtName = "Dhaka District Court",
            description = "Updated description"
        });

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException($"Update case failed ({response.StatusCode}): {body}");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetProperty("title").GetString().Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeleteCase_WithExistingId_ReturnsOk()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var clientId = await CreateTestClientAsync();
        var createResponse = await Client.PostAsJsonAsync("/api/cases", new
        {
            title = "Case To Delete",
            caseType = "Civil",
            courtName = "Dhaka District Court",
            clientIds = new[] { clientId },
            description = "Will be deleted",
            priority = "medium"
        });
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createBody);
        var idProp = createDoc.RootElement.GetProperty("data").GetProperty("id");
        var caseId = idProp.ValueKind == JsonValueKind.String ? idProp.GetGuid() : Guid.Parse(idProp.GetRawText());

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/cases/{caseId}")
        {
            Content = JsonContent.Create(new { email = "admin@verdiq.com", password = "admin123" })
        };
        var response = await Client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException($"Delete case failed ({response.StatusCode}): {body}");
        }

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchCases_ByTitle_ReturnsFilteredResults()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var clientId = await CreateTestClientAsync();
        await Client.PostAsJsonAsync("/api/cases", new
        {
            title = "Alpha Specific Case",
            caseType = "Civil",
            courtName = "Dhaka District Court",
            clientIds = new[] { clientId },
            description = "Search target",
            priority = "low"
        });

        var response = await Client.GetAsync("/api/cases?search=Alpha&page=1&pageSize=10");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        doc!.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        var data = doc.RootElement.GetProperty("data");
        data.GetArrayLength().Should().BeGreaterThan(0);
        var titles = data.EnumerateArray().Select(e => e.GetProperty("title").GetString());
        titles.Should().Contain("Alpha Specific Case");
    }

    [Fact]
    public async Task Lawyer_canDeleteCase_AsNoRoleRestriction()
    {
        var adminToken = await GetAdminTokenAsync();
        SetAuthHeader(adminToken);
        var clientId = await CreateTestClientAsync();
        var createBody = await (await Client.PostAsJsonAsync("/api/cases", new
        {
            title = "Auth Test Case",
            caseType = "Civil",
            courtName = "Dhaka District Court",
            clientIds = new[] { clientId },
            description = "Testing auth",
            priority = "low"
        })).Content.ReadAsStringAsync();
        var createDoc = JsonDocument.Parse(createBody);
        var idProp = createDoc.RootElement.GetProperty("data").GetProperty("id");
        var caseId = idProp.ValueKind == JsonValueKind.String ? idProp.GetGuid() : Guid.Parse(idProp.GetRawText());

        var lawyerToken = await GetLawyerTokenAsync();
        SetAuthHeader(lawyerToken);

        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/cases/{caseId}")
        {
            Content = JsonContent.Create(new { email = "lawyer@verdiq.com", password = "lawyer123" })
        };
        var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Pagination_PageSizeRespected()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var clientId = await CreateTestClientAsync();
        for (int i = 0; i < 5; i++)
        {
            await Client.PostAsJsonAsync("/api/cases", new
            {
                title = $"Pagination Case {i}",
                caseType = "Civil",
                courtName = "Dhaka District Court",
                clientIds = new[] { clientId },
                description = "Pagination test",
                priority = "medium"
            });
        }

        var response = await Client.GetAsync("/api/cases?page=1&pageSize=3&sortBy=id&sortOrder=asc");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        doc!.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetArrayLength().Should().BeLessThanOrEqualTo(3);
        doc.RootElement.GetProperty("totalPages").GetInt32().Should().BeGreaterThanOrEqualTo(1);
    }
}
