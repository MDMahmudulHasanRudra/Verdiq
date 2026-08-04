-- Verdiq PostgreSQL 18 — Development Seed Data
-- Inserts test data for local development. Run after EF Core migrations.

-- Chamber
INSERT INTO "Chambers" ("Id", "Name", "Address", "Phone", "SubscriptionPlan", "CreatedAt", "IsDeleted")
VALUES ('c0000000-0000-0000-0000-000000000001', 'Verdiq Chamber Dev', '42 Gulshan Avenue, Dhaka', '+8801700000000', 'Chamber', NOW(), FALSE)
ON CONFLICT ("Id") DO NOTHING;

-- Users
INSERT INTO "Users" ("Id", "FullName", "Email", "Phone", "PasswordHash", "Role", "IsActive", "ChamberId", "CreatedAt", "IsDeleted")
VALUES
  ('a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'Admin Verdiq', 'admin@verdiq.com', '+8801700000000', '$2a$11$VyGwoqxHC6gMQ9iMsda/7eE9a5TV9SOHBRyX4SgwU.RJNNxnYEera', 'Owner', TRUE, 'c0000000-0000-0000-0000-000000000001', NOW(), FALSE),
  ('e5f6a7b8-c9d0-1234-5678-9abcdef01234', 'Adv. Abdul Karim', 'lawyer@verdiq.com', '+8801712345678', '$2a$11$CnI9Ur82n8LPzJkcFCD6Q.D4J892KK5RHTh7BAXnHCmKE3cQOxOey', 'SeniorLawyer', TRUE, 'c0000000-0000-0000-0000-000000000001', NOW(), FALSE)
ON CONFLICT ("Id") DO NOTHING;

-- Subscription
INSERT INTO "Subscriptions" ("Id", "ChamberId", "Plan", "Status", "CurrentPeriodStart", "CurrentPeriodEnd", "CancelAtPeriodEnd", "CreatedAt", "IsDeleted")
VALUES ('b2c3d4e5-f6a7-8901-bcde-f12345678901', 'c0000000-0000-0000-0000-000000000001', 'Chamber', 'Active', NOW(), NOW() + INTERVAL '1 year', FALSE, NOW(), FALSE)
ON CONFLICT ("Id") DO NOTHING;

-- Sample clients
INSERT INTO "Clients" ("Id", "Name", "Phone", "Email", "ClientType", "IsActive", "ChamberId", "CreatedAt", "IsDeleted")
VALUES
  ('11111111-1111-1111-1111-111111111111', 'Rahman Industries Ltd', '+8801812345678', 'rahman@example.com', 'Corporate', TRUE, 'c0000000-0000-0000-0000-000000000001', NOW(), FALSE),
  ('22222222-2222-2222-2222-222222222222', 'Fatima Begum', '+8801912345678', 'fatima@example.com', 'Individual', TRUE, 'c0000000-0000-0000-0000-000000000001', NOW(), FALSE)
ON CONFLICT ("Id") DO NOTHING;

-- Sample case
INSERT INTO "Cases" ("Id", "Title", "CaseNumber", "CourtName", "CaseType", "FilingDate", "Status", "Priority", "AssignedLawyerId", "ChamberId", "CreatedAt", "IsDeleted")
VALUES ('33333333-3333-3333-3333-333333333333', 'Rahman vs. Government', '2026-CIV-001', 'Dhaka District Court', 'Civil', NOW(), 'Active', 'High', 'e5f6a7b8-c9d0-1234-5678-9abcdef01234', 'c0000000-0000-0000-0000-000000000001', NOW(), FALSE)
ON CONFLICT ("Id") DO NOTHING;

-- Link client to case
INSERT INTO "ClientCases" ("Id", "ClientId", "CaseId", "Role", "CreatedAt", "IsDeleted")
VALUES ('44444444-4444-4444-4444-444444444444', '11111111-1111-1111-1111-111111111111', '33333333-3333-3333-3333-333333333333', 'Plaintiff', NOW(), FALSE)
ON CONFLICT ("Id") DO NOTHING;

-- Sample bail
INSERT INTO "Bails" ("Id", "CaseId", "Status", "BailType", "BailAmount", "BailConditions", "BondNumber", "SuretyName", "SuretyContact", "GrantedBy", "CreatedAt", "IsDeleted")
VALUES ('55555555-5555-5555-5555-555555555555', '33333333-3333-3333-3333-333333333333', 'Pending', 'Regular', 50000.00, 'Must appear at every hearing', 'BND-2026-001', 'Kamal Hossain', '+8801711111111', 'Judge Ahmed', NOW(), FALSE)
ON CONFLICT ("Id") DO NOTHING;
