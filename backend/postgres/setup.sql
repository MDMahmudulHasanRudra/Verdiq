-- Verdiq PostgreSQL 18 Database Setup
-- Run this on a fresh PostgreSQL instance before applying EF Core migrations.

-- 1. Create the database (run as superuser)
-- CREATE DATABASE verdiq
--   WITH ENCODING 'UTF8'
--   LC_COLLATE = 'en_US.UTF-8'
--   LC_CTYPE   = 'en_US.UTF-8'
--   TEMPLATE   = template0;

-- 2. Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- 3. Create application user (optional, for least-privilege)
-- CREATE USER verdiq_app WITH PASSWORD 'change_me_in_production';
-- GRANT CONNECT ON DATABASE verdiq TO verdiq_app;
-- GRANT USAGE ON SCHEMA public TO verdiq_app;
-- GRANT CREATE ON SCHEMA public TO verdiq_app;
-- ALTER DEFAULT PRIVILEGES IN SCHEMA public
--   GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO verdiq_app;
-- ALTER DEFAULT PRIVILEGES IN SCHEMA public
--   GRANT USAGE, SELECT ON SEQUENCES TO verdiq_app;
