-- Verdiq PostgreSQL 18 — Useful Queries
-- Diagnostic and maintenance queries for the Verdiq database.

-- Table row counts
SELECT
  relname AS table_name,
  n_live_tup AS row_count
FROM pg_stat_user_tables
ORDER BY n_live_tup DESC;

-- Table sizes
SELECT
  relname AS table_name,
  pg_size_pretty(pg_total_relation_size(relid)) AS total_size,
  pg_size_pretty(pg_relation_size(relid)) AS table_size,
  pg_size_pretty(pg_indexes_size(relid)) AS index_size
FROM pg_catalog.pg_statio_user_tables
ORDER BY pg_total_relation_size(relid) DESC;

-- Index usage statistics
SELECT
  indexrelname AS index_name,
  idx_scan AS times_used,
  idx_tup_read AS tuples_read,
  idx_tup_fetch AS tuples_fetched
FROM pg_stat_user_indexes
ORDER BY idx_scan DESC;

-- Unused indexes (candidates for removal)
SELECT
  indexrelname AS index_name,
  relname AS table_name,
  idx_scan AS times_used
FROM pg_stat_user_indexes
WHERE idx_scan = 0 AND indexrelname NOT LIKE '%pkey%'
ORDER BY pg_relation_size(indexrelid) DESC;

-- Dead tuples (candidates for VACUUM)
SELECT
  relname AS table_name,
  n_dead_tup AS dead_tuples,
  last_autovacuum
FROM pg_stat_user_tables
WHERE n_dead_tup > 1000
ORDER BY n_dead_tup DESC;

-- Active connections
SELECT
  pid,
  usename,
  application_name,
  client_addr,
  state,
  query_start,
  LEFT(query, 80) AS query_preview
FROM pg_stat_activity
WHERE state = 'active'
ORDER BY query_start;
