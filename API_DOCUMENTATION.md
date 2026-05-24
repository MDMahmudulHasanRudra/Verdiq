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
List cases with pagination and filtering.

**Query Parameters:**
| Param | Type | Default | Description |
|-------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 10 | Items per page |
| search | string | - | Search title, case number, client name |
| status | string | - | Filter by status (Active, Pending, Closed, Appeal) |
| type | string | - | Filter by case type |
| priority | string | - | Filter by priority (Low, Medium, High) |
| sortBy | string | createdAt | Sort field |
| sortOrder | string | desc | asc or desc |

### GET /api/cases/{id}
Get a single case by ID.

### POST /api/cases
Create a new case.

**Request:**
```json
{
  "title": "string",
  "caseType": "Criminal",
  "court": "Dhaka District Court",
  "courtRoom": "Room 101",
  "judgeName": "Judge Name",
  "firNumber": "FIR-2024-001",
  "policeStation": "Gulshan",
  "actsAndSections": "Section 302/34 IPC",
  "description": "Details about the case",
  "priority": "High",
  "clientId": "guid"
}
```

### PUT /api/cases/{id}
Update an existing case. All fields optional.

**Request:**
```json
{
  "title": "Updated Title",
  "caseType": "Civil",
  "status": "Active",
  "priority": "Medium",
  "court": "Updated Court",
  "courtRoom": "Room 202",
  "judgeName": "New Judge",
  "firNumber": "FIR-2024-002",
  "policeStation": "Banani",
  "actsAndSections": "Section 420 IPC",
  "description": "Updated description"
}
```

### DELETE /api/cases/{id}
Soft-delete a case.

### GET /api/cases/search?q=keyword
Search cases by keyword across title, case number, and client name.

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

## Health Check

### GET /health
Returns `Healthy` (text/plain). No auth required.

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
