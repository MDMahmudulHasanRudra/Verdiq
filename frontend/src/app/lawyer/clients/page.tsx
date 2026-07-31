"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Table, Pagination } from "@/components/ui/table";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useClients } from "@/lib/hooks";
import { clientService } from "@/lib/services";
import { getErrorMessage, formatDate, initials } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Users, Plus, Search } from "lucide-react";
import type { CreateClientInput } from "@/types/models";

export default function ClientsPage() {
  const router = useRouter();
  const toast = useToast();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [createOpen, setCreateOpen] = useState(false);

  const { data, isLoading } = useClients({ page, pageSize: 10, search: debouncedSearch || undefined });

  const createMutation = useMutation({
    mutationFn: (input: CreateClientInput) => clientService.create(input),
    onSuccess: (data) => {
      qc.invalidateQueries({ queryKey: ["clients"] });
      setCreateOpen(false);
      toast.success("Client created");
      router.push(`/lawyer/clients/${data.id}`);
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title="Clients"
        subtitle="Manage your clients and their matters."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> New Client
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
          <Input
            placeholder="Search by name, phone, email…"
            className="pl-9"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              window.setTimeout(() => setDebouncedSearch(e.target.value), 350);
            }}
          />
        </div>
      </Card>

      <Card>
        {isLoading ? (
          <Loading />
        ) : data && data.data.length > 0 ? (
          <>
            <Table>
              <thead>
                <tr>
                  <th>Client</th>
                  <th>Contact</th>
                  <th>Type</th>
                  <th>Cases</th>
                  <th>Status</th>
                  <th>Created</th>
                </tr>
              </thead>
              <tbody>
                {data.data.map((c) => (
                  <tr key={c.id} className="cursor-pointer" onClick={() => router.push(`/lawyer/clients/${c.id}`)}>
                    <td>
                      <div className="flex items-center gap-3">
                        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary-50 text-xs font-semibold text-primary-700">
                          {initials(c.name)}
                        </div>
                        <div>
                          <p className="font-medium text-ink">{c.name}</p>
                          {c.clientCode ? <p className="text-xs text-ink-muted">{c.clientCode}</p> : null}
                        </div>
                      </div>
                    </td>
                    <td>
                      <p className="text-ink">{c.phone}</p>
                      <p className="text-xs text-ink-muted">{c.email}</p>
                    </td>
                    <td className="text-ink-muted">{c.clientType ?? "Individual"}</td>
                    <td className="font-medium text-ink">{c.casesCount}</td>
                    <td><StatusBadge value={c.isActive ? "Active" : "Inactive"} /></td>
                    <td className="text-ink-muted">{formatDate(c.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
            <Pagination page={data.page} totalPages={data.totalPages} totalCount={data.totalCount} onChange={setPage} />
          </>
        ) : (
          <EmptyState
            icon={<Users className="h-10 w-10" />}
            title="No clients yet"
            description="Add a client to link them to cases and invoices."
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> New Client</Button>}
          />
        )}
      </Card>

      <CreateClientDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateClientDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: CreateClientInput) => void;
}) {
  const [form, setForm] = useState<CreateClientInput>({
    name: "",
    phone: "",
    email: "",
    address: "",
    clientType: "Individual",
    nid: "",
    companyName: "",
    notes: ""
  });
  const set = (k: keyof CreateClientInput, v: string) => setForm((f) => ({ ...f, [k]: v }));

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="New Client"
      description="Basic client information to get started."
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!form.name || !form.phone} onClick={() => onSubmit(form)}>Create Client</Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Full Name" required className="sm:col-span-2">
          <Input value={form.name} onChange={(e) => set("name", e.target.value)} placeholder="e.g. Abdul Karim" />
        </Field>
        <Field label="Phone" required>
          <Input value={form.phone} onChange={(e) => set("phone", e.target.value)} placeholder="+880 1XXX-XXXXXX" />
        </Field>
        <Field label="Email">
          <Input type="email" value={form.email} onChange={(e) => set("email", e.target.value)} />
        </Field>
        <Field label="Client Type">
          <Select value={form.clientType ?? "Individual"} onChange={(e) => set("clientType", e.target.value)}>
            {["Individual", "Corporate", "Government", "NGO"].map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </Select>
        </Field>
        <Field label="NID">
          <Input value={form.nid ?? ""} onChange={(e) => set("nid", e.target.value)} />
        </Field>
        <Field label="Company (if corporate)" className="sm:col-span-2">
          <Input value={form.companyName ?? ""} onChange={(e) => set("companyName", e.target.value)} />
        </Field>
        <Field label="Address" className="sm:col-span-2">
          <Textarea rows={2} value={form.address ?? ""} onChange={(e) => set("address", e.target.value)} />
        </Field>
      </div>
    </Dialog>
  );
}
