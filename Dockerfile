# =============================================================================
# Verdiq — Single-container Dockerfile (full app: Postgres + API + Web)
# Build everything, then run all three services in ONE container via supervisor.
# =============================================================================

# ---------- Stage 1: Build the .NET API ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

COPY backend/Verdiq.slnx .
COPY backend/Verdiq.Domain/Verdiq.Domain.csproj Verdiq.Domain/
COPY backend/Verdiq.Application/Verdiq.Application.csproj Verdiq.Application/
COPY backend/Verdiq.Infrastructure/Verdiq.Infrastructure.csproj Verdiq.Infrastructure/
COPY backend/Verdiq.API/Verdiq.API.csproj Verdiq.API/

RUN dotnet restore Verdiq.API/Verdiq.API.csproj

COPY backend/ .
RUN dotnet publish Verdiq.API/Verdiq.API.csproj -c Release -o /out/api

# ---------- Stage 2: Build the Next.js web app ----------
FROM node:22-alpine AS web-build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ .

ARG NEXT_PUBLIC_API_URL=http://localhost:5000/api
ARG NEXT_PUBLIC_APP_NAME=Verdiq
ARG NEXT_PUBLIC_APP_URL=http://localhost:3000
ENV NEXT_PUBLIC_API_URL=$NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_APP_NAME=$NEXT_PUBLIC_APP_NAME
ENV NEXT_PUBLIC_APP_URL=$NEXT_PUBLIC_APP_URL
ENV NEXT_TELEMETRY_DISABLED=1

RUN npm run build

# ---------- Stage 3: Runtime — one container, everything inside ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0

ENV DEBIAN_FRONTEND=noninteractive

# Node.js runtime (copy binary from the official node:22 image — older glibc,
# safe to run on the Ubuntu noble base)
COPY --from=node:22 /usr/local/bin/node /usr/local/bin/node

# PostgreSQL 16 + supervisor + curl (for healthchecks)
RUN apt-get update && \
    apt-get install -y --no-install-recommends postgresql-16 supervisor curl && \
    rm -rf /var/lib/apt/lists/*

# Copy the published API
COPY --from=api-build /out/api /app/api

# Copy the standalone Next.js build
COPY --from=web-build /app/.next/standalone /app/web
COPY --from=web-build /app/.next/static /app/web/.next/static

# Supervisor + entrypoint
COPY docker/supervisord.conf /etc/supervisor/supervisord.conf
COPY docker/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

ENV ASPNETCORE_URLS=http://+:5000
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV POSTGRES_PASSWORD=postgres

EXPOSE 3000 5000 5432

HEALTHCHECK --interval=15s --timeout=5s --retries=5 --start-period=40s \
  CMD curl -fsS http://127.0.0.1:5000/health >/dev/null && curl -fsS http://127.0.0.1:3000 >/dev/null || exit 1

ENTRYPOINT ["/entrypoint.sh"]
