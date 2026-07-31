"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { expenseService } from "@/lib/services";
import { getErrorMessage, formatCurrency, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Wallet, Plus } from "lucide-react";

const categories = ["Travel", "Filing Fees", "Office Supplies", "Utilities", "Salary", "Other"];

export default function ExpensesPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [category, setCategory] = useState("");
  const [createOpen, setCreateOpen] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ["expenses", category],
    queryFn: () => expenseService.list({ category: category || undefined })
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => expenseService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["expenses"] });
      setCreateOpen(false);
      toast.success("Expense recorded");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const total = data?.data?.reduce((s, e) => s + e.amount, 0) ?? 0;

  return (
    <div>
      <PageHeader
        title="Expenses"
        subtitle="Track firm expenditures and reimbursables."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> Record Expense
          </Button>
        }
      />

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Total (filtered)</p>
          <p className="mt-1 text-2xl font-bold text-ink">{formatCurrency(total)}</p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Records</p>
          <p className="mt-1 text-2xl font-bold text-ink">{data?.data?.length ?? 0}</p>
        </Card>
      </div>

      <Card className="mb-4 p-4">
        <Select className="sm:w-48" value={category} onChange={(e) => setCategory(e.target.value)}>
          <option value="">All categories</option>
          {categories.map((c) => (
            <option key={c} value={c}>{c}</option>
          ))}
        </Select>
      </Card>

      <Card>
        {isLoading ? (
          <Loading />
        ) : data && data.data.length > 0 ? (
          <table className="table-base">
            <thead>
              <tr>
                <th>Description</th>
                <th>Category</th>
                <th>Case</th>
                <th>Amount</th>
                <th>Date</th>
                <th>Recorded By</th>
              </tr>
            </thead>
            <tbody>
              {data.data.map((e) => (
                <tr key={e.id}>
                  <td className="font-medium text-ink">{e.description}</td>
                  <td className="text-ink-muted">{e.category}</td>
                  <td className="max-w-40 truncate text-ink-muted">{e.caseTitle ?? "—"}</td>
                  <td className="font-medium text-ink">{formatCurrency(e.amount)}</td>
                  <td className="text-ink-muted">{formatDate(e.expenseDate ?? e.createdAt)}</td>
                  <td className="text-ink-muted">{e.createdByName}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <EmptyState
            icon={<Wallet className="h-10 w-10" />}
            title="No expenses"
            description="Record expenses for reimbursement and reporting."
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> Record Expense</Button>}
          />
        )}
      </Card>

      <CreateExpenseDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateExpenseDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({
    description: "",
    amount: "",
    category: "Travel",
    expenseDate: new Date().toISOString().slice(0, 10),
    caseId: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Record Expense"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.description || !form.amount}
            onClick={() =>
              onSubmit({
                description: form.description,
                amount: Number(form.amount),
                category: form.category,
                expenseDate: new Date(form.expenseDate).toISOString(),
                caseId: form.caseId || null
              })
            }
          >
            Record
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Description" required className="sm:col-span-2">
          <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
        <Field label="Amount (BDT)" required>
          <Input type="number" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} />
        </Field>
        <Field label="Category">
          <Select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
        </Field>
        <Field label="Date">
          <Input type="date" value={form.expenseDate} onChange={(e) => setForm({ ...form, expenseDate: e.target.value })} />
        </Field>
        <Field label="Case ID">
          <Input value={form.caseId} onChange={(e) => setForm({ ...form, caseId: e.target.value })} placeholder="Optional" />
        </Field>
      </div>
    </Dialog>
  );
}
