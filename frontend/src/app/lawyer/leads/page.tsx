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
import { useLeads } from "@/lib/hooks";
import { leadService } from "@/lib/services";
import { getErrorMessage, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { UserPlus, Plus, TrendingUp } from "lucide-react";
import type { Lead } from "@/types/models";

const stages = ["New", "Contacted", "Qualified", "Proposal", "Won", "Lost"];

export default function LeadsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);
  const { data: leads, isLoading } = useLeads();

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => leadService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["leads"] });
      setCreateOpen(false);
      toast.success("Lead added");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const stageMutation = useMutation({
    mutationFn: ({ id, stage }: { id: string; stage: string }) => leadService.updateStage(id, stage),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["leads"] });
      toast.success("Lead updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title="Leads"
        subtitle="Track prospective clients through your intake funnel."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> Add Lead
          </Button>
        }
      />

      <div className="mb-6 grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-6">
        {stages.map((s) => (
          <Card key={s} className="p-4 text-center">
            <p className="text-2xl font-bold text-ink">
              {leads?.filter((l) => l.stage === s).length ?? 0}
            </p>
            <p className="mt-1 text-xs text-ink-muted">{s}</p>
          </Card>
        ))}
      </div>

      <Card>
        {isLoading ? (
          <Loading />
        ) : leads && leads.length > 0 ? (
          <table className="table-base">
            <thead>
              <tr>
                <th>Lead</th>
                <th>Contact</th>
                <th>Source</th>
                <th>Value</th>
                <th>Stage</th>
                <th>Date</th>
              </tr>
            </thead>
            <tbody>
              {leads.map((l) => (
                <tr key={l.id}>
                  <td className="font-medium text-ink">{l.name}</td>
                  <td>
                    <p className="text-ink">{l.phone}</p>
                    <p className="text-xs text-ink-muted">{l.email}</p>
                  </td>
                  <td className="text-ink-muted">{l.leadSource ?? "—"}</td>
                  <td className="font-medium text-ink">{l.estimatedValue ? `৳${l.estimatedValue.toLocaleString()}` : "—"}</td>
                  <td>
                    <Select
                      className="h-8 w-32 text-xs"
                      value={l.stage}
                      onChange={(e) => stageMutation.mutate({ id: l.id, stage: e.target.value })}
                    >
                      {stages.map((s) => (
                        <option key={s} value={s}>{s}</option>
                      ))}
                    </Select>
                  </td>
                  <td className="text-ink-muted">{formatDate(l.createdAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <EmptyState
            icon={<UserPlus className="h-10 w-10" />}
            title="No leads yet"
            description="Capture prospective clients as they come in."
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> Add Lead</Button>}
          />
        )}
      </Card>

      <CreateLeadDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateLeadDialog({
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
    phone: "",
    email: "",
    source: "Referral",
    estimatedValue: "",
    notes: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Add Lead"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.name}
            onClick={() =>
              onSubmit({
                name: form.name,
                phone: form.phone || null,
                email: form.email || null,
                source: form.source || null,
                estimatedValue: form.estimatedValue ? Number(form.estimatedValue) : null,
                notes: form.notes || null,
                stage: "New"
              })
            }
          >
            Add Lead
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Lead Name" required className="sm:col-span-2">
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </Field>
        <Field label="Phone">
          <Input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
        </Field>
        <Field label="Email">
          <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
        </Field>
        <Field label="Source">
          <Select value={form.source} onChange={(e) => setForm({ ...form, source: e.target.value })}>
            {["Referral", "Website", "Walk-in", "Social Media", "Other"].map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </Select>
        </Field>
        <Field label="Estimated Value (BDT)">
          <Input type="number" value={form.estimatedValue} onChange={(e) => setForm({ ...form, estimatedValue: e.target.value })} />
        </Field>
        <Field label="Notes" className="sm:col-span-2">
          <Textarea rows={3} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
