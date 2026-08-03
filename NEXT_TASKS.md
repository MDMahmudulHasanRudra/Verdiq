[x] Backend: fix InvoicesController chamber bug (GetUserId->GetChamberId)
[x] Backend: lead conversion -> create Client + Case on stage ConvertedToClient
[x] Backend: LegalDocuments update/delete + fix GetById
[•] Frontend i18n infra: Bengali fonts, LanguageProvider, dictionaries (en/bn), language switcher
[ ] Upgrade Leads page: edit/delete/search/analytics + i18n
[ ] Upgrade Invoices page: client/case pickers, edit, mark-paid + i18n
[ ] Upgrade Documents page: rename/edit, preview, versions + i18n
[ ] Upgrade Tasks page: column moves, edit/delete, pickers + i18n
[ ] Upgrade Legal Database page: proper forms, edit/delete, legal docs tab + i18n
[ ] Upgrade Templates page: working 'Use' render flow, edit/delete, variables + i18n
[ ] i18n: convert shared shell (sidebar, header, layouts, login, common UI components)
[ ] i18n: convert remaining lawyer pages + client portal + super-admin
[ ] Build-verify backend (docker) + frontend (npm build), commit

---

## SESSION LOG — 2026-08-02 (session #2) — do NOT delete

### Status: Tasks 1-3 DONE (backend), Task 4 (i18n infra) IN PROGRESS — frontend files created, NOT yet type-checked / integrated into UI components

### Completed this session
- **Task 1 — InvoicesController chamber bug**: `GetUserId()` → `GetChamberId()` in Create + GetAll (file `backend/Verdiq.API/Controllers/InvoicesController.cs`). VERIFIED in working tree.
- **Task 2 — Lead conversion** (`backend/Verdiq.Infrastructure/Services/LeadService.cs`, `UpdateStageAsync`): on stage `ConvertedToClient`, creates a `Client` (via `IClientService.CreateAsync`) + `Case` (via `ICaseService.CreateAsync`) linked to the lead; sets `lead.ClientId`/`lead.CaseId`. Added `Lead.ClientId`/`CaseId` FKs + nav props (`Lead.cs`, `AppDbContext.cs`), DTO fields `ClientId/ClientName/CaseId/CaseTitle` in `LeadResponseDto`, `UpdateStageAsync` now takes `userId`, controller passes `GetUserId()`. Migration **`20260801163643_LeadClientCaseLinks`** (untracked, needs git add). VERIFIED builds.
- **Task 3 — LegalDocuments**: added `GetByIdAsync` (real lookup, not GetAll filter), `UpdateAsync` (`UpdateLegalDocumentDto`), `DeleteAsync`; controller `GET/{id}`, `PUT/{id}`, `DELETE/{id}`. VERIFIED builds.
- **Backend verify**: `dotnet build Verdiq.slnx` from `backend/` — **Build succeeded, 0 errors** (warnings only: NU1903 OpenApi vuln, CS0618 testcontainers obsolete).

### Task 4 (i18n infra) — files created (ALL NEW, uncommitted, NOT verified)
1. `frontend/src/lib/i18n/types.ts` — `Language`, `Dictionary` interface, `TranslationParams`. Namespaces: `common`, `nav`, `leads`, `invoices`, `documents`, `tasks`, `legalDatabase`, `templates`, `login`, `header`.
2. `frontend/src/lib/i18n/en.ts` — English dictionary (fully populated to match `types.ts`).
3. `frontend/src/lib/i18n/bn.ts` — Bengali dictionary (fully populated).
4. `frontend/src/lib/i18n/index.tsx` — `LanguageProvider` (localStorage key `verdiq-language`, sets `<html lang>`, default `en`), `useLanguage()` hook returning `{ lang, setLang, toggleLang, t, dict }`, `interpolate()` helper. `t(key)` resolves dot-paths, falls back to `en` dict then the raw key.
5. `frontend/src/components/layout/language-switcher.tsx` — `LanguageSwitcher` component (`Languages` icon + EN/BN label, calls `toggleLang`).
6. `frontend/src/app/layout.tsx` — added `Hind_Siliguri` from `next/font/google` as `--font-bengali`; wrapped app in `<LanguageProvider>` (inside `Providers`, outside `ToastProvider`).

### NEXT STEPS (do in this order)
1. **Add Bengali font to CSS**: `frontend/src/app/globals.css` `@theme` — add `--font-sans` fallback chain to include `var(--font-bengali)` for Bangla glyphs (e.g. `--font-sans: var(--font-lato), var(--font-bengali), ...`). Must apply because Hind_Siliguri latin subset still renders Bangla via system fallback otherwise.
2. **Verify i18n infra compiles**: run `npx tsc --noEmit` in `frontend/` (or `npm run build`).
3. **Integrate switcher**: add `<LanguageSwitcher />` to `frontend/src/components/layout/header.tsx` (next to Bell icon).
4. **Task 11 partially**: convert `sidebar.tsx` + `nav-config.tsx` labels to use `t()` (nav groups + item labels) and `header.tsx` strings — this makes the shell bilingual early.
5. Then continue Task 4→5: **Upgrade Leads page** `frontend/src/app/lawyer/leads/page.tsx` — add edit/delete dialogs, server search, analytics cards + translate all strings via `useLanguage().t`. The `leads` dictionary namespace is already populated.
6. Tasks 6-10 follow same pattern (page upgrades + use existing dict namespaces). If a dictionary key is missing, add to BOTH `en.ts` + `bn.ts` + `types.ts`.
7. Task 13: `npx tsc --noEmit` + `npm run build` (frontend), `docker compose up -d --build` (backend), then commit.

### REMINDERS / GOTCHAS
- **Add migration to git**: `backend/Verdiq.Infrastructure/Migrations/20260801163643_LeadClientCaseLinks.cs` (+ Designer) are untracked — must be committed.
- Frontend pages are all `"use client"` — `useLanguage()` works in any of them.
- `Hind_Siliguri` subsets: only `latin` (it does include Bangla glyphs via variable font; if Bangla glyphs look wrong, add `subsets: ["latin", "bengali"]`).
- `layout.tsx` is a server component — `LanguageProvider`/`ToastProvider` usage already follows the existing pattern (client components).
- Do NOT delete the migration or the uncommitted backend/obj+bin churn; only commit intentional changes.
- `NEXT_TASKS.md` was rewritten to this compact 13-line format — original long-form list is gone (in git history if needed).
