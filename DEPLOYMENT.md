# Deployment

## Docker (Local Development)

### Prerequisites
- Docker Desktop
- .NET 10 SDK (for local builds)

### Quick Start

```bash
cd backend

# Build and start services
docker compose up -d

# Check status
docker compose ps

# View logs
docker compose logs -f
```

This starts:
- **db** — PostgreSQL 16-alpine on port 5432
- **api** — Verdiq API on port 5000

### Rebuild After Changes

```bash
docker compose build api
docker compose up -d
```

### Stopping

```bash
docker compose down          # Stop containers
docker compose down -v       # Stop + delete volume (wipes DB data)
```

### Clean Slate (Reset Database)

If the database gets into a bad state (e.g. leftover `__EFMigrationsHistory` table from `Migrate()` when `EnsureCreated()` is expected):

```powershell
# Drop the migration history table
"DROP TABLE IF EXISTS ""__EFMigrationsHistory"" CASCADE;" | docker exec -i backend-db-1 psql -U postgres -d verdiq

# Or reset completely
docker compose down -v && docker compose up -d
```

---

## Environment Variables

### Backend (`backend/Verdiq.API/.env.example`)

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=VerdiqDb;Username=postgres;Password=postgres` |
| `Jwt__Key` | JWT signing key (32+ chars) | Auto-generated in development |
| `Jwt__Issuer` | JWT issuer | `Verdiq` |
| `Jwt__Audience` | JWT audience | `VerdiqApp` |
| `DocumentStorage__BasePath` | File upload directory | `./UploadedDocuments` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |

In Docker Compose, use double-underscore syntax for nested config:

```yaml
environment:
  ConnectionStrings__DefaultConnection: "Host=db;Port=5432;Database=verdiq;Username=postgres;Password=postgres"
```

### Frontend (`frontend/.env.local` — required)

Create `.env.local` from `.env.example`:

```bash
copy .env.example .env.local
```

| Variable | Description | Default |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_URL` | Backend API URL | `http://localhost:5000/api` |
| `NEXT_PUBLIC_APP_NAME` | Application name | `Verdiq` |
| `NEXT_PUBLIC_APP_URL` | Frontend URL | `http://localhost:3000` |

`.env.local` is **required** — the frontend won't connect to the API without it.

---

## CORS Configuration

The backend CORS policy uses `SetIsOriginAllowed(_ => true)` which permits any origin with credentials. This avoids issues when accessing via Docker network IPs (e.g. `http://172.26.32.1:3000`) vs `localhost`.

```csharp
policy.SetIsOriginAllowed(_ => true)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials();
```

For production, replace with explicit origins:

```csharp
policy.WithOrigins("https://app.verdiq.com")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials();
```

---

## Auth Flow

The frontend uses **dual token storage**:

1. **localStorage** — Read by axios interceptor to attach `Authorization: Bearer` header
2. **Cookies** — Read by `middleware.ts` to protect routes (redirects to `/login` if absent)

On login:
```
Login → API /api/auth/login → store access_token + refresh_token in:
  - localStorage.setItem("access_token", token)
  - document.cookie = "access_token=..."
  - Then window.location.href = "/lawyer"
```

On logout:
```
Clear localStorage + cookies → redirect to /login
```

---

## Production Deployment

### 1. Build Images

```bash
cd backend

# Build API image
docker build -t verdiq-api:latest -f Dockerfile .

# Or use docker compose
docker compose build api
```

### 2. Production Docker Compose

Create `docker-compose.prod.yml`:

```yaml
services:
  db:
    image: postgres:16-alpine
    environment:
      POSTGRES_DB: verdiq
      POSTGRES_USER: verdiq
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - pgdata:/var/lib/postgresql/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U verdiq"]
      interval: 10s
      timeout: 5s
      retries: 5

  api:
    image: verdiq-api:latest
    ports:
      - "5000:5000"
    environment:
      ConnectionStrings__DefaultConnection: "Host=db;Port=5432;Database=verdiq;Username=verdiq;Password=${DB_PASSWORD}"
      Jwt__Key: ${JWT_KEY}
      Jwt__Issuer: "Verdiq"
      Jwt__Audience: "VerdiqApp"
      ASPNETCORE_ENVIRONMENT: Production
    depends_on:
      db:
        condition: service_healthy
    restart: unless-stopped

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    depends_on:
      - api
    restart: unless-stopped

volumes:
  pgdata:
```

### 3. Nginx Reverse Proxy (`nginx.conf`)

```nginx
server {
    listen 80;
    server_name verdiq.example.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name verdiq.example.com;

    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;

    location /api/ {
        proxy_pass http://api:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /health {
        proxy_pass http://api:5000;
    }

    location / {
        proxy_pass http://frontend:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }
}
```

### 4. Environment File (`.env.prod`)

```
DB_PASSWORD=your_secure_password
JWT_KEY=your_256bit_jwt_secret_key_here_minimum_length
```

### 5. Deploy

```bash
docker compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

---

## Database Migrations

The project uses `EnsureCreated()` to create the database schema on first run (no migration files). Seed data (admin/lawyer users) is applied via `HasData()` in `AppDbContext.OnModelCreating`.

**Important:** If the database has a leftover `__EFMigrationsHistory` table (from a previous `Migrate()` call), `EnsureCreated()` will see it and skip table creation. Drop the table first:

```powershell
"DROP TABLE IF EXISTS ""__EFMigrationsHistory"" CASCADE;" | docker exec -i backend-db-1 psql -U postgres -d verdiq
```

To switch to migration-based workflow:

```bash
cd backend
dotnet ef migrations add InitialCreate --project Verdiq.Infrastructure --startup-project Verdiq.API
dotnet ef database update --project Verdiq.Infrastructure --startup-project Verdiq.API
```

Then update `Program.cs` to use `db.Database.Migrate()` instead of `db.Database.EnsureCreated()`.

---

## NPM Workaround (Windows)

On Windows, npm may fail with `ENOENT` due to missing `package.json` in `AppData`. Workaround:

```powershell
$env:NPM_CONFIG_PREFIX = "C:\Program Files\nodejs"
```

Run this before any npm/npx command. Add to `$PROFILE` for persistence.

## Security Checklist

- [ ] Change JWT signing key from default
- [ ] Set strong PostgreSQL password
- [ ] Enable HTTPS with Let's Encrypt (certbot)
- [ ] Add CSP headers in nginx
- [ ] Replace `SetIsOriginAllowed(_ => true)` with explicit production origins
- [ ] Set `Secure; HttpOnly` flags on auth cookies in production
- [ ] Configure secure cookie attributes
- [ ] Set up fail2ban for brute force protection
- [ ] Regular database backups
- [ ] Monitor API logs for anomalies
