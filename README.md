# Verdiq — Law Firm Management System 

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

### Docker (Recommended — starts all services)

```bash
cd backend
docker compose build
docker compose up -d
```

This starts PostgreSQL (5432), the API (5000), and the Next.js frontend (3000).

### Backend (Local)

```bash
cd backend
$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"
dotnet restore
dotnet build
docker compose up -d    # starts PostgreSQL only
dotnet run --project Verdiq.API
```

### Frontend (Local)

```bash
cd frontend
$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"
copy .env.example .env.local
npm install
npm run dev
```

## Phase 7 — Advanced Case/Client Fields, Chamber Config, Workflow (Latest)

**May 2026** — Major expansion of data models and configuration system.

### New/Updated Backend Entities
| Entity | Changes |
|--------|---------|
| `Case` | +30 fields: actsAndSections, firNumber, policeStation, gdNumber, judgeName, bench, prosecutor, opposingLawyer, jurisdiction, appealStatus, riskLevel, complexityScore, practiceArea, department, internalNotes, retainerAmount, billingMethod, fixedFee, hourlyRate, budgetLimit, expenseBudget, nextHearingDate, criticalDeadlines, limitationExpiry, plus many-to-many linking to Clients (with roles) and LegalSections |
| `Client` | +25 fields: passportNumber, dateOfBirth, gender, occupation, nationality, tradeLicense, registrationNumber, taxVatNumber, authorizedRepresentative, tags, riskLevel, clientCategory, billingPreference, paymentTerms, creditLimit, preferredContactMethod, whatsAppNumber, secondaryPhone, emergencyContact |
| `ChamberSettings` | **New** — Key-value settings store (general, appearance, case defaults, document, billing, notification, workflow, security, integrations) per chamber |
| `WorkflowTemplate` | **New** — Configurable workflow templates with status transitions per entity type |
| `WorkflowTemplateSection` | **New** — Ordered status steps within a workflow template |
| `LegalSection` | **New** — Legal sections/acts reference table, linkable to Cases |

### New API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET/PUT | `/api/configuration` | Read/write chamber configuration settings |
| GET/PUT | `/api/configuration/{subsection}` | Read/write specific settings subsection |
| GET/POST | `/api/workflow-templates` | List/create workflow templates |
| GET/PUT/DELETE | `/api/workflow-templates/{id}` | Workflow template CRUD |
| GET/POST | `/api/legal-sections` | List/create legal sections |
| GET/PUT/DELETE | `/api/legal-sections/{id}` | Legal section CRUD |

### New Frontend Pages
| Route | Description |
|-------|-------------|
| `/lawyer/configuration` | 12-tab configuration page (General, Appearance, Case Defaults, Document, Billing, Notification, Workflow, Legal Sections, Users, Security, Integrations, Data) |
| Configuration/Workflow tab | Drag-and-drop workflow builder with status transition editor |

### Key UI Changes
- **CaseDialog** — Complete rewrite with progressive disclosure: 20+ fields across 3 groups (Basic → Legal → Financial)
- **ClientDialog** — Complete rewrite with progressive disclosure: 25+ fields across 3 groups (Basic → Legal → Financial)
- **Cases list page** — Uses new CaseDialog for creation
- **Clients list page** — Uses new ClientDialog for creation
- **Client detail page** — Passes all 25+ fields to edit dialog
- **Navbar** — Company name reads from dynamic chamber settings
- **Sidebar** — Configuration link added

### New Frontend Files
```
src/lib/services/
  configuration-service.ts    — Chamber configuration CRUD
  legal-section-service.ts    — Legal sections CRUD

src/lib/hooks/
  use-configuration.ts        — Chamber settings hooks
  use-workflow-templates.ts   — Workflow template hooks
  use-legal-sections.ts       — Legal section hooks

src/components/configuration/  — 12 tab components
  ConfigurationPage.tsx
  GeneralTab.tsx, AppearanceTab.tsx, CaseDefaultsTab.tsx, ...
  WorkflowTab.tsx             — Drag-and-drop builder
  LegalSectionsTab.tsx

src/components/cases/
  CaseDialog.tsx              — Advanced dialog with progressive disclosure

src/components/clients/
  ClientDialog.tsx            — Advanced dialog with progressive disclosure
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

## 14 Modules

| # | Module | Description |
|---|--------|-------------|
| 1 | Authentication & Chamber | Multi-chamber, role-based access (Owner/SeniorLawyer/JuniorLawyer/Assistant/Accountant/Client), permission system |
| 2 | Case Management | Case CRUD with auto-numbering (VER-YYYY-XXXX), search/sort/filter, timeline (CaseActivity), real-time updates via SignalR, hearing management, cause list tracking |
| 3 | Client Management | Profiles (name/nid/company), many-to-many client-case linking, portal account creation/revocation |
| 4 | Document Management | Upload (PDF/DOCX/Image), OCR search, version control, folder structure (Petition/Evidence/Order/Agreement) with client visibility controls |
| 5 | Legal Drafting | Template library, AI draft generator, smart variables ({{client_name}}, {{court_name}}, {{case_number}}) |
| 6 | AI Legal Assistant | Case summary, hearing prep, Bangla chatbot, voice-to-note |
| 7 | Calendar & Reminder | Smart calendar, multi-channel reminders (SMS/Push/WhatsApp/Email) |
| 8 | Billing & Finance | Invoice system (INV-YYYY-XXXX), expense tracking (court fees/stamp/transport), subscription billing |
| 9 | Internal Chamber | Task assignment (Senior→Junior), internal notes, attendance |
| 10 | Court & Legal Database | Laws (Penal Code/CPC/CrPC/Constitution), judgment search (citation/judge/keyword) |
| 11 | Analytics Dashboard | Active cases, win ratio, upcoming hearings, pending bills, lawyer productivity |
| 12 | Client Portal | Secure client login, case tracking with timeline, shared document center, lawyer messaging (chat UI), invoice viewing & payment, task management, real-time notifications |
| 13 | Chamber Configuration | ... |
| 14 | Legal Sections Database | ... |
| SA | Super Admin System | Centralized control: chamber management (upgrade/downgrade/clear/impersonate), user management (reset passwords/toggle status/override subscriptions), system-wide case view, audit logs, billing overview, system config, broadcast notifications, health monitoring |

## Project Structure

```
backend/
  Verdiq.Domain/          # 28 entities (added ChamberSettings, WorkflowTemplate, WorkflowTemplateSection, LegalSection), 13 enums, 5 interfaces
  Verdiq.Application/     # 22 DTO groups, 26 service interfaces, 8 validators
  Verdiq.Infrastructure/  # EF Core (32 DbSets), 23 services, audit interceptor
  Verdiq.API/             # 26 controllers, 3 middleware, 2 SignalR hubs
  tests/
    Verdiq.API.Tests/

frontend/
  src/
    app/                  # 36 pages (added /lawyer/configuration)
    components/           # 21 UI primitives + 18 feature components (added CaseDialog, ClientDialog, 12 config tabs)
    lib/
      services/           # 24 API service files (added configuration, legal-section services)
      hooks/              # 27 React Query hook files (added use-configuration, use-workflow-templates, use-legal-sections)
      store/              # Zustand auth store
      api.ts              # Axios with JWT refresh interceptor
    types/                # 35+ TypeScript interfaces
```

## Database Schema (32 Tables)

- `Chambers` — Multi-chamber support with subscription plan
- `Users` — 6 roles, linked to chamber, optional `ClientId` FK for portal users
- `Permissions`, `RolePermissions` — Fine-grained role-based access control
- `Cases` — Case management core
- `CaseActivities` — Case timeline with `IsClientVisible` flag
- `CauseLists` — Court cause list data
- `Clients` — Client profiles with NID, company, optional `UserId` FK for portal access
- `ClientCases` — Many-to-many client-case join
- `Hearings` — Court hearings with result/next-hearing-date
- `Documents`, `DocumentVersions`, `DocumentContents` — Document management + OCR; `Visibility` (InternalOnly/SharedWithClient) + `SharedWithClientId` FK
- `Messages` — Client-lawyer direct messaging with read status
- `Templates` — Legal drafting templates with smart variables
- `Invoices`, `Expenses`, `Payments` — Billing & finance
- `Subscriptions` — Chamber subscription plans
- `Tasks` — Internal task assignment
- `Reminders` — Multi-channel reminder engine
- `LegalDocuments` — Laws & judgment database
- `Notifications` — User notifications
- `AuditLogs` — Entity change audit trail
- `ChamberSettings` — Key-value settings per chamber (general, case defaults, billing, etc.)
- `WorkflowTemplates`, `WorkflowTemplateSections` — Configurable status transition workflows
- `LegalSections` — Legal acts/sections reference table
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
- Client portal at `/client/*` — separate route group with simplified navigation
- Portal accounts: lawyers create `User` (`Role=Client`) linked to existing `Client` record via `ClientId`
- Document visibility: `InternalOnly` (lawyer-only) or `SharedWithClient` (client-visible)
- Case activities: `IsClientVisible` flag controls client timeline access
- Client-lawyer messaging via `Messages` table with read receipts

## Troubleshooting

| Symptom | Fix |
|---------|-----|
| Login redirects back to `/login` | Create `.env.local` with `NEXT_PUBLIC_API_URL` |
| 401 on API calls | Check `access_token` in localStorage |
| CORS error | Verify API running on port 5000 |
| `npm run dev` fails | Run `$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"` first |
| API exits with `column does not exist` | Postgres filtered index uses `[ColumnName]` brackets — change to `"ColumnName"` in `HasFilter()` |
| API exits with `column "Name" does not exist` | Data migration references old column — wrap in `DO $$ ... END $$` with `IF EXISTS` checks |

## License

Proprietary — Verdiq
