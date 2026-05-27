# Testing

## Test Project

**Location:** `backend/tests/Verdiq.API.Tests/`

**Framework:** xUnit + Testcontainers.PostgreSql + FluentAssertions

The test project uses **real PostgreSQL via Testcontainers** (not in-memory databases) to ensure production-grade reliability.

---

## Running Tests

### Prerequisites
- Docker Desktop (Testcontainers spins up a PostgreSQL container)
- .NET 10 SDK

### From Command Line

```bash
cd backend

# Run all tests
dotnet test

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "AuthTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~Admin_CanLogin"

# Run tests without rebuilding (after dotnet build)
dotnet test --no-build --filter "CasesControllerTests"
```

### NPM Workaround (Windows)

If `dotnet test` fails with npm-related errors:

```powershell
$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"
dotnet test
```

---

## Test Structure

```
tests/Verdiq.API.Tests/
  Integration/
    CustomWebApplicationFactory.cs   # Test infrastructure
    TestBase.cs                      # Shared base class
    AuthTests.cs                     # Authentication tests (9 tests)
    CasesControllerTests.cs          # Cases CRUD tests (9 tests)
```

---

## Test Infrastructure

### CustomWebApplicationFactory

`CustomWebApplicationFactory.cs` sets up the test environment:

1. **Testcontainers PostgreSQL** — Spins up `postgres:16-alpine` container
2. **Overrides DbContext** — Replaces the production connection string with Testcontainer's
3. **DbMigrationFilter** — Runs `EnsureCreated()` + seed data on app startup
4. **Implements IAsyncLifetime** — Container starts before tests, disposes after

### TestBase

`TestBase.cs` provides shared helpers:

| Method | Description |
|--------|-------------|
| `GetAdminTokenAsync()` | Logs in as admin@verdiq.com, returns access token |
| `GetLawyerTokenAsync()` | Logs in as lawyer@verdiq.com, returns access token |
| `SetAuthHeader(token)` | Sets `Authorization: Bearer` on client |
| `ClearAuthHeader()` | Removes auth header |

### Response DTOs for Tests

- `AuthTestResponse` — Parses login response
- `ApiTestResponse<T>` — Parses `ApiResponse<T>` wrapper
- `PagedTestResponse<T>` — Parses `PagedResponse<T>` wrapper

### Testing Environment

The test project sets `ASPNETCORE_ENVIRONMENT=Testing` via `WebApplicationFactory`. This triggers conditional behavior in `Program.cs`:

- **Serilog bootstrap logger disabled** — no file/console logs during tests
- **Serilog request logging disabled** — `UseSerilogRequestLogging` skipped
- **Migration/seed skipped** — `EnsureCreated()` is NOT called in Production block; instead, `DbMigrationFilter` (registered as `IStartupFilter`) runs `EnsureCreated()` + seed
- **AuditSaveChangesInterceptor not registered** — audit logging omitted in tests

---

## Writing Tests

### Pattern

```csharp
public class MyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public MyTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MyTest_Scenario_ExpectedResult()
    {
        // Arrange
        var token = await GetAdminTokenAsync();
        SetAuthHeader(token);
        var content = new StringContent(
            JsonSerializer.Serialize(new { ... }),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await _client.PostAsync("/api/endpoint", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiTestResponse<ExpectedType>>(body);
        result.Success.Should().BeTrue();
    }
}
```

### Best Practices

1. **Use unique data per test** — Avoid shared state conflicts
2. **Prefer Guid IDs** — Cast route parameters: `Guid.NewGuid()` for non-existent lookups
3. **Avoid test interdependence** — Each test should set up its own data
4. **Use FluentAssertions** — `Should().Be()`, `Should().BeTrue()`, etc.
5. **Test both success and failure** — Valid login AND invalid login

---

## Current Test Coverage

### AuthTests (9 tests — all passing)
| Test | Description |
|------|-------------|
| Admin_CanLogin | Valid admin credentials return tokens |
| Lawyer_CanLogin | Valid lawyer credentials return tokens |
| Login_WithInvalidPassword_ReturnsUnauthorized | Wrong password rejected |
| Login_WithNonExistentEmail_ReturnsUnauthorized | Unknown email rejected |
| Register_NewUser_CreatesAccount | Valid registration succeeds |
| Register_DuplicateEmail_ReturnsBadRequest | Duplicate email rejected |
| RefreshToken_WithValidTokens_ReturnsNewTokens | Token refresh works |
| RefreshToken_WithInvalidToken_ReturnsUnauthorized | Bad refresh token rejected |
| ProtectedEndpoint_WithoutToken_ReturnsUnauthorized | No token → 401 |
| ProtectedEndpoint_WithToken_ReturnsData | Valid token → data |
| Logout_InvalidatesToken | After logout, refresh token fails |

### CasesControllerTests (9 tests — 9 passing)
| Test | Description |
|------|-------------|
| GetCases_ReturnsPagedResponse | Pagination format correct |
| CreateCase_ReturnsCreated | Valid case creation (courtName + clientIds[]) |
| GetCaseById_ReturnsCase | Single case retrieval |
| GetCaseById_NonExistent_ReturnsNotFound | Missing ID → 404 |
| UpdateCase_ReturnsUpdatedCase | Case update works |
| DeleteCase_RemovesCase | Soft delete works |
| SearchCases_ByTitle_ReturnsMatches | Search by title |
| Lawyer_canDeleteCase_AsNoRoleRestriction | Lawyer can delete (no admin restriction) |
| GetCases_Pagination_Respected | Page/size limits respected |

**Note:** All tests updated to use correct DTO field names (`courtName` instead of `court`, `clientIds[]` instead of `clientId`). Tests also pass `sortBy`/`sortOrder` params to verify pagination endpoint compatibility.

---

## Test Data

Seed users in test environment (same as production):

| Email | Password | Role |
|-------|----------|------|
| admin@verdiq.com | admin123 | Admin |
| lawyer@verdiq.com | lawyer123 | Lawyer |

Seeding is done in `DbMigrationFilter.SeedTestData()` inside `CustomWebApplicationFactory.cs`.

---

## Adding New Tests

1. Create `{Feature}Tests.cs` in `Integration/`
2. Implement `IClassFixture<CustomWebApplicationFactory>`
3. Use `TestBase` helpers for auth
4. Write Arrange-Act-Assert with FluentAssertions
5. Verify with `dotnet test --filter "{Feature}Tests"`

### Example: HearingsControllerTests

```csharp
public class HearingsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HearingsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUpcomingHearings_ReturnsList()
    {
        var token = await GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/hearings/upcoming");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

---

## Known Issues

| Issue | Workaround |
|-------|-----------|
| `SeedDefaultUsers` function defined but never used in `Program.cs` | Safe to ignore (CS8321 warning); used only in an earlier revision |
| `__EFMigrationsHistory` table from `Migrate()` fools `EnsureCreated()` | Drop the table before first run if switching strategies |
| Batch test failure (1 of 18 fails) | Run the failing test in isolation; may be shared fixture state |

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Testcontainers port conflict | Stop other PostgreSQL containers |
| "Collection was modified" in AuditSaveChangesInterceptor | Ensure `.ToList()` before `foreach` (already fixed) |
| Batch test failure (1 of 18 fails) | Run the failing test in isolation; may be shared fixture state |
| NPM errors during build | Set `$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"` |
