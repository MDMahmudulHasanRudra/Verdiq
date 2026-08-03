"use client";

import { useMemo, useState } from "react";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { StatCard } from "@/components/ui/stat-card";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useLeads } from "@/lib/hooks";
import { leadService } from "@/lib/services";
import { getErrorMessage, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { UserPlus, Plus, TrendingUp, Search, Pencil, Trash2, Handshake, XCircle } from "lucide-react";
import type { Lead } from "@/types/models";

const stages = ["New", "Contacted", "Qualified", "Proposal", "Won", "Lost"];

export default function LeadsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [createOpen, setCreateOpen] = useState(false);
  const [editLead, setEditLead] = useState<Lead | null>(null);
  const [deleteLead, setDeleteLead] = useState<Lead | null>(null);
  const [query, setQuery] = useState("");
  const { data: leads, isLoading } = useLeads();

  const filtered = useMemo(() => {
    if (!query.trim()) return leads;
    const q = query.trim().toLowerCase();
    return leads?.filter(
      (l) =>
        l.name.toLowerCase().includes(q) ||
        l.phone.toLowerCase().includes(q) ||
        (l.email ?? "").toLowerCase().includes(q) ||
        (l.leadSource ?? "").toLowerCase().includes(q)
    );
  }, [leads, query]);

  const invalidate = () => qc.invalidateQueries({ queryKey: ["leads"] });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => leadService.create(input),
    onSuccess: () => {
      invalidate();
      setCreateOpen(false);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: Record<string, unknown> }) =>
      leadService.update(id, input),
    onSuccess: () => {
      invalidate();
      setEditLead(null);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const stageMutation = useMutation({
    mutationFn: ({ id, stage }: { id: string; stage: string }) => leadService.updateStage(id, stage),
    onSuccess: () => {
      invalidate();
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => leadService.remove(id),
    onSuccess: () => {
      invalidate();
      setDeleteLead(null);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const renderLeads = query ? filtered : leads;

  return (
    <div>
      <PageHeader
        title={t("leads.title")}
        subtitle={t("leads.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("leads.addLead")}
          </Button>
        }
      />

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          label={t("leads.new")}
          value={leads?.filter((l) => l.stage === "New").length ?? 0}
          icon={<Plus className="h-5 w-5" />}
          accent="blue"
        />
        <StatCard
          label={t("leads.proposal")}
          value={leads?.filter((l) => l.stage === "Proposal").length ?? 0}
          icon={<TrendingUp className="h-5 w-5" />}
          accent="gold"
        />
        <StatCard
          label={t("leads.won")}
          value={leads?.filter((l) => l.stage === "Won").length ?? 0}
          icon={<Handshake className="h-5 w-5" />}
          accent="green"
        />
        <StatCard
          label={t("leads.lost")}
          value={leads?.filter((l) => l.stage === "Lost").length ?? 0}
          icon={<XCircle className="h-5 w-5" />}
          accent="red"
        />
      </div>

      <div className="mb-4 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="relative w-full max-w-sm">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
          <Input
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder={`${t("common.search")} ${t("leads.lead")}…`}
            className="pl-9"
          />
        </div>
        <p className="text-sm text-ink-muted">
          {t("common.total")}: {renderLeads?.length ?? 0}
        </p>
      </div>

      <Card>
        {isLoading ? (
          <Loading />
        ) : renderLeads && renderLeads.length > 0 ? (
          <table className="table-base">
            <thead>
              <tr>
                <th>{t("leads.lead")}</th>
                <th>{t("leads.contact")}</th>
                <th>{t("leads.source")}</th>
                <th>{t("leads.value")}</th>
                <th>{t("leads.stage")}</th>
                <th>{t("common.date")}</th>
                <th className="text-right">{t("common.actions")}</th>
              </tr>
            </thead>
            <tbody>
              {renderLeads.map((l) => (
                <tr key={l.id}>
                  <td className="font-medium text-ink">{l.name}</td>
                  <td>
                    <p className="text-ink">{l.phone}</p>
                    <p className="text-xs text-ink-muted">{l.email ?? "—"}</p>
                  </td>
                  <td className="text-ink-muted">{l.leadSource ?? "—"}</td>
                  <td className="font-medium text-ink">
                    {l.estimatedValue ? `৳${l.estimatedValue.toLocaleString()}` : "—"}
                  </td>
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
                  <td>
                    <div className="flex items-center justify-end gap-1">
                      <Button variant="ghost" size="icon" onClick={() => setEditLead(l)} aria-label={t("common.edit")}>
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button variant="ghost" size="icon" onClick={() => setDeleteLead(l)} aria-label={t("common.delete")}>
                        <Trash2 className="h-4 w-4 text-red-500" />
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <EmptyState
            icon={<UserPlus className="h-10 w-10" />}
            title={t(query ? "common.noResults" : "leads.noLeads")}
            description={query ? undefined : t("leads.noLeadsDesc")}
            action={
              !query ? (
                <Button onClick={() => setCreateOpen(true)}>
                  <Plus className="h-4 w-4" /> {t("leads.addLead")}
                </Button>
              ) : null
            }
          />
        )}
      </Card>

      <LeadFormDialog
        open={createOpen}
        title={t("leads.addLead")}
        submitLabel={t("common.create")}
        onClose={() => setCreateOpen(false)}
        onSubmit={(v) => createMutation.mutate(v)}
      />

      <LeadFormDialog
        open={!!editLead}
        editing={editLead}
        title={t("common.edit")}
        submitLabel={t("common.save")}
        onClose={() => setEditLead(null)}
        onSubmit={(v) => editLead && updateMutation.mutate({ id: editLead.id, input: v })}
      />

      <Dialog
        open={!!deleteLead}
        onClose={() => setDeleteLead(null)}
        title={t("common.delete")}
        size="sm"
        footer={
          <>
            <Button variant="ghost" onClick={() => setDeleteLead(null)}>{t("common.cancel")}</Button>
            <Button variant="danger" onClick={() => deleteLead && deleteMutation.mutate(deleteLead.id)}>
              {t("common.delete")}
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">
          {t("documents.deleteConfirm")} <span className="font-medium text-ink">{deleteLead?.name}</span>
        </p>
      </Dialog>
    </div>
  );
}

function LeadFormDialog({
  open,
  onClose,
  onSubmit,
  editing,
  title,
  submitLabel
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
  editing?: Lead | null;
  title: string;
  submitLabel: string;
}) {
  const { t } = useLanguage();
  const [form, setForm] = useState(() => ({
    name: editing?.name ?? "",
    phone: editing?.phone ?? "",
    email: editing?.email ?? "",
    source: editing?.leadSource ?? "Direct",
    estimatedValue: editing?.estimatedValue ? String(editing.estimatedValue) : "",
    notes: editing?.notes ?? ""
  }));

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={title}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button
            disabled={!form.name}
            onClick={() =>
              onSubmit({
                name: form.name,
                phone: form.phone || null,
                email: form.email || null,
                source: form.source || null,
                estimatedValue: form.estimatedValue ? Number(form.estimatedValue) : null,
                notes: form.notes || null
              })
            }
          >
            {submitLabel}
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("leads.leadName")} required className="sm:col-span-2">
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </Field>
        <Field label={t("common.phone")}>
          <Input value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
        </Field>
        <Field label={t("common.email")}>
          <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
        </Field>
        <Field label={t("leads.source")}>
          <Select value={form.source} onChange={(e) => setForm({ ...form, source: e.target.value })}>
            {["Direct", "Referral", "Website", "Walk-in", "Social Media", "Other"].map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </Select>
        </Field>
        <Field label={t("leads.estimatedValue")}>
          <Input type="number" value={form.estimatedValue} onChange={(e) => setForm({ ...form, estimatedValue: e.target.value })} />
        </Field>
        <Field label={t("common.notes")} className="sm:col-span-2">
          <Textarea rows={3} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}