# Remaining Tasks

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
