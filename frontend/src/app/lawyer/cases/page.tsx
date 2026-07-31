"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Table, Pagination } from "@/components/ui/table";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useCases } from "@/lib/hooks";
import { caseService } from "@/lib/services";
import { getErrorMessage, formatDate, cn } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import {
  Plus,
  Search,
  FolderOpen,
  ArrowUpRight,
  LayoutGrid,
  List,
  PenLine,
  Trash2,
  Users,
  Gavel,
  FileText,
  CalendarDays,
  Scale
} from "lucide-react";
import type { Case, CreateCaseInput, UpdateCaseInput } from "@/types/models";

const statuses = ["Active", "Pending", "Appeal", "Closed", "Withdrawn", "BailClosed"];
const priorities = ["Low", "Medium", "High", "Urgent"];
const caseTypes = ["Civil", "Criminal", "Family", "Commercial", "Constitutional", "Labor", "Property", "Other"];

interface CaseFormState {
  title: string;
  courtName: string;
  caseType: string;
  filingDate: string;
  status: string;
  priority: string;
  opponent: string;
  actsAndSections: string;
  description: string;
}

const emptyForm = (): CaseFormState => ({
  title: "",
  courtName: "",
  caseType: "Civil",
  filingDate: new Date().toISOString().slice(0, 10),
  status: "Active",
  priority: "Medium",
  opponent: "",
  actsAndSections: "",
  description: ""
});

const fromCase = (c: Case): CaseFormState => ({
  title: c.title,
  courtName: c.courtName,
  caseType: c.caseType,
  filingDate: (c.filingDate || "").slice(0, 10),
  status: c.status,
  priority: c.priority || "Medium",
  opponent: c.opponent ?? "",
  actsAndSections: c.actsAndSections ?? "",
  description: c.description ?? ""
});

type ViewMode = "list" | "grid";

export default function CasesPage() {
  const router = useRouter();
  const toast = useToast();
  const qc = useQueryClient();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [status, setStatus] = useState("");
  const [priority, setPriority] = useState("");
  const [type, setType] = useState("");
  const [sortBy, setSortBy] = useState("filingDate");
  const [sortOrder, setSortOrder] = useState<"asc" | "desc">("desc");
  const [view, setView] = useState<ViewMode>("list");

  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<Case | null>(null);
  const [deleting, setDeleting] = useState<Case | null>(null);

  useEffect(() => {
    const t = window.setTimeout(() => setDebouncedSearch(search), 350);
    return () => window.clearTimeout(t);
  }, [search]);

  const { data, isLoading } = useCases({
    page,
    pageSize: 10,
    search: debouncedSearch || undefined,
    status: status || undefined,
    priority: priority || undefined,
    type: type || undefined,
    sortBy,
    sortOrder
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ["cases"] });

  const createMutation = useMutation({
    mutationFn: (input: CreateCaseInput) => caseService.create(input),
    onSuccess: (data) => {
      invalidate();
      setCreateOpen(false);
      toast.success("Case created");
      router.push(`/lawyer/cases/${data.id}`);
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateCaseInput }) => caseService.update(id, input),
    onSuccess: () => {
      invalidate();
      setEditing(null);
      toast.success("Case updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => caseService.remove(id),
    onSuccess: () => {
      invalidate();
      setDeleting(null);
      setPage(1);
      toast.success("Case deleted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const applyFilter = (fn: () => void) => {
    fn();
    setPage(1);
  };

  const cases = data?.data ?? [];

  return (
    <div>
      <PageHeader
        title="Cases"
        subtitle="Manage all firm cases across courts and practice areas."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> New Case
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <div className="flex flex-col gap-3">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
              <Input
                placeholder="Search case number, title, court, opponent…"
                className="pl-9"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            <Select
              className="sm:w-40"
              value={type}
              onChange={(e) => applyFilter(() => setType(e.target.value))}
            >
              <option value="">All types</option>
              {caseTypes.map((t) => (
                <option key={t} value={t}>{t}</option>
              ))}
            </Select>
            <Select
              className="sm:w-40"
              value={priority}
              onChange={(e) => applyFilter(() => setPriority(e.target.value))}
            >
              <option value="">All priorities</option>
              {priorities.map((p) => (
                <option key={p} value={p}>{p}</option>
              ))}
            </Select>
            <Select
              className="sm:w-48"
              value={`${sortBy}:${sortOrder}`}
              onChange={(e) => {
                const [sb, so] = e.target.value.split(":") as [string, "asc" | "desc"];
                setSortBy(sb);
                setSortOrder(so);
                setPage(1);
              }}
            >
              <option value="filingDate:desc">Newest filing</option>
              <option value="filingDate:asc">Oldest filing</option>
              <option value="createdAt:desc">Recently created</option>
              <option value="title:asc">Title A–Z</option>
              <option value="caseNumber:asc">Case no. A–Z</option>
            </Select>
            <div className="flex items-center gap-1 rounded-lg border border-line bg-slate-50 p-1">
              <button
                onClick={() => setView("list")}
                className={cn(
                  "inline-flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-xs font-medium transition-colors",
                  view === "list" ? "bg-card text-ink shadow-card" : "text-ink-muted hover:text-ink"
                )}
                aria-label="List view"
              >
                <List className="h-4 w-4" /> List
              </button>
              <button
                onClick={() => setView("grid")}
                className={cn(
                  "inline-flex cursor-pointer items-center gap-1.5 rounded-md px-3 py-1.5 text-xs font-medium transition-colors",
                  view === "grid" ? "bg-card text-ink shadow-card" : "text-ink-muted hover:text-ink"
                )}
                aria-label="Grid view"
              >
                <LayoutGrid className="h-4 w-4" /> Grid
              </button>
            </div>
          </div>

          <div className="flex flex-wrap items-center gap-2">
            {["", ...statuses].map((s) => (
              <button
                key={s || "all"}
                onClick={() => applyFilter(() => setStatus(s))}
                className={cn(
                  "cursor-pointer rounded-full border px-3 py-1 text-xs font-medium transition-colors",
                  status === s
                    ? "border-ink bg-ink text-white"
                    : "border-line bg-card text-ink-muted hover:border-ink-soft hover:text-ink"
                )}
              >
                {s || "All"}
              </button>
            ))}
          </div>
        </div>
      </Card>

      {isLoading ? (
        <Card><Loading /></Card>
      ) : cases.length > 0 ? (
        <Card className="p-4">
          <div className="mb-3 flex items-center justify-between px-1">
            <p className="text-sm text-ink-muted">
              {data?.totalCount ?? cases.length} case{data?.totalCount !== 1 ? "s" : ""}
              {data && data.totalPages > 1 ? ` · page ${data.page} of ${data.totalPages}` : ""}
            </p>
          </div>

          {view === "list" ? (
            <div className="overflow-x-auto">
              <Table>
                <thead>
                  <tr>
                    <th>Case Number</th>
                    <th>Title</th>
                    <th>Court</th>
                    <th>Assigned</th>
                    <th>Next Hearing</th>
                    <th>Status</th>
                    <th>Priority</th>
                    <th className="text-right">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {cases.map((c) => (
                    <tr
                      key={c.id}
                      className="cursor-pointer"
                      onClick={() => router.push(`/lawyer/cases/${c.id}`)}
                    >
                      <td className="font-medium text-primary-700">{c.caseNumber}</td>
                      <td className="max-w-72">
                        <p className="truncate font-medium text-ink">{c.title}</p>
                        <p className="truncate text-xs text-ink-muted">
                          {c.caseType}
                          {c.clients.length > 0 ? ` · ${c.clients.map((cl) => cl.name).join(", ")}` : ""}
                        </p>
                      </td>
                      <td className="text-ink-muted">{c.courtName}</td>
                      <td className="text-ink-muted">{c.assignedLawyerName ?? "—"}</td>
                      <td className="whitespace-nowrap text-ink-muted">
                        {c.nextHearingDate ? (
                          <span className="inline-flex items-center gap-1.5">
                            <CalendarDays className="h-3.5 w-3.5 text-gold-600" />
                            {formatDate(c.nextHearingDate)}
                          </span>
                        ) : (
                          "—"
                        )}
                      </td>
                      <td><StatusBadge value={c.status} /></td>
                      <td><StatusBadge value={c.priority} /></td>
                      <td onClick={(e) => e.stopPropagation()}>
                        <div className="flex items-center justify-end gap-1">
                          <button
                            onClick={() => setEditing(c)}
                            className="cursor-pointer rounded-md p-1.5 text-ink-soft transition-colors hover:bg-slate-100 hover:text-ink"
                            title="Edit case"
                          >
                            <PenLine className="h-4 w-4" />
                          </button>
                          <button
                            onClick={() => setDeleting(c)}
                            className="cursor-pointer rounded-md p-1.5 text-ink-soft transition-colors hover:bg-red-50 hover:text-red-600"
                            title="Delete case"
                          >
                            <Trash2 className="h-4 w-4" />
                          </button>
                          <ArrowUpRight className="ml-1 h-4 w-4 text-ink-soft" />
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </Table>
            </div>
          ) : (
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
              {cases.map((c) => (
                <CaseGridCard
                  key={c.id}
                  c={c}
                  onOpen={() => router.push(`/lawyer/cases/${c.id}`)}
                  onEdit={() => setEditing(c)}
                  onDelete={() => setDeleting(c)}
                  onStatusChange={(status) =>
                    updateMutation.mutate({ id: c.id, input: { status } })
                  }
                />
              ))}
            </div>
          )}

          {data && data.totalPages > 1 ? (
            <div className="mt-4">
              <Pagination
                page={data.page}
                totalPages={data.totalPages}
                totalCount={data.totalCount}
                onChange={setPage}
              />
            </div>
          ) : null}
        </Card>
      ) : (
        <Card>
          <EmptyState
            icon={<FolderOpen className="h-10 w-10" />}
            title={debouncedSearch || status || priority || type ? "No matching cases" : "No cases found"}
            description={
              debouncedSearch || status || priority || type
                ? "Try clearing some filters or changing your search."
                : "Create your first case to start tracking filings, hearings and documents."
            }
            action={
              <Button onClick={() => setCreateOpen(true)}>
                <Plus className="h-4 w-4" /> New Case
              </Button>
            }
          />
        </Card>
      )}

      <CaseFormDialog
        open={createOpen}
        title="New Case"
        description="Enter the basic case details to get started."
        submitLabel="Create Case"
        initial={emptyForm()}
        showStatus={false}
        onClose={() => setCreateOpen(false)}
        onSubmit={(form) =>
          createMutation.mutate({
            title: form.title,
            courtName: form.courtName,
            caseType: form.caseType,
            filingDate: form.filingDate,
            opponent: form.opponent || null,
            priority: form.priority || null,
            description: form.description || null,
            actsAndSections: form.actsAndSections || null,
            clientIds: []
          })
        }
      />

      {editing ? (
        <CaseFormDialog
          open
          title="Edit Case"
          description={editing.caseNumber}
          submitLabel="Save Changes"
          initial={fromCase(editing)}
          showStatus
          onClose={() => setEditing(null)}
          onSubmit={(form) =>
            updateMutation.mutate({
              id: editing.id,
              input: {
                title: form.title || undefined,
                courtName: form.courtName || undefined,
                caseType: form.caseType || undefined,
                filingDate: form.filingDate || undefined,
                status: form.status || undefined,
                priority: form.priority || undefined,
                opponent: form.opponent || null,
                description: form.description || null,
                actsAndSections: form.actsAndSections || null
              }
            })
          }
        />
      ) : null}

      <Dialog
        open={!!deleting}
        onClose={() => setDeleting(null)}
        title="Delete case"
        description={
          deleting
            ? `"${deleting.title}" (${deleting.caseNumber}) will be permanently removed. This cannot be undone.`
            : ""
        }
        footer={
          <>
            <Button variant="ghost" onClick={() => setDeleting(null)}>Cancel</Button>
            <Button
              variant="danger"
              disabled={deleteMutation.isPending}
              onClick={() => deleting && deleteMutation.mutate(deleting.id)}
            >
              <Trash2 className="h-4 w-4" /> Delete Case
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">
          Related hearings and documents on the backend may also be affected. Make sure you
          really want to remove this case.
        </p>
      </Dialog>
    </div>
  );
}

function CaseGridCard({
  c,
  onOpen,
  onEdit,
  onDelete,
  onStatusChange
}: {
  c: Case;
  onOpen: () => void;
  onEdit: () => void;
  onDelete: () => void;
  onStatusChange: (status: string) => void;
}) {
  return (
    <Card className="flex cursor-pointer flex-col transition-shadow hover:shadow-pop" onClick={onOpen}>
      <div className="flex items-start justify-between gap-2 border-b border-line-soft px-4 py-3">
        <div className="min-w-0">
          <p className="font-mono text-xs font-semibold text-primary-700">{c.caseNumber}</p>
          <p className="truncate font-display text-base font-semibold text-ink">{c.title}</p>
        </div>
        <div className="flex shrink-0 gap-1">
          <StatusBadge value={c.status} />
          <StatusBadge value={c.priority} />
        </div>
      </div>

      <div className="flex flex-1 flex-col gap-2 px-4 py-3 text-sm">
        <p className="inline-flex items-center gap-1.5 text-ink-muted">
          <Scale className="h-3.5 w-3.5 text-ink-soft" />
          {c.courtName}
          {c.caseType ? <span className="text-ink-soft">· {c.caseType}</span> : null}
        </p>
        {c.opponent ? (
          <p className="truncate text-ink-muted">
            <span className="text-ink-soft">Opponent:</span> {c.opponent}
          </p>
        ) : null}
        <p className="truncate text-ink-muted">
          <span className="text-ink-soft">Assigned:</span> {c.assignedLawyerName ?? "—"}
        </p>
        <div className="flex items-center gap-4 pt-1 text-xs text-ink-muted">
          <span className="inline-flex items-center gap-1">
            <Users className="h-3.5 w-3.5" /> {c.clients.length}
          </span>
          <span className="inline-flex items-center gap-1">
            <Gavel className="h-3.5 w-3.5" /> {c.hearingsCount ?? 0}
          </span>
          <span className="inline-flex items-center gap-1">
            <FileText className="h-3.5 w-3.5" /> {c.documentsCount ?? 0}
          </span>
        </div>
        {c.nextHearingDate ? (
          <p className="inline-flex items-center gap-1.5 text-xs font-medium text-gold-700">
            <CalendarDays className="h-3.5 w-3.5" />
            Next hearing {formatDate(c.nextHearingDate)}
          </p>
        ) : null}
      </div>

      <div
        className="flex items-center justify-between gap-2 border-t border-line-soft px-4 py-2.5"
        onClick={(e) => e.stopPropagation()}
      >
        <Select
          className="h-8 w-32 text-xs"
          value={c.status}
          onChange={(e) => onStatusChange(e.target.value)}
          title="Quick status change"
        >
          {statuses.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </Select>
        <div className="flex items-center gap-1">
          <button
            onClick={onEdit}
            className="cursor-pointer rounded-md p-1.5 text-ink-soft transition-colors hover:bg-slate-100 hover:text-ink"
            title="Edit case"
          >
            <PenLine className="h-4 w-4" />
          </button>
          <button
            onClick={onDelete}
            className="cursor-pointer rounded-md p-1.5 text-ink-soft transition-colors hover:bg-red-50 hover:text-red-600"
            title="Delete case"
          >
            <Trash2 className="h-4 w-4" />
          </button>
        </div>
      </div>
    </Card>
  );
}

function CaseFormDialog({
  open,
  title,
  description,
  submitLabel,
  initial,
  showStatus,
  onClose,
  onSubmit
}: {
  open: boolean;
  title: string;
  description: string;
  submitLabel: string;
  initial: CaseFormState;
  showStatus: boolean;
  onClose: () => void;
  onSubmit: (form: CaseFormState) => void;
}) {
  const [form, setForm] = useState<CaseFormState>(initial);

  useEffect(() => {
    setForm(initial);
  }, [initial]);

  const set = (k: keyof CaseFormState, v: string) => setForm((f) => ({ ...f, [k]: v }));

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={title}
      description={description}
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.title || !form.courtName}
            onClick={() => onSubmit(form)}
          >
            {submitLabel}
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="sm:col-span-2">
          <label className="mb-1.5 block text-sm font-medium text-ink">Case Title <span className="text-red-500">*</span></label>
          <Input value={form.title} onChange={(e) => set("title", e.target.value)} placeholder="e.g. State vs. Rahim Trading Co." />
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-ink">Court Name <span className="text-red-500">*</span></label>
          <Input value={form.courtName} onChange={(e) => set("courtName", e.target.value)} placeholder="e.g. Dhaka District Court" />
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-ink">Case Type</label>
          <Select value={form.caseType} onChange={(e) => set("caseType", e.target.value)}>
            {caseTypes.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </Select>
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-ink">Filing Date</label>
          <Input type="date" value={form.filingDate} onChange={(e) => set("filingDate", e.target.value)} />
        </div>
        {showStatus ? (
          <div>
            <label className="mb-1.5 block text-sm font-medium text-ink">Status</label>
            <Select value={form.status} onChange={(e) => set("status", e.target.value)}>
              {statuses.map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </Select>
          </div>
        ) : null}
        <div>
          <label className="mb-1.5 block text-sm font-medium text-ink">Priority</label>
          <Select value={form.priority} onChange={(e) => set("priority", e.target.value)}>
            {priorities.map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </Select>
        </div>
        <div className="sm:col-span-2">
          <label className="mb-1.5 block text-sm font-medium text-ink">Opponent</label>
          <Input value={form.opponent} onChange={(e) => set("opponent", e.target.value)} placeholder="Opposing party name" />
        </div>
        <div className="sm:col-span-2">
          <label className="mb-1.5 block text-sm font-medium text-ink">Acts & Sections</label>
          <Input value={form.actsAndSections} onChange={(e) => set("actsAndSections", e.target.value)} placeholder="e.g. Penal Code 1860, s. 420" />
        </div>
        <div className="sm:col-span-2">
          <label className="mb-1.5 block text-sm font-medium text-ink">Description</label>
          <textarea
            className="w-full rounded-lg border border-line bg-card px-3 py-2 text-sm text-ink focus:border-primary-600 focus:shadow-glow focus:outline-none"
            rows={3}
            value={form.description}
            onChange={(e) => set("description", e.target.value)}
          />
        </div>
      </div>
    </Dialog>
  );
}
