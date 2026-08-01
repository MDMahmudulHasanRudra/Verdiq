"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { useCase, useCaseActivities, useCaseProcedures } from "@/lib/hooks";
import { caseService, hearingService, documentService, judgmentService, casePhotoService, caseWorkflows } from "@/lib/services";
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
  AlertCircle
} from "lucide-react";
import type { Hearing, Judgment, CreateJudgmentInput, CasePhoto, CaseWorkflow, CaseWorkflowStep, Workflow } from "@/types/models";

const hearingStatuses = ["Scheduled", "Adjourned", "Completed", "Canceled"];
const results = ["Adjourned", "Granted", "Rejected", "Heard", "Deferred", "Dismissed"];
const docCategories = ["Pleadings", "Evidence", "Court Orders", "Correspondence", "Contracts", "Fees", "Other"];

export default function CaseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const toast = useToast();
  const qc = useQueryClient();
  const { data: caseData, isLoading } = useCase(id);
  const { data: activities } = useCaseActivities(id);
  const { data: procedures } = useCaseProcedures(id);
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
      qc.invalidateQueries({ queryKey: ["case", id, "procedures"] });
      toast.success("Procedure completed");
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
            <Button variant="outline" onClick={() => router.push(`/lawyer/documents?caseId=${c.id}`)}>
              <FileUp className="h-4 w-4" /> Documents
            </Button>
            <Button onClick={() => setHearingOpen(true)}>
              <CalendarClock className="h-4 w-4" /> Schedule Hearing
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

      <Tabs tabs={[{ value: "overview", label: "Overview" }, { value: "parties", label: "Parties" }, { value: "hearings", label: "Hearings" }, { value: "judgments", label: "Judgments" }, { value: "documents", label: "Documents" }, { value: "photos", label: "Photos" }, { value: "procedures", label: "Procedures" }, { value: "workflow", label: "Workflow" }, { value: "activity", label: "Activity" }]} value={tab} onChange={setTab} />

      {tab === "overview" && (
        <div className="mt-5 grid grid-cols-1 gap-6 lg:grid-cols-3">
          <Card className="lg:col-span-2">
            <CardHeader title="Case Details" />
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
                  <h4 className="mb-1 text-sm font-semibold text-ink">Description</h4>
                  <p className="text-sm text-ink-muted">{c.description}</p>
                </div>
              ) : null}
              {c.actsAndSections ? (
                <div className="mt-4">
                  <h4 className="mb-1 text-sm font-semibold text-ink">Acts & Sections</h4>
                  <p className="text-sm text-ink-muted">{c.actsAndSections}</p>
                </div>
              ) : null}
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
              <CardHeader title="Stats" />
              <CardContent className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-2xl font-bold text-ink">{c.hearingsCount}</p>
                  <p className="text-xs text-ink-muted">Hearings</p>
                </div>
                <div>
                  <p className="text-2xl font-bold text-ink">{c.documentsCount}</p>
                  <p className="text-xs text-ink-muted">Documents</p>
                </div>
                <div>
                  <p className="text-2xl font-bold text-ink">{c.complexityScore ?? "—"}</p>
                  <p className="text-xs text-ink-muted">Complexity</p>
                </div>
                <div>
                  <p className="text-2xl font-bold text-ink">{c.retainerAmount ? `৳${c.retainerAmount.toLocaleString()}` : "—"}</p>
                  <p className="text-xs text-ink-muted">Retainer</p>
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
          <Card>
            <CardHeader title="Case Procedures" description="Checklist generated from the applicable legal sections." />
            <CardContent className="space-y-3">
              {procedures && procedures.length > 0 ? (
                (procedures as { id: string; title: string; description: string; isCompleted: boolean; dueDate?: string | null }[]).map((p) => (
                  <div key={p.id} className="flex items-start justify-between gap-4 rounded-lg border border-line p-3">
                    <div className="flex items-start gap-3">
                      <CheckCircle2 className={p.isCompleted ? "mt-0.5 h-5 w-5 text-emerald-500" : "mt-0.5 h-5 w-5 text-slate-300"} />
                      <div>
                        <p className={`text-sm font-medium ${p.isCompleted ? "text-ink-muted line-through" : "text-ink"}`}>{p.title}</p>
                        {p.description ? <p className="mt-0.5 text-xs text-ink-muted">{p.description}</p> : null}
                        {p.dueDate ? <p className="mt-1 text-xs text-amber-600">Due {formatDate(p.dueDate)}</p> : null}
                      </div>
                    </div>
                    {!p.isCompleted ? (
                      <Button size="sm" variant="subtle" onClick={() => completeProcedure.mutate(p.id)}>
                        Mark done
                      </Button>
                    ) : null}
                  </div>
                ))
              ) : (
                <EmptyState title="No procedures" description="Generate a procedure checklist from the Legal Database." />
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {tab === "workflow" && (
        <div className="mt-5">
          <WorkflowsTab caseId={c.id} />
        </div>
      )}

      {tab === "activity" && (
        <div className="mt-5">
          <Card>
            <CardHeader title="Case Activity" />
            <CardContent>
              <div className="space-y-4">
                {activities && activities.length > 0 ? (
                  activities.map((a) => (
                    <div key={a.id} className="flex items-start gap-3">
                      <Gavel className="mt-0.5 h-4 w-4 shrink-0 text-ink-soft" />
                      <div>
                        <p className="text-sm text-ink">{a.description || a.activityType}</p>
                        <p className="text-xs text-ink-muted">
                          {a.createdByName ?? a.createdBy} · {formatDateTime(a.createdAt)}
                        </p>
                      </div>
                    </div>
                  ))
                ) : (
                  <EmptyState title="No activity logged" />
                )}
              </div>
            </CardContent>
          </Card>
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

let pendingFile: File | null = null;
let pendingCategory = "Evidence";

function DocumentsTab({ caseId }: { caseId: string }) {
  const toast = useToast();
  const qc = useQueryClient();
  const [uploadOpen, setUploadOpen] = useState(false);

  const { data: documents, isLoading } = useQuery({
    queryKey: ["documents", "case", caseId],
    queryFn: () => documentService.byCase(caseId),
    enabled: !!caseId
  });

  const uploadMutation = useMutation({
    mutationFn: ({ file, docCategory }: { file: File; docCategory: string }) =>
      documentService.upload(file, caseId, docCategory),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["documents", "case", caseId] });
      qc.invalidateQueries({ queryKey: ["documents"] });
      qc.invalidateQueries({ queryKey: ["case", caseId] });
      setUploadOpen(false);
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

  return (
    <>
      <Card>
        <CardHeader
          title="Evidence & Documents"
          description="Upload evidence and documents for this case. They are stored securely and linked to the case."
          action={
            <Button size="sm" onClick={() => setUploadOpen(true)}>
              <Plus className="h-4 w-4" /> Upload Evidence
            </Button>
          }
        />
        <CardContent>
          {isLoading ? (
            <Loading />
          ) : documents && documents.length > 0 ? (
            <div className="space-y-3">
              {documents.map((d) => (
                <div key={d.id} className="flex items-center justify-between gap-4 rounded-lg border border-line p-3">
                  <div className="flex items-center gap-3">
                    <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-50">
                      <FileText className="h-4 w-4 text-primary-700" />
                    </div>
                    <div>
                      <p className="text-sm font-medium text-ink">{d.originalFileName ?? d.fileName}</p>
                      <p className="text-xs text-ink-muted">
                        {d.category} · {d.fileType} · {d.fileSize ? `${(d.fileSize / 1024).toFixed(0)} KB` : "—"} · v{d.version}
                      </p>
                    </div>
                  </div>
                  <div className="flex shrink-0 items-center gap-1">
                    <a
                      className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                      aria-label="Download"
                      title="Download"
                      href={`${API_URL}/documents/download/${d.id}`}
                      target="_blank"
                      rel="noreferrer"
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
          ) : (
            <EmptyState
              title="No documents"
              description="Upload evidence, court orders and correspondence for this case."
              action={
                <Button onClick={() => setUploadOpen(true)}>
                  <Plus className="h-4 w-4" /> Upload Evidence
                </Button>
              }
            />
          )}
        </CardContent>
      </Card>

      <Dialog
        open={uploadOpen}
        onClose={() => setUploadOpen(false)}
        title="Upload Evidence / Document"
        description="Attach a file to this case. It will be stored securely and linked to the case."
        footer={
          <>
            <Button variant="ghost" onClick={() => setUploadOpen(false)}>Cancel</Button>
            <Button
              disabled={!pendingFile || uploadMutation.isPending}
              onClick={() => pendingFile && uploadMutation.mutate({ file: pendingFile, docCategory: pendingCategory })}
            >
              <FileUp className="h-4 w-4" /> Upload
            </Button>
          </>
        }
      >
        <UploadForm
          initialCategory={pendingCategory}
          onReady={(file, docCategory) => {
            pendingFile = file;
            pendingCategory = docCategory;
          }}
        />
      </Dialog>
    </>
  );
}

function UploadForm({
  initialCategory,
  onReady
}: {
  initialCategory: string;
  onReady: (file: File, docCategory: string) => void;
}) {
  const [file, setFile] = useState<File | null>(null);
  const [docCategory, setDocCategory] = useState(initialCategory);

  const update = (f: File | null, cat: string) => {
    setFile(f);
    setDocCategory(cat);
    if (f) onReady(f, cat);
  };

  return (
    <div className="space-y-4">
      <Field label="File" required>
        <input
          type="file"
          className="w-full text-sm text-ink file:mr-4 file:cursor-pointer file:rounded-lg file:border-0 file:bg-primary-50 file:px-4 file:py-2 file:text-sm file:font-medium file:text-primary-800 file:transition-colors hover:file:bg-primary-100"
          onChange={(e) => update(e.target.files?.[0] ?? null, docCategory)}
        />
      </Field>
      <Field label="Category">
        <Select value={docCategory} onChange={(e) => update(file, e.target.value)}>
          {docCategories.map((cat) => (
            <option key={cat} value={cat}>{cat}</option>
          ))}
        </Select>
      </Field>
      {!file ? <p className="text-xs text-ink-muted">Choose a file to enable upload.</p> : null}
    </div>
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
    </Card>
  );
}
