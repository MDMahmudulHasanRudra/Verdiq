"use client";

import { useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { useQuery } from "@tanstack/react-query";
import { useCase, useCaseActivities, useCaseProcedures, useUpcomingHearings } from "@/lib/hooks";
import { caseService, hearingService } from "@/lib/services";
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
import { ArrowLeft, CalendarClock, Plus, CheckCircle2, FileUp, Download, Gavel } from "lucide-react";

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
        {c.nextHearingDate ? <Badge tone="amber">Next hearing {formatDate(c.nextHearingDate)}</Badge> : null}
      </div>

      <Tabs tabs={[{ value: "overview", label: "Overview" }, { value: "parties", label: "Parties" }, { value: "hearings", label: "Hearings" }, { value: "procedures", label: "Procedures" }, { value: "activity", label: "Activity" }]} value={tab} onChange={setTab} />

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
                    <div key={cl.id} className="flex items-center justify-between rounded-lg border border-line px-3 py-2">
                      <div>
                        <p className="text-sm font-medium text-ink">{cl.name}</p>
                        <p className="text-xs text-ink-muted">{cl.role ?? "Primary"}</p>
                      </div>
                      <StatusBadge value={cl.role ?? "Client"} />
                    </div>
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
          <HearingsTab caseId={c.id} caseNumber={c.caseNumber} />
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

function HearingsTab({ caseId, caseNumber }: { caseId: string; caseNumber: string }) {
  const { data: hearings } = useUpcomingHearings();
  const { data: allHearings } = useCaseHearings(caseId);
  return (
    <Card>
      <CardHeader title="Hearings" description={`All scheduled hearings for ${caseNumber}`} />
      <CardContent className="space-y-3">
        {(allHearings ?? []).length > 0 ? (
          allHearings!.map((h) => (
            <div key={h.id} className="flex items-start justify-between gap-4 rounded-lg border border-line p-3">
              <div>
                <p className="text-sm font-medium text-ink">{formatDateTime(h.hearingDate)}</p>
                <p className="mt-0.5 text-xs text-ink-muted">{h.courtroom ?? "Courtroom TBA"}</p>
                {h.result ? <p className="mt-1 text-xs text-ink">Result: {h.result}</p> : null}
              </div>
              <StatusBadge value={h.status} />
            </div>
          ))
        ) : (
          <EmptyState title="No hearings yet" description="Schedule a hearing to track it here." />
        )}
      </CardContent>
    </Card>
  );
}

function useCaseHearings(caseId: string) {
  return useQuery({ queryKey: ["hearings", "case", caseId], queryFn: () => hearingService.byCase(caseId), enabled: !!caseId });
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
