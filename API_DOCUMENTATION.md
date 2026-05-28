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
| search | string | - | Search across case number, title, court, opponent, client name |
| status | string | - | Filter by status (Active, Pending, Closed, Appeal, Withdrawn) |
| priority | string | - | Filter by priority (Low, Medium, High, Urgent) |
| sortBy | string | createdAt | Sort field (caseNumber, title, status, priority, filingDate) |
| sortOrder | string | desc | asc or desc |

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
  "clientIds": ["guid1", "guid2"]
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
  "clientIds": ["guid1", "guid2"]
}
```

### DELETE /api/cases/{id}
Soft-delete a case. CaseActivity record created automatically. SignalR notification sent to case group.

---

## Clients

All endpoints require `[Authorize]`. Base: `/api/clients`

### GET /api/clients
List clients with pagination.

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 10 | Items per page |
| search | string | - | Search name, email, phone |
| status | string | - | active or inactive |
| sortBy | string | createdAt | Sort field |
| sortOrder | string | desc | asc or desc |

### GET /api/clients/{id}
Get client details with case count and total payments.

### POST /api/clients
Create a new client.

```json
{
  "fullName": "string",
  "email": "client@example.com",
  "phone": "+8801XXXXXXXXX",
  "address": "Dhaka, Bangladesh",
  "nationalId": "1234567890",
  "notes": "Referred by..."
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
