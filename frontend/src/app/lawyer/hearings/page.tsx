"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Table, Pagination } from "@/components/ui/table";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useCases } from "@/lib/hooks";
import { hearingService } from "@/lib/services";
import { getErrorMessage, formatDateTime, cn } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { CalendarClock, Plus, PenLine, Trash2, AlertTriangle } from "lucide-react";
import type { Hearing } from "@/types/models";

const hearingStatuses = ["Scheduled", "Adjourned", "Completed", "Canceled"];
const results = ["Adjourned", "Granted", "Rejected", "Heard", "Deferred", "Dismissed"];

type Tab = "upcoming" | "all";

interface HearingFormState {
  caseId: string;
  hearingDate: string;
  hearingTime: string;
  courtroom: string;
  judgeName: string;
  status: string;
  result: string;
  nextHearingDate: string;
  notes: string;
}

const emptyForm = (): HearingFormState => ({
  caseId: "",
  hearingDate: "",
  hearingTime: "10:00",
  courtroom: "",
  judgeName: "",
  status: "Scheduled",
  result: "",
  nextHearingDate: "",
  notes: ""
});

const fromHearing = (h: Hearing): HearingFormState => ({
  caseId: h.caseId,
  hearingDate: (h.hearingDate || "").slice(0, 10),
  hearingTime: (h.hearingDate || "").slice(11, 16) || "10:00",
  courtroom: h.courtroom ?? "",
  judgeName: h.judgeName ?? "",
  status: h.status,
  result: h.result ?? "",
  nextHearingDate: h.nextHearingDate ? h.nextHearingDate.slice(0, 10) : "",
  notes: h.notes ?? ""
});

export default function HearingsPage() {
  const router = useRouter();
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();

  const [page, setPage] = useState(1);
  const [tab, setTab] = useState<Tab>("upcoming");
  const [status, setStatus] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<Hearing | null>(null);
  const [deleting, setDeleting] = useState<Hearing | null>(null);

  const { data: pagedData, isLoading } = useQuery({
    queryKey: ["hearings", "all", page, status],
    queryFn: () =>
      hearingService.list({
        page,
        pageSize: 12
      })
  });

  const { data: upcoming, isLoading: loadingUpcoming } = useQuery({
    queryKey: ["hearings", "upcoming"],
    queryFn: () => hearingService.upcoming(),
    enabled: tab === "upcoming"
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ["hearings"] });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => hearingService.create(input),
    onSuccess: () => {
      invalidate();
      setCreateOpen(false);
      toast.success("Hearing scheduled");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: Record<string, unknown> }) =>
      hearingService.update(id, input),
    onSuccess: () => {
      invalidate();
      setEditing(null);
      toast.success("Hearing updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => hearingService.remove(id),
    onSuccess: () => {
      invalidate();
      setDeleting(null);
      toast.success("Hearing removed");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const showUpcoming = tab === "upcoming";
  const rows: Hearing[] = showUpcoming
    ? (upcoming ?? [])
    : (pagedData?.data ?? []);
  const loading = showUpcoming ? loadingUpcoming : isLoading;

  const filteredRows =
    !showUpcoming && status
      ? rows.filter((h) => h.status === status)
      : rows;

  return (
    <div>
      <PageHeader
        title={t("hearings.title")}
        subtitle={t("hearings.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("hearings.schedule")}
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex items-center gap-1 rounded-lg border border-line bg-slate-50 p-1">
            {(["upcoming", "all"] as Tab[]).map((t) => (
              <button
                key={t}
                onClick={() => setTab(t)}
                className={cn(
                  "cursor-pointer rounded-md px-4 py-1.5 text-sm font-medium capitalize transition-colors",
                  tab === t ? "bg-card text-ink shadow-card" : "text-ink-muted hover:text-ink"
                )}
              >
                {t === "upcoming" ? "Upcoming" : "All hearings"}
              </button>
            ))}
          </div>
          {!showUpcoming ? (
            <Select
              className="sm:w-44"
              value={status}
              onChange={(e) => setStatus(e.target.value)}
            >
              <option value="">All statuses</option>
              {hearingStatuses.map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </Select>
          ) : null}
        </div>
      </Card>

      <Card>
        {loading ? (
          <Loading />
        ) : filteredRows.length > 0 ? (
          <>
            <Table>
              <thead>
                <tr>
                  <th>Case</th>
                  <th>Hearing Date</th>
                  <th>Courtroom</th>
                  <th>Judge</th>
                  <th>Result</th>
                  <th>Status</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredRows.map((h) => (
                  <tr
                    key={h.id}
                    className="cursor-pointer"
                    onClick={() => router.push(`/lawyer/cases/${h.caseId}`)}
                  >
                    <td>
                      <div className="flex items-center gap-2">
                        <p className="font-medium text-primary-700">{h.caseNumber}</p>
                        {h.hasIncompletePreHearingTasks ? (
                          <span title="Has incomplete pre-hearing tasks" className="text-amber-500">
                            <AlertTriangle className="h-4 w-4" />
                          </span>
                        ) : null}
                      </div>
                      <p className="truncate text-xs text-ink-muted">{h.caseTitle}</p>
                    </td>
                    <td className="whitespace-nowrap">{formatDateTime(h.hearingDate)}</td>
                    <td className="text-ink-muted">{h.courtroom ?? "—"}</td>
                    <td className="text-ink-muted">{h.judgeName ?? "—"}</td>
                    <td className="max-w-48 truncate text-ink-muted">{h.result ?? "—"}</td>
                    <td><StatusBadge value={h.status} /></td>
                    <td onClick={(e) => e.stopPropagation()}>
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => setEditing(h)}
                          className="cursor-pointer rounded-md p-1.5 text-ink-soft transition-colors hover:bg-slate-100 hover:text-ink"
                          title="Edit hearing"
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
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
            {!showUpcoming && pagedData && pagedData.totalPages > 1 ? (
              <Pagination
                page={pagedData.page}
                totalPages={pagedData.totalPages}
                totalCount={pagedData.totalCount}
                onChange={setPage}
              />
            ) : null}
          </>
        ) : (
          <EmptyState
            icon={<CalendarClock className="h-10 w-10" />}
            title={showUpcoming ? "No upcoming hearings" : "No hearings scheduled"}
            description={
              showUpcoming
                ? "Great — nothing on the docket right now."
                : "Schedule a hearing from a case or here."
            }
            action={
              <Button onClick={() => setCreateOpen(true)}>
                <Plus className="h-4 w-4" /> Schedule Hearing
              </Button>
            }
          />
        )}
      </Card>

      <HearingFormDialog
        open={createOpen}
        title="Schedule Hearing"
        description="Enter the case and hearing details."
        submitLabel="Schedule"
        isEdit={false}
        initial={emptyForm()}
        onClose={() => setCreateOpen(false)}
        onSubmit={(form) =>
          createMutation.mutate({
            caseId: form.caseId,
            hearingDate: new Date(`${form.hearingDate}T${form.hearingTime}`).toISOString(),
            courtroom: form.courtroom || null,
            judgeName: form.judgeName || null,
            notes: form.notes || null,
            status: "Scheduled"
          })
        }
      />

      {editing ? (
        <HearingFormDialog
          open
          title="Edit Hearing"
          description={`${editing.caseNumber} · ${editing.caseTitle}`}
          submitLabel="Save Changes"
          isEdit
          initial={fromHearing(editing)}
          onClose={() => setEditing(null)}
          onSubmit={(form) =>
            updateMutation.mutate({
              id: editing.id,
              input: {
                hearingDate: form.hearingDate
                  ? new Date(`${form.hearingDate}T${form.hearingTime}`).toISOString()
                  : null,
                courtroom: form.courtroom || null,
                judgeName: form.judgeName || null,
                status: form.status || undefined,
                result: form.result || null,
                nextHearingDate: form.nextHearingDate
                  ? new Date(`${form.nextHearingDate}T10:00`).toISOString()
                  : null,
                notes: form.notes || null
              }
            })
          }
        />
      ) : null}

      <Dialog
        open={!!deleting}
        onClose={() => setDeleting(null)}
        title="Delete hearing"
        description={
          deleting
            ? `Hearing on ${formatDateTime(deleting.hearingDate)} for ${deleting.caseNumber} will be permanently removed.`
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
    </div>
  );
}

function HearingFormDialog({
  open,
  title,
  description,
  submitLabel,
  isEdit,
  initial,
  onClose,
  onSubmit
}: {
  open: boolean;
  title: string;
  description: string;
  submitLabel: string;
  isEdit: boolean;
  initial: HearingFormState;
  onClose: () => void;
  onSubmit: (form: HearingFormState) => void;
}) {
  const [form, setForm] = useState<HearingFormState>(initial);

  useEffect(() => {
    setForm(initial);
  }, [initial]);

  const set = (k: keyof HearingFormState, v: string) => setForm((f) => ({ ...f, [k]: v }));

  const isFormValid = form.caseId && form.hearingDate;

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
          <Button disabled={!isFormValid} onClick={() => onSubmit(form)}>{submitLabel}</Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="sm:col-span-2">
          <label className="mb-1.5 block text-sm font-medium text-ink">
            Case <span className="text-red-500">*</span>
          </label>
          <CaseSelect value={form.caseId} onChange={(v) => set("caseId", v)} disabled={isEdit} />
        </div>
        <Field label="Hearing Date" required>
          <Input type="date" value={form.hearingDate} onChange={(e) => set("hearingDate", e.target.value)} />
        </Field>
        <Field label="Hearing Time">
          <Input type="time" value={form.hearingTime} onChange={(e) => set("hearingTime", e.target.value)} />
        </Field>
        <Field label="Courtroom">
          <Input value={form.courtroom} onChange={(e) => set("courtroom", e.target.value)} />
        </Field>
        <Field label="Judge">
          <Input value={form.judgeName} onChange={(e) => set("judgeName", e.target.value)} />
        </Field>
        {isEdit ? (
          <>
            <Field label="Status">
              <Select value={form.status} onChange={(e) => set("status", e.target.value)}>
                {hearingStatuses.map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </Select>
            </Field>
            <Field label="Result">
              <Select value={form.result} onChange={(e) => set("result", e.target.value)}>
                <option value="">No result recorded</option>
                {results.map((r) => (
                  <option key={r} value={r}>{r}</option>
                ))}
              </Select>
            </Field>
            <Field label="Next Hearing Date" className="sm:col-span-2">
              <Input
                type="date"
                value={form.nextHearingDate}
                onChange={(e) => set("nextHearingDate", e.target.value)}
              />
            </Field>
          </>
        ) : null}
        <Field label="Notes" className="sm:col-span-2">
          <Textarea rows={3} value={form.notes} onChange={(e) => set("notes", e.target.value)} />
        </Field>
      </div>
    </Dialog>
  );
}

function CaseSelect({
  value,
  onChange,
  disabled
}: {
  value: string;
  onChange: (v: string) => void;
  disabled?: boolean;
}) {
  const { data } = useCases({ pageSize: 200 });
  const cases = data?.data ?? [];

  return (
    <Select value={value} onChange={(e) => onChange(e.target.value)} disabled={disabled}>
      <option value="">Select a case…</option>
      {cases.map((c) => (
        <option key={c.id} value={c.id}>
          {c.caseNumber} — {c.title}
        </option>
      ))}
    </Select>
  );
}
