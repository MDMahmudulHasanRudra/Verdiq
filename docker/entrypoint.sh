#!/usr/bin/env bash
set -e

PGDATA=${PGDATA:-/var/lib/verdiq/pgdata}
PG_PORT=${POSTGRES_PORT:-5432}
PG_USER=postgres
PG_DB=verdiq
PG_PASSWORD=${POSTGRES_PASSWORD:-postgres}

export ConnectionStrings__DefaultConnection="Host=127.0.0.1;Port=${PG_PORT};Database=${PG_DB};Username=${PG_USER};Password=${PG_PASSWORD}"
export DocumentStorage__Path=${DocumentStorage__Path:-/data/uploads}

mkdir -p "$(dirname "$PGDATA")" /data/uploads
chown -R postgres:postgres "$(dirname "$PGDATA")"

# --- Initialize the PostgreSQL data directory if empty ---
if [ ! -s "$PGDATA/PG_VERSION" ]; then
  echo "[init] Initializing PostgreSQL data directory at $PGDATA"
  mkdir -p "$PGDATA"
  chown -R postgres:postgres "$PGDATA"
  su postgres -c "/usr/lib/postgresql/16/bin/initdb -D $PGDATA --auth-local=trust --auth-host=scram-sha-256 -U $PG_USER"
fi

# --- Start PostgreSQL in the background ---
echo "[init] Starting PostgreSQL on 127.0.0.1:$PG_PORT"
su postgres -c "/usr/lib/postgresql/16/bin/pg_ctl -D $PGDATA -o '-p $PG_PORT -c listen_addresses=127.0.0.1' -w start"

# --- Ensure role password + database exist ---
su postgres -c "psql -p $PG_PORT -v ON_ERROR_STOP=1 -c \"ALTER USER $PG_USER WITH PASSWORD '$PG_PASSWORD';\""
su postgres -c "psql -p $PG_PORT -tAc \"SELECT 1 FROM pg_database WHERE datname='$PG_DB'\" | grep -q 1" || \
  su postgres -c "createdb -p $PG_PORT -O $PG_USER $PG_DB"

echo "[init] Database ready."

# --- Hand off to supervisor (manages API + Web; Postgres stays as a child) ---
exec /usr/bin/supervisord -c /etc/supervisor/supervisord.conf -n
