"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { Loading, EmptyState } from "@/components/ui/loading";
import { budgetService } from "@/lib/services";
import { getErrorMessage, formatCurrency } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { PiggyBank, Plus } from "lucide-react";

export default function BudgetPage() {
  const { t } = useLanguage();
  const toast = useToast();
  const qc = useQueryClient();
  const [year, setYear] = useState(new Date().getFullYear());
  const [createOpen, setCreateOpen] = useState(false);

  const { data: budgets, isLoading } = useQuery({
    queryKey: ["budget", year],
    queryFn: () => budgetService.list(year)
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => budgetService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["budget"] });
      setCreateOpen(false);
      toast.success(t("budget.budgetCreated"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const approve = useMutation({
    mutationFn: (id: string) => budgetService.approve(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["budget"] });
      toast.success(t("budget.budgetApproved"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title={t("budget.title")}
        subtitle={t("budget.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("budget.newBudget")}
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <Select className="sm:w-40" value={year} onChange={(e) => setYear(Number(e.target.value))}>
          {[2024, 2025, 2026, 2027].map((y) => (
            <option key={y} value={y}>{y}</option>
          ))}
        </Select>
      </Card>

      {isLoading ? (
        <Loading />
      ) : budgets && budgets.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {budgets.map((b) => {
            const pct = b.totalAmount > 0 ? (b.totalSpent / b.totalAmount) * 100 : 0;
            return (
              <Card key={b.id} className="p-5">
                <div className="flex items-start justify-between">
                  <div>
                    <h3 className="font-display text-lg font-semibold text-ink">{b.name}</h3>
                    <p className="text-xs text-ink-muted">FY {b.fiscalYear} · {b.createdByName}</p>
                  </div>
                  <StatusBadge value={b.status} />
                </div>
                <div className="mt-4 space-y-3">
                  <div className="flex justify-between text-sm">
                    <span className="text-ink-muted">{t("budget.spent")}</span>
                    <span className="font-medium text-ink">{formatCurrency(b.totalSpent)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-ink-muted">{t("budget.allocated")}</span>
                    <span className="font-medium text-ink">{formatCurrency(b.totalAmount)}</span>
                  </div>
                  <div className="h-2 overflow-hidden rounded-full bg-slate-100">
                    <div
                      className={`h-full rounded-full ${pct > 90 ? "bg-red-500" : pct > 70 ? "bg-gold-600" : "bg-emerald-500"}`}
                      style={{ width: `${Math.min(100, pct)}%` }}
                    />
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-xs text-ink-muted">{pct.toFixed(0)}% {t("budget.utilized")}</span>
                    <span className="text-xs font-medium text-ink-muted">{formatCurrency(b.remaining)} {t("budget.remaining")}</span>
                  </div>
                </div>
                {b.status !== "Approved" ? (
                  <Button size="sm" variant="subtle" className="mt-4 w-full" onClick={() => approve.mutate(b.id)}>
                    {t("budget.approve")}
                  </Button>
                ) : null}
              </Card>
            );
          })}
        </div>
      ) : (
        <Card>
          <EmptyState
            icon={<PiggyBank className="h-10 w-10" />}
            title={t("budget.noBudgets")}
            description={t("budget.noBudgetsDesc")}
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> {t("budget.newBudget")}</Button>}
          />
        </Card>
      )}

      <CreateBudgetDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateBudgetDialog({
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
    name: "",
    fiscalYear: String(new Date().getFullYear()),
    totalAmount: "",
    description: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("budget.newBudget")}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button
            disabled={!form.name || !form.totalAmount}
            onClick={() =>
              onSubmit({
                name: form.name,
                fiscalYear: Number(form.fiscalYear),
                totalAmount: Number(form.totalAmount),
                description: form.description || null
              })
            }
          >
            {t("budget.createBudget")}
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("budget.budgetName")} required>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder={t("budget.budgetNamePlaceholder")} />
        </Field>
        <Field label={t("budget.fiscalYear")}>
          <Select value={form.fiscalYear} onChange={(e) => setForm({ ...form, fiscalYear: e.target.value })}>
            {[2024, 2025, 2026, 2027].map((y) => (
              <option key={y} value={y}>{y}</option>
            ))}
          </Select>
        </Field>
        <Field label={t("budget.totalAmountBdt")} required className="sm:col-span-2">
          <Input type="number" value={form.totalAmount} onChange={(e) => setForm({ ...form, totalAmount: e.target.value })} />
        </Field>
        <Field label={t("common.notes")} className="sm:col-span-2">
          <Textarea rows={2} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
