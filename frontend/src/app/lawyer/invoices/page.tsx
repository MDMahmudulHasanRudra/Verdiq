"use client";

import { useState } from "react";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { StatCard } from "@/components/ui/stat-card";
import { StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useInvoices } from "@/lib/hooks";
import { invoiceService, clientService, caseService } from "@/lib/services";
import { getErrorMessage, formatCurrency, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { Receipt, Plus, CheckCircle2, Search, User, FolderOpen } from "lucide-react";
import type { Client, Case } from "@/types/models";

const statuses = ["", "Draft", "Pending", "Paid", "Overdue", "Cancelled"];

export default function InvoicesPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [status, setStatus] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const { data: invoices, isLoading } = useInvoices(status || undefined);

  const invalidate = () => qc.invalidateQueries({ queryKey: ["invoices"] });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => invoiceService.create(input),
    onSuccess: () => {
      invalidate();
      setCreateOpen(false);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const markPaid = useMutation({
    mutationFn: (id: string) => invoiceService.markPaid(id),
    onSuccess: () => {
      invalidate();
      toast.success(t("invoices.paid"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const pending = invoices?.filter((i) => i.status === "Pending" || i.status === "Overdue") ?? [];
  const totalPending = pending.reduce((sum, i) => sum + i.amount, 0);

  return (
    <div>
      <PageHeader
        title={t("invoices.title")}
        subtitle={t("invoices.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("invoices.addInvoice")}
          </Button>
        }
      />

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatCard
          label={`${t("common.total")} (${t("invoices.invoice")})`}
          value={formatCurrency((invoices ?? []).reduce((s, i) => s + i.amount, 0))}
          icon={<Receipt className="h-5 w-5" />}
          accent="primary"
        />
        <StatCard
          label={t("common.status")}
          value={formatCurrency(totalPending)}
          icon={<Receipt className="h-5 w-5" />}
          accent="gold"
          trend={`${pending.length} ${t("invoices.unpaid")}`}
        />
        <StatCard
          label={t("invoices.paid")}
          value={(invoices ?? []).filter((i) => i.status === "Paid").length}
          icon={<CheckCircle2 className="h-5 w-5" />}
          accent="green"
        />
      </div>

      <div className="mb-4">
        <Select className="sm:w-48" value={status} onChange={(e) => setStatus(e.target.value)}>
          {statuses.map((s) => (
            <option key={s} value={s}>{s === "" ? t("common.all") : s}</option>
          ))}
        </Select>
      </div>

      <Card>
        {isLoading ? (
          <Loading />
        ) : invoices && invoices.length > 0 ? (
          <table className="table-base">
            <thead>
              <tr>
                <th>{t("invoices.invoice")}</th>
                <th>{t("invoices.client")}</th>
                <th>{t("invoices.case")}</th>
                <th>{t("invoices.amount")}</th>
                <th>{t("invoices.dueDate")}</th>
                <th>{t("common.status")}</th>
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
                        <CheckCircle2 className="h-3.5 w-3.5" /> {t("invoices.markPaid")}
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
            title={t("invoices.noInvoices")}
            description={t("invoices.noInvoicesDesc")}
            action={
              <Button onClick={() => setCreateOpen(true)}>
                <Plus className="h-4 w-4" /> {t("invoices.addInvoice")}
              </Button>
            }
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
  const { t } = useLanguage();
  const [form, setForm] = useState({
    clientId: "",
    caseId: "",
    amount: "",
    dueDate: "",
    description: ""
  });
  const [clientQuery, setClientQuery] = useState("");
  const [caseQuery, setCaseQuery] = useState("");

  const { data: clients } = useQuery({
    queryKey: ["clients", "search", clientQuery],
    queryFn: () => clientService.search(clientQuery),
    enabled: open && clientQuery.trim().length >= 2
  });

  const { data: cases } = useQuery({
    queryKey: ["cases", "search", caseQuery],
    queryFn: () => caseService.search(caseQuery.trim()),
    enabled: open && caseQuery.trim().length >= 2
  });

  const selectedClient = clients?.find((c) => c.id === form.clientId);

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("invoices.addInvoice")}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
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
            {t("common.create")}
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("invoices.client")} required className="sm:col-span-2">
          <div className="relative">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
            <Input
              value={clientQuery}
              onChange={(e) => setClientQuery(e.target.value)}
              placeholder={`${t("common.search")} ${t("invoices.client")}…`}
              className="pl-9"
            />
          </div>
          {form.clientId && selectedClient ? (
            <p className="mt-1.5 flex items-center gap-1.5 text-sm font-medium text-primary-700">
              <User className="h-4 w-4" /> {selectedClient.name}
            </p>
          ) : null}
          {clientQuery.trim().length >= 2 && !selectedClient ? (
            <ul className="mt-2 max-h-40 space-y-0.5 overflow-y-auto rounded-lg border border-line bg-card">
              {clients && clients.length > 0 ? (
                clients.slice(0, 20).map((c) => (
                  <li key={c.id}>
                    <button
                      type="button"
                      onClick={() => {
                        setForm({ ...form, clientId: c.id });
                        setClientQuery(c.name);
                      }}
                      className="flex w-full cursor-pointer items-center justify-between gap-2 rounded-md px-3 py-2 text-left text-sm text-ink hover:bg-primary-50"
                    >
                      <span className="flex items-center gap-2">
                        <User className="h-4 w-4 text-ink-muted" /> {c.name}
                      </span>
                      <span className="text-xs text-ink-muted">{c.phone || c.email}</span>
                    </button>
                  </li>
                ))
              ) : (
                <li className="px-3 py-2 text-sm text-ink-muted">{t("common.noResults")}</li>
              )}
            </ul>
          ) : null}
        </Field>

        <Field label={t("invoices.case")} className="sm:col-span-2">
          <div className="relative">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
            <Input
              value={caseQuery}
              onChange={(e) => setCaseQuery(e.target.value)}
              placeholder={`${t("common.search")} ${t("invoices.case")} (${t("common.optional")})…`}
              className="pl-9"
            />
          </div>
          {caseQuery.trim().length >= 2 ? (
            <ul className="mt-2 max-h-40 space-y-0.5 overflow-y-auto rounded-lg border border-line bg-card">
              {cases && cases.length > 0 ? (
                cases.slice(0, 20).map((c) => (
                  <li key={c.id}>
                    <button
                      type="button"
                      onClick={() => {
                        setForm({ ...form, caseId: c.id });
                        setCaseQuery(c.title);
                      }}
                      className="flex w-full cursor-pointer items-center gap-2 rounded-md px-3 py-2 text-left text-sm text-ink hover:bg-primary-50"
                    >
                      <FolderOpen className="h-4 w-4 shrink-0 text-ink-muted" />
                      <span className="min-w-0">
                        <span className="block truncate">{c.title}</span>
                        <span className="block text-xs text-ink-muted">{c.caseNumber}</span>
                      </span>
                    </button>
                  </li>
                ))
              ) : (
                <li className="px-3 py-2 text-sm text-ink-muted">{t("common.noResults")}</li>
              )}
            </ul>
          ) : null}
        </Field>

        <Field label={`${t("invoices.amount")} (BDT)`} required>
          <Input type="number" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} />
        </Field>
        <Field label={t("invoices.dueDate")}>
          <Input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} />
        </Field>
        <Field label={t("invoices.description")} className="sm:col-span-2">
          <Textarea rows={2} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}