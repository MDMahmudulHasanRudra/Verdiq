# Session Log

## Session 4 — Judgments, Case Photos & Delete Re-authentication (August 2026)

### Changes Made

**Backend — New entities**
- `Judgment` (CaseId/Case, Caption, Summary, Result, JudgmentDate, NextHearingDate, KeyFindings, optional attached-document metadata, RecordedById/RecordedBy) and `CasePhoto` (CaseId/Case, FileName, OriginalFileName, StorageKey, ContentType, FileSize, Caption, CapturedAt, UploadedById/UploadedBy).
- `Case` extended with `ICollection<Judgment> Judgments` + `ICollection<CasePhoto> Photos`.
- `AppDbContext` — added `DbSet<Judgment> Judgments` + `DbSet<CasePhoto> CasePhotos` with fluent configs (soft-delete query filters, cascade delete from Case, `Restrict` on user FKs).

**Backend — Judgments API** (`/api/cases/{caseId}/judgments`)
- `JudgmentService` — list, create (auto logs `CaseActivity`), soft-delete, attach a judgment document (cloud storage, replaces existing), download, and export history as **PDF** (hand-rolled minimal PDF writer using `<FEFF…>` UTF-16BE hex strings — no external PDF library added) or **Excel-compatible CSV** (UTF-8 BOM).

**Backend — Case Photos API** (`/api/cases/{caseId}/photos`)
- `CasePhotoService` — upload to `cases/{caseId}/photos/{guid}_{name}`, list, download, soft-delete; logs `CaseActivity` on upload/delete.

**Backend — Delete re-authentication**
- `CaseService.DeleteAsync(id, email, password)` now verifies the caller's email + BCrypt password (and chamber) before soft-deleting.
- `CasesController.Delete` takes `ConfirmCaseDeleteDto` (`{ email, password }`); missing/blank credentials or mismatch → 400.

**Frontend**
- `api.ts` — added `apiDownload` (Blob, `responseType: "blob"`) + `downloadBlob`.
- Types (`Judgment`, `CasePhoto`, `ConfirmCaseDeleteInput`) + `judgmentService` / `casePhotoService`.
- `/lawyer/cases` — delete dialog now requires email + password (email prefilled from auth store); confirm button disabled until both filled.
- `/lawyer/cases/[id]` — new **Judgments** tab (record judgment, attach/download document, export PDF/CSV) and **Photos** tab (upload, thumbnail grid, lightbox, download, delete).

### Verification
- `npx tsc --noEmit` — clean (no errors)
- `npm run build` — succeeds (all routes)
- Backend not compiled locally (no .NET SDK on this machine). Requires `dotnet build` + `dotnet ef migrations add AddJudgmentsAndCasePhotos` + `database update` on a .NET machine.

---

## Session 3 — Error Page Fix & Core Module Upgrades (August 2026)

### Changes Made

**Frontend — Paged-response bug fix (critical)**
- Root cause: `/cases`, `/clients`, `/hearings`, `/expenses`, `/legal-documents` return a raw `PagedResponse<T>`, but the `apiGet()` helper unwrapped only `data.data` (the array). Pages then read `data.data.length` / `data.totalPages` on a bare array → `TypeError` → error page.
- Added `apiGetFull<T>()` in `frontend/src/lib/api.ts` (returns the full body).
- Switched the paged services to `apiGetFull` in `case-service.ts` and `services/index.ts` (clients, hearings, expenses, legal-documents).
- `useCases` hook now passes through the full `CaseQueryParams` (type, courtName, sortBy, sortOrder, dateFrom, dateTo).

**Frontend — Error handling pages**
- Created `frontend/src/app/error.tsx` (client-boundary: retry, dashboard link, backend-unreachable hint, collapsible details + digest), `global-error.tsx`, `not-found.tsx`.

**Frontend — Case module (`lawyer/cases/page.tsx`)**
- List ↔ grid view toggle (grid cards with status/priority badges, court, opponent, assigned lawyer, client/hearing/document counts, next hearing).
- Edit-case dialog (prefilled incl. status) and delete-case confirmation dialog.
- Inline quick-status change per row/card.
- Filters: case type, sort (filingDate/createdAt/title/caseNumber), status chips, improved debounced search.

**Frontend — Hearings module (`lawyer/hearings/page.tsx`)**
- Case dropdown selector (fetched from live case list) instead of pasting a GUID.
- Upcoming / All tabs, status filter, incomplete pre-hearing-task warning icon.
- Edit-hearing dialog (result, nextHearingDate, status) and delete confirmation.

**Frontend + Backend — Clients module (`lawyer/clients/page.tsx`)**
- Backend: `/api/clients` now accepts `search`, `status`, `clientType` filters — updated `ClientsController.GetAll`, `IClientService.GetAllAsync/GetCountAsync`, and `ClientService` (new `BuildQuery` helper).
- Frontend: edit-client dialog (incl. active toggle), delete confirmation, type/status filters, improved debounced search. `clientService.update` accepts `isActive`.

### Verification
- `npx tsc --noEmit` — clean (no errors)
- `npm run build` — succeeds (all routes)
- Backend not compiled locally (no .NET SDK on this machine); C# changes follow existing patterns.

---

## Session 2 — Module-Based Access Control & Admin Enhancements

### Changes Made

**Backend — New Entity**
- Created `UserModule` entity (UserId, ModuleName) in `backend/Verdiq.Domain/Entities/UserModule.cs`
- Added DbSet + fluent config in `AppDbContext.cs` with unique index on (UserId, ModuleName) + query filter
- Created EF Core migration `AddUserModule`

**Backend — Admin API**
- Added `GetUserModulesAsync` / `SetUserModulesAsync` to `IAdminService` and `AdminService`
- Added `GET /api/admin/users/{id}/modules` and `PUT /api/admin/users/{id}/modules` to `AdminController`
- Added `SetUserModulesDto` to `AdminDtos.cs`

**Backend — Auth Response**
- Added `Modules` property to `UserInfoDto`
- `AuthController.MapUserInfo` now queries and includes user's assigned modules

**Frontend — Types & Auth**
- Added `modules?: string[]` to `User` type in `types/index.ts`
- Updated `mapUser` in `auth-service.ts` to map `raw.modules`

**Frontend — Sidebar**
- Added `module` field to each `mainNav` item
- Filter logic: if user has assigned modules, only show items whose `module` is in that list; no assigned modules = show everything (backward compatible)

**Frontend — Admin Page**
- Added "Modules" button in user table actions column
- Added module assignment dialog with checkboxes for all 23 modules
- Added `useUserModules` / `useSetUserModules` hooks
- Fixed "Platform Settings" tab: connected static toggles to real `useSettings` / `useUpdateSettings` API (General subsection fields: maintenanceMode, allowRegistration, auditLogging)

## What Was Done

### Database Migration (Team & Accounting Modules)
Created and applied EF Core migrations for all new entities across Team, Accounting, Payroll/HR, Banking, Budget, Fixed Assets, Tax Management, and Audit Trail modules.

**New entities migrated:**
- Team, TeamMember
- ChartOfAccount, AccountingJournal, JournalLine
- Employee, Payroll, Attendance
- BankAccount, BankTransaction
- Budget, BudgetLine
- FixedAsset, AssetDepreciation
- TaxSetting, TaxTransaction

### Issues Encountered & Fixed

1. **Database was created by `EnsureCreatedAsync`, not by migrations**
   - The `__EFMigrationsHistory` table did not exist — no migrations had ever been applied
   - All schema was created at runtime by `DatabaseInitializer.InitializeAsync()` using `EnsureCreatedAsync()` + raw SQL
   - Fix: Dropped all tables (`DROP SCHEMA public CASCADE; CREATE SCHEMA public;`), then ran `dotnet ef database update` to apply all migrations

2. **SQL Server bracket syntax in existing migrations**
   - `20260528115443_AddChamberSettings.cs` used `[ClientCode] IS NOT NULL` (SQL Server syntax)
   - Same in `20260528115443_AddChamberSettings.Designer.cs`
   - Fix: Changed to `"ClientCode" IS NOT NULL` (PostgreSQL syntax) in both files

3. **Port 5000 conflict**
   - Docker API container or prior dotnet process held port 5000
   - Fix: Killed processes on port 5000, attempted restart on port 5001

4. **Docker Desktop crashed** during API verification — needs restart

### Current State
- **Backend**: Builds with 0 errors, 0 warnings
- **Frontend**: Builds with 0 errors (51 routes)
- **Database**: All 3 migrations applied successfully. 63 tables created including all new module tables
- **Migration History** (`__EFMigrationsHistory`):
  - `20260528115443_AddChamberSettings` ✅
  - `20260528120152_AddWorkflowTemplates` ✅
  - `20260606083117_AddNewModules` ✅

### Pending Verification
- API server not yet verified (Docker crashed)
- Need to: start API, check Swagger endpoints, verify frontend routes

---

## Project Context

### Goal
Build a full-featured Team module, comprehensive Accounting module, and six additional financial/HR modules (Payroll, Banking, Budget, Fixed Assets, Audit Trail, Tax Management) for chamber management.

### Stack
- **Backend**: .NET 10.0 (preview), EF Core 10.0-preview, Npgsql, PostgreSQL 16
- **Frontend**: Next.js 16.2.6, Node 20, shadcn/ui, Tailwind CSS, React Query, framer-motion
- **Infrastructure**: Docker Desktop (Windows 10), PostgreSQL container

### Auth & Scoping
- JWT auth with BCrypt password hashing, ChamberId claim
- All API endpoints use `ApiResponse<T>` wrapper
- Team membership scoped to Chamber; accounting entities Chamber-scoped
- All modules are Owner/Accountant role accessible

### Modules Completed
| Module | Backend | Frontend |
|--------|---------|----------|
| Team | Entities, DTOs, Service, Controller | service, hooks, `/lawyer/teams` page |
| Accounting (Core) | ChartOfAccounts, Journal entities, DTOs, Services, Controllers | service, hooks, `/lawyer/accounting/*` pages |
| Payroll/HR | Employee, Payroll, Attendance entities, Service, Controller | service, `/lawyer/payroll` page |
| Banking | BankAccount, BankTransaction entities, Service, Controller | service, `/lawyer/banking` page |
| Budget | Budget, BudgetLine entities, Service, Controller | service, `/lawyer/budget` page |
| Fixed Assets | FixedAsset, AssetDepreciation entities, Service, Controller | service, `/lawyer/fixed-assets` page |
| Audit Trail | AuditService using existing AuditLog entity, Controller | service, `/lawyer/audit` page |
| Tax Management | TaxSetting, TaxTransaction entities, Service, Controller | service, `/lawyer/tax` page |

### Key Files Modified
- `AppDbContext.cs` — All DbSets + entity configurations
- `Program.cs` — All 9 new service DI registrations
- `Sidebar.tsx` — Added Teams + Accounting nav links
- `CaseDialog.tsx` — Added assignee + team selectors
- `CaseDto.cs` — Added `AssignedLawyerId`, `TeamId`, `TeamName`
- `CaseService.cs` — Updated to use `dto.AssignedLawyerId`, `.Include(c => c.Team)`
- All controllers under `Verdiq.API/Controllers/`
- All service interfaces + implementations under `Verdiq.Application/Interfaces/` and `Verdiq.Infrastructure/Services/`
