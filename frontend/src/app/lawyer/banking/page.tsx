"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { Loading, EmptyState } from "@/components/ui/loading";
import { bankingService } from "@/lib/services";
import { getErrorMessage, formatCurrency, formatDateTime } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { Landmark, Plus } from "lucide-react";

export default function BankingPage() {
  const { t } = useLanguage();
  const toast = useToast();
  const qc = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const { data: accounts, isLoading } = useQuery({
    queryKey: ["banking", "accounts"],
    queryFn: () => bankingService.accounts()
  });

  const { data: transactions } = useQuery({
    queryKey: ["banking", "transactions", selectedId],
    queryFn: () => bankingService.transactions(selectedId!, 1, 20),
    enabled: !!selectedId
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => bankingService.createAccount(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["banking"] });
      setCreateOpen(false);
      toast.success(t("banking.bankAccountAdded"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const totalBalance = (accounts ?? []).reduce((s, a) => s + a.currentBalance, 0);

  return (
    <div>
      <PageHeader
        title={t("banking.title")}
        subtitle={t("banking.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("banking.addAccount")}
          </Button>
        }
      />

      {isLoading ? (
        <Loading />
      ) : (
        <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
          <div className="space-y-4 xl:col-span-1">
            <Card className="bg-primary-800 p-5 text-white">
              <p className="text-sm text-primary-200">{t("banking.totalBalance")}</p>
              <p className="mt-1 text-3xl font-bold">{formatCurrency(totalBalance)}</p>
            </Card>
            {accounts && accounts.length > 0 ? (
              accounts.map((a) => (
                <button
                  key={a.id}
                  onClick={() => setSelectedId(a.id)}
                  className={`w-full cursor-pointer rounded-xl border p-4 text-left transition-colors ${
                    selectedId === a.id ? "border-primary-600 bg-primary-50" : "border-line bg-card hover:border-primary-300"
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-50">
                        <Landmark className="h-4 w-4 text-primary-700" />
                      </div>
                      <div>
                        <p className="text-sm font-semibold text-ink">{a.accountName}</p>
                        <p className="text-xs text-ink-muted">{a.bankName} · {a.accountNumber ?? a.accountType}</p>
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-bold text-ink">{formatCurrency(a.currentBalance)}</p>
                      <StatusBadge value={a.isActive ? "Active" : "Inactive"} />
                    </div>
                  </div>
                </button>
              ))
            ) : (
              <Card>
                <EmptyState icon={<Landmark className="h-10 w-10" />} title={t("banking.noBankAccounts")} description={t("banking.noBankAccountsDesc")} />
              </Card>
            )}
          </div>

          <Card className="xl:col-span-2">
            <CardHeader title={selectedId ? t("banking.transactions") : t("banking.selectAnAccount")} />
            <CardContent>
              {selectedId && transactions && transactions.length > 0 ? (
                <div className="space-y-3">
                  {transactions.map((t) => (
                    <div key={t.id} className="flex items-center justify-between rounded-lg border border-line px-3 py-2">
                      <div>
                        <p className="text-sm font-medium text-ink">{t.description}</p>
                        <p className="text-xs text-ink-muted">{formatDateTime(t.transactionDate ?? t.createdAt)}</p>
                      </div>
                      <div className="flex items-center gap-3">
                        <p className={`text-sm font-semibold ${t.transactionType === "Credit" ? "text-emerald-600" : "text-red-600"}`}>
                          {t.transactionType === "Credit" ? "+" : "−"}{formatCurrency(t.amount)}
                        </p>
                        <StatusBadge value={t.reconciliationStatus ?? (t.reconciledAt ? "Reconciled" : "Unreconciled")} />
                      </div>
                    </div>
                  ))}
                </div>
              ) : selectedId ? (
                <EmptyState title={t("banking.noTransactions")} description={t("banking.noTransactionsDesc")} />
              ) : (
                <EmptyState title={t("banking.noAccountSelected")} description={t("banking.noAccountSelectedDesc")} />
              )}
            </CardContent>
          </Card>
        </div>
      )}

      <CreateAccountDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateAccountDialog({
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
    accountName: "",
    bankName: "",
    accountNumber: "",
    accountType: "Current",
    openingBalance: "0"
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("banking.addBankAccount")}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button disabled={!form.accountName || !form.bankName} onClick={() => onSubmit(form)}>{t("banking.addAccount")}</Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("banking.accountName")} required>
          <Input value={form.accountName} onChange={(e) => setForm({ ...form, accountName: e.target.value })} />
        </Field>
        <Field label={t("banking.bankName")} required>
          <Input value={form.bankName} onChange={(e) => setForm({ ...form, bankName: e.target.value })} />
        </Field>
        <Field label={t("banking.accountNumber")}>
          <Input value={form.accountNumber} onChange={(e) => setForm({ ...form, accountNumber: e.target.value })} />
        </Field>
        <Field label={t("banking.accountType")}>
          <Select value={form.accountType} onChange={(e) => setForm({ ...form, accountType: e.target.value })}>
            {["Current", "Savings", "Fixed Deposit", "Mobile Wallet"].map((type) => (
              <option key={type} value={type}>{t("banking.accountTypes." + type)}</option>
            ))}
          </Select>
        </Field>
        <Field label={t("banking.openingBalance")}>
          <Input type="number" value={form.openingBalance} onChange={(e) => setForm({ ...form, openingBalance: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
