# Architecture

Verdiq follows **Clean Architecture** with 4 layers. Dependencies flow inward: API → Infrastructure → Application → Domain.

## Layer Diagram

```
┌──────────────────────────────────────────────┐
│               Verdiq.API                     │
│  Controllers · Middleware · Program.cs       │
│  Depends on: Application, Infrastructure     │
├──────────────────────────────────────────────┤
│           Verdiq.Infrastructure              │
│  EF Core · Repositories · Services           │
│  Depends on: Application                     │
├──────────────────────────────────────────────┤
│            Verdiq.Application                │
│  DTOs · Validators · Interfaces              │
│  Depends on: Domain                          │
├──────────────────────────────────────────────┤
│              Verdiq.Domain                   │
│  Entities · Enums · Domain Interfaces        │
│  No external dependencies                    │
└──────────────────────────────────────────────┘
```

## Domain Layer (`Verdiq.Domain`)

Pure C# with zero external dependencies.

### Entities (all extend `BaseEntity`)
- **User** — Lawyers, admins, assistants with JWT refresh token support
- **Client** — Case clients with contact info, linked to assigned lawyer
- **Case** — Legal cases with status/priority workflow
- **Hearing** — Court hearing dates with reminders
- **Document** — Uploaded case documents with type/category classification
- **Notification** — User notifications with read tracking
- **Subscription** — SaaS billing plans (Free/Pro/Chamber)
- **Payment** — Payment records with bkash/Nagad/card support
- **AuditLog** — Entity change audit trail

### Enums
`UserRole`, `CaseStatus`, `CasePriority`, `HearingStatus`, `DocumentStatus`, `SubscriptionPlan`, `SubscriptionStatus`, `PaymentMethod`

### Domain Interfaces
- `IAuthService`, `IDashboardService`
- `IJwtService` — Token generation/validation
- `IGenericRepository<T>` — CRUD + soft-delete query support
- `IUnitOfWork` — Repository access + `CompleteAsync`

## Application Layer (`Verdiq.Application`)

Orchestration layer with DTOs, validators, and application-level interfaces.

### DTOs by Module
- **Auth:** LoginDto, RegisterDto, AuthResponseDto, TokenRefreshDto
- **Case:** CreateCaseDto, UpdateCaseDto, CaseResponseDto
- **Client:** CreateClientDto, UpdateClientDto, ClientResponseDto
- **Document:** DocumentResponseDto
- **Hearing:** CreateHearingDto, UpdateHearingDto, HearingResponseDto
- **Notification:** CreateNotificationDto, NotificationResponseDto
- **Subscription:** ChangePlanDto, SubscriptionResponseDto
- **Dashboard:** DashboardResponseDto, CaseStatsDto, RecentCaseDto, UpcomingHearingDto

### Validators (FluentValidation)
- `CreateCaseDtoValidator`, `CreateClientDtoValidator`, `CreateHearingDtoValidator`
- `LoginDtoValidator`, `RegisterDtoValidator`
- `AuthValidators` (email/password helpers)

### Application Interfaces
- Service interfaces: `ICaseService`, `IClientService`, `IHearingService`, `IDocumentService`, `INotificationService`, `ISubscriptionService`

## Infrastructure Layer (`Verdiq.Infrastructure`)

Persistence, external services, and infrastructure concerns.

### Data Layer
- **AppDbContext** — EF Core context with 9 DbSets, Fluent API config, global `!IsDeleted` query filters, seed data via `HasData()`
- **AuditSaveChangesInterceptor** — Auto-sets `CreatedAt`/`UpdatedAt`, creates `AuditLog` entries on entity changes
- **GenericRepository\<T\>** — Full CRUD with soft-delete awareness
- **UnitOfWork** — Lazy-loaded repositories, single `SaveChangesAsync` per request

### Services (implement Application interfaces)
- **AuthService** — Register, Login (BCrypt verify + JWT generation), Refresh, Logout (ExecuteDeleteAsync)
- **JwtService** — Access/refresh token generation with `GenerateClaims`
- **CaseService** — CRUD + search + pagination + auto case number generation
- **ClientService** — CRUD + search + pagination
- **HearingService** — CRUD + upcoming/by-date/by-case queries + reminder
- **DocumentService** — Upload/download with file storage + metadata
- **DashboardService** — Aggregated stats (counts by status, upcoming hearings)
- **NotificationService** — CRUD + unread count + mark-all-read
- **SubscriptionService** — Plan change, cancel, admin list-all

## API Layer (`Verdiq.API`)

ASP.NET Core Web API with controllers and middleware.

### Controllers (8 + BaseController)
| Controller | Route | Auth |
|-----------|-------|------|
| AuthController | `api/auth` | No (login/register/refresh) |
| CasesController | `api/cases` | JWT |
| ClientsController | `api/clients` | JWT |
| HearingsController | `api/hearings` | JWT |
| DocumentsController | `api/documents` | JWT |
| NotificationsController | `api/notifications` | JWT |
| SubscriptionController | `api/subscription` | JWT |
| DashboardController | `api/dashboard` | JWT |

**BaseController** provides shared helpers:
- `GetUserId()` → Guid from `ClaimTypes.NameIdentifier`
- `GetUserRole()` → string from `ClaimTypes.Role`
- `IsAdmin()` → bool check
- `GetIpAddress()` → from `X-Forwarded-For` or `RemoteIpAddress`

### Middleware Pipeline
```
ExceptionMiddleware → RequestLoggingMiddleware → RateLimiting → CORS → Auth → Authorization → Controllers
```

### CORS
```csharp
// AllowFrontend policy — permits any origin with credentials
policy.SetIsOriginAllowed(_ => true)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials();
```
Previously used `WithOrigins("http://localhost:3000")` but changed to `SetIsOriginAllowed` to support both `localhost` and Docker network IPs during development.

### Response Wrappers
- `ApiResponse<T>` — `{ success, message, data, errors? }`
- `PagedResponse<T>` — extends ApiResponse with `{ page, pageSize, totalCount, totalPages, hasPreviousPage, hasNextPage }`

### Rate Limiting
- 100 requests per minute per IP (fixed window)
- Returns 429 Too Many Requests when exceeded

### Database Initialization
- `EnsureCreated()` runs in non-Testing environments
- Seed data (admin + lawyer users, subscriptions) is applied via `HasData()` in `OnModelCreating`
- No migration files are used — switch to `Migrate()` if migration-based workflow is needed
- `__EFMigrationsHistory` table from a previous `Migrate()` call will cause `EnsureCreated()` to skip table creation (must be dropped if switching strategies)

## Frontend Architecture (`frontend/`)

### Pages (Next.js App Router)
```
(app)/
  (auth)/
    login/
  (dashboard)/
    lawyer/
      page.tsx            # Dashboard with stats + recent activity
      cases/page.tsx      # Case list with filters + pagination
      clients/page.tsx    # Client list + create dialog
      hearings/page.tsx   # Hearing calendar + detail panel
      documents/page.tsx  # Document grid/list + upload + preview
      billing/page.tsx    # Subscription management
      ai-assistant/       # AI chat assistant
    admin/                # Admin panel
```

### Auth Flow
```
Login Page → authService.login() → API /api/auth/login
  ↓
Stores tokens in:
  - localStorage (for axios interceptor — reads from here)
  - Cookies (for middleware.ts — reads from here)
  ↓
Redirects to /lawyer
  ↓
middleware.ts checks access_token cookie:
  - Found → allow
  - Not found → redirect /login
```

### Field Mapping
Frontend services map API field names to local types to decouple components from API shapes:

| API Field | Mapped To | Service |
|-----------|-----------|---------|
| `fullName` | `name` | auth-service (mapUser) |
| `avatarUrl` | `avatar` | auth-service (mapUser) |
| `hearingDate` | `date` | hearing-service |
| `judgeName` | `judge` | hearing-service |

### Data Flow
```
Page/Component → React Query Hook → Service (axios) → Backend API
                                    ↕
                              Cache invalidation
```

### State Management
- **Server state:** TanStack React Query (6 hook files with cache invalidation)
- **Client state:** Zustand (`auth-store.ts` — user, tokens, login/logout/initialize, clears cookies on logout)
- **HTTP client:** Axios instance with JWT refresh interceptor, `withCredentials: true`

### Environment
- `.env.local` is required (`NEXT_PUBLIC_API_URL=http://localhost:5000/api`)
- Without it, the fallback URL is used, but Next.js may optimize away the env check

## Key Design Decisions

1. **Guid Ids** — All entities use `Guid` primary keys (no auto-increment)
2. **Soft Delete** — Global `IsDeleted` query filter on all entities
3. **Pagination** — Every list endpoint returns `PagedResponse<T>` with page metadata
4. **Field Mapping** — Frontend services map API field names via mapper functions (decouples components from API shapes)
5. **Dual Token Storage** — localStorage for axios interceptor + cookies for Next.js middleware
6. **base-ui runtime** — All pages use `"use client"` directive
7. **Tailwind CSS v4** — No `tailwind.config.js`; all config in CSS via `@import "tailwindcss"`
8. **OpenApi v2** — Microsoft.OpenApi namespace (not Swashbuckle's Swashbuckle.AspNetCore)
9. **CORS: SetIsOriginAllowed** — Allows any origin during development (avoids IP mismatch issues)

## New Modules (Phases 1-4)

### Phase 1: Real Analytics
- `GET /api/dashboard/case-chart?months=12` — Monthly case status breakdown
- `GET /api/dashboard/recent-activities?count=10` — Recent case/hearing/document activity feed
- Frontend uses **recharts** (`BarChart`, `PieChart`) replacing CSS-based charts
- `RecentActivities` component with type-based icons and relative timestamps

### Phase 2: AI Legal Assistant
- `Verdiq.Domain.Entities.AiConversation` — Stores chat history per user
- `IAIService` with 6 methods: Chat, AnalyzeCase, SummarizeDocument, GenerateLegalNotice, SearchJudgements, GeneratePetition
- `AIService` implementation: calls OpenAI API when `OpenAI:ApiKey` is configured; falls back to keyword-based responses when not
- `AIController`: 7 endpoints (`POST /api/ai/*` + `GET /api/ai/history`)
- Frontend: `useAiChat()` mutation hook, real API calling (no more setTimeout mock)

### Phase 3: Admin Panel
- `IAdminService` with user management, case overview, revenue analytics, system stats
- `AdminController` (Admin-only): `GET /api/admin/users`, `PATCH /api/admin/users/:id/status`, `DELETE /api/admin/users/:id`, `GET /api/admin/cases`, `GET /api/admin/revenue`, `GET /api/admin/system-stats`
- Frontend: `useAdminUsers`, `useAdminSystemStats`, `useAdminRevenue` hooks, real data in dashboard stats cards, user table with suspend/delete actions

### Phase 4: Global Search Engine
- `ISearchService` — Searches cases, clients, hearings, documents by `LOWER LIKE` across multiple fields
- `SearchController`: `GET /api/search?q=...&limit=10`
- Frontend: `useSearch` hook (enabled only when query >= 2 chars), `search-service.ts`
- Navbar search input is now functional with live dropdown showing type-icon + title + subtitle + status badge

### Phase 5: Real Navbar
- User info pulled from Zustand `useAuthStore` (no hardcoded "Adv. A. Karim")
- Notification bell shows real unread count from `useUnreadCount()`
- Dropdown links: Settings `/lawyer/settings`, Admin Panel `/admin` (role-gated), Logout
- Global search integrated directly into navbar with debounced API calls
