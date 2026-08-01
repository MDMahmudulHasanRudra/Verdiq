"use client";

import { useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { useCase, useCaseActivities, useCaseProcedures } from "@/lib/hooks";
import { caseService, hearingService, documentService } from "@/lib/services";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Badge, StatusBadge } from "@/components/ui/badge";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Tabs } from "@/components/ui/tabs";
import { Dialog } from "@/components/ui/dialog";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Loading, EmptyState } from "@/components/ui/loading";
import { getErrorMessage, formatDate, formatDateTime } from "@/lib/utils";
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
  CalendarDays
} from "lucide-react";
import type { Hearing } from "@/types/models";

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

      <Tabs tabs={[{ value: "overview", label: "Overview" }, { value: "parties", label: "Parties" }, { value: "hearings", label: "Hearings" }, { value: "documents", label: "Documents" }, { value: "procedures", label: "Procedures" }, { value: "activity", label: "Activity" }]} value={tab} onChange={setTab} />

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
                      href={`${process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000/api"}/documents/download/${d.id}`}
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
