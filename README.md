# Verdiq — Law Firm Management System (11 Modules)

A production-grade SaaS Law Firm/Chamber Management System for the Bangladesh legal market. Built with ASP.NET Core 10 + Next.js 16 with PostgreSQL.

## Tech Stack

### Backend
- **Runtime:** .NET 10 (SDK 10.0.300)
- **Framework:** ASP.NET Core 10
- **Database:** PostgreSQL 16 + Entity Framework Core 10.0.0-preview
- **Auth:** JWT Bearer with refresh token rotation, BCrypt password hashing, ChamberId claim
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
$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"
dotnet restore
dotnet build
docker compose up -d
dotnet run --project Verdiq.API
```

### Frontend

```bash
cd frontend
$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"
copy .env.example .env.local
npm install
npm run dev
```

### Seed Users

| Email | Password | Role |
|-------|----------|------|
| admin@verdiq.com | admin123 | Owner |
| lawyer@verdiq.com | lawyer123 | SeniorLawyer |

### Super Admin Access

| User ID | Password | URL |
|---------|----------|-----|
| rudra | rudra | `/super-admin/login` |

## 11 Modules

| # | Module | Description |
|---|--------|-------------|
| 1 | Authentication & Chamber | Multi-chamber, role-based access (Owner/SeniorLawyer/JuniorLawyer/Assistant/Accountant/Client), permission system |
| 2 | Case Management | Case CRUD with auto-numbering (VER-YYYY-XXXX), search/sort/filter, timeline (CaseActivity), real-time updates via SignalR, hearing management, cause list tracking |
| 3 | Client Management | Profiles (name/nid/company), many-to-many client-case linking, client timeline |
| 4 | Document Management | Upload (PDF/DOCX/Image), OCR search, version control, folder structure (Petition/Evidence/Order/Agreement) |
| 5 | Legal Drafting | Template library, AI draft generator, smart variables ({{client_name}}, {{court_name}}, {{case_number}}) |
| 6 | AI Legal Assistant | Case summary, hearing prep, Bangla chatbot, voice-to-note |
| 7 | Calendar & Reminder | Smart calendar, multi-channel reminders (SMS/Push/WhatsApp/Email) |
| 8 | Billing & Finance | Invoice system (INV-YYYY-XXXX), expense tracking (court fees/stamp/transport), subscription billing |
| 9 | Internal Chamber | Task assignment (Senior→Junior), internal notes, attendance |
| 10 | Court & Legal Database | Laws (Penal Code/CPC/CrPC/Constitution), judgment search (citation/judge/keyword) |
| 11 | Analytics Dashboard | Active cases, win ratio, upcoming hearings, pending bills, lawyer productivity |
| SA | Super Admin System | Centralized control: chamber management (upgrade/downgrade/clear/impersonate), user management (reset passwords/toggle status/override subscriptions), system-wide case view, audit logs, billing overview, system config, broadcast notifications, health monitoring |

## Project Structure

```
backend/
  Verdiq.Domain/          # 24 entities, 13 enums, 5 interfaces
  Verdiq.Application/     # 18 DTO groups, 19 service interfaces, 6 validators
  Verdiq.Infrastructure/  # EF Core (27 DbSets), 17 services, audit interceptor
  Verdiq.API/             # 21 controllers, 3 middleware, 2 SignalR hubs
  tests/
    Verdiq.API.Tests/

frontend/
  src/
    app/                  # 30 pages (App Router)
    components/           # 21 UI primitives + 15 feature components
    lib/
      services/           # 19 API service files
      hooks/              # 22 React Query hook files
      store/              # Zustand auth store
      api.ts              # Axios with JWT refresh interceptor
    types/                # 20+ TypeScript interfaces
```

## Database Schema (27 Tables)

- `Chambers` — Multi-chamber support with subscription plan
- `Users` — 6 roles, linked to chamber
- `Permissions`, `RolePermissions` — Fine-grained role-based access control
- `Cases` — Case management core
- `CaseActivities` — Case timeline/hearing/order/note tracking
- `CauseLists` — Court cause list data
- `Clients` — Client profiles with NID, company
- `ClientCases` — Many-to-many client-case join
- `Hearings` — Court hearings with result/next-hearing-date
- `Documents`, `DocumentVersions`, `DocumentContents` — Document management + OCR
- `Templates` — Legal drafting templates with smart variables
- `Invoices`, `Expenses`, `Payments` — Billing & finance
- `Subscriptions` — Chamber subscription plans
- `Tasks` — Internal task assignment
- `Reminders` — Multi-channel reminder engine
- `LegalDocuments` — Laws & judgment database
- `Notifications` — User notifications
- `AuditLogs` — Entity change audit trail
- `AiConversations` — AI chat history

## Key Conventions

- All pages use `"use client"` (base-ui runtime requirement)
- `BaseEntity.Id` is `Guid`
- List endpoints return `PagedResponse<T>`; mutation endpoints return `ApiResponse<T>`
- Soft delete via `IsDeleted` global query filter on all entities
- Audit logging via `AuditSaveChangesInterceptor`
- Auth tokens in **both** localStorage and cookies
- Frontend services map API field names via mapper functions
- CORS: `SetIsOriginAllowed(_ => true)` with credentials
- `.env.local` is required for frontend
- All queries scoped by `ChamberId` from JWT claim

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Login redirects back to `/login` | Create `.env.local` with `NEXT_PUBLIC_API_URL` |
| 401 on API calls | Check `access_token` in localStorage |
| CORS error | Verify API running on port 5000 |
| `npm run dev` fails | Run `$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"` first |

## License

Proprietary — Verdiq
