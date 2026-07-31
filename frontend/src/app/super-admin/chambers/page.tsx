"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/badge";
import { Table } from "@/components/ui/table";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { superAdminService } from "@/lib/services/super-admin-service";
import { getErrorMessage, formatCurrency, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Building2, Plus } from "lucide-react";
import type { SuperAdminChamber } from "@/types/super-admin";

export default function SuperAdminChambersPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ["super-admin", "chambers"],
    queryFn: () => superAdminService.chambers()
  });

  const createMutation = useMutation({
    mutationFn: (input: { name: string; address?: string; phone?: string; plan?: string }) =>
      superAdminService.createChamber(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["super-admin", "chambers"] });
      qc.invalidateQueries({ queryKey: ["super-admin", "dashboard"] });
      setCreateOpen(false);
      toast.success("Chamber created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const planMutation = useMutation({
    mutationFn: (v: { id: string; plan: string }) => superAdminService.updateChamberPlan(v.id, v.plan),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["super-admin", "chambers"] });
      toast.success("Plan updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => superAdminService.deleteChamber(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["super-admin", "chambers"] });
      toast.success("Chamber deleted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="font-display text-2xl font-bold tracking-tight text-white">Chambers</h1>
          <p className="mt-1 text-sm text-slate-400">Manage every law chamber on the platform.</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="h-4 w-4" /> Create Chamber
        </Button>
      </div>

      <Card className="border-slate-800 bg-slate-900">
        {isLoading ? (
          <Loading dark />
        ) : data && data.length > 0 ? (
          <Table className="dark-table">
            <thead>
              <tr>
                <th>Chamber</th>
                <th>Plan</th>
                <th>Users</th>
                <th>Cases</th>
                <th>Revenue</th>
                <th>Created</th>
                <th>Status</th>
                <th className="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.map((c: SuperAdminChamber) => (
                <tr key={c.id}>
                  <td>
                    <p className="font-medium text-white">{c.name}</p>
                    <p className="text-xs text-slate-400">{c.address ?? "—"}</p>
                  </td>
                  <td>
                    <Select
                      className="w-32 border-slate-700 bg-slate-800 text-white"
                      value={c.subscriptionPlan}
                      onChange={(e) => planMutation.mutate({ id: c.id, plan: e.target.value })}
                    >
                      <option value="Free">Free</option>
                      <option value="Pro">Pro</option>
                      <option value="Chamber">Chamber</option>
                    </Select>
                  </td>
                  <td className="text-slate-300">{c.usersCount}</td>
                  <td className="text-slate-300">{c.casesCount}</td>
                  <td className="text-slate-300">{formatCurrency(c.totalRevenue)}</td>
                  <td className="text-slate-400">{formatDate(c.createdAt)}</td>
                  <td><StatusBadge value={c.isActive ? "Active" : "Inactive"} /></td>
                  <td className="text-right">
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-red-400 hover:bg-red-500/10 hover:text-red-300"
                      onClick={() => {
                        if (confirm(`Delete ${c.name}? This cannot be undone.`)) deleteMutation.mutate(c.id);
                      }}
                    >
                      Delete
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <EmptyState icon={<Building2 className="h-10 w-10" />} title="No chambers" description="Create the first chamber to get started." />
        )}
      </Card>

      <CreateChamberDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateChamberDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: { name: string; address?: string; phone?: string; plan?: string }) => void;
}) {
  const [form, setForm] = useState({ name: "", address: "", phone: "", plan: "Pro" });
  const isFormValid = !!form.name;

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Create Chamber"
      description="Register a new law chamber."
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!isFormValid} onClick={() => onSubmit({ ...form, address: form.address || undefined, phone: form.phone || undefined })}>
            Create Chamber
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Chamber Name" required className="sm:col-span-2">
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="e.g. Rahman & Associates" />
        </Field>
        <Field label="Address" className="sm:col-span-2">
          <Textarea rows={2} value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
        </Field>
        <Field label="Phone">
          <Input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
        </Field>
        <Field label="Plan">
          <Select value={form.plan} onChange={(e) => setForm({ ...form, plan: e.target.value })}>
            <option value="Free">Free</option>
            <option value="Pro">Pro</option>
            <option value="Chamber">Chamber</option>
          </Select>
        </Field>
      </div>
    </Dialog>
  );
}
