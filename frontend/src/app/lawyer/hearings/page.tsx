"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Table, Pagination } from "@/components/ui/table";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useHearings } from "@/lib/hooks";
import { hearingService } from "@/lib/services";
import { getErrorMessage, formatDateTime } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { CalendarClock, Plus } from "lucide-react";

export default function HearingsPage() {
  const router = useRouter();
  const toast = useToast();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [createOpen, setCreateOpen] = useState(false);
  const { data, isLoading } = useHearings();

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => hearingService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["hearings"] });
      setCreateOpen(false);
      toast.success("Hearing scheduled");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title="Hearings"
        subtitle="Track upcoming and past court appearances."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> Schedule Hearing
          </Button>
        }
      />

      <Card>
        {isLoading ? (
          <Loading />
        ) : data && data.data.length > 0 ? (
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
                </tr>
              </thead>
              <tbody>
                {data.data.map((h) => (
                  <tr key={h.id} className="cursor-pointer" onClick={() => router.push(`/lawyer/cases/${h.caseId}`)}>
                    <td>
                      <p className="font-medium text-primary-700">{h.caseNumber}</p>
                      <p className="truncate text-xs text-ink-muted">{h.caseTitle}</p>
                    </td>
                    <td>{formatDateTime(h.hearingDate)}</td>
                    <td className="text-ink-muted">{h.courtroom ?? "—"}</td>
                    <td className="text-ink-muted">{h.judgeName ?? "—"}</td>
                    <td className="max-w-48 truncate text-ink-muted">{h.result ?? "—"}</td>
                    <td><StatusBadge value={h.status} /></td>
                  </tr>
                ))}
              </tbody>
            </Table>
            <Pagination page={data.page} totalPages={data.totalPages} totalCount={data.totalCount} onChange={setPage} />
          </>
        ) : (
          <EmptyState
            icon={<CalendarClock className="h-10 w-10" />}
            title="No hearings scheduled"
            description="Schedule a hearing from a case or here."
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> Schedule Hearing</Button>}
          />
        )}
      </Card>

      <ScheduleHearingDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function ScheduleHearingDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({
    caseId: "",
    hearingDate: "",
    hearingTime: "10:00",
    courtroom: "",
    judgeName: "",
    notes: ""
  });

  const isFormValid = form.caseId && form.hearingDate;

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Schedule Hearing"
      description="Enter the case and hearing details."
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!isFormValid}
            onClick={() =>
              onSubmit({
                caseId: form.caseId,
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
        <Field label="Case ID" required className="sm:col-span-2">
          <Input value={form.caseId} onChange={(e) => setForm({ ...form, caseId: e.target.value })} placeholder="Paste the case GUID" />
        </Field>
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
