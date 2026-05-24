# Verdiq — Lawyer Management System

A production-grade SaaS Lawyer Management System for the Bangladesh legal market. Built with ASP.NET Core 10 + Next.js 16 with PostgreSQL.

## Tech Stack

### Backend
- **Runtime:** .NET 10 (SDK 10.0.300)
- **Framework:** ASP.NET Core 10
- **Database:** PostgreSQL 16 + Entity Framework Core 10.0.0-preview
- **Auth:** JWT Bearer with refresh token rotation, BCrypt password hashing
- **API:** RESTful, OpenAPI 2.4.1 (Swagger)
- **Validation:** FluentValidation
- **Logging:** Serilog
- **Rate Limiting:** System.Threading.RateLimiting (100 req/min per IP)

### Frontend
- **Framework:** Next.js 16.2.6 (App Router)
- **Bundler:** Turbopack
- **UI Library:** `@base-ui/react` v1.5 (shadcn v4+)
- **Styling:** Tailwind CSS v4 (OKLCH color tokens, no tailwind.config.js)
- **State:** Zustand (client state), TanStack React Query (server state)
- **HTTP:** Axios with token refresh interceptor
- **Icons:** lucide-react

## Quick Start

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- Docker Desktop (for PostgreSQL)

### Backend

```bash
cd backend

# Set environment variable workaround for npm (Windows)
$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"

# Restore and build
dotnet restore
dotnet build

# Run with PostgreSQL via Docker
docker compose up -d
# API at http://localhost:5000

# Or run directly (requires PostgreSQL on localhost:5432)
dotnet run --project Verdiq.API
```

### Frontend

```bash
cd frontend

# NPM workaround (Windows)
$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"

# Create environment config (required!)
copy .env.example .env.local

npm install
npm run dev
# App at http://localhost:3000
```

### Seed Users

| Email | Password | Role |
|-------|----------|------|
| admin@verdiq.com | admin123 | Admin |
| lawyer@verdiq.com | lawyer123 | Lawyer |

## Project Structure

```
backend/
  Verdiq.Domain/          # Entities, enums, interfaces (pure C#)
  Verdiq.Application/     # DTOs, validators, application interfaces
  Verdiq.Infrastructure/  # EF Core DbContext, repositories, services
  Verdiq.API/             # Controllers, middleware, Program.cs
  tests/
    Verdiq.API.Tests/     # xUnit + Testcontainers integration tests
  Dockerfile
  docker-compose.yml

frontend/
  src/
    app/                  # Next.js App Router pages
    components/           # Shared UI + feature components
    lib/
      services/           # API service layer (8 service files)
      hooks/              # React Query hooks (6 hook files)
      store/              # Zustand stores
      api.ts              # Axios client with interceptors
      api-response.ts     # ApiResponse<T> + PagedResponse<T> types
    types/                # TypeScript type definitions
```

## Key Conventions

- All pages use `"use client"` (base-ui runtime requirement)
- `BaseEntity.Id` is `Guid` (not int)
- All list endpoints return `PagedResponse<T>` (page, pageSize, totalCount, totalPages)
- All mutation endpoints return `ApiResponse<T>`
- Soft delete via `IsDeleted` global query filter
- Audit logging via `AuditSaveChangesInterceptor`
- Auth tokens stored in **both** localStorage and cookies (middleware checks cookies, axios reads localStorage)
- Frontend services map API field names (`fullName` → `name`, `avatarUrl` → `avatar`) via mapper functions
- CORS allows any origin with credentials (`SetIsOriginAllowed(_ => true)`)
- NPM requires workaround on Windows: `$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"`
- Next.js 16.2.6 deprecates `middleware.ts` in favor of `proxy.ts`
- `.env.local` is required for the frontend (not optional)

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Login redirects back to `/login` | Make sure `.env.local` exists with `NEXT_PUBLIC_API_URL` and restart dev server |
| 401 on API calls | Tokens stored in localStorage after login; check `access_token` in Application > Local Storage |
| CORS error in browser | Backend uses `SetIsOriginAllowed(_ => true)`, but verify the API is running on port 5000 |
| `npm run dev` fails | Run `$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"` first |

## License

Proprietary — Verdiq
