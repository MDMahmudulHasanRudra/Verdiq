"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { useCase, useCaseActivities } from "@/lib/hooks";
import { caseService, hearingService, documentService, judgmentService, casePhotoService, caseWorkflows, legalSectionService } from "@/lib/services";
import { downloadBlob } from "@/lib/api";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Badge, StatusBadge } from "@/components/ui/badge";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Tabs } from "@/components/ui/tabs";
import { Dialog } from "@/components/ui/dialog";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Loading, EmptyState } from "@/components/ui/loading";
import { getErrorMessage, formatDate, formatDateTime, API_URL } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import {
  ArrowLeft,
  CalendarClock,
  Plus,
  CheckCircle2,
  FileUp,
  Download,
  Gavel,
  FileText,
  PenLine,
  Trash2,
  CalendarDays,
  Upload,
  Image as ImageIcon,
  Workflow as WorkflowIcon,
  Lock,
  Play,
  Link2,
  Ban,
  ListChecks,
  AlertCircle,
  Copy,
  Save,
  Search,
  ChevronDown,
  ChevronRight,
  ExternalLink
} from "lucide-react";
import type { Hearing, Judgment, CreateJudgmentInput, CasePhoto, CaseWorkflow, CaseWorkflowStep, Workflow } from "@/types/models";
import { FileUploadZone, type PendingFile } from "@/components/ui/file-upload-zone";
import { DocumentPreview } from "@/components/ui/document-preview";

const hearingStatuses = ["Scheduled", "Adjourned", "Completed", "Canceled"];
const results = ["Adjourned", "Granted", "Rejected", "Heard", "Deferred", "Dismissed"];

export default function CaseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const { data: caseData, isLoading } = useCase(id);
  const { data: activities } = useCaseActivities(id);
  const [tab, setTab] = useState("overview");
  const [hearingOpen, setHearingOpen] = useState(false);

  const addHearing = useMutation({
    mutationFn: (input: Record<string, unknown>) => hearingService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["hearings"] });
      qc.invalidateQueries({ queryKey: ["case", id] });
      setHearingOpen(false);
      toast.success("Hearing scheduled");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const completeProcedure = useMutation({
    mutationFn: (procedureId: string) => caseService.completeProcedure(id, procedureId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["case", id] });
      toast.success("Procedure completed");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const [editingNotes, setEditingNotes] = useState(false);
  const [notesValue, setNotesValue] = useState("");

  const saveNotes = useMutation({
    mutationFn: (notes: string) => caseService.update(id, { internalNotes: notes }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["case", id] });
      setEditingNotes(false);
      toast.success("Notes saved");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const duplicateMutation = useMutation({
    mutationFn: () => caseService.duplicate(id),
    onSuccess: (newCase) => {
      qc.invalidateQueries({ queryKey: ["cases"] });
      toast.success(`Case duplicated as ${newCase.caseNumber}`);
      router.push(`/lawyer/cases/${newCase.id}`);
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  if (isLoading) return <Loading label="Loading case…" />;
  if (!caseData) return <EmptyState title="Case not found" description="The case you are looking for does not exist." />;

  const c = caseData;

  return (
    <div>
      <PageHeader
        title={
          <span className="flex items-center gap-3">
            <Link href="/lawyer/cases" className="text-ink-muted transition-colors hover:text-ink">
              <ArrowLeft className="h-5 w-5" />
            </Link>
            <span>
              {c.caseNumber}
              <span className="ml-3 align-middle text-lg font-normal text-ink-muted">{c.title}</span>
            </span>
          </span>
        }
        subtitle={`${c.courtName} · Filed ${formatDate(c.filingDate)}`}
        actions={
          <>
            <Button variant="outline" onClick={() => {
              setNotesValue(c.internalNotes ?? "");
              setEditingNotes(true);
            }}>
              <PenLine className="h-4 w-4" /> Notes
            </Button>
            <Button variant="outline" onClick={() => duplicateMutation.mutate()} disabled={duplicateMutation.isPending}>
              <Copy className="h-4 w-4" /> {duplicateMutation.isPending ? "Duplicating..." : "Duplicate"}
            </Button>
            <Button variant="outline" onClick={() => router.push(`/lawyer/documents?caseId=${c.id}`)}>
              <FileUp className="h-4 w-4" /> {t("clientDetail.documents")}
            </Button>
            <Button onClick={() => setHearingOpen(true)}>
              <CalendarClock className="h-4 w-4" /> {t("hearings.schedule")}
            </Button>
          </>
        }
      />

      <div className="mb-5 flex flex-wrap items-center gap-2">
        <StatusBadge value={c.status} />
        <StatusBadge value={c.priority} />
        <Badge tone="blue">{c.caseType}</Badge>
        {c.assignedLawyerName ? <Badge tone="primary">Counsel: {c.assignedLawyerName}</Badge> : null}
        {c.teamName ? <Badge tone="purple">Team: {c.teamName}</Badge> : null}
        {c.nextHearingDate ? (
          <Badge tone="amber">
            <CalendarDays className="mr-1 h-3.5 w-3.5" />
            Next hearing {formatDate(c.nextHearingDate)}
          </Badge>
        ) : null}
        {c.lastHearingDate ? (
          <Badge tone="green">
            Last hearing {formatDate(c.lastHearingDate)}
            {c.lastHearingResult ? ` · ${c.lastHearingResult}` : ""}
          </Badge>
        ) : null}
      </div>

      <Tabs tabs={[{ value: "overview", label: t("clientDetail.overview") }, { value: "parties", label: "Parties" }, { value: "hearings", label: t("hearings.title") }, { value: "judgments", label: "Judgments" }, { value: "documents", label: t("clientDetail.documents") }, { value: "photos", label: "Photos" }, { value: "procedures", label: "Procedures" }, { value: "workflow", label: t("workflows.title") }, { value: "activity", label: "Activity" }]} value={tab} onChange={setTab} />

      {tab === "overview" && (
        <div className="mt-5 grid grid-cols-1 gap-6 lg:grid-cols-3">
          <Card className="lg:col-span-2">
            <CardHeader title={t("clientDetail.overview")} />
            <CardContent>
              <dl className="grid grid-cols-1 gap-x-8 gap-y-4 sm:grid-cols-2">
                <InfoRow label="Court" value={c.courtName} />
                <InfoRow label="Judge / Bench" value={c.judgeName ?? c.bench ?? "—"} />
                <InfoRow label="Opponent" value={c.opponent ?? "—"} />
                <InfoRow label="Opposing Counsel" value={c.opposingLawyer ?? "—"} />
                <InfoRow label="Prosecutor" value={c.prosecutor ?? "—"} />
                <InfoRow label="Jurisdiction" value={c.jurisdiction ?? "—"} />
                <InfoRow label="Police Station" value={c.policeStation ?? "—"} />
                <InfoRow label="FIR / GD Number" value={c.firNumber ?? c.gdNumber ?? "—"} />
                <InfoRow label="Practice Area" value={c.practiceArea ?? "—"} />
                <InfoRow label="Department" value={c.department ?? "—"} />
                <InfoRow label="Risk Level" value={c.riskLevel ?? "—"} />
                <InfoRow label="Appeal Status" value={c.appealStatus ?? "—"} />
              </dl>
              {c.description ? (
                <div className="mt-6">
                  <h4 className="mb-1 text-sm font-semibold text-ink">{t("clientDetail.notes")}</h4>
                  <p className="text-sm text-ink-muted">{c.description}</p>
                </div>
              ) : null}
              {c.actsAndSections ? (
                <div className="mt-4">
                  <h4 className="mb-1 text-sm font-semibold text-ink">Acts &amp; Sections</h4>
                  <p className="text-sm text-ink-muted">{c.actsAndSections}</p>
                </div>
              ) : null}

              <div className="mt-6 border-t border-line pt-4">
                <div className="mb-2 flex items-center justify-between">
                  <h4 className="text-sm font-semibold text-ink">Internal Notes</h4>
                  {!editingNotes ? (
                    <Button size="sm" variant="ghost" onClick={() => {
                      setNotesValue(c.internalNotes ?? "");
                      setEditingNotes(true);
                    }}>
                      <PenLine className="h-3.5 w-3.5" /> Edit
                    </Button>
                  ) : (
                    <Button size="sm" variant="ghost" onClick={() => saveNotes.mutate(notesValue)} disabled={saveNotes.isPending}>
                      <Save className="h-3.5 w-3.5" /> {saveNotes.isPending ? "Saving..." : "Save"}
                    </Button>
                  )}
                </div>
                {editingNotes ? (
                  <Textarea
                    value={notesValue}
                    onChange={(e) => setNotesValue(e.target.value)}
                    rows={4}
                    placeholder="Add internal notes about this case..."
                    className="w-full"
                  />
                ) : (
                  <p className="whitespace-pre-wrap text-sm text-ink-muted">
                    {c.internalNotes || "No notes yet."}
                  </p>
                )}
              </div>
            </CardContent>
          </Card>

          <div className="space-y-6">
            <Card>
              <CardHeader title="Clients" />
              <CardContent className="space-y-3">
                {c.clients.length > 0 ? (
                  c.clients.map((cl) => (
                    <button
                      key={cl.id}
                      onClick={() => router.push(`/lawyer/clients/${cl.id}`)}
                      className="flex w-full cursor-pointer items-center justify-between rounded-lg border border-line px-3 py-2 text-left transition-colors hover:bg-slate-50"
                    >
                      <div>
                        <p className="text-sm font-medium text-ink">{cl.name}</p>
                        <p className="text-xs text-ink-muted">{cl.role ?? "Primary"}</p>
                      </div>
                      <StatusBadge value={cl.role ?? "Client"} />
                    </button>
                  ))
                ) : (
                  <p className="text-sm text-ink-muted">No clients linked.</p>
                )}
              </CardContent>
            </Card>
            <Card>
              <CardHeader title="Quick Stats" />
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-3">
                  <div className="rounded-lg bg-slate-50 p-3">
                    <p className="text-2xl font-bold text-ink">{c.hearingsCount}</p>
                    <p className="text-xs text-ink-muted">Hearings</p>
                  </div>
                  <div className="rounded-lg bg-slate-50 p-3">
                    <p className="text-2xl font-bold text-ink">{c.documentsCount}</p>
                    <p className="text-xs text-ink-muted">Documents</p>
                  </div>
                </div>
                {c.retainerAmount ? (
                  <div className="rounded-lg bg-emerald-50 p-3">
                    <p className="text-lg font-bold text-emerald-800">৳{c.retainerAmount.toLocaleString()}</p>
                    <p className="text-xs text-emerald-600">Retainer Amount</p>
                  </div>
                ) : null}
                {c.limitationExpiry ? (
                  <div className="rounded-lg bg-amber-50 p-3">
                    <p className="text-sm font-semibold text-amber-800">{formatDate(c.limitationExpiry)}</p>
                    <p className="text-xs text-amber-600">Limitation Expiry</p>
                  </div>
                ) : null}
                <div className="flex flex-wrap gap-2">
                  <Button size="sm" variant="subtle" onClick={() => setTab("hearings")} className="flex-1">
                    <CalendarDays className="h-3.5 w-3.5" /> Hearings
                  </Button>
                  <Button size="sm" variant="subtle" onClick={() => setTab("documents")} className="flex-1">
                    <FileText className="h-3.5 w-3.5" /> Docs
                  </Button>
                  <Button size="sm" variant="subtle" onClick={() => setTab("workflow")} className="flex-1">
                    <WorkflowIcon className="h-3.5 w-3.5" /> Flow
                  </Button>
                </div>
              </CardContent>
            </Card>
            <WorkflowOverviewCard caseId={c.id} />
          </div>
        </div>
      )}

      {tab === "parties" && (
        <div className="mt-5 grid grid-cols-1 gap-6 lg:grid-cols-2">
          <Card>
            <CardHeader title="Opposition" />
            <CardContent className="space-y-3">
              <InfoRow label="Opponent" value={c.opponent ?? "—"} />
              <InfoRow label="Opposing Lawyer" value={c.opposingLawyer ?? "—"} />
              <InfoRow label="Prosecutor" value={c.prosecutor ?? "—"} />
            </CardContent>
          </Card>
          <Card>
            <CardHeader title="Assigned" />
            <CardContent className="space-y-3">
              <InfoRow label="Lead Counsel" value={c.assignedLawyerName ?? "—"} />
              <InfoRow label="Team" value={c.teamName ?? "—"} />
            </CardContent>
          </Card>
        </div>
      )}

      {tab === "hearings" && (
        <div className="mt-5">
          <HearingsTab caseId={c.id} caseNumber={c.caseNumber} onChanged={() => qc.invalidateQueries({ queryKey: ["case", id] })} />
        </div>
      )}

      {tab === "documents" && (
        <div className="mt-5">
          <DocumentsTab caseId={c.id} />
        </div>
      )}

      {tab === "judgments" && (
        <div className="mt-5">
          <JudgmentsTab caseId={c.id} caseNumber={c.caseNumber} />
        </div>
      )}

      {tab === "photos" && (
        <div className="mt-5">
          <PhotosTab caseId={c.id} />
        </div>
      )}

      {tab === "procedures" && (
        <div className="mt-5">
          <ProceduresTab caseId={c.id} legalSections={c.legalSections} />
        </div>
      )}

      {tab === "workflow" && (
        <div className="mt-5">
          <WorkflowsTab caseId={c.id} />
        </div>
      )}

      {tab === "activity" && (
        <div className="mt-5">
          <ActivityTab activities={activities ?? []} />
        </div>
      )}

      <ScheduleHearingDialog
        open={hearingOpen}
        onClose={() => setHearingOpen(false)}
        caseId={c.id}
        caseNumber={c.caseNumber}
        onSubmit={(v) => addHearing.mutate(v)}
      />
    </div>
  );
}

function InfoRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <dt className="text-xs font-semibold uppercase tracking-wide text-ink-muted">{label}</dt>
      <dd className="mt-0.5 text-sm text-ink">{value}</dd>
    </div>
  );
}

function useCaseHearings(caseId: string) {
  return useQuery({ queryKey: ["hearings", "case", caseId], queryFn: () => hearingService.byCase(caseId), enabled: !!caseId });
}

function HearingsTab({ caseId, caseNumber, onChanged }: { caseId: string; caseNumber: string; onChanged: () => void }) {
  const toast = useToast();
  const qc = useQueryClient();
  const { data: hearings, isLoading } = useCaseHearings(caseId);
  const [editing, setEditing] = useState<Hearing | null>(null);
  const [deleting, setDeleting] = useState<Hearing | null>(null);

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: Record<string, unknown> }) => hearingService.update(id, input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["hearings"] });
      qc.invalidateQueries({ queryKey: ["hearings", "case", caseId] });
      qc.invalidateQueries({ queryKey: ["case", caseId] });
      setEditing(null);
      onChanged();
      toast.success("Hearing updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => hearingService.remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["hearings"] });
      qc.invalidateQueries({ queryKey: ["hearings", "case", caseId] });
      qc.invalidateQueries({ queryKey: ["case", caseId] });
      setDeleting(null);
      onChanged();
      toast.success("Hearing removed");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const now = new Date();
  const upcoming = (hearings ?? [])
    .filter((h) => h.status === "Scheduled" && new Date(h.hearingDate) >= now)
    .sort((a, b) => new Date(a.hearingDate).getTime() - new Date(b.hearingDate).getTime());

  return (
    <>
      <Card>
        <CardHeader title="Hearings" description={`All hearings for ${caseNumber}. Click edit to record the judgment and next hearing day.`} />
        <CardContent className="space-y-3">
          {isLoading ? (
            <Loading />
          ) : hearings && hearings.length > 0 ? (
            hearings.map((h) => {
              const isNext = upcoming.length > 0 && upcoming[0].id === h.id;
              return (
                <div
                  key={h.id}
                  className={`flex items-start justify-between gap-4 rounded-lg border p-3 ${isNext ? "border-gold-300 bg-gold-50/50" : "border-line"}`}
                >
                  <div>
                    <p className="inline-flex items-center gap-2 text-sm font-medium text-ink">
                      {formatDateTime(h.hearingDate)}
                      {isNext ? (
                        <Badge tone="amber">Next hearing</Badge>
                      ) : null}
                    </p>
                    <p className="mt-0.5 text-xs text-ink-muted">
                      {h.courtroom ?? "Courtroom TBA"}
                      {h.judgeName ? ` · Judge ${h.judgeName}` : ""}
                    </p>
                    {h.result ? (
                      <p className="mt-1 inline-flex items-center gap-1.5 text-xs font-medium text-ink">
                        <Gavel className="h-3.5 w-3.5 text-gold-600" />
                        Judgment: {h.result}
                      </p>
                    ) : null}
                    {h.nextHearingDate ? (
                      <p className="mt-0.5 text-xs text-ink-muted">Next hearing: {formatDate(h.nextHearingDate)}</p>
                    ) : null}
                    {h.notes ? <p className="mt-1 text-xs text-ink-muted">{h.notes}</p> : null}
                  </div>
                  <div className="flex shrink-0 items-center gap-2">
                    <StatusBadge value={h.status} />
                    <button
                      onClick={() => setEditing(h)}
                      className="cursor-pointer rounded-md p-1.5 text-ink-soft transition-colors hover:bg-slate-100 hover:text-ink"
                      title="Record judgment / edit"
                    >
                      <PenLine className="h-4 w-4" />
                    </button>
                    <button
                      onClick={() => setDeleting(h)}
                      className="cursor-pointer rounded-md p-1.5 text-ink-soft transition-colors hover:bg-red-50 hover:text-red-600"
                      title="Delete hearing"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </div>
              );
            })
          ) : (
            <EmptyState title="No hearings yet" description="Schedule a hearing to track it here." />
          )}
        </CardContent>
      </Card>

      {editing ? (
        <HearingEditDialog
          hearing={editing}
          onClose={() => setEditing(null)}
          onSubmit={(input) => updateMutation.mutate({ id: editing.id, input })}
        />
      ) : null}

      <Dialog
        open={!!deleting}
        onClose={() => setDeleting(null)}
        title="Delete hearing"
        description={
          deleting ? `Hearing on ${formatDateTime(deleting.hearingDate)} will be permanently removed.` : ""
        }
        footer={
          <>
            <Button variant="ghost" onClick={() => setDeleting(null)}>Cancel</Button>
            <Button variant="danger" disabled={deleteMutation.isPending} onClick={() => deleting && deleteMutation.mutate(deleting.id)}>
              <Trash2 className="h-4 w-4" /> Delete Hearing
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">
          This action cannot be undone. Consider marking the hearing as &quot;Adjourned&quot; instead
          if it was simply moved.
        </p>
      </Dialog>
    </>
  );
}

function HearingEditDialog({
  hearing,
  onClose,
  onSubmit
}: {
  hearing: Hearing;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({
    hearingDate: (hearing.hearingDate || "").slice(0, 10),
    hearingTime: (hearing.hearingDate || "").slice(11, 16) || "10:00",
    courtroom: hearing.courtroom ?? "",
    judgeName: hearing.judgeName ?? "",
    status: hearing.status,
    result: hearing.result ?? "",
    nextHearingDate: hearing.nextHearingDate ? hearing.nextHearingDate.slice(0, 10) : "",
    notes: hearing.notes ?? ""
  });

  return (
    <Dialog
      open
      onClose={onClose}
      title="Record Hearing Judgment"
      description={`${hearing.caseNumber} — record the outcome and the next hearing day.`}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.hearingDate}
            onClick={() =>
              onSubmit({
                hearingDate: new Date(`${form.hearingDate}T${form.hearingTime}`).toISOString(),
                courtroom: form.courtroom || null,
                judgeName: form.judgeName || null,
                status: form.status || undefined,
                result: form.result || null,
                nextHearingDate: form.nextHearingDate
                  ? new Date(`${form.nextHearingDate}T10:00`).toISOString()
                  : null,
                notes: form.notes || null
              })
            }
          >
            Save
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Hearing Date" required>
          <Input type="date" value={form.hearingDate} onChange={(e) => setForm({ ...form, hearingDate: e.target.value })} />
        </Field>
        <Field label="Hearing Time">
          <Input type="time" value={form.hearingTime} onChange={(e) => setForm({ ...form, hearingTime: e.target.value })} />
        </Field>
        <Field label="Status">
          <Select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })}>
            {hearingStatuses.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </Select>
        </Field>
        <Field label="Result / Judgment">
          <Select value={form.result} onChange={(e) => setForm({ ...form, result: e.target.value })}>
            <option value="">No result recorded</option>
            {results.map((r) => (
              <option key={r} value={r}>{r}</option>
            ))}
          </Select>
        </Field>
        <Field label="Next Hearing Date" className="sm:col-span-2">
          <Input type="date" value={form.nextHearingDate} onChange={(e) => setForm({ ...form, nextHearingDate: e.target.value })} />
        </Field>
        <Field label="Courtroom">
          <Input value={form.courtroom} onChange={(e) => setForm({ ...form, courtroom: e.target.value })} />
        </Field>
        <Field label="Judge">
          <Input value={form.judgeName} onChange={(e) => setForm({ ...form, judgeName: e.target.value })} />
        </Field>
        <Field label="Notes" className="sm:col-span-2">
          <Textarea rows={3} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}

const evidenceCategories = [
  { value: "Evidence", label: "Evidence" },
  { value: "Pleadings", label: "Pleadings" },
  { value: "Court Orders", label: "Court Orders" },
  { value: "Correspondence", label: "Correspondence" },
  { value: "Contracts", label: "Contracts" },
  { value: "Witness Statements", label: "Witness Statements" },
  { value: "Expert Reports", label: "Expert Reports" },
  { value: "Fees", label: "Fees" },
  { value: "Other", label: "Other" }
];

const docCategories = ["Pleadings", "Evidence", "Court Orders", "Correspondence", "Contracts", "Fees", "Other"];

function DocumentsTab({ caseId }: { caseId: string }) {
  const toast = useToast();
  const qc = useQueryClient();
  const [uploadOpen, setUploadOpen] = useState(false);
  const [pendingFiles, setPendingFiles] = useState<PendingFile[]>([]);
  const [selectedCategory, setSelectedCategory] = useState("Evidence");
  const [filterCategory, setFilterCategory] = useState<string>("");
  const [searchQuery, setSearchQuery] = useState("");
  const [previewDoc, setPreviewDoc] = useState<{ id: string; name: string; type: string } | null>(null);

  const { data: documents, isLoading } = useQuery({
    queryKey: ["documents", "case", caseId],
    queryFn: () => documentService.byCase(caseId),
    enabled: !!caseId
  });

  const uploadMutation = useMutation({
    mutationFn: async ({ file, category }: { file: File; category: string }) => {
      const form = new FormData();
      form.append("file", file);
      const url = `${API_URL}/documents/upload?caseId=${caseId}&category=${encodeURIComponent(category)}`;
      const token = typeof window !== "undefined" ? localStorage.getItem("verdiq_access_token") : null;
      const resp = await fetch(url, {
        method: "POST",
        headers: token ? { Authorization: `Bearer ${token}` } : {},
        body: form
      });
      if (!resp.ok) throw new Error("Upload failed");
      return resp.json();
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["documents", "case", caseId] });
      qc.invalidateQueries({ queryKey: ["documents"] });
      qc.invalidateQueries({ queryKey: ["case", caseId] });
      toast.success("Document uploaded");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (docId: string) => documentService.remove(docId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["documents", "case", caseId] });
      qc.invalidateQueries({ queryKey: ["case", caseId] });
      toast.success("Document deleted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const handleFilesAdd = (newFiles: File[]) => {
    const items: PendingFile[] = newFiles.map((f) => ({
      file: f,
      id: `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`,
      status: "pending" as const,
      progress: 0
    }));
    setPendingFiles((prev) => [...prev, ...items]);
  };

  const handleFileRemove = (id: string) => {
    setPendingFiles((prev) => prev.filter((f) => f.id !== id));
  };

  const handleUploadAll = async () => {
    const pending = pendingFiles.filter((f) => f.status === "pending");
    for (const pf of pending) {
      setPendingFiles((prev) =>
        prev.map((f) => (f.id === pf.id ? { ...f, status: "uploading" as const, progress: 0 } : f))
      );
      try {
        const form = new FormData();
        form.append("file", pf.file);
        const token = typeof window !== "undefined" ? localStorage.getItem("verdiq_access_token") : null;
        const url = `${API_URL}/documents/upload?caseId=${caseId}&category=${encodeURIComponent(selectedCategory)}`;
        await new Promise<void>((resolve, reject) => {
          const xhr = new XMLHttpRequest();
          xhr.open("POST", url);
          if (token) xhr.setRequestHeader("Authorization", `Bearer ${token}`);
          xhr.upload.onprogress = (e) => {
            if (e.lengthComputable) {
              const pct = Math.round((e.loaded * 100) / e.total);
              setPendingFiles((prev) =>
                prev.map((f) => (f.id === pf.id ? { ...f, progress: pct } : f))
              );
            }
          };
          xhr.onload = () => {
            if (xhr.status >= 200 && xhr.status < 300) resolve();
            else reject(new Error(`Upload failed (${xhr.status})`));
          };
          xhr.onerror = () => reject(new Error("Network error"));
          xhr.send(form);
        });
        setPendingFiles((prev) =>
          prev.map((f) => (f.id === pf.id ? { ...f, status: "done" as const, progress: 100 } : f))
        );
      } catch {
        setPendingFiles((prev) =>
          prev.map((f) => (f.id === pf.id ? { ...f, status: "error" as const, error: "Failed" } : f))
        );
      }
    }
    qc.invalidateQueries({ queryKey: ["documents", "case", caseId] });
    qc.invalidateQueries({ queryKey: ["case", caseId] });
    const doneCount = pendingFiles.filter((f) => f.status === "done" || f.status === "uploading").length;
    if (doneCount > 0) toast.success(`${doneCount} document(s) uploaded`);
    setTimeout(() => setPendingFiles((prev) => prev.filter((f) => f.status !== "done")), 2000);
  };

  const filteredDocs = (documents ?? []).filter((d) => {
    if (filterCategory && d.category !== filterCategory) return false;
    if (searchQuery) {
      const q = searchQuery.toLowerCase();
      return (
        d.originalFileName?.toLowerCase().includes(q) ||
        d.fileName?.toLowerCase().includes(q) ||
        d.category?.toLowerCase().includes(q) ||
        d.tags?.toLowerCase().includes(q)
      );
    }
    return true;
  });

  const groupedDocs = filteredDocs.reduce(
    (acc, d) => {
      const cat = d.category || "Other";
      if (!acc[cat]) acc[cat] = [];
      acc[cat].push(d);
      return acc;
    },
    {} as Record<string, typeof filteredDocs>
  );

  return (
    <>
      <Card>
        <CardHeader
          title="Evidence & Documents"
          description="Upload, organize and manage all case documents. Drag and drop files or click to browse."
          action={
            <Button size="sm" onClick={() => setUploadOpen(true)}>
              <Plus className="h-4 w-4" /> Upload Files
            </Button>
          }
        />
        <CardContent>
          <div className="mb-4 flex flex-wrap items-center gap-3">
            <div className="relative flex-1">
              <Input
                placeholder="Search documents..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="h-9 pl-9"
              />
              <FileText className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
            </div>
            <Select value={filterCategory} onChange={(e) => setFilterCategory(e.target.value)} className="h-9 w-44">
              <option value="">All categories</option>
              {evidenceCategories.map((c) => (
                <option key={c.value} value={c.value}>{c.label}</option>
              ))}
            </Select>
          </div>

          {isLoading ? (
            <Loading />
          ) : filteredDocs.length > 0 ? (
            <div className="space-y-5">
              {Object.entries(groupedDocs).map(([category, docs]) => (
                <div key={category}>
                  <h4 className="mb-2 flex items-center gap-2 text-xs font-semibold uppercase tracking-wide text-ink-muted">
                    <FileText className="h-3.5 w-3.5" />
                    {category}
                    <Badge tone="slate">{docs.length}</Badge>
                  </h4>
                  <div className="space-y-2">
                    {docs.map((d) => (
                      <div
                        key={d.id}
                        className="group flex items-center justify-between gap-4 rounded-lg border border-line p-3 transition-colors hover:bg-slate-50/50"
                      >
                        <button
                          onClick={() => setPreviewDoc({ id: d.id, name: d.originalFileName ?? d.fileName, type: d.fileType })}
                          className="flex min-w-0 flex-1 items-center gap-3 text-left"
                        >
                          <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary-50">
                            <FileText className="h-4 w-4 text-primary-700" />
                          </div>
                          <div className="min-w-0">
                            <p className="truncate text-sm font-medium text-ink group-hover:text-primary-700">
                              {d.originalFileName ?? d.fileName}
                            </p>
                            <p className="text-xs text-ink-muted">
                              {d.fileType?.split("/").pop()?.toUpperCase() ?? "FILE"}
                              {" · "}
                              {d.fileSize ? (d.fileSize > 1048576 ? `${(d.fileSize / 1048576).toFixed(1)} MB` : `${(d.fileSize / 1024).toFixed(0)} KB`) : "—"}
                              {d.version > 1 ? ` · v${d.version}` : ""}
                              {d.viewCount > 0 ? ` · ${d.viewCount} views` : ""}
                            </p>
                          </div>
                        </button>
                        <div className="flex shrink-0 items-center gap-1">
                          <button
                            onClick={() => setPreviewDoc({ id: d.id, name: d.originalFileName ?? d.fileName, type: d.fileType })}
                            className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                            title="Preview"
                          >
                            <FileUp className="h-4 w-4" />
                          </button>
                          <a
                            className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                            aria-label="Download"
                            title="Download"
                            href={`${API_URL}/documents/download/${d.id}`}
                            target="_blank"
                            rel="noreferrer"
                            onClick={(e) => e.stopPropagation()}
                          >
                            <Download className="h-4 w-4" />
                          </a>
                          <button
                            onClick={() => deleteMutation.mutate(d.id)}
                            className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
                            aria-label="Delete"
                          >
                            <Trash2 className="h-4 w-4" />
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState
              title="No documents"
              description="Upload evidence, court orders and correspondence for this case."
              action={
                <Button onClick={() => setUploadOpen(true)}>
                  <Plus className="h-4 w-4" /> Upload Files
                </Button>
              }
            />
          )}
        </CardContent>
      </Card>

      <Dialog
        open={uploadOpen}
        onClose={() => { setUploadOpen(false); setPendingFiles([]); }}
        title="Upload Evidence / Documents"
        description="Drag and drop files or click to browse. All files are stored securely and linked to this case."
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => { setUploadOpen(false); setPendingFiles([]); }}>Cancel</Button>
            <Button
              disabled={pendingFiles.filter((f) => f.status === "pending").length === 0}
              onClick={handleUploadAll}
            >
              <FileUp className="h-4 w-4" /> Upload {pendingFiles.filter((f) => f.status === "pending").length} File(s)
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <Field label="Category">
            <Select value={selectedCategory} onChange={(e) => setSelectedCategory(e.target.value)}>
              {evidenceCategories.map((c) => (
                <option key={c.value} value={c.value}>{c.label}</option>
              ))}
            </Select>
          </Field>
          <FileUploadZone
            files={pendingFiles}
            onFilesAdd={handleFilesAdd}
            onFileRemove={handleFileRemove}
            multiple
            maxFiles={20}
            maxSizeMb={50}
            accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.gif,.txt,.rtf"
          />
        </div>
      </Dialog>

      {previewDoc && (
        <DocumentPreview
          documentId={previewDoc.id}
          fileName={previewDoc.name}
          fileType={previewDoc.type}
          open={!!previewDoc}
          onClose={() => setPreviewDoc(null)}
        />
      )}
    </>
  );
}

function ScheduleHearingDialog({
  open,
  onClose,
  caseId,
  caseNumber,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  caseId: string;
  caseNumber: string;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({
    hearingDate: "",
    hearingTime: "10:00",
    courtroom: "",
    judgeName: "",
    notes: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Schedule Hearing"
      description={`${caseNumber} — ${new Date().toISOString().slice(0, 10)}`}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.hearingDate}
            onClick={() =>
              onSubmit({
                caseId,
                hearingDate: new Date(`${form.hearingDate}T${form.hearingTime}`).toISOString(),
                courtroom: form.courtroom || null,
                judgeName: form.judgeName || null,
                notes: form.notes || null,
                status: "Scheduled"
              })
            }
          >
            Schedule
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Hearing Date" required>
          <Input type="date" value={form.hearingDate} onChange={(e) => setForm({ ...form, hearingDate: e.target.value })} />
        </Field>
        <Field label="Hearing Time">
          <Input type="time" value={form.hearingTime} onChange={(e) => setForm({ ...form, hearingTime: e.target.value })} />
        </Field>
        <Field label="Courtroom">
          <Input value={form.courtroom} onChange={(e) => setForm({ ...form, courtroom: e.target.value })} />
        </Field>
        <Field label="Judge">
          <Input value={form.judgeName} onChange={(e) => setForm({ ...form, judgeName: e.target.value })} />
        </Field>
        <Field label="Notes" className="sm:col-span-2">
          <Textarea rows={3} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}

function JudgmentsTab({ caseId, caseNumber }: { caseId: string; caseNumber: string }) {
  const toast = useToast();
  const qc = useQueryClient();
  const [recordOpen, setRecordOpen] = useState(false);
  const [deleting, setDeleting] = useState<Judgment | null>(null);

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["judgments", caseId] });
    qc.invalidateQueries({ queryKey: ["case", caseId] });
  };

  const { data: judgments, isLoading } = useQuery({
    queryKey: ["judgments", caseId],
    queryFn: () => judgmentService.byCase(caseId),
    enabled: !!caseId
  });

  const createMutation = useMutation({
    mutationFn: (input: CreateJudgmentInput) => judgmentService.create(caseId, input),
    onSuccess: () => {
      invalidate();
      setRecordOpen(false);
      toast.success("Judgment recorded");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => judgmentService.remove(caseId, id),
    onSuccess: () => {
      invalidate();
      setDeleting(null);
      toast.success("Judgment deleted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const uploadMutation = useMutation({
    mutationFn: ({ judgmentId, file }: { judgmentId: string; file: File }) =>
      judgmentService.uploadDocument(caseId, judgmentId, file),
    onSuccess: () => {
      invalidate();
      toast.success("Document attached to judgment");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const handleDownload = async (j: Judgment) => {
    try {
      const blob = await judgmentService.downloadDocument(caseId, j.id);
      downloadBlob(blob, j.originalFileName ?? `judgment-${j.caption}.pdf`);
    } catch (e) {
      toast.error(getErrorMessage(e));
    }
  };

  const handleExport = async (format: "pdf" | "csv") => {
    try {
      const blob = await judgmentService.exportData(caseId, format);
      const stamp = new Date().toISOString().slice(0, 10).replace(/-/g, "");
      downloadBlob(blob, `judgments-${caseNumber}-${stamp}.${format}`);
    } catch (e) {
      toast.error(getErrorMessage(e));
    }
  };

  return (
    <>
      <Card>
        <CardHeader
          title="Judgments"
          description={`Recorded judgments and orders for ${caseNumber}. Export the full history as PDF or Excel (CSV).`}
          action={
            <div className="flex flex-wrap items-center gap-2">
              <Button size="sm" variant="outline" onClick={() => handleExport("pdf")}>
                <FileText className="h-4 w-4" /> Export PDF
              </Button>
              <Button size="sm" variant="outline" onClick={() => handleExport("csv")}>
                <Download className="h-4 w-4" /> Export Excel
              </Button>
              <Button size="sm" onClick={() => setRecordOpen(true)}>
                <Plus className="h-4 w-4" /> Record Judgment
              </Button>
            </div>
          }
        />
        <CardContent className="space-y-3">
          {isLoading ? (
            <Loading />
          ) : judgments && judgments.length > 0 ? (
            judgments.map((j) => (
              <div key={j.id} className="rounded-lg border border-line p-3">
                <div className="flex items-start justify-between gap-4">
                  <div className="min-w-0">
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="text-sm font-semibold text-ink">{j.caption}</p>
                      {j.result ? (
                        <Badge tone={/dismiss|reject|rejected|dismissed/.test(j.result.toLowerCase()) ? "red" : "green"}>{j.result}</Badge>
                      ) : null}
                    </div>
                    <p className="mt-0.5 text-xs text-ink-muted">
                      {formatDate(j.judgmentDate)}
                      {j.recordedByName ? ` · Recorded by ${j.recordedByName}` : ""}
                      {j.nextHearingDate ? ` · Next hearing ${formatDate(j.nextHearingDate)}` : ""}
                    </p>
                    {j.keyFindings ? <p className="mt-1 text-xs text-ink-muted">{j.keyFindings}</p> : null}
                    {j.summary ? <p className="mt-1 text-sm text-ink">{j.summary}</p> : null}
                  </div>
                  <div className="flex shrink-0 items-center gap-1">
                    <label
                      className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                      title="Attach document"
                    >
                      <Upload className="h-4 w-4" />
                      <input
                        type="file"
                        className="hidden"
                        accept=".pdf,.doc,.docx,.txt"
                        onChange={(e) => {
                          const f = e.target.files?.[0];
                          if (f) uploadMutation.mutate({ judgmentId: j.id, file: f });
                          e.target.value = "";
                        }}
                      />
                    </label>
                    {j.hasDocument ? (
                      <button
                        onClick={() => handleDownload(j)}
                        className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                        title={j.originalFileName ?? "Download document"}
                      >
                        <Download className="h-4 w-4" />
                      </button>
                    ) : null}
                    <button
                      onClick={() => setDeleting(j)}
                      className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
                      title="Delete judgment"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </div>
                {j.originalFileName ? (
                  <p className="mt-2 inline-flex items-center gap-1.5 rounded-md bg-slate-50 px-2 py-1 text-xs text-ink-muted">
                    <FileText className="h-3 w-3" />
                    {j.originalFileName}
                    {j.fileSize ? ` · ${(j.fileSize / 1024).toFixed(0)} KB` : ""}
                  </p>
                ) : null}
              </div>
            ))
          ) : (
            <EmptyState
              title="No judgments recorded"
              description="Record the outcome of each hearing so you can export a full judgment history."
              action={
                <Button onClick={() => setRecordOpen(true)}>
                  <Plus className="h-4 w-4" /> Record Judgment
                </Button>
              }
            />
          )}
        </CardContent>
      </Card>

      {recordOpen ? (
        <RecordJudgmentDialog
          onClose={() => setRecordOpen(false)}
          onSubmit={(input) => createMutation.mutate(input)}
        />
      ) : null}

      <Dialog
        open={!!deleting}
        onClose={() => setDeleting(null)}
        title="Delete judgment"
        description={deleting ? `"${deleting.caption}" will be permanently removed.` : ""}
        footer={
          <>
            <Button variant="ghost" onClick={() => setDeleting(null)}>Cancel</Button>
            <Button variant="danger" disabled={deleteMutation.isPending} onClick={() => deleting && deleteMutation.mutate(deleting.id)}>
              <Trash2 className="h-4 w-4" /> Delete Judgment
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">This action cannot be undone.</p>
      </Dialog>
    </>
  );
}

function RecordJudgmentDialog({
  onClose,
  onSubmit
}: {
  onClose: () => void;
  onSubmit: (input: CreateJudgmentInput) => void;
}) {
  const [form, setForm] = useState({
    caption: "",
    result: "",
    judgmentDate: new Date().toISOString().slice(0, 10),
    nextHearingDate: "",
    keyFindings: "",
    summary: ""
  });

  return (
    <Dialog
      open
      onClose={onClose}
      title="Record Judgment"
      description="Record the court's order, its outcome and any next hearing date."
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.caption}
            onClick={() =>
              onSubmit({
                caption: form.caption,
                result: form.result || null,
                judgmentDate: form.judgmentDate ? new Date(`${form.judgmentDate}T10:00`).toISOString() : null,
                nextHearingDate: form.nextHearingDate ? new Date(`${form.nextHearingDate}T10:00`).toISOString() : null,
                keyFindings: form.keyFindings || null,
                summary: form.summary || null
              })
            }
          >
            Save Judgment
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Caption" required className="sm:col-span-2">
          <Input
            value={form.caption}
            onChange={(e) => setForm({ ...form, caption: e.target.value })}
            placeholder="e.g. Final judgment — suit decreed in favour of the plaintiff"
          />
        </Field>
        <Field label="Result">
          <Input
            value={form.result}
            onChange={(e) => setForm({ ...form, result: e.target.value })}
            placeholder="e.g. Decree granted"
          />
        </Field>
        <Field label="Judgment Date">
          <Input type="date" value={form.judgmentDate} onChange={(e) => setForm({ ...form, judgmentDate: e.target.value })} />
        </Field>
        <Field label="Next Hearing Date" className="sm:col-span-2">
          <Input type="date" value={form.nextHearingDate} onChange={(e) => setForm({ ...form, nextHearingDate: e.target.value })} />
        </Field>
        <Field label="Key Findings" className="sm:col-span-2">
          <Textarea
            rows={3}
            value={form.keyFindings}
            onChange={(e) => setForm({ ...form, keyFindings: e.target.value })}
            placeholder="Summary of the court's reasoning and findings"
          />
        </Field>
        <Field label="Summary" className="sm:col-span-2">
          <Textarea rows={3} value={form.summary} onChange={(e) => setForm({ ...form, summary: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}

function usePhotoBlobUrls(caseId: string, photos: CasePhoto[]) {
  const [urls, setUrls] = useState<Record<string, string>>({});
  useEffect(() => {
    let alive = true;
    const created: string[] = [];
    (async () => {
      const next: Record<string, string> = {};
      for (const p of photos) {
        try {
          const blob = await casePhotoService.download(caseId, p.id);
          const u = URL.createObjectURL(blob);
          created.push(u);
          next[p.id] = u;
        } catch {
          /* unreadable photo — leave placeholder */
        }
      }
      if (alive) setUrls(next);
      else created.forEach((u) => URL.revokeObjectURL(u));
    })();
    return () => {
      alive = false;
      created.forEach((u) => URL.revokeObjectURL(u));
    };
  }, [caseId, photos]);
  return urls;
}

let pendingPhotoFile: File | null = null;
let pendingPhotoCaption = "";

function PhotosTab({ caseId }: { caseId: string }) {
  const toast = useToast();
  const qc = useQueryClient();
  const [uploadOpen, setUploadOpen] = useState(false);
  const [viewing, setViewing] = useState<CasePhoto | null>(null);

  const { data: photos, isLoading } = useQuery({
    queryKey: ["photos", caseId],
    queryFn: () => casePhotoService.byCase(caseId),
    enabled: !!caseId
  });
  const urls = usePhotoBlobUrls(caseId, photos ?? []);

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["photos", caseId] });
    qc.invalidateQueries({ queryKey: ["case", caseId] });
  };

  const uploadMutation = useMutation({
    mutationFn: ({ file, caption }: { file: File; caption: string }) =>
      casePhotoService.upload(caseId, file, caption || undefined),
    onSuccess: () => {
      invalidate();
      setUploadOpen(false);
      toast.success("Photo uploaded");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (photoId: string) => casePhotoService.remove(caseId, photoId),
    onSuccess: () => {
      invalidate();
      toast.success("Photo deleted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const handleDownload = async (p: CasePhoto) => {
    try {
      const blob = await casePhotoService.download(caseId, p.id);
      downloadBlob(blob, p.originalFileName);
    } catch (e) {
      toast.error(getErrorMessage(e));
    }
  };

  return (
    <>
      <Card>
        <CardHeader
          title="Case Photos"
          description="Evidence photos, document scans and scene images attached to this case."
          action={
            <Button size="sm" onClick={() => setUploadOpen(true)}>
              <FileUp className="h-4 w-4" /> Add Photo
            </Button>
          }
        />
        <CardContent>
          {isLoading ? (
            <Loading />
          ) : photos && photos.length > 0 ? (
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
              {photos.map((p) => (
                <div key={p.id} className="group overflow-hidden rounded-lg border border-line">
                  <button onClick={() => setViewing(p)} className="block w-full cursor-zoom-in">
                    {urls[p.id] ? (
                      <img
                        src={urls[p.id]}
                        alt={p.caption ?? p.originalFileName}
                        className="h-36 w-full object-cover transition-transform duration-200 group-hover:scale-105"
                      />
                    ) : (
                      <div className="flex h-36 w-full items-center justify-center bg-slate-100">
                        <ImageIcon className="h-6 w-6 text-ink-soft" />
                      </div>
                    )}
                  </button>
                  <div className="flex items-center justify-between gap-2 px-2.5 py-2">
                    <p className="truncate text-xs text-ink-muted">{p.caption ?? p.originalFileName}</p>
                    <button
                      onClick={() => deleteMutation.mutate(p.id)}
                      className="shrink-0 cursor-pointer rounded p-1 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
                      title="Delete photo"
                    >
                      <Trash2 className="h-3.5 w-3.5" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState
              title="No photos"
              description="Add photos of documents, evidence or scene visits for this case."
              action={
                <Button onClick={() => setUploadOpen(true)}>
                  <Plus className="h-4 w-4" /> Add Photo
                </Button>
              }
            />
          )}
        </CardContent>
      </Card>

      <Dialog
        open={uploadOpen}
        onClose={() => setUploadOpen(false)}
        title="Add Photo"
        description="Attach a photo of a document, evidence or scene."
        footer={
          <>
            <Button variant="ghost" onClick={() => setUploadOpen(false)}>Cancel</Button>
            <Button
              disabled={!pendingPhotoFile || uploadMutation.isPending}
              onClick={() =>
                pendingPhotoFile &&
                uploadMutation.mutate({ file: pendingPhotoFile, caption: pendingPhotoCaption })
              }
            >
              <FileUp className="h-4 w-4" /> Upload Photo
            </Button>
          </>
        }
      >
        <PhotoUploadForm
          onReady={(file, caption) => {
            pendingPhotoFile = file;
            pendingPhotoCaption = caption;
          }}
        />
      </Dialog>

      <Dialog
        open={!!viewing}
        onClose={() => setViewing(null)}
        title={viewing?.caption ?? "Photo"}
        description={
          viewing
            ? `${viewing.originalFileName} · ${formatDateTime(viewing.capturedAt)}${viewing.uploadedByName ? ` · by ${viewing.uploadedByName}` : ""}`
            : ""
        }
        size="lg"
        footer={
          <>
            {viewing ? (
              <Button variant="outline" onClick={() => handleDownload(viewing)}>
                <Download className="h-4 w-4" /> Download
              </Button>
            ) : null}
            <Button variant="ghost" onClick={() => setViewing(null)}>Close</Button>
          </>
        }
      >
        {viewing && urls[viewing.id] ? (
          <img
            src={urls[viewing.id]}
            alt={viewing.caption ?? viewing.originalFileName}
            className="max-h-[70vh] w-full rounded-lg object-contain"
          />
        ) : null}
      </Dialog>
    </>
  );
}

function PhotoUploadForm({ onReady }: { onReady: (file: File, caption: string) => void }) {
  const [file, setFile] = useState<File | null>(null);
  const [caption, setCaption] = useState("");

  const update = (f: File | null, c: string) => {
    setFile(f);
    setCaption(c);
    if (f) onReady(f, c);
  };

  return (
    <div className="space-y-4">
      <Field label="Photo" required>
        <input
          type="file"
          accept="image/*"
          className="w-full text-sm text-ink file:mr-4 file:cursor-pointer file:rounded-lg file:border-0 file:bg-primary-50 file:px-4 file:py-2 file:text-sm file:font-medium file:text-primary-800 file:transition-colors hover:file:bg-primary-100"
          onChange={(e) => update(e.target.files?.[0] ?? null, caption)}
        />
      </Field>
      <Field label="Caption">
        <Input
          value={caption}
          onChange={(e) => update(file, e.target.value)}
          placeholder="e.g. Evidence exhibit A"
        />
      </Field>
      {!file ? <p className="text-xs text-ink-muted">Choose an image to enable upload.</p> : null}
    </div>
  );
}

function useCaseWorkflowList(caseId: string) {
  return useQuery({
    queryKey: ["case", caseId, "workflows"],
    queryFn: () => caseWorkflows.byCase(caseId),
    enabled: !!caseId
  });
}

function ProgressBar({ percent, tone }: { percent: number; tone?: "gold" | "green" | "red" }) {
  const fill =
    tone === "green" ? "bg-emerald-500" : tone === "red" ? "bg-red-500" : tone === "gold" ? "bg-gold-500" : "bg-primary-600";
  return (
    <div className="h-2 w-full overflow-hidden rounded-full bg-slate-100">
      <div className={`h-full rounded-full transition-all duration-300 ${fill}`} style={{ width: `${Math.min(100, Math.max(0, percent))}%` }} />
    </div>
  );
}

function WorkflowOverviewCard({ caseId, onOpen }: { caseId: string; onOpen?: () => void }) {
  const { data: workflows } = useCaseWorkflowList(caseId);
  const running = (workflows ?? []).filter((w) => w.status === "InProgress");

  if (!workflows || workflows.length === 0) return null;

  return (
    <Card>
      <CardHeader
        title={
          <button onClick={onOpen} className="inline-flex cursor-pointer items-center gap-2 text-left">
            <WorkflowIcon className="h-4 w-4 text-primary-700" /> Workflows
          </button>
        }
        action={running.length > 0 ? <Badge tone="amber">{running.length} running</Badge> : <Badge tone="green">All done</Badge>}
      />
      <CardContent className="space-y-4">
        {workflows.map((w) => (
          <div key={w.id}>
            <div className="mb-1.5 flex items-center justify-between gap-2">
              <p className="truncate text-sm font-medium text-ink">{w.workflowName}</p>
              <span className={`text-xs font-medium ${w.isOverdue ? "text-red-600" : "text-ink-muted"}`}>
                {w.percentComplete}%
              </span>
            </div>
            <ProgressBar percent={w.percentComplete} tone={w.isOverdue ? "red" : w.status === "Completed" ? "green" : undefined} />
          </div>
        ))}
      </CardContent>
    </Card>
  );
}

function WorkflowsTab({ caseId }: { caseId: string }) {
  const toast = useToast();
  const qc = useQueryClient();
  const [linkOpen, setLinkOpen] = useState(false);
  const [completing, setCompleting] = useState<{ workflowId: string; step: CaseWorkflowStep } | null>(null);

  const { data: workflows, isLoading } = useCaseWorkflowList(caseId);
  const { data: presets } = useQuery({
    queryKey: ["workflows"],
    queryFn: () => caseWorkflows.list()
  });

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["case", caseId, "workflows"] });
    qc.invalidateQueries({ queryKey: ["case", caseId] });
    qc.invalidateQueries({ queryKey: ["case", caseId, "activities"] });
  };

  const linkMutation = useMutation({
    mutationFn: (workflowId: string) => caseWorkflows.link(caseId, workflowId),
    onSuccess: () => {
      invalidate();
      setLinkOpen(false);
      toast.success("Workflow linked to case");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const startMutation = useMutation({
    mutationFn: ({ workflowId, stepId }: { workflowId: string; stepId: string }) =>
      caseWorkflows.startStep(caseId, workflowId, stepId),
    onSuccess: () => {
      invalidate();
      toast.success("Step started");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const completeMutation = useMutation({
    mutationFn: ({ workflowId, stepId, notes }: { workflowId: string; stepId: string; notes?: string }) =>
      caseWorkflows.completeStep(caseId, workflowId, stepId, notes),
    onSuccess: () => {
      invalidate();
      setCompleting(null);
      toast.success("Step completed");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const cancelMutation = useMutation({
    mutationFn: (workflowId: string) => caseWorkflows.cancel(caseId, workflowId),
    onSuccess: () => {
      invalidate();
      toast.success("Workflow cancelled");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const unlinkMutation = useMutation({
    mutationFn: (workflowId: string) => caseWorkflows.unlink(caseId, workflowId),
    onSuccess: () => {
      invalidate();
      toast.success("Workflow removed from case");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const activePresets = (presets ?? []).filter((p) => p.isActive && !(workflows ?? []).some((w) => w.workflowId === p.id));

  return (
    <>
      <Card>
        <CardHeader
          title="Case Workflow"
          description="Link a process to this case. Steps unlock in order — complete the current step to unlock the next one."
          action={
            <Button size="sm" onClick={() => setLinkOpen(true)}>
              <Link2 className="h-4 w-4" /> Link Workflow
            </Button>
          }
        />
        <CardContent>
          {isLoading ? (
            <Loading />
          ) : workflows && workflows.length > 0 ? (
            <div className="space-y-5">
              {workflows.map((w) => (
                <WorkflowCard
                  key={w.id}
                  workflow={w}
                  onStart={(stepId) => startMutation.mutate({ workflowId: w.id, stepId })}
                  onComplete={(step) => setCompleting({ workflowId: w.id, step })}
                  onCancel={() => cancelMutation.mutate(w.id)}
                  onUnlink={() => unlinkMutation.mutate(w.id)}
                />
              ))}
            </div>
          ) : (
            <EmptyState
              icon={<WorkflowIcon className="h-10 w-10" />}
              title="No workflow linked"
              description="Link a workflow (e.g. a bail hearing process) to run its steps in sequence on this case."
              action={<Button onClick={() => setLinkOpen(true)}><Link2 className="h-4 w-4" /> Link Workflow</Button>}
            />
          )}
        </CardContent>
      </Card>

      <Dialog
        open={linkOpen}
        onClose={() => setLinkOpen(false)}
        title="Link a Workflow"
        description="Choose a workflow to attach. Its steps are copied into this case with due dates."
        footer={
          <>
            <Button variant="ghost" onClick={() => setLinkOpen(false)}>Cancel</Button>
          </>
        }
      >
        {activePresets.length > 0 ? (
          <div className="space-y-2">
            {activePresets.map((p: Workflow) => (
              <button
                key={p.id}
                onClick={() => linkMutation.mutate(p.id)}
                className="flex w-full cursor-pointer items-start justify-between gap-3 rounded-lg border border-line p-3 text-left transition-colors hover:bg-slate-50"
              >
                <div>
                  <p className="text-sm font-medium text-ink">{p.name}</p>
                  {p.description ? <p className="mt-0.5 text-xs text-ink-muted">{p.description}</p> : null}
                </div>
                <Badge tone="primary">{p.stepCount} steps</Badge>
              </button>
            ))}
          </div>
        ) : (
          <EmptyState
            title="No workflows available"
            description="All active workflows are already linked to this case, or you have not created any yet."
            action={<a className="text-sm font-medium text-primary-700 hover:underline" href="/lawyer/workflows">Create a workflow</a>}
          />
        )}
      </Dialog>

      <Dialog
        open={!!completing}
        onClose={() => setCompleting(null)}
        title="Complete step"
        description={completing ? `"${completing.step.title}" — record a short note (optional).` : ""}
        footer={
          <>
            <Button variant="ghost" onClick={() => setCompleting(null)}>Cancel</Button>
            <Button
              disabled={completeMutation.isPending}
              onClick={() => {
                if (!completing) return;
                const notes = (document.getElementById("step-note") as HTMLInputElement | null)?.value;
                completeMutation.mutate({ workflowId: completing.workflowId, stepId: completing.step.id, notes });
              }}
            >
              <CheckCircle2 className="h-4 w-4" /> Complete
            </Button>
          </>
        }
      >
        <Field label="Note">
          <Textarea id="step-note" rows={3} placeholder="e.g. Bail petition filed in court" />
        </Field>
      </Dialog>
    </>
  );
}

function WorkflowCard({
  workflow,
  onStart,
  onComplete,
  onCancel,
  onUnlink
}: {
  workflow: CaseWorkflow;
  onStart: (stepId: string) => void;
  onComplete: (step: CaseWorkflowStep) => void;
  onCancel: () => void;
  onUnlink: () => void;
}) {
  const [confirmCancel, setConfirmCancel] = useState(false);

  return (
    <>
    <Card>
      <CardHeader
        title={
          <span className="flex items-center gap-2">
            <WorkflowIcon className="h-4 w-4 text-primary-700" />
            {workflow.workflowName}
            {workflow.isOverdue ? (
              <Badge tone="red"><AlertCircle className="h-3 w-3" /> Overdue</Badge>
            ) : null}
          </span>
        }
        description={
          workflow.workflowDescription ??
          `${workflow.completedStepCount}/${workflow.stepCount} steps done · Started ${formatDate(workflow.startedAt)}${workflow.startedByName ? ` by ${workflow.startedByName}` : ""}`
        }
        action={
          <div className="flex shrink-0 items-center gap-1">
            <StatusBadge value={workflow.status} />
            {workflow.status === "InProgress" ? (
              <button
                onClick={() => setConfirmCancel(true)}
                className="cursor-pointer rounded-lg p-2 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
                title="Cancel workflow"
              >
                <Ban className="h-4 w-4" />
              </button>
            ) : null}
            <button
              onClick={onUnlink}
              className="cursor-pointer rounded-lg p-2 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
              title="Remove from case"
            >
              <Trash2 className="h-4 w-4" />
            </button>
          </div>
        }
      />
      <CardContent className="space-y-4">
        <div>
          <div className="mb-1.5 flex items-center justify-between text-xs">
            <span className="text-ink-muted">
              {workflow.status === "Completed"
                ? "Workflow completed"
                : workflow.nextStepTitle
                  ? `Next: ${workflow.nextStepTitle}`
                  : "All steps complete"}
            </span>
            <span className={`font-medium ${workflow.isOverdue ? "text-red-600" : "text-ink"}`}>{workflow.percentComplete}%</span>
          </div>
          <ProgressBar percent={workflow.percentComplete} tone={workflow.isOverdue ? "red" : workflow.status === "Completed" ? "green" : undefined} />
        </div>

        <ol className="space-y-2">
          {workflow.steps.map((s, i) => (
            <li
              key={s.id}
              className={`flex items-start gap-3 rounded-lg border p-3 ${
                s.isActive ? "border-primary-200 bg-primary-50/40" : s.isCompleted ? "border-line bg-slate-50/50" : "border-line opacity-70"
              }`}
            >
              <span
                className={`mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-xs font-semibold ${
                  s.isCompleted
                    ? "bg-emerald-100 text-emerald-700"
                    : s.isActive
                      ? "bg-primary-700 text-white"
                      : "bg-slate-100 text-slate-500"
                }`}
              >
                {s.isCompleted ? <CheckCircle2 className="h-4 w-4" /> : i + 1}
              </span>
              <div className="min-w-0 flex-1">
                <p className={`text-sm font-medium ${s.isCompleted ? "text-ink-muted line-through" : "text-ink"}`}>{s.title}</p>
                {s.description ? <p className="mt-0.5 text-xs text-ink-muted">{s.description}</p> : null}
                <div className="mt-1 flex flex-wrap items-center gap-x-3 gap-y-1 text-xs text-ink-muted">
                  {s.dueDate ? (
                    <span className={s.isOverdue ? "font-medium text-red-600" : ""}>
                      Due {formatDate(s.dueDate)}
                      {s.isOverdue ? " · overdue" : ""}
                    </span>
                  ) : null}
                  {s.isCompleted && s.completedByName ? (
                    <span>Completed by {s.completedByName}{s.completedAt ? ` · ${formatDate(s.completedAt)}` : ""}</span>
                  ) : null}
                  {s.notes ? <span className="italic">“{s.notes}”</span> : null}
                </div>
              </div>
              <div className="flex shrink-0 items-center gap-1">
                {s.isCompleted ? (
                  <Badge tone="green">Done</Badge>
                ) : s.isActive ? (
                  <>
                    <Button size="sm" variant="subtle" onClick={() => onStart(s.id)}>
                      <Play className="h-3.5 w-3.5" /> {s.status === "InProgress" ? "Started" : "Start"}
                    </Button>
                    <Button size="sm" variant="gold" onClick={() => onComplete(s)}>
                      <CheckCircle2 className="h-3.5 w-3.5" /> Complete
                    </Button>
                  </>
                ) : (
                  <Badge tone="slate">
                    <Lock className="h-3 w-3" /> Locked
                  </Badge>
                )}
              </div>
            </li>
          ))}
        </ol>
      </CardContent>
    </Card>

    <Dialog
        open={confirmCancel}
        onClose={() => setConfirmCancel(false)}
        title="Cancel workflow"
        description="The workflow will be marked cancelled and no further steps can be completed."
        footer={
          <>
            <Button variant="ghost" onClick={() => setConfirmCancel(false)}>Keep running</Button>
            <Button variant="danger" onClick={onCancel}>
              <Ban className="h-4 w-4" /> Cancel Workflow
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">You can still remove it from the case afterwards.</p>
      </Dialog>
    </>
  );
}

function ActivityTab({ activities }: { activities: { id: string; activityType: string; description: string; createdBy: string; createdByName?: string; createdAt: string; isClientVisible?: boolean }[] }) {
  const [filter, setFilter] = useState<string>("all");

  const activityIconMap: Record<string, React.ReactNode> = {
    CaseCreated: <Plus className="h-4 w-4 text-blue-500" />,
    CaseUpdated: <PenLine className="h-4 w-4 text-amber-500" />,
    HearingScheduled: <CalendarClock className="h-4 w-4 text-purple-500" />,
    HearingCompleted: <CheckCircle2 className="h-4 w-4 text-emerald-500" />,
    DocumentUploaded: <FileUp className="h-4 w-4 text-cyan-500" />,
    ProcedureCompleted: <ListChecks className="h-4 w-4 text-emerald-500" />,
    JudgmentAdded: <Gavel className="h-4 w-4 text-red-500" />,
    NotesUpdated: <PenLine className="h-4 w-4 text-ink-soft" />,
    StatusChanged: <AlertCircle className="h-4 w-4 text-orange-500" />,
  };

  const activityTypes = ["all", ...new Set(activities.map((a) => a.activityType))];
  const filtered = filter === "all" ? activities : activities.filter((a) => a.activityType === filter);

  const getActivityLabel = (type: string) => {
    const labels: Record<string, string> = {
      CaseCreated: "Case Created",
      CaseUpdated: "Case Updated",
      HearingScheduled: "Hearing Scheduled",
      HearingCompleted: "Hearing Completed",
      DocumentUploaded: "Document Uploaded",
      ProcedureCompleted: "Procedure Completed",
      JudgmentAdded: "Judgment Added",
      NotesUpdated: "Notes Updated",
      StatusChanged: "Status Changed"
    };
    return labels[type] ?? type.replace(/([A-Z])/g, " $1").trim();
  };

  return (
    <Card>
      <CardHeader title="Case Activity" description="Complete log of all changes and actions on this case." />
      <CardContent>
        {activities.length > 0 ? (
          <>
            {activityTypes.length > 2 ? (
              <div className="mb-4 flex flex-wrap gap-1.5">
                {activityTypes.map((t) => (
                  <Button key={t} size="sm" variant={filter === t ? "subtle" : "ghost"} onClick={() => setFilter(t)}>
                    {t === "all" ? "All" : getActivityLabel(t)}
                  </Button>
                ))}
              </div>
            ) : null}

            <div className="relative space-y-0">
              <div className="absolute left-[19px] top-2 bottom-2 w-px bg-line" />
              {filtered.map((a) => (
                <div key={a.id} className="relative flex gap-3 py-3">
                  <div className="relative z-10 flex h-10 w-10 shrink-0 items-center justify-center rounded-full border border-line bg-white">
                    {activityIconMap[a.activityType] ?? <Gavel className="h-4 w-4 text-ink-soft" />}
                  </div>
                  <div className="min-w-0 flex-1 pt-1">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-medium text-ink">{getActivityLabel(a.activityType)}</span>
                      {a.isClientVisible ? <Badge tone="blue" className="text-[10px]">Client visible</Badge> : null}
                    </div>
                    {a.description && a.description !== a.activityType ? (
                      <p className="mt-0.5 text-sm text-ink-muted">{a.description}</p>
                    ) : null}
                    <p className="mt-1 text-xs text-ink-muted">
                      {a.createdByName ?? a.createdBy} · {formatDateTime(a.createdAt)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </>
        ) : (
          <EmptyState
            icon={<ListChecks className="h-10 w-10" />}
            title="No activity logged"
            description="Actions on this case — scheduling hearings, uploading documents, updating status — will appear here."
          />
        )}
      </CardContent>
    </Card>
  );
}

function ProceduresTab({ caseId, legalSections }: { caseId: string; legalSections: { id: string; legalSectionId: string; sectionCode: string; sectionTitle: string; lawName: string; procedures: { id: string; procedureTitle: string; stepNumber: number; description: string | null; requiredDocuments: string | null; recommendedTimeline: string | null; responsibleRole: string | null; isMandatory: boolean; isCompleted: boolean; completedAt: string | null; completedBy: string | null; notes: string | null }[] }[] }) {
  const toast = useToast();
  const qc = useQueryClient();
  const [genOpen, setGenOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [expanded, setExpanded] = useState<Record<string, boolean>>(() => {
    const init: Record<string, boolean> = {};
    legalSections.forEach((s) => { init[s.id] = true; });
    return init;
  });

  const { data: allSections, isLoading: sectionsLoading } = useQuery({
    queryKey: ["legal-sections"],
    queryFn: () => legalSectionService.list(),
    enabled: genOpen
  });

  const generateMutation = useMutation({
    mutationFn: (legalSectionId: string) => caseService.generateProcedures(caseId, legalSectionId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["case", caseId] });
      toast.success("Procedures generated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const completeProcedure = useMutation({
    mutationFn: (procedureId: string) => caseService.completeProcedure(caseId, procedureId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["case", caseId] });
      toast.success("Procedure completed");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const linkedSectionIds = new Set(legalSections.map((s) => s.legalSectionId));
  const availableSections = (allSections ?? []).filter(
    (s) => !linkedSectionIds.has(s.id) && (search === "" || s.sectionCode.toLowerCase().includes(search.toLowerCase()) || s.sectionTitle.toLowerCase().includes(search.toLowerCase()) || s.lawName.toLowerCase().includes(search.toLowerCase()))
  );

  const totalProcedures = legalSections.reduce((sum, s) => sum + s.procedures.length, 0);
  const completedProcedures = legalSections.reduce((sum, s) => sum + s.procedures.filter((p) => p.isCompleted).length, 0);
  const progress = totalProcedures > 0 ? Math.round((completedProcedures / totalProcedures) * 100) : 0;

  const toggleSection = (id: string) => setExpanded((prev) => ({ ...prev, [id]: !prev[id] }));

  return (
    <>
      <Card>
        <CardHeader
          title="Legal Procedures"
          description="Procedures linked from the Legal Database, grouped by section."
          action={
            <Button size="sm" onClick={() => setGenOpen(true)}>
              <Plus className="h-4 w-4" /> Generate from Legal DB
            </Button>
          }
        />
        <CardContent>
          {totalProcedures > 0 ? (
            <div className="mb-5">
              <div className="flex items-center justify-between text-xs text-ink-muted">
                <span>{completedProcedures} of {totalProcedures} completed</span>
                <span>{progress}%</span>
              </div>
              <div className="mt-1.5 h-2 w-full overflow-hidden rounded-full bg-slate-100">
                <div className="h-full rounded-full bg-emerald-500 transition-all duration-300" style={{ width: `${progress}%` }} />
              </div>
            </div>
          ) : null}

          {legalSections.length > 0 ? (
            <div className="space-y-4">
              {legalSections.map((section) => {
                const sectionDone = section.procedures.filter((p) => p.isCompleted).length;
                const isOpen = expanded[section.id] !== false;
                return (
                  <div key={section.id} className="rounded-lg border border-line">
                    <button
                      onClick={() => toggleSection(section.id)}
                      className="flex w-full cursor-pointer items-center gap-3 p-3 text-left hover:bg-slate-50/60"
                    >
                      {isOpen ? <ChevronDown className="h-4 w-4 shrink-0 text-ink-muted" /> : <ChevronRight className="h-4 w-4 shrink-0 text-ink-muted" />}
                      <div className="min-w-0 flex-1">
                        <div className="flex items-center gap-2">
                          <span className="font-mono text-xs font-bold text-primary-700">{section.sectionCode}</span>
                          <span className="text-sm font-medium text-ink">{section.sectionTitle}</span>
                        </div>
                        <p className="text-xs text-ink-muted">{section.lawName}</p>
                      </div>
                      <span className="shrink-0 text-xs text-ink-muted">{sectionDone}/{section.procedures.length}</span>
                    </button>
                    {isOpen ? (
                      <div className="space-y-1 border-t border-line px-3 py-2">
                        {section.procedures.sort((a, b) => a.stepNumber - b.stepNumber).map((proc) => (
                          <div key={proc.id} className="flex items-start gap-3 rounded-md px-2 py-2 hover:bg-slate-50">
                            <button
                              onClick={() => !proc.isCompleted && completeProcedure.mutate(proc.id)}
                              className="mt-0.5 shrink-0 cursor-pointer"
                              disabled={proc.isCompleted}
                            >
                              <CheckCircle2 className={proc.isCompleted ? "h-5 w-5 text-emerald-500" : "h-5 w-5 text-slate-300 hover:text-emerald-400"} />
                            </button>
                            <div className="min-w-0 flex-1">
                              <div className="flex items-center gap-2">
                                <span className="text-xs font-mono text-ink-muted">#{proc.stepNumber}</span>
                                <p className={`text-sm font-medium ${proc.isCompleted ? "text-ink-muted line-through" : "text-ink"}`}>{proc.procedureTitle}</p>
                                {proc.isMandatory && !proc.isCompleted ? <Badge tone="red" className="text-[10px]">Required</Badge> : null}
                              </div>
                              {proc.description ? <p className="mt-0.5 text-xs text-ink-muted line-clamp-2">{proc.description}</p> : null}
                              <div className="mt-1 flex flex-wrap gap-3 text-[11px] text-ink-muted">
                                {proc.responsibleRole ? <span>Role: {proc.responsibleRole}</span> : null}
                                {proc.recommendedTimeline ? <span>Timeline: {proc.recommendedTimeline}</span> : null}
                                {proc.requiredDocuments ? <span>Docs: {proc.requiredDocuments}</span> : null}
                              </div>
                              {proc.completedAt ? <p className="mt-1 text-[11px] text-emerald-600">Completed {formatDateTime(proc.completedAt)}{proc.completedBy ? ` by ${proc.completedBy}` : ""}</p> : null}
                            </div>
                          </div>
                        ))}
                      </div>
                    ) : null}
                  </div>
                );
              })}
            </div>
          ) : (
            <EmptyState
              icon={<ListChecks className="h-10 w-10" />}
              title="No procedures linked"
              description="Generate procedure checklists from the Legal Database to track required steps for this case."
              action={<Button onClick={() => setGenOpen(true)}><Plus className="h-4 w-4" /> Generate from Legal DB</Button>}
            />
          )}
        </CardContent>
      </Card>

      <Dialog
        open={genOpen}
        onClose={() => { setGenOpen(false); setSearch(""); }}
        title="Generate Procedures from Legal Database"
        description="Select a legal section to generate its procedure checklist for this case."
        size="lg"
      >
        <div className="space-y-3">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-muted" />
            <Input
              placeholder="Search by section code, title, or law name..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="pl-9"
            />
          </div>
          {sectionsLoading ? (
            <Loading label="Loading legal sections..." />
          ) : availableSections.length > 0 ? (
            <div className="max-h-80 space-y-2 overflow-y-auto">
              {availableSections.map((s) => (
                <button
                  key={s.id}
                  onClick={() => { generateMutation.mutate(s.id); setGenOpen(false); setSearch(""); }}
                  disabled={generateMutation.isPending}
                  className="flex w-full cursor-pointer items-center justify-between gap-3 rounded-lg border border-line p-3 text-left transition-colors hover:border-primary-300 hover:bg-primary-50/40 disabled:opacity-50"
                >
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="font-mono text-xs font-bold text-primary-700">{s.sectionCode}</span>
                      <span className="text-sm font-medium text-ink">{s.sectionTitle}</span>
                    </div>
                    <p className="text-xs text-ink-muted">{s.lawName} · {s.procedureCount} procedures</p>
                  </div>
                  <ExternalLink className="h-4 w-4 shrink-0 text-ink-muted" />
                </button>
              ))}
            </div>
          ) : (
            <p className="py-6 text-center text-sm text-ink-muted">
              {search ? "No matching sections found." : "All available sections are already linked."}
            </p>
          )}
        </div>
      </Dialog>
    </>
  );
}
