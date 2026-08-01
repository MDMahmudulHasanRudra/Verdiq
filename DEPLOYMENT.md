# Deployment

## What this deploys

Verdiq ships as a **single Docker image/container** that runs all three
processes (PostgreSQL 16 + .NET API + Next.js) via supervisor. Everything
you need is in the root `docker-compose.yml` and `Dockerfile`.

| Process  | Port  | Notes                                        |
|----------|-------|----------------------------------------------|
| Web      | 3000  | Next.js standalone build                     |
| API      | 5000  | ASP.NET Core, `/health` healthcheck          |
| Database | 5432  | PostgreSQL, listens on loopback **inside** the container only (not published) |

Data persists in two Docker volumes:
- `pgdata` → `/var/lib/verdiq/pgdata` (PostgreSQL + generated JWT key)
- `uploads` → `/data/uploads` (uploaded documents/photos)

---

## Local Development

### Prerequisites
- Docker Desktop (the build uses the `dotnet/sdk:10.0` and `node:22` images, so a local .NET/Node SDK is **not** required)

### Build + start

```bash
docker compose up -d --build
```

First boot initializes PostgreSQL, applies all EF migrations
(`MigrateAsync()` at startup), and seeds the default chamber + users.
Wait ~30-60s for the healthcheck to pass.

- Web: http://localhost:3000
- API: http://localhost:5000  (`/health` returns `Healthy`)
- Swagger: only in Development

### Seed logins

| Role  | Email              | Password  |
|-------|--------------------|-----------|
| Admin | `admin@verdiq.com` | `admin123`|
| Lawyer| `lawyer@verdiq.com`| `lawyer123`|

### Useful commands

```bash
docker compose ps                 # status (health)
docker compose logs -f            # follow all logs
docker compose down               # stop (keeps data)
docker compose down -v            # stop + wipe volumes (fresh reset)
docker compose restart            # restart (data + JWT key preserved)
```

---

## Deploy to a VPS

### What you need to do (that's it)

1. Copy the repo to the server.
2. (Recommended) set a database password:
   ```bash
   echo "POSTGRES_PASSWORD=change-me" > .env
   ```
3. Start it:
   ```bash
   docker compose up -d --build
   ```
4. Open `http://<SERVER_IP>:3000` and log in.

That's the whole flow — **no other configuration required**:

- The frontend derives the API URL at runtime from the page's hostname
  (`<same-host>:5000/api`), so the browser talks to the right API without a
  build-time `NEXT_PUBLIC_API_URL`. To override it, set `NEXT_PUBLIC_API_URL`
  as a build arg (see below).
- If no `JWT_KEY` is provided, a **unique key is generated on first boot and
  persisted** in the `pgdata` volume, so sessions survive restarts.
- Migrations + seed data run automatically on first boot.

### Firewall

Open only what you need on the VPS:

- `3000/tcp` — web UI (or put it behind a reverse proxy / HTTPS)
- `5000/tcp` — API (reachable by the browser; required by the default API-URL derivation)

PostgreSQL (`5432`) is **not** exposed — it only listens on loopback inside the container.

### Options (all optional)

Create a `.env` next to `docker-compose.yml`:

```bash
# Strong DB password (default: postgres)
POSTGRES_PASSWORD=change-me

# Fixed JWT signing key (32+ chars). If omitted, one is auto-generated + persisted.
JWT_KEY=your-256bit-secret

# Override baked-in frontend build values (defaults: Verdiq, localhost)
NEXT_PUBLIC_APP_NAME=Verdiq
NEXT_PUBLIC_API_URL=https://api.example.com/api
```

If you need a baked-in `NEXT_PUBLIC_API_URL` (e.g. API behind a separate
domain), build with:

```bash
docker compose build --build-arg NEXT_PUBLIC_API_URL=https://api.example.com/api
```

### HTTPS / reverse proxy

Put the container behind nginx/Caddy on the VPS (recommended for production):

```nginx
# nginx.conf (example)
server {
    listen 80;
    server_name verdiq.example.com;
    return 301 https://$server_name$request_uri;
}
server {
    listen 443 ssl http2;
    server_name verdiq.example.com;
    ssl_certificate     /etc/letsencrypt/live/verdiq.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/verdiq.example.com/privkey.pem;

    location /api/ {
        proxy_pass http://127.0.0.1:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
    location /uploads/ {
        proxy_pass http://127.0.0.1:5000;
    }
    location /health {
        proxy_pass http://127.0.0.1:5000;
    }
    location / {
        proxy_pass http://127.0.0.1:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

If you front the API on the same origin through nginx, set
`NEXT_PUBLIC_API_URL=https://verdiq.example.com/api` as a build arg so the
frontend uses HTTPS instead of the raw `:5000` port.

---

## Database Migrations

The project uses **EF Core migrations**, applied automatically at startup
(`DatabaseInitializer` → `db.Database.MigrateAsync()`). A fresh volume runs
every migration in order from `__EFMigrationsHistory`.

Current migration (workflow/process tables):
`20260801140144_AddCaseWorkflows` — `Judgments`, `CasePhotos`, `Workflows`,
`WorkflowSteps`, `CaseWorkflows`, `CaseWorkflowSteps`.

To create a new migration (requires a .NET 10 SDK, or run inside the SDK
image):

```bash
dotnet ef migrations add MyChange \
  --project backend/Verdiq.Infrastructure \
  --startup-project backend/Verdiq.API
```

Migrations apply automatically on next container start — no manual
`dotnet ef database update` needed.

### Reset a broken database

```bash
docker compose down -v && docker compose up -d --build
```

---

## Environment Variables

### Backend

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | set by entrypoint (loopback, `verdiq` DB) |
| `Jwt__Key` / `JWT_KEY` | JWT signing key | auto-generated + persisted |
| `Jwt__Issuer` | JWT issuer | `Verdiq` |
| `Jwt__Audience` | JWT audience | `VerdiqApp` |
| `DocumentStorage__Path` | File upload directory | `/data/uploads` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |

### Frontend (baked at build time)

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_URL` | Backend API URL | runtime-derived from hostname (`:5000/api`) |
| `NEXT_PUBLIC_APP_NAME` | Application name | `Verdiq` |
| `NEXT_PUBLIC_APP_URL` | Frontend URL | `http://localhost:3000` |

---

## CORS

The API uses `SetIsOriginAllowed(_ => true)` so any origin can call it with
credentials. This keeps the runtime hostname-derived API URL working on any
IP/domain. For stricter production hardening, replace it with explicit
origins in `backend/Verdiq.API/Program.cs`:

```csharp
policy.WithOrigins("https://app.verdiq.com")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials();
```

---

## Auth Flow

The frontend uses **dual token storage**:

1. **localStorage** — read by the axios interceptor to attach `Authorization: Bearer`.
2. **Cookies** — read by the Next.js proxy/middleware to protect routes (redirect to `/login` when absent).

On login: `/api/auth/login` → store `access_token` + `refresh_token` in
localStorage and cookies → redirect to `/lawyer`. Logout clears both and
redirects to `/login`.

---

## Security Checklist

- [x] JWT key auto-generated + persisted per deployment (no shared default)
- [x] PostgreSQL not exposed to the host network
- [ ] Set `POSTGRES_PASSWORD` on the server (`.env`)
- [ ] Enable HTTPS (Let's Encrypt / Caddy) in front of ports 3000/5000
- [ ] Consider restricting CORS to explicit origins
- [ ] Regular database backups (`docker exec verdiq pg_dump -U postgres verdiq`)
- [ ] Monitor API logs (`docker compose logs -f`)
