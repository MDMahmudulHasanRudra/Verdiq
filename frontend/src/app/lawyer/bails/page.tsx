"use client";

import { useState } from "react";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useQuery } from "@tanstack/react-query";
import { bailService } from "@/lib/services";
import { getErrorMessage, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { Handshake, Plus } from "lucide-react";

export default function BailsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [createOpen, setCreateOpen] = useState(false);
  const [statusFilter, setStatusFilter] = useState("");

  const { data: bails, isLoading } = useQuery({
    queryKey: ["bails", statusFilter],
    queryFn: () => bailService.list(statusFilter || undefined)
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => bailService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["bails"] });
      setCreateOpen(false);
      toast.success("Bail record created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title={t("bails.title")}
        subtitle={t("bails.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("bails.addBail")}
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <Select
          className="sm:w-56"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="">All statuses</option>
          <option value="Pending">{t("bails.pending")}</option>
          <option value="Granted">{t("bails.granted")}</option>
          <option value="Revoked">Revoked</option>
          <option value="Forfeited">Forfeited</option>
        </Select>
      </Card>

      <Card>
        {isLoading ? (
          <Loading />
        ) : bails && bails.length > 0 ? (
          <table className="table-base">
            <thead>
              <tr>
                <th>{t("bails.case")}</th>
                <th>{t("bails.bailApplication")}</th>
                <th>{t("bails.hearingDate")}</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {bails.map((b) => (
                <tr key={b.id}>
                  <td>
                    <p className="font-medium text-primary-700">{b.caseNumber}</p>
                    <p className="truncate text-xs text-ink-muted">{b.caseTitle}</p>
                  </td>
                  <td className="font-medium text-ink">
                    {b.bailAmount ? `৳${b.bailAmount.toLocaleString()}` : "—"}
                  </td>
                  <td className="text-ink-muted">{b.bailHearingDate ? formatDate(b.bailHearingDate) : "—"}</td>
                  <td>
                    <StatusBadge value={b.status} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <EmptyState
            icon={<Handshake className="h-10 w-10" />}
            title={t("bails.noBails")}
            description={t("bails.noBailsDesc")}
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> {t("bails.addBail")}</Button>}
          />
        )}
      </Card>

      <CreateBailDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateBailDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const { t } = useLanguage();
  const [form, setForm] = useState({
    caseId: "",
    bailAmount: "",
    bailConditions: "",
    bailHearingDate: "",
    suretyName: "",
    suretyContact: "",
    notes: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("bails.addBail")}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.caseId}
            onClick={() =>
              onSubmit({
                caseId: form.caseId,
                bailAmount: form.bailAmount ? Number(form.bailAmount) : null,
                bailConditions: form.bailConditions || null,
                bailHearingDate: form.bailHearingDate ? new Date(form.bailHearingDate).toISOString() : null,
                suretyName: form.suretyName || null,
                suretyContact: form.suretyContact || null,
                notes: form.notes || null,
                status: "Pending"
              })
            }
          >
            Create
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Case ID" required className="sm:col-span-2">
          <Input value={form.caseId} onChange={(e) => setForm({ ...form, caseId: e.target.value })} placeholder="Paste the case GUID" />
        </Field>
        <Field label="Bail Amount (BDT)">
          <Input type="number" value={form.bailAmount} onChange={(e) => setForm({ ...form, bailAmount: e.target.value })} />
        </Field>
        <Field label="Bail Hearing Date">
          <Input type="date" value={form.bailHearingDate} onChange={(e) => setForm({ ...form, bailHearingDate: e.target.value })} />
        </Field>
        <Field label="Surety Name">
          <Input value={form.suretyName} onChange={(e) => setForm({ ...form, suretyName: e.target.value })} />
        </Field>
        <Field label="Surety Contact">
          <Input value={form.suretyContact} onChange={(e) => setForm({ ...form, suretyContact: e.target.value })} />
        </Field>
        <Field label="Bail Conditions" className="sm:col-span-2">
          <Textarea rows={2} value={form.bailConditions} onChange={(e) => setForm({ ...form, bailConditions: e.target.value })} />
        </Field>
        <Field label="Notes" className="sm:col-span-2">
          <Textarea rows={2} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
