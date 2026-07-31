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
import { taxService } from "@/lib/services";
import { getErrorMessage, formatCurrency, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Percent, Plus } from "lucide-react";

export default function TaxPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [year, setYear] = useState(new Date().getFullYear());
  const [createOpen, setCreateOpen] = useState(false);
  const [transOpen, setTransOpen] = useState(false);

  const { data: settings, isLoading } = useQuery({
    queryKey: ["tax", "settings"],
    queryFn: () => taxService.settings()
  });
  const { data: transactions } = useQuery({
    queryKey: ["tax", "transactions", year],
    queryFn: () => taxService.transactions(year)
  });

  const createSetting = useMutation({
    mutationFn: (input: Record<string, unknown>) => taxService.createSetting(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["tax"] });
      setCreateOpen(false);
      toast.success("Tax setting created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const createTrans = useMutation({
    mutationFn: (input: Record<string, unknown>) => taxService.createTransaction(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["tax"] });
      setTransOpen(false);
      toast.success("Tax transaction recorded");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const totalTax = (transactions ?? []).reduce((s, t) => s + t.taxAmount, 0);

  return (
    <div>
      <PageHeader
        title="Tax"
        subtitle="VAT, income tax and tax transaction tracking."
        actions={
          <>
            <Button variant="outline" onClick={() => setTransOpen(true)}>
              <Plus className="h-4 w-4" /> Record Tax
            </Button>
            <Button onClick={() => setCreateOpen(true)}>
              <Percent className="h-4 w-4" /> Tax Setting
            </Button>
          </>
        }
      />

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Tax Settings</p>
          <p className="mt-1 text-2xl font-bold text-ink">{settings?.length ?? 0}</p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Tax Paid ({year})</p>
          <p className="mt-1 text-2xl font-bold text-ink">{formatCurrency(totalTax)}</p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Tax Records</p>
          <p className="mt-1 text-2xl font-bold text-ink">{transactions?.length ?? 0}</p>
        </Card>
      </div>

      <div className="mb-4">
        <Select className="sm:w-40" value={year} onChange={(e) => setYear(Number(e.target.value))}>
          {[2024, 2025, 2026, 2027].map((y) => (
            <option key={y} value={y}>{y}</option>
          ))}
        </Select>
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
        <Card>
          <CardHeader title="Tax Settings" />
          {isLoading ? (
            <Loading />
          ) : settings && settings.length > 0 ? (
            <CardContent className="space-y-3">
              {settings.map((s) => (
                <div key={s.id} className="flex items-center justify-between rounded-lg border border-line px-3 py-2">
                  <div>
                    <p className="text-sm font-medium text-ink">{s.name}</p>
                    <p className="text-xs text-ink-muted">{s.taxType}{s.threshold ? ` · threshold ৳${s.threshold.toLocaleString()}` : ""}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-sm font-bold text-primary-700">{s.rate}%</span>
                    <StatusBadge value={s.isActive ? "Active" : "Inactive"} />
                  </div>
                </div>
              ))}
            </CardContent>
          ) : (
            <EmptyState title="No tax settings" />
          )}
        </Card>

        <Card>
          <CardHeader title={`Tax Transactions (${year})`} />
          {isLoading ? (
            <Loading />
          ) : transactions && transactions.length > 0 ? (
            <CardContent className="space-y-3">
              {transactions.map((t) => (
                <div key={t.id} className="flex items-center justify-between rounded-lg border border-line px-3 py-2">
                  <div>
                    <p className="text-sm font-medium text-ink">{t.taxTypeName}</p>
                    <p className="text-xs text-ink-muted">
                      {t.referenceNumber} · {formatDate(t.transactionDate)}
                      {t.challanNo ? ` · Challan ${t.challanNo}` : ""}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-semibold text-ink">{formatCurrency(t.taxAmount)}</p>
                    <p className="text-xs text-ink-muted">on {formatCurrency(t.taxableAmount)}</p>
                  </div>
                </div>
              ))}
            </CardContent>
          ) : (
            <EmptyState title="No tax transactions" />
          )}
        </Card>
      </div>

      <CreateSettingDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createSetting.mutate(v)} />
      <CreateTransDialog
        open={transOpen}
        onClose={() => setTransOpen(false)}
        settings={settings ?? []}
        year={year}
        onSubmit={(v) => createTrans.mutate(v)}
      />
    </div>
  );
}

function CreateSettingDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({ taxType: "VAT", name: "", rate: "15", threshold: "", description: "" });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="New Tax Setting"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!form.name || !form.rate} onClick={() => onSubmit({
            taxType: form.taxType,
            name: form.name,
            rate: Number(form.rate),
            threshold: form.threshold ? Number(form.threshold) : null,
            description: form.description || null
          })}>
            Save
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Tax Type">
          <Select value={form.taxType} onChange={(e) => setForm({ ...form, taxType: e.target.value })}>
            {["VAT", "Income Tax", "Advance Tax", "Other"].map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </Select>
        </Field>
        <Field label="Name" required>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </Field>
        <Field label="Rate (%)" required>
          <Input type="number" value={form.rate} onChange={(e) => setForm({ ...form, rate: e.target.value })} />
        </Field>
        <Field label="Threshold (BDT)">
          <Input type="number" value={form.threshold} onChange={(e) => setForm({ ...form, threshold: e.target.value })} />
        </Field>
        <Field label="Description" className="sm:col-span-2">
          <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}

function CreateTransDialog({
  open,
  onClose,
  settings,
  year,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  settings: import("@/types/models").TaxSetting[];
  year: number;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({
    taxSettingId: "",
    taxableAmount: "",
    transactionDate: new Date().toISOString().slice(0, 10),
    challanNo: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Record Tax Transaction"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.taxSettingId || !form.taxableAmount}
            onClick={() =>
              onSubmit({
                taxSettingId: form.taxSettingId,
                taxableAmount: Number(form.taxableAmount),
                transactionDate: new Date(form.transactionDate).toISOString(),
                challanNo: form.challanNo || null,
                year
              })
            }
          >
            Record
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Tax Setting" required className="sm:col-span-2">
          <Select value={form.taxSettingId} onChange={(e) => setForm({ ...form, taxSettingId: e.target.value })}>
            <option value="">Select a tax type</option>
            {settings.map((s) => (
              <option key={s.id} value={s.id}>{s.name} ({s.rate}%)</option>
            ))}
          </Select>
        </Field>
        <Field label="Taxable Amount (BDT)" required>
          <Input type="number" value={form.taxableAmount} onChange={(e) => setForm({ ...form, taxableAmount: e.target.value })} />
        </Field>
        <Field label="Transaction Date">
          <Input type="date" value={form.transactionDate} onChange={(e) => setForm({ ...form, transactionDate: e.target.value })} />
        </Field>
        <Field label="Challan No." className="sm:col-span-2">
          <Input value={form.challanNo} onChange={(e) => setForm({ ...form, challanNo: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
