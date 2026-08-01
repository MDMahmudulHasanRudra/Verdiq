# Remaining Tasks

## 0. Completed — Recent (August 2026)

- [x] **Fixed list-endpoint crash (Case / Hearing / Client / Expense pages)**
  - Root cause: backend list endpoints return raw `PagedResponse<T>`, but `apiGet()` unwrapped only the inner `data` array.
  - Fix: added `apiGetFull<T>()` in `frontend/src/lib/api.ts`; used by `caseService`, `clientService`, `hearingService`, `expenseService`, `legalDocumentService`.
- [x] **Added proper error handling pages** — `error.tsx` (retry + backend-unreachable hint + error details), `global-error.tsx`, `not-found.tsx`.
- [x] **Case module upgrade** — list/grid view toggle, edit + delete dialogs, inline quick status change, type/sort/status filters.
- [x] **Hearings module upgrade** — case dropdown selector (no more GUID paste), Upcoming/All tabs, edit (result/next date/status) + delete.
- [x] **Clients module upgrade** — backend `/api/clients` now supports `search`, `status`, `clientType` filters; edit (active toggle) + delete dialogs.
- [x] **Judgments module** — per-case judgment records (caption, result, judgment date, next hearing, key findings) with list/create/soft-delete, attach + download judgment documents, export history as PDF (hand-rolled writer) / Excel-compatible CSV.
- [x] **Case photos** — per-case photo upload (to cloud storage), thumbnail grid, lightbox, download, soft-delete.
- [x] **Delete case re-authentication** — `DELETE /api/cases/{id}` now requires the caller's `email` + `password` (BCrypt-verified) in the request body; confirm dialog on the cases page.
- [x] **Case Workflows / Processes** — user-created workflow presets (`/lawyer/workflows`, step builder with due-in-days, activate/deactivate) linkable to a case; steps unlock sequentially (locked until previous step completes) with due-date/overdue tracking, per-workflow progress, cancel/remove, case-detail Workflow tab + Overview progress snippet. Backend: `Workflow`/`WorkflowStep`/`CaseWorkflow`/`CaseWorkflowStep` entities, `WorkflowService`, `WorkflowsController` + `CaseWorkflowsController`.
- [ ] Backend compile-verify `dotnet build` on a machine with the .NET 10 SDK (client machine had no SDK), then add EF migration(s) for the new tables + `dotnet ef database update`:
  - `AddJudgmentsAndCasePhotos` — `Judgments` / `CasePhotos`
  - `AddCaseWorkflows` — `Workflows` / `WorkflowSteps` / `CaseWorkflows` / `CaseWorkflowSteps`
  - `AddConfirmCaseDelete` — no schema change (body-only API change; skip if already applied)
- [ ] Manual test the Workflow feature end-to-end (create preset → link to case → complete steps in sequence → confirm the next step unlocks → export/delete).


## 1.  Verify API (High Priority)
- [ ] Start PostgreSQL container: `docker compose up -d db`
- [ ] Start API server: `dotnet run --project Verdiq.API --urls http://localhost:5001`
- [ ] Verify Swagger loads: `http://localhost:5001/swagger`
- [ ] Test new API endpoints:
  - `/api/teams` — CRUD operations
  - `/api/accounting/charts` — Chart of Accounts CRUD
  - `/api/accounting/journals` — Journal entry CRUD
  - `/api/accounting/dashboard` — Dashboard stats
  - `/api/accounting/profit-loss` — P&L report
  - `/api/accounting/balance-sheet` — Balance sheet
  - `/api/accounting/reports/monthly` — Monthly reports
  - `/api/payroll/*` — Employee, Payroll, Attendance
  - `/api/banking/*` — Bank accounts, transactions
  - `/api/budget/*` — Budget CRUD
  - `/api/fixed-assets/*` — Fixed asset CRUD
  - `/api/tax/*` — Tax settings, transactions
  - `/api/audit/*` — Audit log queries

## 2. Test Frontend Routes (Medium Priority)
- [ ] Start frontend: `npm run dev` (from `frontend/` directory)
- [ ] Verify all new page routes compile and render:
  - `/lawyer/teams` — Team management grid
  - `/lawyer/accounting` — Dashboard with stats + trend
  - `/lawyer/accounting/charts` — Chart of Accounts tree
  - `/lawyer/accounting/journals` — Journal entry form
  - `/lawyer/accounting/profit-loss` — P&L report
  - `/lawyer/accounting/reports` — Monthly reports
  - `/lawyer/payroll` — Employees/Payrolls/Attendance tabs
  - `/lawyer/banking` — Account cards + transactions
  - `/lawyer/budget` — Budget cards + vs-actuals
  - `/lawyer/fixed-assets` — Asset register
  - `/lawyer/tax` — Settings + Transactions tabs
  - `/lawyer/audit` — Activity log with filters

## 3. Fix Warnings (Low Priority)
- [ ] Resolve EF Core query filter warnings:
  - Document ↔ DocumentActivity (required relationship with query filter)
  - Document ↔ DocumentFavorite
  - Document ↔ DocumentShare
  - Task ↔ TaskAttachment
  - Task ↔ TaskComment
  - Task ↔ TaskWatcher
  - Fix: Either make navigation optional or add matching filters to child entities

## 4. End-to-End Testing (Medium Priority)
- [ ] Login with seed credentials (admin@verdiq.com / lawyer@verdiq.com)
- [ ] Test Team CRUD flow
- [ ] Test Accounting journal posting (debit = credit validation)
- [ ] Test Payroll generate → approve → pay workflow
- [ ] Test Banking reconciliation flow
- [ ] Test Budget creation with vs-actual tracking
- [ ] Test Fixed Asset disposal
- [ ] Test Tax transaction recording

## 5. Production Readiness (Future)
- [ ] Review `DatabaseInitializer.cs` — should only seed data, not manage schema (migrations handle schema now)
- [ ] Remove `EnsureCreatedAsync()` call if no longer needed
- [ ] Remove raw SQL `CREATE TABLE IF NOT EXISTS` from `ApplySchemaUpdatesAsync()` for tables now managed by migrations
- [ ] Add proper error handling + logging
- [ ] Performance testing for accounting reports with large datasets

## Useful Commands

```powershell
# Start PostgreSQL
docker compose -f backend\docker-compose.yml up -d db

# Build backend
dotnet build backend\Verdiq.slnx

# Run API
dotnet run --project backend\Verdiq.API --urls http://localhost:5001

# Apply migrations
dotnet ef database update --project backend\Verdiq.Infrastructure --startup-project backend\Verdiq.API

# Run frontend
cd frontend && npm run dev

# Check database tables
docker exec verdiq-db psql -U postgres -d verdiq -c "\dt"
```
