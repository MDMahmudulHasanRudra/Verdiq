# Architecture — 11 + Super Admin Module System

Verdiq follows **Clean Architecture** with 4 layers. Dependencies flow inward: API → Infrastructure → Application → Domain. All queries scoped by `ChamberId` for multi-chamber isolation. A **Super Admin** layer sits above all chambers for centralized system control.

```
┌──────────────────────────────────────────────┐
│           Super Admin System                 │
│  SuperAdminController · SuperAdminService    │
│  Hardcoded credentials: rudra / rudra        │
├──────────────────────────────────────────────┤
│               Verdiq.API                     │
│  21 Controllers · 3 Middleware · Program.cs  │
│  Depends on: Application, Infrastructure     │
├──────────────────────────────────────────────┤
│           Verdiq.Infrastructure              │
│  EF Core (27 DbSets) · 18 Services           │
│  Depends on: Application                     │
├──────────────────────────────────────────────┤
│            Verdiq.Application                │
│  19 DTO Groups · 20 Interfaces · 6 Validators│
│  Depends on: Domain                          │
├──────────────────────────────────────────────┤
│              Verdiq.Domain                   │
│  24 Entities · 13 Enums · 5 Interfaces       │
│  No external dependencies                    │
└──────────────────────────────────────────────┘
```

## Module 1 — Authentication & Chamber System

### Domain Entities
- `Chamber` — id, name, logo, address, phone, subscription_plan
- `User` — chamber_id (FK), name, email, phone, password_hash, role (Owner|SeniorLawyer|JuniorLawyer|Assistant|Accountant|Client), status
- `Permission` — id, name, description, module
- `RolePermission` — role, permission_id

### API Endpoints
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | No | Register with chamber |
| POST | `/api/auth/login` | No | Login (returns JWT + chamberId claim) |
| POST | `/api/auth/refresh` | No | Refresh tokens |
| POST | `/api/auth/logout` | Yes | Logout |
| GET | `/api/auth/me` | Yes | Current user with chamber |
| POST | `/api/chambers` | No | Create chamber (auto-create Owner user) |

### Permission Matrix (20 permissions across 7 modules)
- Cases: create, view, edit, delete
- Clients: create, view, edit, delete
- Documents: upload, view, delete
- Hearings: create, view, edit
- Billing: create invoices, view invoices
- Tasks: assign, view
- Reports/Settings: view reports, manage settings

### Chamber Scoping
Every entity includes `ChamberId`. JWT includes `ChamberId` claim accessible via `BaseController.GetChamberId()`. All services filter by `ChamberId`.

## Module 2 — Case Management

### Entities
- `Case` — title, case_number, court_name, case_type, filing_date, opponent, status (Active|Pending|Closed|Appeal|Withdrawn), priority, chamber_id, assigned_lawyer_id
- `CaseActivity` — case_id, activity_type (Hearing|Order|Note|Document|StatusChange|Task), description, created_by
- `Hearing` — case_id, hearing_date, courtroom, judge_name, result, next_hearing_date, status (Scheduled|Completed|Adjourned|Cancelled)
- `CauseList` — court_name, case_number, hearing_date, status

### API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET/POST | `/api/cases` | List (with search/sort/filter/pagination) / create cases |
| GET/PUT/DELETE | `/api/cases/{id}` | Case CRUD (CaseActivity created on mutations) |
| GET | `/api/cases/search?q=` | Search cases by title, case number, court, client name |
| GET | `/api/cases/{id}/activities` | Case timeline (auto-created on create/update/delete) |
| GET/POST | `/api/hearings` | Hearing list/create |
| GET/PUT/DELETE | `/api/hearings/{id}` | Hearing CRUD |
| GET | `/api/hearings/upcoming` | Upcoming hearings |
| GET | `/api/hearings/by-case/{caseId}` | Hearings for case |

**Real-time:** Case create/update/delete triggers SignalR notification (`CaseUpdated` event) to all group members.
**Activity tracking:** `CaseActivity` records (ActivityType: Hearing, Order, Note, Document, StatusChange, Task) are auto-created in the service.

## Module 3 — Client Management

### Entities
- `Client` — name, phone, email, address, nid, company_name, chamber_id
- `ClientCase` — client_id, case_id (many-to-many join)

### API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET/POST | `/api/clients` | List/create clients |
| GET/PUT/DELETE | `/api/clients/{id}` | Client CRUD |

## Module 4 — Document Management

### Entities
- `Document` — file_name, file_path, file_type, category, folder_path (Petition|Evidence|Order|Agreement), version, case_id, uploaded_by
- `DocumentVersion` — version_number, file_name, file_path, change_notes
- `DocumentContent` — document_id, extracted_text (OCR)

### API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/documents` | List documents |
| POST | `/api/documents/upload` | Upload file |
| GET | `/api/documents/{id}` | Document metadata |
| GET | `/api/documents/download/{id}` | Download file |
| DELETE | `/api/documents/{id}` | Soft-delete |

## Module 5 — Legal Drafting

### Entities
- `Template` — title, category (Bail Petition|Legal Notice|Affidavit|Agreement|Vakalatnama), content, variables (JSON)

### API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET/POST | `/api/templates` | List/create templates |
| GET | `/api/templates/{id}` | Get template |
| POST | `/api/templates/{id}/render` | Render with variables |

### Smart Variables
`{{client_name}}`, `{{court_name}}`, `{{case_number}}`, `{{opponent}}`, `{{filing_date}}`

## Module 6 — AI Legal Assistant

### Entities
- `AiConversation` — user_id, role, content, tokens_used

### API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/ai/chat` | AI chat (Bangla + English) |
| POST | `/api/ai/case-analysis` | Analyze case strengths |
| POST | `/api/ai/document-summary` | Summarize document |
| POST | `/api/ai/legal-notice` | Generate legal notice |
| POST | `/api/ai/judgement-search` | Search judgments |
| POST | `/api/ai/petition-generator` | Generate petition |

## Module 7 — Calendar & Reminder

### Entities
- `Reminder` — user_id, type, channel (SMS|PushNotification|WhatsApp|Email), scheduled_at, message, sent_status

### API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/reminders` | Create reminder |
| GET | `/api/reminders` | My reminders |

## Module 8 — Billing & Finance

### Entities
- `Invoice` — invoice_number (INV-YYYY-XXXX), amount, currency, status, client_id, case_id
- `Expense` — description, amount, category (Court Fees|Stamp Fees|Transport), chamber_id, case_id
- `Payment` — invoice_number, amount, method (Bkash|Nagad|Card|BankTransfer), gateway, client_id

### API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET/POST | `/api/invoices` | List/create invoices |
| POST | `/api/invoices/{id}/mark-paid` | Mark invoice paid |
| GET/POST | `/api/expenses` | List/create expenses |
| GET | `/api/expenses/total` | Total expenses |

## Module 9 — Internal Chamber Management

### Entities
- `Task` — title, description, due_date, status, assigned_to (Junior), assigned_by (Senior), case_id, chamber_id

### API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET/POST | `/api/tasks` | List/create tasks |
| GET | `/api/tasks/my` | My assigned tasks |

## Module 10 — Court & Legal Database

### Entities
- `LegalDocument` — title, category (PenalCode|CPC|CrPC|Constitution|Evidence|Judgment), content, citation, judge_name, keywords

### API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET/POST | `/api/legal-documents` | List/create legal docs |
| GET | `/api/legal-documents/search` | Search by citation/judge/keyword |

## Module 11 — Analytics Dashboard

### API Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/dashboard/stats` | Active/pending/closed cases, upcoming hearings |
| GET | `/api/dashboard/case-chart` | Monthly case status breakdown |
| GET | `/api/dashboard/recent-activities` | Recent activity feed |
| GET | `/api/dashboard/lawyer-productivity` | Cases per lawyer, task completion |
| GET | `/api/dashboard/win-ratio` | Win/loss ratio by lawyer |

## Super Admin System

A standalone control layer with hardcoded credentials (`rudra` / `rudra`) for system-wide administration.

### Super Admin Features
- **Chamber Management**: View all chambers, upgrade/downgrade subscription plans, clear/delete chambers, create new chambers
- **User Management**: View all users across all chambers, reset any user's password, toggle user active/inactive status, override user subscription plan/status/period
- **Case Management**: View all cases across all chambers with search and pagination
- **Subscription Management**: View all subscriptions with plan details, period tracking, and filtering
- **Permission Management**: Assign/remove permissions per role (module-grouped, select/deselect all)
- **Audit Logs**: Paginated system-wide activity log with search by action and entity
- **Billing Overview**: Revenue stats, payment breakdown (pending/completed/failed), recent payments table
- **System Config**: Toggle self-registration, maintenance mode, AI features, email verification, trial days
- **Broadcast Notifications**: Send system-wide or chamber-targeted notifications
- **Chamber Impersonation**: Generate a JWT token to log directly into any chamber's admin portal as the Owner (or specified user)
- **Health Monitoring**: Database status, system-wide statistics, active alerts with refresh
- **Charts**: Revenue chart (monthly), chamber growth chart

### API Endpoints (`/api/super-admin`)
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/super-admin/login` | Authenticate with userId + password |
| GET | `/api/super-admin/dashboard` | Aggregated system stats + all chambers |
| GET | `/api/super-admin/chambers` | List all chambers with stats |
| GET | `/api/super-admin/chambers/{id}` | Chamber details with revenue |
| POST | `/api/super-admin/chambers` | Create a new chamber |
| PUT | `/api/super-admin/chambers/{id}` | Update chamber details |
| PUT | `/api/super-admin/chambers/{id}/plan` | Upgrade/downgrade subscription plan |
| POST | `/api/super-admin/chambers/{id}/impersonate` | Generate impersonation token |
| DELETE | `/api/super-admin/chambers/{id}/clear` | Soft-delete all chamber data |
| DELETE | `/api/super-admin/chambers/{id}` | Delete chamber |
| GET | `/api/super-admin/users` | List all users (optional chamber filter) |
| POST | `/api/super-admin/users/{id}/reset-password` | Reset any user's password |
| POST | `/api/super-admin/users/{id}/toggle-status` | Activate/deactivate user |
| PUT | `/api/super-admin/users/{id}/subscription` | Override user subscription plan/status/period |
| GET | `/api/super-admin/subscriptions` | List all subscriptions |
| GET | `/api/super-admin/subscriptions/{chamberId}` | Chamber subscription details |
| GET | `/api/super-admin/permissions` | List all permissions |
| GET | `/api/super-admin/role-permissions` | Role-permission mappings |
| PUT | `/api/super-admin/role-permissions` | Assign permissions to role |
| DELETE | `/api/super-admin/role-permissions/{role}/{permissionId}` | Remove permission from role |
| GET | `/api/super-admin/audit-logs` | Paginated system activity log |
| GET | `/api/super-admin/billing` | Revenue stats + payment breakdown |
| POST | `/api/super-admin/broadcast` | Send system-wide notification |
| GET | `/api/super-admin/config` | Get system configuration |
| PUT | `/api/super-admin/config` | Update system configuration |
| GET | `/api/super-admin/cases` | View all cases across all chambers |
| GET | `/api/super-admin/health` | Database status + system stats + alerts |
| GET | `/api/super-admin/revenue-chart` | Monthly revenue chart data |
| GET | `/api/super-admin/chamber-growth` | Chamber growth chart data |

### Frontend Routes
| Route | Description |
|-------|-------------|
| `/super-admin/login` | Super Admin login page |
| `/super-admin/dashboard` | Dashboard with 12 stat cards + 8 quick actions + chamber table + alerts |
| `/super-admin/dashboard/chambers/{id}` | Chamber detail (inline edit, plan change, impersonate, clear/delete) |
| `/super-admin/dashboard/users` | User management (search, reset pwd, toggle status, subscription override modal) |
| `/super-admin/dashboard/subscriptions` | All subscriptions table with period tracking |
| `/super-admin/dashboard/permissions` | Role-based permission assignment (module-grouped with select/deselect all) |
| `/super-admin/dashboard/audit-logs` | Paginated system activity log with search |
| `/super-admin/dashboard/billing` | Revenue stats + payment breakdown + recent payments |
| `/super-admin/dashboard/cases` | All cases across chambers with search and pagination |
| `/super-admin/dashboard/settings` | System config toggles + broadcast notification sender |
| `/super-admin/dashboard/health` | System health monitoring with refresh |

### Security
- Credentials hardcoded & BCrypt-hashed
- JWT with role `SuperAdmin` and `isSuperAdmin=true` claim
- All endpoints require `[Authorize(Roles = "SuperAdmin")]`
- Impersonation tokens are time-limited (2 hours)
- Impersonation marks tokens with `impersonatedBy=SuperAdmin` and `isImpersonated=true`

## Infrastructure Layer

### AppDbContext (27 DbSets)
Chambers, Users, Permissions, RolePermissions, Cases, CaseActivities, CauseLists, Clients, ClientCases, Hearings, Documents, DocumentVersions, DocumentContents, Templates, Invoices, Expenses, Payments, Subscriptions, Reminders, Tasks, LegalDocuments, Notifications, AuditLogs, AiConversations

### Services (18 total)
| Service | Key Methods |
|---------|------------|
| AuthService | Register/Login/Refresh/Logout with BCrypt + JWT |
| JwtService | Token generation with ChamberId claim |
| ChamberService | Chamber CRUD with owner assignment |
| CaseService | CRUD + search/sort/filter + auto case number generation + client linking + CaseActivity creation |
| ClientService | CRUD + search by name/phone/company |
| HearingService | CRUD + upcoming/by-date/by-case queries |
| DocumentService | Upload/download + version control |
| InvoiceService | Auto invoice numbers + mark-as-paid |
| ExpenseService | Category-based expense tracking |
| TaskService | Assignment-based task management |
| TemplateService | Template CRUD + variable rendering |
| ReminderService | Multi-channel reminder creation |
| LegalDocumentService | Laws + judgments with search |
| PermissionService | Role-permission matrix |
| SuperAdminService | System-wide chamber/user/case/subscription/permission/audit/billing/config/health management |
| DashboardService | Aggregated stats + charts |
| AIService | OpenAI integration with Bangla legal fallback |
| RealtimeNotifier | SignalR push for case updates, notifications, activities |

## API Layer (21 Controllers)

| Controller | Route Prefix | Key Endpoints |
|-----------|-------------|---------------|
| AuthController | `/api/auth` | login, register, refresh, logout, me |
| ChambersController | `/api/chambers` | CRUD + my chamber |
| CasesController | `/api/cases` | CRUD + search + paginated list with sort/filter + realtime notifications via SignalR |
| ClientsController | `/api/clients` | CRUD + search |
| HearingsController | `/api/hearings` | CRUD + upcoming + by-date |
| DocumentsController | `/api/documents` | upload, download, list |
| InvoicesController | `/api/invoices` | CRUD + mark-paid |
| ExpensesController | `/api/expenses` | CRUD + total |
| TasksController | `/api/tasks` | CRUD + my tasks |
| TemplatesController | `/api/templates` | CRUD + render |
| RemindersController | `/api/reminders` | CRUD |
| LegalDocumentsController | `/api/legal-documents` | CRUD + search |
| CauseListsController | `/api/cause-lists` | CRUD |
| PermissionsController | `/api/permissions` | role-permissions |
| NotificationsController | `/api/notifications` | CRUD + unread-count |
| SubscriptionController | `/api/subscription` | plan, change, cancel |
| DashboardController | `/api/dashboard` | stats, charts, productivity |
| AdminController | `/api/admin` | users, cases, revenue, system |
| AIController | `/api/ai` | chat, analysis, drafting |
| SearchController | `/api/search` | global search |
| SuperAdminController | `/api/super-admin` | 28 endpoints: system control, chambers CRUD, users, subscriptions, permissions, audit logs, billing, broadcast, config, cases, health, revenue/growth charts |

## Frontend Architecture

### Pages (30 routes)
```
/ → redirect to /login
/login → login form
/lawyer → dashboard with stats/charts
/lawyer/cases → case list + create dialog with search/sort/filter
/lawyer/cases/[id] → case detail with timeline, hearings, documents
/lawyer/clients → client list + create
/lawyer/clients/[id] → client detail
/lawyer/hearings → hearing calendar + list
/lawyer/documents → document grid + upload
/lawyer/tasks → task assignment board
/lawyer/templates → template library
/lawyer/legal-database → law/judgment search
/lawyer/invoices → invoice management
/lawyer/expenses → expense tracking
/lawyer/billing → subscription + billing
/lawyer/ai-assistant → AI chat interface
/lawyer/notifications → notification list
/lawyer/settings → profile settings
/admin → admin panel (Owner only)

/super-admin/login → Super Admin login
/super-admin/dashboard → Dashboard with 12 stats + 8 quick actions + chamber table
/super-admin/dashboard/chambers/[id] → Chamber detail with inline edit, plan, impersonate
/super-admin/dashboard/users → User management with subscription override modal
/super-admin/dashboard/cases → All cases across chambers (search, pagination)
/super-admin/dashboard/subscriptions → All subscriptions table
/super-admin/dashboard/permissions → Role-based permission assignment
/super-admin/dashboard/audit-logs → Paginated activity log
/super-admin/dashboard/billing → Revenue stats + payments
/super-admin/dashboard/settings → Config toggles + broadcast sender
/super-admin/dashboard/health → System health monitoring
```

### Services (19 files)
Auth, Chamber, Case, Client, Hearing, Document, Invoice, Expense, Task, Template, Reminder, LegalDocument, Notification, Subscription, Payment, Admin, AI, Search, SuperAdmin

### Hooks (22 files)
All services have corresponding React Query hooks with cache invalidation. Key patterns: `useQuery` for fetches, `useMutation` for creates/updates, `queryClient.invalidateQueries` on success. Super admin hooks include: `useSuperAdminDashboard`, `useSuperAdminChambers`, `useSuperAdminChamber`, `useSuperAdminUsers`, `useSuperAdminCases`, `useSubscriptions`, `usePermissions`, `useRolePermissions`, `useAuditLogs`, `useBillingOverview`, `useSystemConfig`, `useSystemHealth`, `useRevenueChart`, `useChamberGrowth`, and mutations for user/subscription/permission changes.

### State Management
- **Server state:** TanStack React Query (20 hook files)
- **Client state:** Zustand (`auth-store.ts` — user, tokens, chamber info)
- **HTTP client:** Axios with JWT refresh interceptor (queues concurrent 401s)

### Auth Flow
```
Login → /api/auth/login → stores tokens in localStorage + cookies
→ JWT includes ChamberId claim → all requests scoped to chamber
→ middleware.ts checks access_token cookie for route protection
→ Axios interceptor reads localStorage, auto-refreshes on 401
```

## Key Design Decisions

1. **Multi-Chamber Architecture** — All entities scoped by `ChamberId`; JWT includes chamber claim
2. **Role-Based Access** — 6 roles with 20 granular permissions
3. **Guid Ids** — All entities use `Guid` primary keys
4. **Soft Delete** — Global `IsDeleted` query filter on all 27 entities
5. **Pagination** — `PagedResponse<T>` with page metadata on all list endpoints
6. **Many-to-Many Clients** — `ClientCases` join table (a case can have multiple clients)
7. **Document Versioning** — `DocumentVersion` tracks edit history; `DocumentContent` stores OCR text
8. **Auto Numbering** — Cases: `VER-YYYY-XXXX`, Invoices: `INV-YYYY-XXXX`
9. **Template Rendering** — `{{variable}}` replacement with `RenderTemplateAsync`
10. **Field Mapping** — Frontend services map API field names (e.g., `fullName` → `name`)
11. **Dual Token Storage** — localStorage for axios + cookies for middleware
12. **CORS** — `SetIsOriginAllowed(_ => true)` with credentials
13. **Rate Limiting** — 100 req/min per IP, exempt for `/hubs` and `/health`
14. **Case Search/Sort** — `GET /api/cases` accepts `search`, `sortBy`, `sortOrder` params; supports multi-column sort (caseNumber, title, status, priority, filingDate)
15. **Case Activity Tracking** — `CaseActivity` records auto-created on case create/update/delete for full timeline
16. **Real-Time Case Updates** — SignalR `CaseUpdated` event broadcast to case group on every mutation
17. **Super Admin Case View** — `GET /api/super-admin/cases` returns all cases across chambers with full details
