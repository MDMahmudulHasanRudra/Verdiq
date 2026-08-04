using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Verdiq.API.Tests.Integration;

public class ConfigurationControllerTests : TestBase
{
    public ConfigurationControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task UpdateConfiguration_WithExtendedSections_PersistsSettings()
    {
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);

        var response = await Client.PutAsJsonAsync("/api/configuration", new
        {
            general = new
            {
                companyName = "Acme Legal",
                companyNameBn = "এসিএম লিগ্যাল",
                language = "bn",
                currency = "USD"
            },
            billing = new
            {
                taxRatePercent = 12,
                invoiceDueDays = 14,
                lateFeePercent = 2
            },
            integrations = new
            {
                googleDriveEnabled = true,
                storageProvider = "cloudinary"
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonDocument>();
        body.Should().NotBeNull();
        body!.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();

        var readResponse = await Client.GetAsync("/api/configuration");
        readResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readBody = await readResponse.Content.ReadFromJsonAsync<JsonDocument>();
        readBody.Should().NotBeNull();
        var settings = readBody!.RootElement.GetProperty("data").GetProperty("settings");

        settings.GetProperty("general").GetProperty("companyName").GetString().Should().Be("Acme Legal");
        settings.GetProperty("billing").GetProperty("invoiceDueDays").GetInt32().Should().Be(14);
        settings.GetProperty("integrations").GetProperty("storageProvider").GetString().Should().Be("cloudinary");
    }
}
