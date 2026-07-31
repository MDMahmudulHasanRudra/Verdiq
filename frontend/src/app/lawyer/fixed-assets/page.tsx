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
import { fixedAssetService } from "@/lib/services";
import { getErrorMessage, formatCurrency, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Boxes, Plus } from "lucide-react";

export default function FixedAssetsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);

  const { data: assets, isLoading } = useQuery({
    queryKey: ["fixed-assets"],
    queryFn: () => fixedAssetService.list()
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => fixedAssetService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["fixed-assets"] });
      setCreateOpen(false);
      toast.success("Asset added");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const dispose = useMutation({
    mutationFn: ({ id, input }: { id: string; input: { disposalDate: string; reason: string } }) =>
      fixedAssetService.dispose(id, input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["fixed-assets"] });
      toast.success("Asset disposed");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const totalValue = (assets ?? []).reduce((s, a) => s + a.currentValue, 0);

  return (
    <div>
      <PageHeader
        title="Fixed Assets"
        subtitle="Track firm equipment, vehicles and property with depreciation."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> Add Asset
          </Button>
        }
      />

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Assets</p>
          <p className="mt-1 text-2xl font-bold text-ink">{assets?.length ?? 0}</p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Total Book Value</p>
          <p className="mt-1 text-2xl font-bold text-ink">{formatCurrency(totalValue)}</p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Active Assets</p>
          <p className="mt-1 text-2xl font-bold text-emerald-600">
            {(assets ?? []).filter((a) => a.status !== "Disposed").length}
          </p>
        </Card>
      </div>

      <Card>
        {isLoading ? (
          <Loading />
        ) : assets && assets.length > 0 ? (
          <table className="table-base">
            <thead>
              <tr>
                <th>Asset</th>
                <th>Category</th>
                <th>Purchase Date</th>
                <th>Cost</th>
                <th>Book Value</th>
                <th>Depreciation</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {assets.map((a) => (
                <tr key={a.id}>
                  <td>
                    <p className="font-medium text-ink">{a.name}</p>
                    <p className="text-xs text-ink-muted">{a.assetCode}</p>
                  </td>
                  <td className="text-ink-muted">{a.category}</td>
                  <td className="text-ink-muted">{formatDate(a.purchaseDate)}</td>
                  <td className="text-ink">{formatCurrency(a.purchaseCost)}</td>
                  <td className="font-medium text-ink">{formatCurrency(a.currentValue)}</td>
                  <td className="text-ink-muted">
                    {a.depreciationMethod ? `${a.depreciationMethod}` : "—"}
                  </td>
                  <td><StatusBadge value={a.status} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <EmptyState
            icon={<Boxes className="h-10 w-10" />}
            title="No fixed assets"
            description="Add firm assets to track depreciation."
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> Add Asset</Button>}
          />
        )}
      </Card>

      <CreateAssetDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateAssetDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({
    name: "",
    category: "Office Equipment",
    purchaseCost: "",
    purchaseDate: new Date().toISOString().slice(0, 10),
    depreciationMethod: "Straight-Line",
    serialNumber: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Add Asset"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.name || !form.purchaseCost}
            onClick={() =>
              onSubmit({
                name: form.name,
                category: form.category,
                purchaseCost: Number(form.purchaseCost),
                purchaseDate: new Date(form.purchaseDate).toISOString(),
                depreciationMethod: form.depreciationMethod,
                serialNumber: form.serialNumber || null
              })
            }
          >
            Add Asset
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Asset Name" required>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </Field>
        <Field label="Category">
          <Select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
            {["Office Equipment", "Furniture", "Vehicles", "Property", "Electronics", "Other"].map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
        </Field>
        <Field label="Purchase Cost (BDT)" required>
          <Input type="number" value={form.purchaseCost} onChange={(e) => setForm({ ...form, purchaseCost: e.target.value })} />
        </Field>
        <Field label="Purchase Date">
          <Input type="date" value={form.purchaseDate} onChange={(e) => setForm({ ...form, purchaseDate: e.target.value })} />
        </Field>
        <Field label="Depreciation Method">
          <Select value={form.depreciationMethod} onChange={(e) => setForm({ ...form, depreciationMethod: e.target.value })}>
            {["Straight-Line", "Reducing Balance", "Sum-of-Years"].map((m) => (
              <option key={m} value={m}>{m}</option>
            ))}
          </Select>
        </Field>
        <Field label="Serial Number">
          <Input value={form.serialNumber} onChange={(e) => setForm({ ...form, serialNumber: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
