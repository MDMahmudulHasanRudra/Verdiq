"use client";

import { useState, useEffect } from "react";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useQuery } from "@tanstack/react-query";
import { timeEntryService } from "@/lib/services";
import { useStartTimer, useStopTimer } from "@/lib/hooks";
import { getErrorMessage, formatCurrency, formatDateTime } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Play, Square, Plus, Clock } from "lucide-react";

export default function TimeEntriesPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);
  const [now, setNow] = useState(Date.now());
  const [elapsed, setElapsed] = useState(0);

  const { data: running } = useQuery({
    queryKey: ["time-entries", "running"],
    queryFn: () => timeEntryService.running()
  });

  const { data: entries, isLoading } = useQuery({
    queryKey: ["time-entries", "list"],
    queryFn: () => timeEntryService.list()
  });

  const startTimer = useStartTimer();
  const stopTimer = useStopTimer();

  useEffect(() => {
    if (!running) return;
    const startedAt = new Date(running.startTime).getTime();
    const iv = setInterval(() => {
      setElapsed(Math.floor((Date.now() - startedAt) / 1000));
    }, 1000);
    return () => clearInterval(iv);
  }, [running]);

  const fmt = (s: number) => {
    const h = Math.floor(s / 3600);
    const m = Math.floor((s % 3600) / 60);
    const sec = s % 60;
    return [h, m, sec].map((v) => String(v).padStart(2, "0")).join(":");
  };

  return (
    <div>
      <PageHeader
        title="Time Entries"
        subtitle="Track billable hours with a live timer."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> Add Manual Entry
          </Button>
        }
      />

      {running ? (
        <Card className="mb-6 border-gold-600/40 bg-gold-50/50 p-5">
          <div className="flex flex-col items-center justify-between gap-4 sm:flex-row">
            <div className="flex items-center gap-4">
              <div className="flex h-14 w-14 items-center justify-center rounded-full bg-gold-600 text-white">
                <Clock className="h-6 w-6" />
              </div>
              <div>
                <p className="font-display text-3xl font-bold tabular-nums text-ink">{fmt(elapsed)}</p>
                <p className="text-sm text-ink-muted">{running.description}</p>
              </div>
            </div>
            <Button variant="danger" onClick={() => stopTimer.mutate(running.id)}>
              <Square className="h-4 w-4" /> Stop Timer
            </Button>
          </div>
        </Card>
      ) : null}

      <Card>
        <CardHeader title="All Time Entries" />
        {isLoading ? (
          <Loading />
        ) : entries && entries.length > 0 ? (
          <table className="table-base">
            <thead>
              <tr>
                <th>Description</th>
                <th>Case</th>
                <th>Duration</th>
                <th>Rate</th>
                <th>Amount</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {entries.map((e) => (
                <tr key={e.id}>
                  <td className="font-medium text-ink">{e.description}</td>
                  <td className="max-w-40 truncate text-ink-muted">{e.caseTitle ?? "—"}</td>
                  <td className="tabular-nums text-ink-muted">
                    {e.durationMinutes != null ? `${(e.durationMinutes / 60).toFixed(2)}h` : "—"}
                  </td>
                  <td className="text-ink-muted">{e.hourlyRate ? formatCurrency(e.hourlyRate) : "—"}</td>
                  <td className="font-medium text-ink">{e.totalAmount ? formatCurrency(e.totalAmount) : "—"}</td>
                  <td className="text-ink-muted capitalize">{e.status?.replace(/([A-Z])/g, " $1").toLowerCase() ?? "—"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <EmptyState
            icon={<Clock className="h-10 w-10" />}
            title="No time entries"
            description="Start a timer while you work to capture billable time."
            action={
              running ? null : (
                <Button
                  variant="gold"
                  onClick={() => startTimer.mutate({ description: "General work", billable: true })}
                >
                  <Play className="h-4 w-4" /> Start Timer
                </Button>
              )
            }
          />
        )}
      </Card>

      <NewEntryDialog open={createOpen} onClose={() => setCreateOpen(false)} />
    </div>
  );
}

function NewEntryDialog({ open, onClose }: { open: boolean; onClose: () => void }) {
  const startTimer = useStartTimer();
  const [form, setForm] = useState({
    caseId: "",
    description: "",
    hourlyRate: "",
    billable: true
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="New Time Entry"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.description}
            onClick={() => {
              startTimer.mutate({
                caseId: form.caseId || null,
                description: form.description,
                hourlyRate: form.hourlyRate ? Number(form.hourlyRate) : undefined,
                billable: form.billable
              });
              onClose();
            }}
          >
            <Play className="h-4 w-4" /> Start Timer
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Description" required className="sm:col-span-2">
          <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
        <Field label="Case ID">
          <Input value={form.caseId} onChange={(e) => setForm({ ...form, caseId: e.target.value })} placeholder="Optional GUID" />
        </Field>
        <Field label="Hourly Rate (BDT)">
          <Input type="number" value={form.hourlyRate} onChange={(e) => setForm({ ...form, hourlyRate: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
