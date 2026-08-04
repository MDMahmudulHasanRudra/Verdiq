"use client";

import { useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Badge, StatusBadge } from "@/components/ui/badge";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Dialog } from "@/components/ui/dialog";
import { Input, Select, Field } from "@/components/ui/field";
import { Loading, EmptyState } from "@/components/ui/loading";
import { clientService, invoiceService } from "@/lib/services";
import { getErrorMessage, formatDate, initials, formatCurrency, formatDateTime } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { ArrowLeft, Phone, Mail, MapPin, UserPlus, FolderOpen, CalendarClock, ArrowUpRight, Plus, Pencil, Trash2, Gavel, FileText } from "lucide-react";
import { Tabs } from "@/components/ui/tabs";
import type { ClientPastAffair } from "@/types/models";

export default function ClientDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const toast = useToast();
  const qc = useQueryClient();
  const [tab, setTab] = useState("overview");
  const [portalOpen, setPortalOpen] = useState(false);

  const { data: client, isLoading } = useQuery({
    queryKey: ["client", id],
    queryFn: () => clientService.get(id),
    enabled: !!id
  });

  const { data: clientCases } = useQuery({
    queryKey: ["client", id, "cases"],
    queryFn: () => clientService.cases(id),
    enabled: !!id
  });

  const { data: clientHearings } = useQuery({
    queryKey: ["client", id, "hearings"],
    queryFn: () => clientService.hearings(id),
    enabled: !!id
  });

  const { data: invoices } = useQuery({
    queryKey: ["invoices", "by-client", id],
    queryFn: () => invoiceService.byClient(id),
    enabled: !!id
  });

  const grantPortal = useMutation({
    mutationFn: (input: { fullName: string; email: string; password: string }) =>
      clientService.grantPortalAccess(id, input),
    onSuccess: () => {
      setPortalOpen(false);
      toast.success("Client portal access granted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  if (isLoading) return <Loading label="Loading client…" />;
  if (!client) return <EmptyState title="Client not found" />;

  const c = client;
  const upcomingHearings = (clientHearings ?? []).filter(
    (h) => new Date(h.hearingDate) >= new Date() && h.status === "Scheduled"
  );

  return (
    <div>
      <PageHeader
        title={
          <span className="flex items-center gap-3">
            <Link href="/lawyer/clients" className="text-ink-muted transition-colors hover:text-ink">
              <ArrowLeft className="h-5 w-5" />
            </Link>
            <span className="flex items-center gap-3">
              <span className="flex h-10 w-10 items-center justify-center rounded-full bg-primary-700 text-sm font-semibold text-white">
                {initials(c.name)}
              </span>
              {c.name}
            </span>
          </span>
        }
        subtitle={c.clientCode ? `Client code ${c.clientCode}` : "Client profile"}
        actions={
          <>
            <Button variant="outline" onClick={() => setPortalOpen(true)}>
              <UserPlus className="h-4 w-4" /> Portal Access
            </Button>
            <Button onClick={() => router.push(`/lawyer/cases?client=${c.id}`)}>
              <FolderOpen className="h-4 w-4" /> New Case
            </Button>
          </>
        }
      />

      <div className="mb-5 flex flex-wrap items-center gap-2">
        <Badge tone="blue">{c.clientType ?? "Individual"}</Badge>
        <StatusBadge value={c.isActive ? "Active" : "Inactive"} />
        {c.isBlacklisted ? <StatusBadge value="Blacklisted" /> : null}
        {c.riskLevel ? <Badge tone="amber">Risk: {c.riskLevel}</Badge> : null}
      </div>

      <Tabs tabs={[{ value: "overview", label: "Overview" }, { value: "cases", label: "Cases" }, { value: "hearings", label: "Hearings" }, { value: "past-affairs", label: "Past Affairs" }, { value: "invoices", label: "Invoices" }]} value={tab} onChange={setTab} />

      {tab === "overview" && (
        <div className="mt-5 grid grid-cols-1 gap-6 lg:grid-cols-3">
          <Card className="lg:col-span-2">
            <CardHeader title="Contact Information" />
            <CardContent className="space-y-4">
              <div className="flex items-center gap-3">
                <Phone className="h-4 w-4 text-ink-soft" />
                <span className="text-sm text-ink">{c.phone}</span>
              </div>
              {c.email ? (
                <div className="flex items-center gap-3">
                  <Mail className="h-4 w-4 text-ink-soft" />
                  <span className="text-sm text-ink">{c.email}</span>
                </div>
              ) : null}
              {c.address ? (
                <div className="flex items-center gap-3">
                  <MapPin className="h-4 w-4 text-ink-soft" />
                  <span className="text-sm text-ink">{c.address}</span>
                </div>
              ) : null}
              <dl className="grid grid-cols-1 gap-x-8 gap-y-4 pt-2 sm:grid-cols-2">
                <InfoRow label="NID" value={c.nid ?? "—"} />
                <InfoRow label="Company" value={c.companyName ?? "—"} />
                <InfoRow label="Occupation" value={c.occupation ?? "—"} />
                <InfoRow label="Nationality" value={c.nationality ?? "—"} />
                <InfoRow label="Preferred Contact" value={c.preferredContactMethod ?? "—"} />
                <InfoRow label="Billing Preference" value={c.billingPreference ?? "—"} />
              </dl>
              {c.notes ? <p className="pt-2 text-sm text-ink-muted">{c.notes}</p> : null}
            </CardContent>
          </Card>
          <Card>
            <CardHeader title="At a Glance" />
            <CardContent className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-2xl font-bold text-ink">{c.casesCount}</p>
                <p className="text-xs text-ink-muted">Cases</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-ink">{invoices?.length ?? 0}</p>
                <p className="text-xs text-ink-muted">Invoices</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-ink">{upcomingHearings.length}</p>
                <p className="text-xs text-ink-muted">Upcoming hearings</p>
              </div>
              <div>
                <p className="text-2xl font-bold text-ink">{formatDate(c.createdAt, "MMM YYYY")}</p>
                <p className="text-xs text-ink-muted">Client Since</p>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {tab === "cases" && (
        <div className="mt-5">
          <Card>
            <CardHeader title="Linked Cases" description="Cases this client is linked to and their next hearing day." />
            <CardContent>
              {clientCases && clientCases.length > 0 ? (
                <div className="space-y-3">
                  {clientCases.map((cs) => (
                    <button
                      key={cs.id}
                      onClick={() => router.push(`/lawyer/cases/${cs.id}`)}
                      className="flex w-full cursor-pointer items-center justify-between gap-4 rounded-lg border border-line p-3 text-left transition-colors hover:border-primary-300 hover:bg-primary-50/40"
                    >
                      <div className="min-w-0">
                        <p className="font-mono text-xs font-semibold text-primary-700">{cs.caseNumber}</p>
                        <p className="truncate text-sm font-medium text-ink">{cs.title}</p>
                        <p className="text-xs text-ink-muted">
                          {cs.caseType} · {cs.assignedLawyerName}
                        </p>
                      </div>
                      <div className="flex shrink-0 items-center gap-4">
                        <div className="text-right">
                          {cs.nextHearingDate ? (
                            <>
                              <p className="inline-flex items-center gap-1.5 text-sm font-medium text-gold-700">
                                <CalendarClock className="h-3.5 w-3.5" />
                                {formatDate(cs.nextHearingDate)}
                              </p>
                              <p className="text-xs text-ink-muted">Next hearing</p>
                            </>
                          ) : (
                            <p className="text-xs text-ink-muted">No hearing scheduled</p>
                          )}
                        </div>
                        <StatusBadge value={cs.status} />
                        <ArrowUpRight className="h-4 w-4 text-ink-soft" />
                      </div>
                    </button>
                  ))}
                </div>
              ) : (
                <EmptyState
                  title="No cases linked"
                  description="Create a case and link this client to it."
                  action={
                    <Button onClick={() => router.push(`/lawyer/cases?client=${c.id}`)}>
                      <FolderOpen className="h-4 w-4" /> New Case for {c.name}
                    </Button>
                  }
                />
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {tab === "hearings" && (
        <div className="mt-5">
          <Card>
            <CardHeader title="Hearings" description="Which case is heard on which day." />
            <CardContent>
              {clientHearings && clientHearings.length > 0 ? (
                <div className="space-y-3">
                  {clientHearings.map((h) => {
                    const isUpcoming = new Date(h.hearingDate) >= new Date() && h.status === "Scheduled";
                    return (
                      <button
                        key={h.id}
                        onClick={() => router.push(`/lawyer/cases/${h.caseId}`)}
                        className="flex w-full cursor-pointer items-center justify-between gap-4 rounded-lg border border-line p-3 text-left transition-colors hover:border-primary-300 hover:bg-primary-50/40"
                      >
                        <div className="min-w-0">
                          <p className="font-mono text-xs font-semibold text-primary-700">{h.caseNumber}</p>
                          <p className="truncate text-sm font-medium text-ink">{h.caseTitle}</p>
                          <p className="text-xs text-ink-muted">
                            {h.courtroom ?? "Courtroom TBA"}
                            {h.judgeName ? ` · Judge ${h.judgeName}` : ""}
                            {h.result ? ` · Result: ${h.result}` : ""}
                          </p>
                        </div>
                        <div className="flex shrink-0 items-center gap-3">
                          <div className="text-right">
                            <p className={isUpcoming ? "text-sm font-semibold text-gold-700" : "text-sm text-ink-muted"}>
                              {formatDateTime(h.hearingDate)}
                            </p>
                            {h.nextHearingDate ? (
                              <p className="text-xs text-ink-muted">Next: {formatDate(h.nextHearingDate)}</p>
                            ) : null}
                          </div>
                          <StatusBadge value={h.status} />
                        </div>
                      </button>
                    );
                  })}
                </div>
              ) : (
                <EmptyState title="No hearings" description="Hearings across this client's cases will appear here." />
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {tab === "invoices" && (
        <div className="mt-5">
          <Card>
            <CardHeader title="Invoices" />
            <CardContent className="space-y-3">
              {invoices && invoices.length > 0 ? (
                invoices.map((inv) => (
                  <div key={inv.id} className="flex items-center justify-between rounded-lg border border-line p-3">
                    <div>
                      <p className="text-sm font-medium text-ink">{inv.invoiceNumber ?? inv.id}</p>
                      <p className="text-xs text-ink-muted">{inv.status} · {inv.createdAt ? formatDate(inv.createdAt) : ""}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-semibold text-ink">{formatCurrency(inv.amount)}</p>
                      <StatusBadge value={inv.status} />
                    </div>
                  </div>
                ))
              ) : (
                <EmptyState title="No invoices" />
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {tab === "past-affairs" && (
        <div className="mt-5">
          <PastAffairsTab clientId={id} clientName={c.name} />
        </div>
      )}

      <GrantPortalDialog
        open={portalOpen}
        onClose={() => setPortalOpen(false)}
        clientName={c.name}
        onSubmit={(v) => grantPortal.mutate(v)}
      />
    </div>
  );
}

function InfoRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <dt className="text-xs font-semibold uppercase tracking-wide text-ink-muted">{label}</dt>
      <dd className="mt-0.5 text-sm text-ink">{value}</dd>
    </div>
  );
}

function GrantPortalDialog({
  open,
  onClose,
  clientName,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  clientName: string;
  onSubmit: (input: { fullName: string; email: string; password: string }) => void;
}) {
  const [form, setForm] = useState({ fullName: clientName, email: "", password: "" });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Grant Client Portal Access"
      description={`Create login credentials for ${clientName}.`}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!form.email || !form.password} onClick={() => onSubmit(form)}>Grant Access</Button>
        </>
      }
    >
      <div className="space-y-4">
        <Field label="Display Name" required>
          <Input value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
        </Field>
        <Field label="Email / Username" required>
          <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
        </Field>
        <Field label="Password" required hint="The client uses these credentials to log in at /client.">
          <Input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}

function PastAffairsTab({ clientId, clientName }: { clientId: string; clientName: string }) {
  const toast = useToast();
  const qc = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);
  const [editAffair, setEditAffair] = useState<ClientPastAffair | null>(null);
  const [filter, setFilter] = useState<"all" | "criminal" | "civil">("all");

  const { data: affairs, isLoading } = useQuery({
    queryKey: ["client", clientId, "past-affairs"],
    queryFn: () => clientService.pastAffairs.list(clientId),
    enabled: !!clientId
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => clientService.pastAffairs.create(clientId, input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["client", clientId, "past-affairs"] });
      setCreateOpen(false);
      toast.success("Past affair recorded");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: Record<string, unknown> }) =>
      clientService.pastAffairs.update(clientId, id, input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["client", clientId, "past-affairs"] });
      setEditAffair(null);
      toast.success("Record updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => clientService.pastAffairs.remove(clientId, id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["client", clientId, "past-affairs"] });
      toast.success("Record deleted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const filtered = (affairs ?? []).filter((a) => {
    if (filter === "criminal") return a.isCriminal;
    if (filter === "civil") return !a.isCriminal;
    return true;
  });

  const criminalCount = (affairs ?? []).filter((a) => a.isCriminal).length;
  const civilCount = (affairs ?? []).filter((a) => !a.isCriminal).length;

  return (
    <>
      <Card>
        <CardHeader
          title="Past Affairs & Criminal History"
          description="Track client's past cases, criminal records, and legal history."
          action={
            <Button size="sm" onClick={() => setCreateOpen(true)}>
              <Plus className="h-4 w-4" /> Add Record
            </Button>
          }
        />
        <CardContent>
          <div className="mb-4 flex flex-wrap items-center gap-3">
            <div className="flex gap-1">
              <Button size="sm" variant={filter === "all" ? "subtle" : "ghost"} onClick={() => setFilter("all")}>
                All ({affairs?.length ?? 0})
              </Button>
              <Button size="sm" variant={filter === "criminal" ? "subtle" : "ghost"} onClick={() => setFilter("criminal")}>
                <Gavel className="h-3.5 w-3.5" /> Criminal ({criminalCount})
              </Button>
              <Button size="sm" variant={filter === "civil" ? "subtle" : "ghost"} onClick={() => setFilter("civil")}>
                Civil ({civilCount})
              </Button>
            </div>
          </div>

          {isLoading ? (
            <Loading />
          ) : filtered.length > 0 ? (
            <div className="space-y-3">
              {filtered.map((a) => (
                <div key={a.id} className="flex items-start justify-between gap-4 rounded-lg border border-line p-4">
                  <div className="min-w-0 flex-1">
                    <div className="flex items-center gap-2">
                      <h4 className="text-sm font-semibold text-ink">{a.caseTitle}</h4>
                      <Badge tone={a.isCriminal ? "red" : "blue"}>
                        {a.isCriminal ? "Criminal" : "Civil"}
                      </Badge>
                      {a.status ? <StatusBadge value={a.status} /> : null}
                    </div>
                    <div className="mt-1 grid grid-cols-2 gap-x-6 gap-y-1 text-xs text-ink-muted sm:grid-cols-4">
                      {a.caseNumber ? <span>Case: {a.caseNumber}</span> : null}
                      {a.courtName ? <span>Court: {a.courtName}</span> : null}
                      {a.filingDate ? <span>Filed: {formatDate(a.filingDate)}</span> : null}
                      {a.closingDate ? <span>Closed: {formatDate(a.closingDate)}</span> : null}
                      {a.judgeName ? <span>Judge: {a.judgeName}</span> : null}
                      {a.lawyerName ? <span>Lawyer: {a.lawyerName}</span> : null}
                      {a.opponent ? <span>Opponent: {a.opponent}</span> : null}
                      {a.verdict ? <span>Verdict: {a.verdict}</span> : null}
                    </div>
                    {a.description ? <p className="mt-2 text-xs text-ink-muted line-clamp-2">{a.description}</p> : null}
                    {a.actsAndSections ? <p className="mt-1 text-xs text-amber-700">Acts: {a.actsAndSections}</p> : null}
                  </div>
                  <div className="flex shrink-0 gap-1">
                    <button
                      onClick={() => setEditAffair(a)}
                      className="cursor-pointer rounded-lg p-1.5 text-ink-muted hover:bg-slate-100 hover:text-ink"
                    >
                      <Pencil className="h-4 w-4" />
                    </button>
                    <button
                      onClick={() => { if (confirm("Delete this record?")) deleteMutation.mutate(a.id); }}
                      className="cursor-pointer rounded-lg p-1.5 text-ink-muted hover:bg-red-50 hover:text-red-600"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState
              icon={<Gavel className="h-10 w-10" />}
              title="No past affairs"
              description="Record client's past legal history and criminal records."
              action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> Add Record</Button>}
            />
          )}
        </CardContent>
      </Card>

      <PastAffairDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onSubmit={(v) => createMutation.mutate(v)}
      />

      {editAffair && (
        <PastAffairDialog
          open={!!editAffair}
          editing={editAffair}
          onClose={() => setEditAffair(null)}
          onSubmit={(v) => updateMutation.mutate({ id: editAffair.id, input: v })}
        />
      )}
    </>
  );
}

function PastAffairDialog({
  open,
  onClose,
  onSubmit,
  editing
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
  editing?: ClientPastAffair;
}) {
  const [form, setForm] = useState({
    caseTitle: editing?.caseTitle ?? "",
    caseNumber: editing?.caseNumber ?? "",
    courtName: editing?.courtName ?? "",
    caseType: editing?.caseType ?? "",
    status: editing?.status ?? "",
    filingDate: editing?.filingDate ? editing.filingDate.slice(0, 10) : "",
    closingDate: editing?.closingDate ? editing.closingDate.slice(0, 10) : "",
    opponent: editing?.opponent ?? "",
    judgeName: editing?.judgeName ?? "",
    verdict: editing?.verdict ?? "",
    description: editing?.description ?? "",
    actsAndSections: editing?.actsAndSections ?? "",
    lawyerName: editing?.lawyerName ?? "",
    isCriminal: editing?.isCriminal ?? true,
    outcome: editing?.outcome ?? "",
    notes: editing?.notes ?? ""
  });

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={editing ? "Edit Past Affair" : "Record Past Affair"}
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.caseTitle}
            onClick={() => onSubmit({
              ...form,
              filingDate: form.filingDate ? new Date(form.filingDate).toISOString() : null,
              closingDate: form.closingDate ? new Date(form.closingDate).toISOString() : null,
              caseNumber: form.caseNumber || null,
              courtName: form.courtName || null,
              caseType: form.caseType || null,
              status: form.status || null,
              opponent: form.opponent || null,
              judgeName: form.judgeName || null,
              verdict: form.verdict || null,
              description: form.description || null,
              actsAndSections: form.actsAndSections || null,
              lawyerName: form.lawyerName || null,
              outcome: form.outcome || null,
              notes: form.notes || null
            })}
          >
            {editing ? "Save Changes" : "Record"}
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Case Title" required className="sm:col-span-2">
          <Input value={form.caseTitle} onChange={(e) => setForm({ ...form, caseTitle: e.target.value })} placeholder="e.g. State vs. Client" />
        </Field>
        <Field label="Case Type">
          <Select value={form.caseType} onChange={(e) => setForm({ ...form, caseType: e.target.value })}>
            <option value="">Select...</option>
            {["Criminal", "Civil", "Family", "Corporate", "Labor", "Property", "Constitutional", "Other"].map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </Select>
        </Field>
        <Field label="Status">
          <Select value={form.status} onChange={(e) => setForm({ ...form, status: e.target.value })}>
            <option value="">Select...</option>
            {["Pending", "Ongoing", "Disposed", "Acquitted", "Convicted", "Settled", "Withdrawn"].map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </Select>
        </Field>
        <Field label="Case Number">
          <Input value={form.caseNumber} onChange={(e) => setForm({ ...form, caseNumber: e.target.value })} />
        </Field>
        <Field label="Court Name">
          <Input value={form.courtName} onChange={(e) => setForm({ ...form, courtName: e.target.value })} />
        </Field>
        <Field label="Filing Date">
          <Input type="date" value={form.filingDate} onChange={(e) => setForm({ ...form, filingDate: e.target.value })} />
        </Field>
        <Field label="Closing Date">
          <Input type="date" value={form.closingDate} onChange={(e) => setForm({ ...form, closingDate: e.target.value })} />
        </Field>
        <Field label="Opponent">
          <Input value={form.opponent} onChange={(e) => setForm({ ...form, opponent: e.target.value })} />
        </Field>
        <Field label="Judge Name">
          <Input value={form.judgeName} onChange={(e) => setForm({ ...form, judgeName: e.target.value })} />
        </Field>
        <Field label="Verdict / Outcome">
          <Input value={form.verdict} onChange={(e) => setForm({ ...form, verdict: e.target.value })} />
        </Field>
        <Field label="Lawyer Name">
          <Input value={form.lawyerName} onChange={(e) => setForm({ ...form, lawyerName: e.target.value })} />
        </Field>
        <Field label="Acts & Sections">
          <Input value={form.actsAndSections} onChange={(e) => setForm({ ...form, actsAndSections: e.target.value })} placeholder="e.g. Section 302 IPC" />
        </Field>
        <Field label="Type">
          <Select value={form.isCriminal ? "criminal" : "civil"} onChange={(e) => setForm({ ...form, isCriminal: e.target.value === "criminal" })}>
            <option value="criminal">Criminal</option>
            <option value="civil">Civil</option>
          </Select>
        </Field>
        <Field label="Description" className="sm:col-span-2">
          <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
        <Field label="Notes" className="sm:col-span-2">
          <Input value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
