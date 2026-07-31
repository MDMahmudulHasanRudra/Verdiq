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
import { Input, Field } from "@/components/ui/field";
import { Loading, EmptyState } from "@/components/ui/loading";
import { clientService, invoiceService } from "@/lib/services";
import { getErrorMessage, formatDate, initials, formatCurrency } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { ArrowLeft, Phone, Mail, MapPin, UserPlus } from "lucide-react";
import { Tabs } from "@/components/ui/tabs";

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
          </>
        }
      />

      <div className="mb-5 flex flex-wrap items-center gap-2">
        <Badge tone="blue">{c.clientType ?? "Individual"}</Badge>
        <StatusBadge value={c.isActive ? "Active" : "Inactive"} />
        {c.isBlacklisted ? <StatusBadge value="Blacklisted" /> : null}
        {c.riskLevel ? <Badge tone="amber">Risk: {c.riskLevel}</Badge> : null}
      </div>

      <Tabs tabs={[{ value: "overview", label: "Overview" }, { value: "cases", label: "Cases" }, { value: "invoices", label: "Invoices" }]} value={tab} onChange={setTab} />

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
                <p className="text-2xl font-bold text-ink">{c.creditLimit ? formatCurrency(c.creditLimit) : "—"}</p>
                <p className="text-xs text-ink-muted">Credit Limit</p>
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
            <CardHeader title="Linked Cases" />
            <CardContent>
              <EmptyState title="Cases appear here" description="Link cases to this client from a case record." />
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
