"use client";

import { useState } from "react";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useInvoices } from "@/lib/hooks";
import { invoiceService } from "@/lib/services";
import { getErrorMessage, formatCurrency, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Receipt, Plus, CheckCircle2 } from "lucide-react";

const statuses = ["", "Draft", "Pending", "Paid", "Overdue", "Cancelled"];

export default function InvoicesPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [status, setStatus] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const { data: invoices, isLoading } = useInvoices(status || undefined);

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => invoiceService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["invoices"] });
      setCreateOpen(false);
      toast.success("Invoice created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const markPaid = useMutation({
    mutationFn: (id: string) => invoiceService.markPaid(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["invoices"] });
      toast.success("Invoice marked as paid");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const pending = invoices?.filter((i) => i.status === "Pending" || i.status === "Overdue") ?? [];
  const totalPending = pending.reduce((sum, i) => sum + i.amount, 0);

  return (
    <div>
      <PageHeader
        title="Invoices"
        subtitle="Bill clients for your firm's legal services."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> New Invoice
          </Button>
        }
      />

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Total Invoiced</p>
          <p className="mt-1 text-2xl font-bold text-ink">
            {formatCurrency((invoices ?? []).reduce((s, i) => s + i.amount, 0))}
          </p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Outstanding</p>
          <p className="mt-1 text-2xl font-bold text-gold-700">{formatCurrency(totalPending)}</p>
          <p className="mt-1 text-xs text-ink-muted">{pending.length} unpaid invoices</p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Paid Invoices</p>
          <p className="mt-1 text-2xl font-bold text-emerald-600">
            {(invoices ?? []).filter((i) => i.status === "Paid").length}
          </p>
        </Card>
      </div>

      <Card className="mb-4 p-4">
        <Select className="sm:w-48" value={status} onChange={(e) => setStatus(e.target.value)}>
          {statuses.map((s) => (
            <option key={s} value={s}>{s === "" ? "All statuses" : s}</option>
          ))}
        </Select>
      </Card>

      <Card>
        {isLoading ? (
          <Loading />
        ) : invoices && invoices.length > 0 ? (
          <table className="table-base">
            <thead>
              <tr>
                <th>Invoice</th>
                <th>Client</th>
                <th>Case</th>
                <th>Amount</th>
                <th>Due Date</th>
                <th>Status</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {invoices.map((i) => (
                <tr key={i.id}>
                  <td className="font-medium text-primary-700">{i.invoiceNumber}</td>
                  <td className="text-ink">{i.clientName}</td>
                  <td className="max-w-40 truncate text-ink-muted">{i.caseTitle ?? "—"}</td>
                  <td className="font-medium text-ink">{formatCurrency(i.amount)}</td>
                  <td className="text-ink-muted">{i.dueDate ? formatDate(i.dueDate) : "—"}</td>
                  <td><StatusBadge value={i.status} /></td>
                  <td>
                    {i.status !== "Paid" ? (
                      <Button size="sm" variant="subtle" onClick={() => markPaid.mutate(i.id)}>
                        <CheckCircle2 className="h-3.5 w-3.5" /> Mark Paid
                      </Button>
                    ) : null}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <EmptyState
            icon={<Receipt className="h-10 w-10" />}
            title="No invoices"
            description="Create an invoice to bill your clients."
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> New Invoice</Button>}
          />
        )}
      </Card>

      <CreateInvoiceDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateInvoiceDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({
    clientId: "",
    caseId: "",
    amount: "",
    dueDate: "",
    description: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="New Invoice"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.clientId || !form.amount}
            onClick={() =>
              onSubmit({
                clientId: form.clientId,
                caseId: form.caseId || null,
                amount: Number(form.amount),
                dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
                description: form.description || null,
                status: "Pending"
              })
            }
          >
            Create Invoice
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Client ID" required className="sm:col-span-2">
          <Input value={form.clientId} onChange={(e) => setForm({ ...form, clientId: e.target.value })} placeholder="Paste the client GUID" />
        </Field>
        <Field label="Case ID">
          <Input value={form.caseId} onChange={(e) => setForm({ ...form, caseId: e.target.value })} placeholder="Optional" />
        </Field>
        <Field label="Amount (BDT)" required>
          <Input type="number" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} />
        </Field>
        <Field label="Due Date">
          <Input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} />
        </Field>
        <Field label="Description" className="sm:col-span-2">
          <Textarea rows={2} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
