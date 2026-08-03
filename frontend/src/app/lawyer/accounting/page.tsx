"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Dialog } from "@/components/ui/dialog";
import { Loading, EmptyState } from "@/components/ui/loading";
import { accountingService } from "@/lib/services";
import { getErrorMessage, formatCurrency, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { TrendingUp, Plus } from "lucide-react";

export default function AccountingPage() {
  const { t } = useLanguage();
  const toast = useToast();
  const qc = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);

  const { data: dash, isLoading } = useQuery({
    queryKey: ["accounting", "dashboard"],
    queryFn: () => accountingService.dashboard()
  });
  const { data: pnl } = useQuery({
    queryKey: ["accounting", "pnl"],
    queryFn: () => accountingService.profitLoss()
  });
  const { data: journals } = useQuery({
    queryKey: ["accounting", "journals"],
    queryFn: () => accountingService.journals({ page: 1, pageSize: 15 })
  });

  const d = dash as Record<string, unknown> | undefined;
  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => accountingService.createJournal(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["accounting"] });
      setCreateOpen(false);
      toast.success(t("accounting.journalEntryCreated"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title={t("accounting.title")}
        subtitle={t("accounting.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("accounting.journalEntry")}
          </Button>
        }
      />

      {isLoading ? (
        <Loading />
      ) : (
        <>
          <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <SummaryCard label={t("accounting.totalRevenue")} value={d?.totalRevenue as number} />
            <SummaryCard label={t("accounting.totalExpenses")} value={d?.totalExpenses as number} tone="gold" />
            <SummaryCard label={t("accounting.netProfit")} value={d?.netProfit as number} tone="green" />
            <SummaryCard label={t("accounting.outstandingReceivables")} value={d?.outstandingReceivables as number} tone="red" />
          </div>

          <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
            <Card>
              <CardHeader title={t("accounting.profitLoss")} />
              <CardContent className="space-y-2">
                <PnlRow label={t("accounting.revenue")} value={pnl?.revenue as number} />
                <PnlRow label={t("accounting.costOfServices")} value={pnl?.costOfServices as number} />
                <PnlRow label={t("accounting.operatingExpenses")} value={pnl?.operatingExpenses as number} />
                <div className="border-t border-line pt-2">
                  <PnlRow label={t("accounting.netProfit")} value={pnl?.netProfit as number} bold />
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader title={t("accounting.recentJournalEntries")} />
              <CardContent>
                {journals && (journals as unknown as Record<string, unknown>[])?.length ? (
                  <div className="space-y-3">
                    {(journals as unknown as Record<string, unknown>[]).slice(0, 8).map((j) => (
                      <div key={String(j.id)} className="flex items-center justify-between rounded-lg border border-line px-3 py-2">
                        <div>
                          <p className="text-sm font-medium text-ink">{String(j.description ?? t("accounting.journalEntry"))}</p>
                          <p className="text-xs text-ink-muted">{formatDate(String(j.entryDate ?? j.createdAt ?? ""))}</p>
                        </div>
                        <p className="text-sm font-semibold text-ink">{formatCurrency(Number(j.totalAmount ?? j.amount ?? 0))}</p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <EmptyState title={t("accounting.noJournalEntries")} />
                )}
              </CardContent>
            </Card>
          </div>
        </>
      )}

      <CreateJournalDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function SummaryCard({ label, value, tone = "primary" }: { label: string; value?: number; tone?: "primary" | "gold" | "green" | "red" }) {
  const colors = {
    primary: "text-ink",
    gold: "text-gold-700",
    green: "text-emerald-600",
    red: "text-red-600"
  };
  return (
    <Card className="p-5">
      <p className="text-sm text-ink-muted">{label}</p>
      <p className={`mt-1 text-2xl font-bold ${colors[tone]}`}>
        {value !== undefined ? formatCurrency(value) : "—"}
      </p>
    </Card>
  );
}

function PnlRow({ label, value, bold }: { label: string; value?: number; bold?: boolean }) {
  return (
    <div className="flex items-center justify-between">
      <span className={`text-sm ${bold ? "font-semibold text-ink" : "text-ink-muted"}`}>{label}</span>
      <span className={`text-sm tabular-nums ${bold ? "font-bold text-ink" : "text-ink"}`}>
        {value !== undefined ? formatCurrency(value) : "—"}
      </span>
    </div>
  );
}

function CreateJournalDialog({
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
    description: "",
    entryDate: new Date().toISOString().slice(0, 10),
    debitAccountId: "",
    creditAccountId: "",
    amount: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("accounting.journalEntry")}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button
            disabled={!form.description || !form.amount}
            onClick={() =>
              onSubmit({
                description: form.description,
                entryDate: new Date(form.entryDate).toISOString(),
                debitAccountId: form.debitAccountId || null,
                creditAccountId: form.creditAccountId || null,
                amount: Number(form.amount)
              })
            }
          >
            {t("accounting.postEntry")}
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("accounting.description")} required className="sm:col-span-2">
          <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
        <Field label={t("accounting.entryDate")}>
          <Input type="date" value={form.entryDate} onChange={(e) => setForm({ ...form, entryDate: e.target.value })} />
        </Field>
        <Field label={t("accounting.amount")} required>
          <Input type="number" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} />
        </Field>
        <Field label={t("accounting.debitAccountId")}>
          <Input value={form.debitAccountId} onChange={(e) => setForm({ ...form, debitAccountId: e.target.value })} />
        </Field>
        <Field label={t("accounting.creditAccountId")}>
          <Input value={form.creditAccountId} onChange={(e) => setForm({ ...form, creditAccountId: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
