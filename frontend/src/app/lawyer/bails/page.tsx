"use client";

import { useState } from "react";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Badge, StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { bailService, caseService } from "@/lib/services";
import { getErrorMessage, formatDate, formatDateTime } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import {
  Handshake, Plus, Search, FileText, CalendarDays, DollarSign,
  User, Phone, MapPin, Clock, CheckCircle2, XCircle, Ban,
  AlertCircle, Edit, Trash2, ExternalLink, History
} from "lucide-react";
import type { Bail } from "@/types/models";

const bailStatuses = ["Pending", "Granted", "Revoked", "Forfeited", "Cancelled"];
const bailTypes = ["Regular", "Anticipatory"];

export default function BailsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [createOpen, setCreateOpen] = useState(false);
  const [detailBail, setDetailBail] = useState<Bail | null>(null);
  const [editBail, setEditBail] = useState<Bail | null>(null);
  const [statusFilter, setStatusFilter] = useState("");
  const [searchQuery, setSearchQuery] = useState("");

  const { data: bails, isLoading } = useQuery({
    queryKey: ["bails", statusFilter],
    queryFn: () => bailService.list(statusFilter || undefined)
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => bailService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["bails"] });
      setCreateOpen(false);
      toast.success("Bail record created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: Record<string, unknown> }) =>
      bailService.update(id, input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["bails"] });
      setEditBail(null);
      toast.success("Bail record updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const statusMutation = useMutation({
    mutationFn: ({ id, status, reason }: { id: string; status: string; reason?: string }) =>
      bailService.updateStatus(id, { status, revokedReason: reason }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["bails"] });
      toast.success("Bail status updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => bailService.remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["bails"] });
      toast.success("Bail record deleted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const filteredBails = (bails ?? []).filter((b) => {
    if (!searchQuery) return true;
    const q = searchQuery.toLowerCase();
    return (
      b.caseNumber?.toLowerCase().includes(q) ||
      b.caseTitle?.toLowerCase().includes(q) ||
      b.suretyName?.toLowerCase().includes(q) ||
      b.bondNumber?.toLowerCase().includes(q)
    );
  });

  const stats = {
    total: bails?.length ?? 0,
    pending: bails?.filter((b) => b.status === "Pending").length ?? 0,
    granted: bails?.filter((b) => b.status === "Granted").length ?? 0,
    revoked: bails?.filter((b) => b.status === "Revoked").length ?? 0,
    totalAmount: bails?.reduce((sum, b) => sum + (b.bailAmount ?? 0), 0) ?? 0,
    grantedAmount: bails?.filter((b) => b.status === "Granted").reduce((sum, b) => sum + (b.bailAmount ?? 0), 0) ?? 0,
    anticipatory: bails?.filter((b) => b.bailType === "Anticipatory").length ?? 0
  };

  return (
    <div>
      <PageHeader
        title={t("bails.title")}
        subtitle={t("bails.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("bails.addBail")}
          </Button>
        }
      />

      <div className="mb-6 grid grid-cols-2 gap-3 sm:grid-cols-5">
        <Card className="p-3">
          <p className="text-2xl font-bold text-ink">{stats.total}</p>
          <p className="text-xs text-ink-muted">Total Bails</p>
        </Card>
        <Card className="p-3">
          <p className="text-2xl font-bold text-amber-600">{stats.pending}</p>
          <p className="text-xs text-ink-muted">Pending</p>
        </Card>
        <Card className="p-3">
          <p className="text-2xl font-bold text-emerald-600">{stats.granted}</p>
          <p className="text-xs text-ink-muted">Granted</p>
        </Card>
        <Card className="p-3">
          <p className="text-2xl font-bold text-purple-600">{stats.anticipatory}</p>
          <p className="text-xs text-ink-muted">Anticipatory</p>
        </Card>
        <Card className="p-3">
          <p className="text-2xl font-bold text-ink">{stats.grantedAmount > 0 ? `৳${stats.grantedAmount.toLocaleString()}` : "—"}</p>
          <p className="text-xs text-ink-muted">Granted Amount</p>
        </Card>
      </div>

      <Card className="mb-4">
        <div className="flex flex-wrap items-center gap-3 p-4">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
            <Input
              placeholder="Search by case, surety, bond number..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="h-9 pl-9"
            />
          </div>
          <Select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="h-9 w-44">
            <option value="">All statuses</option>
            {bailStatuses.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </Select>
        </div>
      </Card>

      <Card>
        {isLoading ? (
          <Loading />
        ) : filteredBails.length > 0 ? (
          <div className="divide-y divide-line">
            {filteredBails.map((b) => (
              <button
                key={b.id}
                onClick={() => setDetailBail(b)}
                className="flex w-full items-center justify-between gap-4 px-4 py-3 text-left transition-colors hover:bg-slate-50/50"
              >
                <div className="flex items-center gap-4">
                  <div className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-lg ${
                    b.status === "Granted" ? "bg-emerald-50" :
                    b.status === "Revoked" ? "bg-red-50" :
                    b.status === "Pending" ? "bg-amber-50" : "bg-slate-50"
                  }`}>
                    <Handshake className={`h-5 w-5 ${
                      b.status === "Granted" ? "text-emerald-600" :
                      b.status === "Revoked" ? "text-red-600" :
                      b.status === "Pending" ? "text-amber-600" : "text-slate-500"
                    }`} />
                  </div>
                  <div className="min-w-0">
                    <div className="flex items-center gap-2">
                      <p className="text-sm font-medium text-ink">{b.caseNumber}</p>
                      <StatusBadge value={b.status} />
                      {b.bailType === "Anticipatory" ? <Badge tone="purple">Anticipatory</Badge> : null}
                    </div>
                    <p className="truncate text-xs text-ink-muted">{b.caseTitle}</p>
                  </div>
                </div>
                <div className="hidden shrink-0 text-right sm:block">
                  <p className="text-sm font-medium text-ink">
                    {b.bailAmount ? `৳${b.bailAmount.toLocaleString()}` : "—"}
                  </p>
                  <p className="text-xs text-ink-muted">
                    {b.nextHearingDate
                      ? `Next: ${formatDate(b.nextHearingDate)}`
                      : b.bailHearingDate
                        ? formatDate(b.bailHearingDate)
                        : "No hearing date"}
                  </p>
                </div>
              </button>
            ))}
          </div>
        ) : (
          <EmptyState
            icon={<Handshake className="h-10 w-10" />}
            title={t("bails.noBails")}
            description={t("bails.noBailsDesc")}
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> {t("bails.addBail")}</Button>}
          />
        )}
      </Card>

      <CreateBailDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />

      {detailBail && (
        <BailDetailDialog
          bail={detailBail}
          open={!!detailBail}
          onClose={() => setDetailBail(null)}
          onEdit={() => { setEditBail(detailBail); setDetailBail(null); }}
          onStatusChange={(status, reason) => statusMutation.mutate({ id: detailBail.id, status, reason })}
          onDelete={() => { deleteMutation.mutate(detailBail.id); setDetailBail(null); }}
        />
      )}

      {editBail && (
        <EditBailDialog
          bail={editBail}
          open={!!editBail}
          onClose={() => setEditBail(null)}
          onSubmit={(v) => updateMutation.mutate({ id: editBail.id, input: v })}
        />
      )}
    </div>
  );
}

function BailDetailDialog({
  bail,
  open,
  onClose,
  onEdit,
  onStatusChange,
  onDelete
}: {
  bail: Bail;
  open: boolean;
  onClose: () => void;
  onEdit: () => void;
  onStatusChange: (status: string, reason?: string) => void;
  onDelete: () => void;
}) {
  const [statusAction, setStatusAction] = useState<string>("");
  const [revokeReason, setRevokeReason] = useState("");

  const statusActions = [
    { value: "Granted", label: "Grant Bail", icon: CheckCircle2, color: "text-emerald-600", bg: "bg-emerald-50 hover:bg-emerald-100" },
    { value: "Revoked", label: "Revoke Bail", icon: XCircle, color: "text-red-600", bg: "bg-red-50 hover:bg-red-100" },
    { value: "Forfeited", label: "Forfeit Bail", icon: Ban, color: "text-orange-600", bg: "bg-orange-50 hover:bg-orange-100" },
    { value: "Cancelled", label: "Cancel Bail", icon: AlertCircle, color: "text-slate-600", bg: "bg-slate-50 hover:bg-slate-100" }
  ].filter((a) => a.value !== bail.status);

  return (
    <Dialog open={open} onClose={onClose} title="Bail Record" size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Close</Button>
          <Button variant="outline" onClick={onEdit}><Edit className="h-4 w-4" /> Edit</Button>
          <Button variant="danger" onClick={() => { if (confirm("Delete this bail record?")) onDelete(); }}>
            <Trash2 className="h-4 w-4" /> Delete
          </Button>
        </>
      }
    >
      <div className="space-y-6">
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-2">
              <h3 className="text-lg font-semibold text-ink">{bail.caseNumber}</h3>
              <StatusBadge value={bail.status} />
              {bail.bailType === "Anticipatory" ? <Badge tone="purple">Anticipatory</Badge> : <Badge tone="slate">Regular</Badge>}
            </div>
            <p className="text-sm text-ink-muted">{bail.caseTitle}</p>
          </div>
          {bail.bailAmount ? (
            <div className="text-right">
              <p className="text-2xl font-bold text-ink">৳{bail.bailAmount.toLocaleString()}</p>
              <p className="text-xs text-ink-muted">Bail Amount</p>
            </div>
          ) : null}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <InfoItem icon={CalendarDays} label="Initial Hearing" value={bail.bailHearingDate ? formatDate(bail.bailHearingDate) : "—"} />
          <InfoItem icon={CalendarDays} label="Next Hearing" value={bail.nextHearingDate ? formatDate(bail.nextHearingDate) : "—"} />
          <InfoItem icon={Clock} label="Granted At" value={bail.bailGrantedAt ? formatDateTime(bail.bailGrantedAt) : "—"} />
          <InfoItem icon={FileText} label="Bond Number" value={bail.bondNumber ?? "—"} />
          <InfoItem icon={User} label="Granted By" value={bail.grantedBy ?? "—"} />
          <InfoItem icon={DollarSign} label="Bail Type" value={bail.bailType ?? "Regular"} />
        </div>

        {bail.bailConditions ? (
          <div>
            <h4 className="mb-1 text-sm font-semibold text-ink">Bail Conditions</h4>
            <p className="whitespace-pre-wrap rounded-lg bg-slate-50 p-3 text-sm text-ink-muted">{bail.bailConditions}</p>
          </div>
        ) : null}

        <div>
          <h4 className="mb-2 text-sm font-semibold text-ink">Surety Details</h4>
          <div className="grid grid-cols-2 gap-3">
            <InfoItem icon={User} label="Name" value={bail.suretyName ?? "—"} />
            <InfoItem icon={Phone} label="Contact" value={bail.suretyContact ?? "—"} />
            <InfoItem icon={MapPin} label="Address" value={bail.suretyAddress ?? "—"} span2 />
          </div>
        </div>

        {bail.status === "Revoked" && bail.revokedReason ? (
          <div className="rounded-lg border border-red-200 bg-red-50 p-3">
            <p className="text-sm font-medium text-red-800">Revocation Reason</p>
            <p className="mt-1 text-sm text-red-700">{bail.revokedReason}</p>
            {bail.revokedAt ? <p className="mt-1 text-xs text-red-600">Revoked on {formatDateTime(bail.revokedAt)}</p> : null}
          </div>
        ) : bail.status === "Forfeited" ? (
          <div className="rounded-lg border border-orange-200 bg-orange-50 p-3">
            <p className="text-sm font-medium text-orange-800">Bail Forfeited</p>
            {bail.revokedReason ? <p className="mt-1 text-sm text-orange-700">{bail.revokedReason}</p> : null}
            {bail.revokedAt ? <p className="mt-1 text-xs text-orange-600">Forfeited on {formatDateTime(bail.revokedAt)}</p> : null}
          </div>
        ) : bail.status === "Cancelled" ? (
          <div className="rounded-lg border border-slate-200 bg-slate-50 p-3">
            <p className="text-sm font-medium text-slate-800">Bail Cancelled</p>
            {bail.revokedReason ? <p className="mt-1 text-sm text-slate-700">{bail.revokedReason}</p> : null}
            {bail.revokedAt ? <p className="mt-1 text-xs text-slate-600">Cancelled on {formatDateTime(bail.revokedAt)}</p> : null}
          </div>
        ) : null}

        {bail.notes ? (
          <div>
            <h4 className="mb-1 text-sm font-semibold text-ink">Notes</h4>
            <p className="whitespace-pre-wrap text-sm text-ink-muted">{bail.notes}</p>
          </div>
        ) : null}

        {statusActions.length > 0 ? (
          <div>
            <h4 className="mb-2 text-sm font-semibold text-ink">Change Status</h4>
            <div className="flex flex-wrap gap-2">
              {statusActions.map((a) => (
                <button
                  key={a.value}
                  onClick={() => {
                    if (a.value === "Revoked" || a.value === "Forfeited") {
                      setStatusAction(a.value);
                    } else {
                      onStatusChange(a.value);
                    }
                  }}
                  className={`flex items-center gap-1.5 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${a.bg} ${a.color}`}
                >
                  <a.icon className="h-4 w-4" /> {a.label}
                </button>
              ))}
            </div>
            {statusAction === "Revoked" || statusAction === "Forfeited" ? (
              <div className="mt-3 space-y-2">
                <Textarea
                  rows={2}
                  placeholder={`Reason for ${statusAction.toLowerCase()}...`}
                  value={revokeReason}
                  onChange={(e) => setRevokeReason(e.target.value)}
                />
                <div className="flex gap-2">
                  <Button size="sm" variant="danger" onClick={() => { onStatusChange(statusAction, revokeReason); setStatusAction(""); setRevokeReason(""); }}>
                    Confirm {statusAction}
                  </Button>
                  <Button size="sm" variant="ghost" onClick={() => { setStatusAction(""); setRevokeReason(""); }}>
                    Cancel
                  </Button>
                </div>
              </div>
            ) : null}
          </div>
        ) : null}
      </div>
    </Dialog>
  );
}

function InfoItem({ icon: Icon, label, value, span2 }: { icon: React.ElementType; label: string; value: string; span2?: boolean }) {
  return (
    <div className={span2 ? "sm:col-span-2" : ""}>
      <div className="flex items-center gap-1.5 text-xs text-ink-muted">
        <Icon className="h-3.5 w-3.5" /> {label}
      </div>
      <p className="mt-0.5 text-sm font-medium text-ink">{value}</p>
    </div>
  );
}

function CreateBailDialog({
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
    caseId: "",
    bailType: "Regular",
    bailAmount: "",
    bailConditions: "",
    bailHearingDate: "",
    nextHearingDate: "",
    bondNumber: "",
    suretyName: "",
    suretyAddress: "",
    suretyContact: "",
    grantedBy: "",
    notes: ""
  });

  const { data: casesData } = useQuery({
    queryKey: ["cases"],
    queryFn: () => caseService.list({ pageSize: 200 }),
    enabled: open
  });
  const cases = casesData?.data ?? [];

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("bails.addBail")}
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button
            disabled={!form.caseId}
            onClick={() =>
              onSubmit({
                caseId: form.caseId,
                bailType: form.bailType,
                bailAmount: form.bailAmount ? Number(form.bailAmount) : null,
                bailConditions: form.bailConditions || null,
                bailHearingDate: form.bailHearingDate ? new Date(form.bailHearingDate).toISOString() : null,
                nextHearingDate: form.nextHearingDate ? new Date(form.nextHearingDate).toISOString() : null,
                bondNumber: form.bondNumber || null,
                suretyName: form.suretyName || null,
                suretyAddress: form.suretyAddress || null,
                suretyContact: form.suretyContact || null,
                grantedBy: form.grantedBy || null,
                notes: form.notes || null
              })
            }
          >
            Create
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("bails.case")} required className="sm:col-span-2">
          <Select value={form.caseId} onChange={(e) => setForm({ ...form, caseId: e.target.value })}>
            <option value="">Select a case...</option>
            {cases.map((c) => (
              <option key={c.id} value={c.id}>{c.caseNumber} — {c.title}</option>
            ))}
          </Select>
        </Field>
        <Field label="Bail Type" required>
          <Select value={form.bailType} onChange={(e) => setForm({ ...form, bailType: e.target.value })}>
            {bailTypes.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </Select>
        </Field>
        <Field label="Bail Amount (BDT)">
          <Input type="number" value={form.bailAmount} onChange={(e) => setForm({ ...form, bailAmount: e.target.value })} placeholder="0.00" />
        </Field>
        <Field label="Bail Hearing Date">
          <Input type="date" value={form.bailHearingDate} onChange={(e) => setForm({ ...form, bailHearingDate: e.target.value })} />
        </Field>
        <Field label="Next Hearing Date">
          <Input type="date" value={form.nextHearingDate} onChange={(e) => setForm({ ...form, nextHearingDate: e.target.value })} />
        </Field>
        <Field label="Bond Number">
          <Input value={form.bondNumber} onChange={(e) => setForm({ ...form, bondNumber: e.target.value })} placeholder="e.g. BND-2026-001" />
        </Field>
        <Field label="Granted By">
          <Input value={form.grantedBy} onChange={(e) => setForm({ ...form, grantedBy: e.target.value })} placeholder="Judge or authority name" />
        </Field>
        <Field label="Surety Name">
          <Input value={form.suretyName} onChange={(e) => setForm({ ...form, suretyName: e.target.value })} />
        </Field>
        <Field label="Surety Contact">
          <Input value={form.suretyContact} onChange={(e) => setForm({ ...form, suretyContact: e.target.value })} />
        </Field>
        <Field label="Surety Address" className="sm:col-span-2">
          <Input value={form.suretyAddress} onChange={(e) => setForm({ ...form, suretyAddress: e.target.value })} />
        </Field>
        <Field label="Bail Conditions" className="sm:col-span-2">
          <Textarea rows={3} value={form.bailConditions} onChange={(e) => setForm({ ...form, bailConditions: e.target.value })} placeholder="Enter conditions for bail..." />
        </Field>
        <Field label="Notes" className="sm:col-span-2">
          <Textarea rows={2} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}

function EditBailDialog({
  bail,
  open,
  onClose,
  onSubmit
}: {
  bail: Bail;
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({
    bailType: bail.bailType ?? "Regular",
    bailAmount: bail.bailAmount?.toString() ?? "",
    bailConditions: bail.bailConditions ?? "",
    bailHearingDate: bail.bailHearingDate ? bail.bailHearingDate.slice(0, 10) : "",
    nextHearingDate: bail.nextHearingDate ? bail.nextHearingDate.slice(0, 10) : "",
    bondNumber: bail.bondNumber ?? "",
    suretyName: bail.suretyName ?? "",
    suretyAddress: bail.suretyAddress ?? "",
    suretyContact: bail.suretyContact ?? "",
    grantedBy: bail.grantedBy ?? "",
    notes: bail.notes ?? ""
  });

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Edit Bail Record"
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button onClick={() => onSubmit({
            bailType: form.bailType,
            bailAmount: form.bailAmount ? Number(form.bailAmount) : null,
            bailConditions: form.bailConditions || null,
            bailHearingDate: form.bailHearingDate ? new Date(form.bailHearingDate).toISOString() : null,
            nextHearingDate: form.nextHearingDate ? new Date(form.nextHearingDate).toISOString() : null,
            bondNumber: form.bondNumber || null,
            suretyName: form.suretyName || null,
            suretyAddress: form.suretyAddress || null,
            suretyContact: form.suretyContact || null,
            grantedBy: form.grantedBy || null,
            notes: form.notes || null
          })}>Save Changes</Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Bail Type">
          <Select value={form.bailType} onChange={(e) => setForm({ ...form, bailType: e.target.value })}>
            {bailTypes.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </Select>
        </Field>
        <Field label="Bail Amount (BDT)">
          <Input type="number" value={form.bailAmount} onChange={(e) => setForm({ ...form, bailAmount: e.target.value })} />
        </Field>
        <Field label="Bail Hearing Date">
          <Input type="date" value={form.bailHearingDate} onChange={(e) => setForm({ ...form, bailHearingDate: e.target.value })} />
        </Field>
        <Field label="Next Hearing Date">
          <Input type="date" value={form.nextHearingDate} onChange={(e) => setForm({ ...form, nextHearingDate: e.target.value })} />
        </Field>
        <Field label="Bond Number">
          <Input value={form.bondNumber} onChange={(e) => setForm({ ...form, bondNumber: e.target.value })} />
        </Field>
        <Field label="Granted By">
          <Input value={form.grantedBy} onChange={(e) => setForm({ ...form, grantedBy: e.target.value })} />
        </Field>
        <Field label="Surety Name">
          <Input value={form.suretyName} onChange={(e) => setForm({ ...form, suretyName: e.target.value })} />
        </Field>
        <Field label="Surety Contact">
          <Input value={form.suretyContact} onChange={(e) => setForm({ ...form, suretyContact: e.target.value })} />
        </Field>
        <Field label="Surety Address" className="sm:col-span-2">
          <Input value={form.suretyAddress} onChange={(e) => setForm({ ...form, suretyAddress: e.target.value })} />
        </Field>
        <Field label="Bail Conditions" className="sm:col-span-2">
          <Textarea rows={3} value={form.bailConditions} onChange={(e) => setForm({ ...form, bailConditions: e.target.value })} />
        </Field>
        <Field label="Notes" className="sm:col-span-2">
          <Textarea rows={2} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
