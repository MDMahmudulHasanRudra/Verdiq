# API Documentation

**Base URL:** `http://localhost:5000/api`

All endpoints return JSON. Authenticated endpoints require `Authorization: Bearer <token>` header. The frontend stores this token in both localStorage and cookies after login.

---

## Response Format

### Success
```json
{
  "success": true,
  "message": "Success",
  "data": { ... }
}
```

### Error
```json
{
  "success": false,
  "message": "Error description",
  "errors": ["field-specific error"]
}
```

### Paginated
```json
{
  "success": true,
  "message": "Success",
  "data": [ ... ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## Authentication

### POST /api/auth/register
Create a new user account.

**Request:**
```json
{
  "fullName": "string",
  "email": "user@example.com",
  "password": "string",
  "confirmPassword": "string",
  "phone": "+8801XXXXXXXXX",
  "role": "Lawyer",
  "barCouncilId": "BC-2024-001",
  "chamberAddress": "42 Gulshan Avenue, Dhaka"
}
```

**Response (201):**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "id": "guid",
    "fullName": "string",
    "email": "string",
    "phone": "string",
    "role": "string",
    "avatarUrl": null,
    "barCouncilId": "string",
    "isActive": true
  }
}
```

### POST /api/auth/login
Authenticate and receive JWT tokens.

**Request:**
```json
{
  "email": "admin@verdiq.com",
  "password": "admin123"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Login successful",
  "accessToken": "eyJ...",
  "refreshToken": "base64...",
  "user": {
    "id": "guid",
    "fullName": "string",
    "email": "string",
    "phone": "string",
    "role": "Admin",
    "avatarUrl": null,
    "barCouncilId": null,
    "isActive": true
  }
}
```

### POST /api/auth/refresh
Exchange an expired access token for a new pair.

**Request:**
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "base64..."
}
```

### POST /api/auth/logout
Invalidate the current refresh token. Requires auth.

---

## Cases

All endpoints require `[Authorize]`. Base: `/api/cases`

### GET /api/cases
List cases with pagination, search, sort, and filtering.

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 10 | Items per page |
| search | string | - | Search across case number, title, court, opponent, client name, FIR number |
| status | string | - | Filter by status (Active, Pending, Closed, Appeal, Withdrawn) |
| priority | string | - | Filter by priority (Low, Medium, High, Urgent) |
| sortBy | string | createdAt | Sort field (caseNumber, title, status, priority, filingDate) |
| sortOrder | string | desc | asc or desc |
| assignedLawyerId | guid | - | Filter by assigned lawyer |
| practiceArea | string | - | Filter by practice area |
| clientId | guid | - | Filter by linked client ID |

### GET /api/cases/search?q=keyword
Search cases by keyword across case number, title, court name, opponent, FIR number, and client name.

### GET /api/cases/{id}
Get a single case by ID (includes assigned lawyer, clients, hearings count, documents count).

### POST /api/cases
Create a new case. Case number (VER-YYYY-XXXX) auto-generated. CaseActivity record created automatically.

**Request:**
```json
{
  "title": "State vs. Md. Karim",
  "caseType": "Criminal",
  "courtName": "Dhaka District Court",
  "filingDate": "2026-05-27T00:00:00Z",
  "opponent": "State of Bangladesh",
  "priority": "Medium",
  "description": "Details about the case",
  "actsAndSections": "Section 302/34 IPC",
  "firNumber": "FIR-2024-001",
  "policeStation": "Gulshan",
  "gdNumber": "GD-2024-042",
  "judgeName": "Md. Rahman",
  "bench": "Bench 2",
  "prosecutor": "Mr. Kamal",
  "opposingLawyer": "Mr. Hasan",
  "jurisdiction": "Dhaka",
  "appealStatus": "First Appeal",
  "riskLevel": "Medium",
  "complexityScore": 7,
  "practiceArea": "Criminal",
  "department": "Litigation",
  "internalNotes": "Sensitive case",
  "retainerAmount": 50000,
  "billingMethod": "Fixed",
  "fixedFee": 100000,
  "hourlyRate": 3000,
  "budgetLimit": 200000,
  "expenseBudget": 50000,
  "nextHearingDate": "2026-06-15T10:00:00Z",
  "criticalDeadlines": "2026-07-01 - Evidence submission",
  "limitationExpiry": "2026-12-31",
  "clientIds": ["guid1", "guid2"],
  "clientRoles": [
    { "clientId": "guid1", "role": "Plaintiff" },
    { "clientId": "guid2", "role": "Witness" }
  ],
  "legalSectionIds": ["guid3", "guid4"]
}
```

### PUT /api/cases/{id}
Update an existing case. All fields optional. CaseActivity record created automatically.

**Request:**
```json
{
  "title": "Updated Title",
  "caseType": "Civil",
  "courtName": "Updated Court",
  "status": "Active",
  "priority": "High",
  "opponent": "Updated Opponent",
  "description": "Updated description",
  "actsAndSections": "Section 420 IPC",
  "firNumber": "FIR-2024-002",
  "policeStation": "Banani",
  "judgeName": "Updated Judge",
  "bench": "Bench 1",
  "prosecutor": "Updated Prosecutor",
  "opposingLawyer": "Updated Opposing Counsel",
  "jurisdiction": "Dhaka",
  "appealStatus": "Second Appeal",
  "riskLevel": "High",
  "complexityScore": 8,
  "practiceArea": "Criminal",
  "department": "Litigation",
  "internalNotes": "Updated notes",
  "retainerAmount": 75000,
  "billingMethod": "Hourly",
  "fixedFee": 0,
  "hourlyRate": 5000,
  "budgetLimit": 300000,
  "expenseBudget": 75000,
  "nextHearingDate": "2026-07-10T10:00:00Z",
  "criticalDeadlines": "2026-08-01 - Filing deadline",
  "limitationExpiry": "2027-01-15",
  "clientIds": ["guid1", "guid2"],
  "clientRoles": [
    { "clientId": "guid1", "role": "Plaintiff" }
  ],
  "legalSectionIds": ["guid3"]
}
```

### DELETE /api/cases/{id}
Soft-delete a case. Requires **re-authentication**: the body must contain the signed-in user's `email` and `password` (BCrypt-verified). Missing/blank fields or a password mismatch → 400.

**Request:**
```json
{
  "email": "admin@verdiq.com",
  "password": "admin123"
}
```

CaseActivity record created automatically. SignalR notification sent to case group.

---

## Judgments

All endpoints require `[Authorize]`. Base: `/api/cases/{caseId}/judgments`

### GET /api/cases/{caseId}/judgments
List judgment records for a case (newest judgment date first).

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "caseId": "guid",
      "caption": "Final Judgment",
      "summary": "Court ruled in favour of the plaintiff...",
      "result": "Decree in favour of plaintiff",
      "judgmentDate": "2026-07-10T00:00:00Z",
      "nextHearingDate": null,
      "keyFindings": "Compensation awarded; costs on defendant",
      "fileName": null,
      "originalFileName": null,
      "fileType": null,
      "fileSize": null,
      "hasDocument": false,
      "recordedByName": "Jane Smith",
      "createdAt": "2026-07-11T09:00:00Z"
    }
  ]
}
```

### POST /api/cases/{caseId}/judgments
Record a judgment for a case. Logs a `CaseActivity`. `caption` is required; `judgmentDate` defaults to now.

**Request:**
```json
{
  "caption": "Final Judgment",
  "summary": "Court ruled in favour of the plaintiff...",
  "result": "Decree in favour of plaintiff",
  "judgmentDate": "2026-07-10T00:00:00Z",
  "nextHearingDate": null,
  "keyFindings": "Compensation awarded; costs on defendant"
}
```

### POST /api/cases/{caseId}/judgments/{judgmentId}/upload-document
Attach a judgment document (`multipart/form-data`, `file` field). Replaces any existing attachment (old file is removed from storage).

| Constraint | Value |
|------------|-------|
| Max size | 50 MB |

**Response:** Updated `JudgmentDto` with `hasDocument: true`.

### GET /api/cases/{caseId}/judgments/{judgmentId}/download-document
Download the attached judgment document. Returns 404 if no document is attached.

### GET /api/cases/{caseId}/judgments/export?format=pdf|csv
Export the case's judgment history.
- `format=pdf` (default) — minimal hand-built PDF (UTF-16BE text, no external PDF library), filename `judgments-{caseNumber}-{yyyyMMdd}.pdf`, `application/pdf`.
- `format=csv` — Excel-compatible CSV with UTF-8 BOM, filename `judgments-{caseNumber}-{yyyyMMdd}.csv`, `text/csv`.

### DELETE /api/cases/{caseId}/judgments/{judgmentId}
Soft-delete a judgment record (also removes its attached file from storage).

### DTO — JudgmentDto
| Field | Type | Description |
|-------|------|-------------|
| id | Guid | Judgment ID |
| caseId | Guid | Associated case ID |
| caption | string | Judgment title |
| summary | string? | Summary text |
| result | string? | Outcome |
| judgmentDate | DateTime | Judgment date |
| nextHearingDate | DateTime? | Next hearing date |
| keyFindings | string? | Key findings |
| fileName | string? | Stored attachment file name |
| originalFileName | string? | Original attachment name |
| fileType | string? | Attachment MIME type |
| fileSize | long? | Attachment size in bytes |
| hasDocument | bool | Whether a document is attached |
| recordedByName | string? | Recording user's full name |
| createdAt | DateTime | Creation timestamp |

---

## Case Photos

All endpoints require `[Authorize]`. Base: `/api/cases/{caseId}/photos`

### GET /api/cases/{caseId}/photos
List photos for a case (newest captured date first).

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "caseId": "guid",
      "fileName": "a1b2c3d4e5f6_scene.jpg",
      "originalFileName": "scene.jpg",
      "contentType": "image/jpeg",
      "fileSize": 2458000,
      "caption": "Exhibit A — crime scene",
      "capturedAt": "2026-06-01T10:05:00Z",
      "uploadedByName": "Jane Smith",
      "createdAt": "2026-06-01T10:05:00Z"
    }
  ]
}
```

### POST /api/cases/{caseId}/photos/upload
Upload a photo (`multipart/form-data`). Logs a `CaseActivity`.

**Form Data:**
| Field | Type | Required |
|-------|------|----------|
| file | file | Yes |
| caption | string | No |

| Constraint | Value |
|------------|-------|
| Max size | 20 MB |

### GET /api/cases/{caseId}/photos/{photoId}/download
Download the photo file.

### DELETE /api/cases/{caseId}/photos/{photoId}
Soft-delete a photo (also removes its file from storage).

### DTO — CasePhotoDto
| Field | Type | Description |
|-------|------|-------------|
| id | Guid | Photo ID |
| caseId | Guid | Associated case ID |
| fileName | string | Stored file name |
| originalFileName | string | Original upload name |
| contentType | string | MIME type |
| fileSize | long | File size in bytes |
| caption | string? | Caption |
| capturedAt | DateTime | Photo captured/uploaded timestamp |
| uploadedByName | string? | Uploading user's full name |
| createdAt | DateTime | Upload timestamp |

---

## Clients

All endpoints require `[Authorize]`. Base: `/api/clients`

### GET /api/clients
List clients with pagination, search, and filters.

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 10 | Items per page |
| search | string | - | Search name, email, phone, NID, company, client code |
| status | string | - | `active` or `inactive` |
| clientType | string | - | Filter by client type (Individual, Corporate, Government, NGO) |
| sortBy | string | createdAt | Sort field |
| sortOrder | string | desc | asc or desc |

### GET /api/clients/{id}
Get client details with case count and total payments.

### POST /api/clients
Create a new client.

```json
{
  "name": "Client Name",
  "email": "client@example.com",
  "phone": "+8801XXXXXXXXX",
  "address": "Dhaka, Bangladesh",
  "nid": "1234567890",
  "companyName": "ABC Corp",
  "notes": "Referred by...",
  "clientType": "Individual",
  "passportNumber": "AB123456",
  "dateOfBirth": "1990-01-15",
  "gender": "Male",
  "occupation": "Businessman",
  "nationality": "Bangladeshi",
  "tradeLicense": "TL-2024-001",
  "registrationNumber": "REG-2024-001",
  "taxVatNumber": "TAX-123456",
  "authorizedRepresentative": "Md. Ali",
  "tags": "VIP, Urgent",
  "riskLevel": "Medium",
  "clientCategory": "Individual",
  "billingPreference": "Monthly",
  "paymentTerms": "Net 30",
  "creditLimit": 100000,
  "preferredContactMethod": "Phone",
  "whatsAppNumber": "+8801XXXXXXXXX",
  "secondaryPhone": "+8801XXXXXXXXY",
  "emergencyContact": "Wife - +8801XXXXXXXXZ",
  "avatarUrl": null
}
```

### PUT /api/clients/{id}
Update client. All fields optional.

### DELETE /api/clients/{id}
Soft-delete a client.

### GET /api/clients/search?q=keyword
Search clients.

### POST /api/clients/{id}/portal-access
Create portal user account linked to this client. Creates a `User` with `Role=Client` and links it to the client record.

```json
{
  "email": "client@example.com",
  "password": "TempPass123!",
  "fullName": "Client Name"
}
```

### POST /api/clients/{id}/revoke-portal
Revoke portal access for this client. Soft-deletes the linked portal user.

### POST /api/clients/{id}/avatar
Upload a profile photo for the client.

**Request:** `multipart/form-data` with a single `file` field.

| Constraint | Value |
|------------|-------|
| Max size | 5 MB |
| Allowed types | JPEG, PNG, GIF, WebP |

**Response:** Returns the updated `ClientResponseDto` with `avatarUrl` populated.

### DTO — ClientResponseDto
| Field | Type | Description |
|-------|------|-------------|
| id | Guid | Client ID |
| name | string | Full name |
| phone | string | Phone number |
| email | string | Email address |
| address | string? | Physical address |
| nid | string? | National ID |
| companyName | string? | Company name |
| notes | string? | Notes |
| isActive | bool | Active status |
| casesCount | int | Number of linked cases |
| createdAt | DateTime | Creation timestamp |
| clientType | string? | Client type |
| clientCode | string? | Auto-generated client code |
| avatarUrl | string? | Profile photo URL |
| passportNumber | string? | Passport number |
| dateOfBirth | DateTime? | Date of birth |
| gender | string? | Gender |
| occupation | string? | Occupation |
| nationality | string? | Nationality |
| tradeLicense | string? | Trade license number |
| registrationNumber | string? | Business registration |
| taxVatNumber | string? | Tax/VAT number |
| authorizedRepresentative | string? | Authorized representative |
| tags | string? | Tags |
| riskLevel | string? | Low, Medium, High |
| clientCategory | string? | Client category |
| billingPreference | string? | Billing preference |
| paymentTerms | string? | Payment terms |
| creditLimit | decimal? | Credit limit |
| preferredContactMethod | string? | Preferred contact method |
| whatsAppNumber | string? | WhatsApp number |
| secondaryPhone | string? | Secondary phone |
| emergencyContact | string? | Emergency contact |
| isBlacklisted | bool | Blacklist status |

---

## Hearings

All endpoints require `[Authorize]`. Base: `/api/hearings`

### GET /api/hearings
List hearings with pagination and filtering.

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 10 | Items per page |
| search | string | - | Search case title, court |
| status | string | - | Scheduled, Completed, Adjourned, Cancelled |
| type | string | - | Hearing type |
| dateFrom | date | - | Filter start date (ISO 8601) |
| dateTo | date | - | Filter end date (ISO 8601) |
| sortBy | string | hearingDate | Sort field |
| sortOrder | string | asc | asc or desc |

### GET /api/hearings/upcoming
Get upcoming hearings for the current user.

### GET /api/hearings/by-date?date=2024-06-15
Get hearings for a specific date.

### GET /api/hearings/by-case/{caseId}
Get all hearings for a specific case.

### GET /api/hearings/{id}
Get a single hearing.

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "caseId": "guid",
    "caseNumber": "VER-2026-0012",
    "caseTitle": "State vs. Defendant",
    "hearingDate": "2024-06-15T10:00:00Z",
    "courtroom": "Room 302",
    "judgeName": "Hon. Judge",
    "result": null,
    "nextHearingDate": null,
    "status": "Scheduled",
    "notes": "Bring all documents",
    "createdAt": "2024-06-01T08:00:00Z",
    "hasIncompletePreHearingTasks": false,
    "hasPreHearingTasks": false
  }
}
```

### POST /api/hearings
Create a hearing.

```json
{
  "caseId": "guid",
  "hearingDate": "2024-06-15T10:00:00Z",
  "time": "10:00 AM",
  "court": "Dhaka District Court",
  "courtRoom": "Room 302",
  "judgeName": "Hon. Judge",
  "hearingType": "Initial Hearing",
  "notes": "Bring all documents"
}
```

### PUT /api/hearings/{id}
Update a hearing. All fields optional.

### DELETE /api/hearings/{id}
Soft-delete a hearing.

### POST /api/hearings/{id}/send-reminder
Send a reminder notification for a hearing.

### DTO — HearingResponseDto
| Field | Type | Description |
|-------|------|-------------|
| id | Guid | Hearing ID |
| caseId | Guid | Associated case ID |
| caseNumber | string | Case number |
| caseTitle | string | Case title |
| hearingDate | DateTime | Scheduled hearing date |
| courtroom | string? | Courtroom location |
| judgeName | string? | Presiding judge |
| result | string? | Hearing outcome |
| nextHearingDate | DateTime? | Next hearing date |
| status | string | Scheduled, Completed, Adjourned, Cancelled |
| notes | string? | Hearing notes |
| createdAt | DateTime | Creation timestamp |
| hasIncompletePreHearingTasks | bool | Whether any pre-hearing tasks are incomplete |
| hasPreHearingTasks | bool | Whether any pre-hearing tasks exist |

---

## Tasks

All endpoints require `[Authorize]`. Base: `/api/tasks`

### POST /api/tasks
Create a new task.

**Request Body:**
```json
{
  "title": "Prepare case brief",
  "description": "Review all documents and prepare summary",
  "dueDate": "2024-06-20T17:00:00Z",
  "priority": "High",
  "assignedTo": "guid",
  "caseId": "guid",
  "hearingId": "guid",
  "isPreHearing": true,
  "sortOrder": 0,
  "isRecurring": false,
  "recurrencePattern": null,
  "recurrenceInterval": null,
  "estimatedHours": 2.5,
  "watcherIds": ["guid"]
}
```

### GET /api/tasks
List tasks with filtering.

**Query Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| status | string | Pending, InProgress, Completed, Cancelled |
| priority | string | Low, Medium, High, Urgent |
| assignedTo | Guid | Filter by assignee |

### GET /api/tasks/my
Get tasks assigned to the current user.

### GET /api/tasks/by-case/{caseId}
Get all tasks for a specific case.

### GET /api/tasks/by-hearing/{hearingId}
Get all tasks for a specific hearing (e.g. pre-hearing tasks).

### GET /api/tasks/{id}
Get a single task by ID.

### PUT /api/tasks/{id}
Update a task. All fields optional.

### DELETE /api/tasks/{id}
Delete a task.

### GET /api/tasks/overdue
Get overdue tasks for the current chamber.

### POST /api/tasks/reorder
Reorder tasks (drag-and-drop).

**Request Body:**
```json
{
  "tasks": [
    { "id": "guid", "sortOrder": 1, "status": "Pending" }
  ]
}
```

### POST /api/tasks/{id}/comments
Add a comment to a task.

### GET /api/tasks/{id}/comments
Get comments for a task.

### POST /api/tasks/{id}/watchers
Toggle watcher status for the current user.

### POST /api/tasks/{id}/start-timer
Start time tracking on a task.

### POST /api/tasks/{id}/stop-timer
Stop time tracking. Send `{ "minutes": 30 }` in request body.

### DTO — TaskResponseDto
| Field | Type | Description |
|-------|------|-------------|
| id | Guid | Task ID |
| title | string | Task title |
| description | string | Task description |
| dueDate | DateTime | Due date |
| status | string | Pending, InProgress, Completed, Cancelled |
| priority | string? | Low, Medium, High, Urgent |
| assignedTo | Guid | Assignee user ID |
| assignedToName | string | Assignee display name |
| assignedByName | string | Assigner display name |
| caseId | Guid? | Associated case ID |
| caseTitle | string? | Case title |
| hearingId | Guid? | Associated hearing ID (for pre-hearing tasks) |
| isPreHearing | bool | Whether this is a pre-hearing task |
| createdAt | DateTime | Creation timestamp |
| sortOrder | int | Display order |
| isRecurring | bool | Recurring task flag |
| completedAt | DateTime? | Completion timestamp |
| estimatedHours | double? | Estimated effort |
| actualHours | double? | Logged hours |
| commentCount | int | Number of comments |
| attachmentCount | int | Number of attachments |

---

## Documents

All endpoints require `[Authorize]`. Base: `/api/documents`

### GET /api/documents
List documents with pagination.

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 10 | Items per page |
| search | string | - | Search file name |
| category | string | - | Filter by category |
| status | string | - | Draft, Final, Filed |
| caseId | guid | - | Filter by case |
| sortBy | string | createdAt | Sort field |
| sortOrder | string | desc | asc or desc |

### GET /api/documents/by-case/{caseId}
Get all documents for a case.

### GET /api/documents/{id}
Get document metadata.

### POST /api/documents/upload
Upload a document (multipart/form-data).

**Form Data:**
| Field | Type | Required |
|-------|------|----------|
| file | file | Yes |
| caseId | guid | Yes |
| documentType | string | Yes |
| category | string | No |

### GET /api/documents/download/{id}
Download the document file.

### DELETE /api/documents/{id}
Soft-delete a document.

---

## Notifications

All endpoints require `[Authorize]`. Base: `/api/notifications`

### GET /api/notifications?unreadOnly=false
List user notifications.

### GET /api/notifications/unread-count
Get count of unread notifications.

```json
{
  "success": true,
  "message": "Success",
  "data": { "count": 5 }
}
```

### POST /api/notifications
Create a notification (admin).

```json
{
  "userId": "guid",
  "title": "Case Update",
  "message": "Your case has been updated",
  "type": "case_update",
  "referenceId": "case-guid"
}
```

### PUT /api/notifications/{id}/read
Mark a notification as read.

### PUT /api/notifications/read-all
Mark all user notifications as read.

### DELETE /api/notifications/{id}
Delete a notification.

---

## Subscription

All endpoints require `[Authorize]`. Base: `/api/subscription`

### GET /api/subscription/my
Get the current user's subscription.

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": "guid",
    "userId": "guid",
    "plan": "Chamber",
    "status": "Active",
    "currentPeriodStart": "2024-01-01T00:00:00Z",
    "currentPeriodEnd": "2025-01-01T00:00:00Z",
    "cancelAtPeriodEnd": false
  }
}
```

### PUT /api/subscription/change-plan
Change subscription plan.

```json
{
  "plan": "Pro"
}
```

### POST /api/subscription/cancel
Cancel subscription (effective at period end).

### GET /api/subscription
List all subscriptions. Requires `[Authorize(Roles = "Admin")]`.

---

## Dashboard

### GET /api/dashboard/stats
Get dashboard statistics. Requires `[Authorize]`.

```json
{
  "success": true,
  "message": "Success",
  "data": {
    "stats": {
      "totalCases": 25,
      "activeCases": 12,
      "pendingCases": 8,
      "closedCases": 5,
      "hearingsToday": 3,
      "upcomingHearings": 7,
      "totalClients": 42,
      "unreadNotifications": 2
    },
    "recentCases": [ ... ],
    "upcomingHearings": [ ... ]
  }
}
```

---

## Client Portal

All endpoints require `[Authorize(Roles = "Client")]`. Base: `/api/client-portal`

### GET /api/client-portal/dashboard
Get client dashboard with aggregated stats, recent cases, upcoming hearings, and invoices.

```json
{
  "success": true,
  "data": {
    "activeCases": 3,
    "upcomingHearings": 2,
    "pendingInvoices": 5,
    "sharedDocuments": 12,
    "recentCases": [
      { "id": "guid", "caseNumber": "VER-2026-0012", "caseType": "Civil", "status": "Active", "opponentName": "John Doe", "assignedLawyer": "Jane Smith", "nextHearingDate": "2026-06-15T10:00:00", "lawyerName": "Jane Smith", "hearingCount": 4, "lastActivity": "2026-05-20T14:30:00" }
    ],
    "upcomingHearings": [
      { "id": "guid", "caseNumber": "VER-2026-0012", "caseType": "Civil", "hearingDate": "2026-06-15T10:00:00", "courtName": "Dhaka District Court", "courtRoom": "Room 301", "judgeName": "Judge Rahman", "status": "Scheduled" }
    ],
    "recentInvoices": [
      { "id": "guid", "invoiceNumber": "INV-2026-0042", "amount": 25000, "status": "Pending", "dueDate": "2026-06-30", "description": "Consultation fees" }
    ]
  }
}
```

### GET /api/client-portal/profile
Get client profile with chamber info.

### GET /api/client-portal/cases
List all cases linked to the authenticated client.

### GET /api/client-portal/cases/{id}
Get case detail with client-visible timeline.

```json
{
  "success": true,
  "data": {
    "id": "guid",
    "caseNumber": "VER-2026-0012",
    "caseType": "Civil",
    "status": "Active",
    "filingDate": "2026-01-15",
    "courtName": "Dhaka District Court",
    "opponentName": "John Doe",
    "assignedLawyer": "Jane Smith",
    "lawyerEmail": "jane@lawfirm.com",
    "lawyerPhone": "+8801XXXXXXXXX",
    "hearingCount": 4,
    "documentCount": 8,
    "timeline": [
      { "type": "hearing", "title": "Hearing - Civil", "description": "Argument hearing", "date": "2026-05-20T10:00:00", "actor": "Jane Smith" },
      { "type": "activity", "title": "Document filed", "description": "Evidence exhibit A submitted", "date": "2026-05-18T14:30:00", "actor": "Jane Smith" }
    ]
  }
}
```

### GET /api/client-portal/hearings
List upcoming hearings for the client.

### GET /api/client-portal/documents
List shared documents (Visibility = SharedWithClient or SharedWithClientId matches client).

```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "fileName": "Case_Summary.pdf",
      "fileType": "pdf",
      "fileSize": 245000,
      "category": "Petition",
      "uploadedAt": "2026-05-10T09:00:00",
      "uploaderName": "Jane Smith"
    }
  ]
}
```

### GET /api/client-portal/documents/{id}
Get single shared document with download URL.

### GET /api/client-portal/invoices
List client invoices.

```json
{
  "success": true,
  "data": {
    "totalInvoiceCount": 8,
    "totalOutstanding": 75000,
    "totalOverdue": 25000,
    "totalPaid": 120000,
    "invoices": [
      { "id": "guid", "invoiceNumber": "INV-2026-0042", "amount": 25000, "status": "Pending", "dueDate": "2026-06-30", "description": "Consultation fees", "isOverdue": false }
    ]
  }
}
```

### GET /api/client-portal/tasks
List tasks assigned to the client.

### POST /api/client-portal/messages
Send a message. Accessible to Client role.

```json
{
  "receiverId": "guid",
  "caseId": "guid (optional)",
  "content": "I have a question about my case",
  "attachmentUrl": "https://... (optional)"
}
```

### GET /api/client-portal/messages
Get client message history.

### GET /api/client-portal/messages/unread-count
Get unread message count.

```json
{
  "success": true,
  "data": 3
}
```

### POST /api/client-portal/messages/{id}/read
Mark a message as read.

---

## Messages

All endpoints require `[Authorize]`. Base: `/api/messages`

### GET /api/messages/conversation/{userId}
Get conversation between current user and specified user. Optional query param `?caseId=guid`.

### GET /api/messages/client/{clientId}
Get conversation with a client (for lawyer users).

### POST /api/messages
Send a message. Body:
```json
{
  "receiverId": "guid",
  "caseId": "guid (optional)",
  "content": "Message text",
  "attachmentUrl": "https://... (optional)"
}
```

### POST /api/messages/{id}/read
Mark message as read.

### GET /api/messages/unread-count
Get unread message count.

```json
{
  "success": true,
  "data": 5
}
```

---

## Health Check

### GET /health
Returns `Healthy` (text/plain). No auth required.

---

## Super Admin

All Super Admin endpoints require `[Authorize(Roles = "SuperAdmin")]`. Base: `/api/super-admin`

### POST /api/super-admin/login
Authenticate with hardcoded Super Admin credentials.

**Request:**
```json
{
  "userId": "rudra",
  "password": "rudra"
}
```

### GET /api/super-admin/dashboard
Get aggregated system stats (12 stats: chambers, users, cases, clients, revenue, subscriptions, documents, hearings, payments, new this month) + all chambers + system alerts.

### GET /api/super-admin/cases
View all cases across all chambers (no chamber scoping).

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "caseNumber": "VER-2026-0001",
      "title": "State vs. Md. Karim",
      "caseType": "Criminal",
      "status": "Active",
      "courtName": "Dhaka District Court",
      "assignedLawyerName": "Lawyer Name",
      "filingDate": "2026-05-01T00:00:00Z",
      "createdAt": "2026-05-01T10:00:00Z"
    }
  ]
}
```

### GET /api/super-admin/users
List all users with optional chamber filter and subscription details.

### GET /api/super-admin/subscriptions
List all subscriptions with plan, status, period dates.

### GET /api/super-admin/permissions
List all available permissions (module-grouped).

### GET /api/super-admin/audit-logs?page=1&pageSize=50
Paginated system activity log.

### GET /api/super-admin/billing
Revenue overview with payment breakdown and recent payments.

### GET /api/super-admin/config
Get system configuration (self-registration, maintenance mode, AI features, etc.).

### GET /api/super-admin/health
Database status + system-wide statistics + active alerts.

---

## Chamber Configuration

All endpoints require `[Authorize]`. Base: `/api/configuration`

### GET /api/configuration
Get all chamber settings with defaults.

**Response:**
```json
{
  "success": true,
  "message": "Settings retrieved",
  "data": {
    "id": "guid",
    "chamberId": "guid",
    "settings": {
      "general": {
        "companyName": "Verdiq Law Chamber",
        "companyNameBn": "",
        "logoUrl": "",
        "address": "",
        "phone": "",
        "email": "",
        "timezone": "Asia/Dhaka",
        "dateFormat": "DD-MM-YYYY",
        "currency": "BDT",
        "language": "en"
      },
      "caseDefaults": {
        "caseNumberPrefix": "VER",
        "caseNumberFormat": "{PREFIX}-{YYYY}-{XXXX}",
        "caseTypes": ["Criminal","Civil","Family","Corporate","Tax","Labor","Property"],
        "priorityLevels": ["Low","Medium","High","Urgent"],
        "statuses": ["Active","Pending","Closed","Appeal","Withdrawn"],
        "courtPresets": ["Dhaka District Court","High Court Division","Supreme Court"]
      },
      "clientManagement": {
        "clientTypes": ["Individual","Company","NGO","Government"],
        "enablePortalAccess": true,
        "portalRegistrationApproval": false,
        "defaultDocumentSharing": false
      },
      "billing": {
        "taxRatePercent": 15,
        "invoicePrefix": "INV",
        "paymentMethods": ["Bkash","Nagad","Card","Bank Transfer","Cash"],
        "expenseCategories": ["Court Fees","Stamp Fees","Transport","Stationery","Admin","Other"]
      },
      "documentManagement": {
        "categories": ["Pleading","Evidence","Correspondence","Court Order","Contract","Other"],
        "maxFileSizeMb": 25,
        "allowedMimeTypes": ["application/pdf","image/jpeg","image/png","application/msword"],
        "enableOcr": false,
        "storageProvider": "local"
      },
      "hearingsReminders": {
        "hearingTypes": ["Appearance","Argument","Order","Judgment"],
        "reminderOffsetsDays": [1,3,7],
        "enableEmailReminders": true,
        "enableSmsReminders": false,
        "enableWhatsAppReminders": false,
        "defaultReminderChannel": "email"
      },
      "legalDrafting": {
        "templateCategories": ["Petition","Affidavit","Contract","Notice","Deed"],
        "enableSmartVariables": true
      },
      "notifications": {
        "enableEmailNotifications": true,
        "enablePushNotifications": true,
        "smtpConfigured": false,
        "smsConfigured": false,
        "whatsappConfigured": false
      },
      "aiAssistant": {
        "enabled": true,
        "apiKeyConfigured": false,
        "model": "gpt-4o-mini"
      },
      "securitySession": {
        "enableMfa": false,
        "sessionTimeoutMinutes": 60,
        "maxLoginAttempts": 5,
        "lockoutDurationMinutes": 15
      },
      "dashboardUi": {
        "companyName": "Verdiq Law Chamber",
        "showWelcomeWidget": true,
        "showCaseStats": true,
        "showHearingWidget": true,
        "defaultWidgets": ["caseStats","upcomingHearings","recentActivities","invoiceSummary"]
      }
    },
    "updatedAt": "2026-05-28T13:07:45.8430271Z"
  }
}
```

### PUT /api/configuration
Update all chamber settings at once. Supports partial updates — only non-null sections are merged.

**Request Body** (`UpdateChamberSettingsDto`):
```json
{
  "general": { "companyName": "Updated Chamber Name", "timezone": "Asia/Dhaka" },
  "caseDefaults": { "caseNumberPrefix": "VER" },
  "clientManagement": { "enablePortalAccess": true },
  "billing": { "taxRatePercent": 10 },
  "documentManagement": { "maxFileSizeMb": 50 },
  "hearingsReminders": { "enableEmailReminders": true },
  "legalDrafting": { "enableSmartVariables": true },
  "notifications": { "enablePushNotifications": false },
  "aiAssistant": { "enabled": true },
  "securitySession": { "sessionTimeoutMinutes": 120 },
  "dashboardUi": { "showWelcomeWidget": false }
}
```

**Response:**
```json
{
  "success": true,
  "message": "Settings updated",
  "data": { "...": "..." }
}
```

### GET /api/configuration/{subsection}
Get a single subsection (general, caseDefaults, clientManagement, billing, documentManagement, hearingsReminders, legalDrafting, notifications, aiAssistant, securitySession, dashboardUi).

### PUT /api/configuration/{subsection}
Update a single subsection. Body is a flat JSON object of the subsection fields to update.

---

## Workflow Templates

All endpoints require `[Authorize]`. Base: `/api/workflow-templates`

### GET /api/workflow-templates
List all workflow templates for the chamber.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "name": "Criminal Case Workflow",
      "description": "Standard criminal case workflow",
      "isDefault": true,
      "sections": [
        { "id": "guid", "legalSectionId": "guid", "sectionCode": "302", "sectionTitle": "Punishment for Murder", "lawName": "Penal Code, 1860", "displayOrder": 1 },
        { "id": "guid", "legalSectionId": "guid", "sectionCode": "CrPC-161", "sectionTitle": "Examination of Witnesses", "lawName": "CrPC", "displayOrder": 2 }
      ],
      "createdAt": "2026-05-28T00:00:00Z"
    }
  ]
}
```

### POST /api/workflow-templates
Create a workflow template linked to legal sections.

**Request:**
```json
{
  "name": "Criminal Case Workflow",
  "description": "Standard criminal case workflow",
  "isDefault": false,
  "legalSectionIds": ["guid1", "guid2", "guid3"]
}
```

### PUT /api/workflow-templates/{id}
Update a workflow template.

```json
{
  "name": "Updated Workflow",
  "description": "Updated description",
  "isDefault": true,
  "legalSectionIds": ["guid1", "guid2"]
}
```

### DELETE /api/workflow-templates/{id}
Delete a workflow template (cascades to sections).

---

## Legal Sections

All endpoints require `[Authorize]`. Base: `/api/legal-sections`

### GET /api/legal-sections
List all legal sections for the chamber.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "sectionCode": "302",
      "sectionTitle": "Punishment for Murder",
      "lawName": "Penal Code, 1860",
      "country": "Bangladesh",
      "category": "PenalCode",
      "description": "Whoever commits murder shall be punished with death...",
      "severity": "High",
      "isActive": true,
      "procedureCount": 3,
      "createdAt": "2026-05-28T00:00:00Z"
    }
  ]
}
```

### POST /api/legal-sections
Create a legal section.

```json
{
  "sectionCode": "420",
  "sectionTitle": "Cheating",
  "lawName": "Penal Code, 1860",
  "country": "Bangladesh",
  "category": "PenalCode",
  "description": "Whoever cheats and thereby dishonestly induces...",
  "severity": "Medium"
}
```

### PUT /api/legal-sections/{id}
Update a legal section.

### DELETE /api/legal-sections/{id}
Soft-delete a legal section.

---

## Error Codes

| Status | Meaning |
|--------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized (missing/invalid token) |
| 403 | Forbidden (insufficient role) |
| 404 | Not Found |
| 429 | Rate Limit Exceeded (100 req/min) |
| 500 | Internal Server Error |

## Rate Limiting

- 100 requests per minute per IP address
- Exceeded requests return HTTP 429 with `Retry-After` header
