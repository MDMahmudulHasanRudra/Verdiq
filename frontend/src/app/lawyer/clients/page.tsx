"use client";

import { useEffect, useState } from "react";
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
import { useLanguage } from "@/lib/i18n";
import { Users, Plus, Search, PenLine, Trash2, ArrowUpRight } from "lucide-react";
import type { Client, CreateClientInput } from "@/types/models";

const clientTypes = ["Individual", "Corporate", "Government", "NGO"];

interface ClientFormState {
  name: string;
  phone: string;
  email: string;
  clientType: string;
  nid: string;
  companyName: string;
  address: string;
  isActive: boolean;
}

const emptyForm = (): ClientFormState => ({
  name: "",
  phone: "",
  email: "",
  clientType: "Individual",
  nid: "",
  companyName: "",
  address: "",
  isActive: true
});

const fromClient = (c: Client): ClientFormState => ({
  name: c.name,
  phone: c.phone,
  email: c.email,
  clientType: c.clientType ?? "Individual",
  nid: c.nid ?? "",
  companyName: c.companyName ?? "",
  address: c.address ?? "",
  isActive: c.isActive
});

export default function ClientsPage() {
  const router = useRouter();
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();

  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [status, setStatus] = useState("");
  const [clientType, setClientType] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<Client | null>(null);
  const [deleting, setDeleting] = useState<Client | null>(null);

  useEffect(() => {
    const t = window.setTimeout(() => setDebouncedSearch(search), 350);
    return () => window.clearTimeout(t);
  }, [search]);

  const { data, isLoading } = useClients({
    page,
    pageSize: 10,
    search: debouncedSearch || undefined,
    status: status || undefined,
    clientType: clientType || undefined
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ["clients"] });

  const createMutation = useMutation({
    mutationFn: (input: CreateClientInput) => clientService.create(input),
    onSuccess: (data) => {
      invalidate();
      setCreateOpen(false);
      toast.success("Client created");
      router.push(`/lawyer/clients/${data.id}`);
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: Partial<CreateClientInput> & { isActive?: boolean } }) =>
      clientService.update(id, input),
    onSuccess: () => {
      invalidate();
      setEditing(null);
      toast.success("Client updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => clientService.remove(id),
    onSuccess: () => {
      invalidate();
      setDeleting(null);
      setPage(1);
      toast.success("Client deleted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const applyFilter = (fn: () => void) => {
    fn();
    setPage(1);
  };

  const clients = data?.data ?? [];

  return (
    <div>
      <PageHeader
        title={t("nav.clients")}
        subtitle={t("clients.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("clients.newClient")}
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
            <Input
              placeholder="Search by name, phone, email, client code…"
              className="pl-9"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <Select
            className="sm:w-44"
            value={clientType}
            onChange={(e) => applyFilter(() => setClientType(e.target.value))}
          >
            <option value="">All types</option>
            {clientTypes.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </Select>
          <Select
            className="sm:w-40"
            value={status}
            onChange={(e) => applyFilter(() => setStatus(e.target.value))}
          >
            <option value="">Active & inactive</option>
            <option value="active">Active</option>
            <option value="inactive">Inactive</option>
          </Select>
        </div>
      </Card>

      <Card>
        {isLoading ? (
          <Loading />
        ) : clients.length > 0 ? (
          <>
            <Table>
              <thead>
                <tr>
                  <th>{t("clients.name")}</th>
                  <th>{t("clients.contact")}</th>
                  <th>{t("clients.type")}</th>
                  <th>{t("clients.cases")}</th>
                  <th>{t("teams.status")}</th>
                  <th>{t("teams.joined")}</th>
                  <th className="text-right">Actions</th>
                </tr>
              </thead>
              <tbody>
                {clients.map((c) => (
                  <tr
                    key={c.id}
                    className="cursor-pointer"
                    onClick={() => router.push(`/lawyer/clients/${c.id}`)}
                  >
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
                    <td onClick={(e) => e.stopPropagation()}>
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => setEditing(c)}
                          className="cursor-pointer rounded-md p-1.5 text-ink-soft transition-colors hover:bg-slate-100 hover:text-ink"
                          title="Edit client"
                        >
                          <PenLine className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => setDeleting(c)}
                          className="cursor-pointer rounded-md p-1.5 text-ink-soft transition-colors hover:bg-red-50 hover:text-red-600"
                          title="Delete client"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                        <ArrowUpRight className="ml-1 h-4 w-4 text-ink-soft" />
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
            {data && data.totalPages > 1 ? (
              <Pagination
                page={data.page}
                totalPages={data.totalPages}
                totalCount={data.totalCount}
                onChange={setPage}
              />
            ) : null}
          </>
        ) : (
          <EmptyState
            icon={<Users className="h-10 w-10" />}
            title={debouncedSearch || clientType || status ? "No matching clients" : t("clients.newClient")}
            description={
              debouncedSearch || clientType || status
                ? "Try clearing some filters or changing your search."
                : t("clients.subtitle")
            }
            action={
              <Button onClick={() => setCreateOpen(true)}>
                <Plus className="h-4 w-4" /> {t("clients.newClient")}
              </Button>
            }
          />
        )}
      </Card>

      <ClientFormDialog
        open={createOpen}
        title={t("clients.newClient")}
        description="Basic client information to get started."
        submitLabel="Create Client"
        isEdit={false}
        initial={emptyForm()}
        onClose={() => setCreateOpen(false)}
        onSubmit={(form) =>
          createMutation.mutate({
            name: form.name,
            phone: form.phone,
            email: form.email,
            address: form.address || null,
            nid: form.nid || null,
            companyName: form.companyName || null,
            clientType: form.clientType
          })
        }
      />

      {editing ? (
        <ClientFormDialog
          open
          title="Edit Client"
          description={editing.clientCode || editing.email}
          submitLabel={t("configuration.saveChanges")}
          isEdit
          initial={fromClient(editing)}
          onClose={() => setEditing(null)}
          onSubmit={(form) =>
            updateMutation.mutate({
              id: editing.id,
              input: {
                name: form.name || undefined,
                phone: form.phone || undefined,
                email: form.email || undefined,
                clientType: form.clientType || undefined,
                nid: form.nid || null,
                companyName: form.companyName || null,
                address: form.address || null,
                isActive: form.isActive
              }
            })
          }
        />
      ) : null}

      <Dialog
        open={!!deleting}
        onClose={() => setDeleting(null)}
        title="Delete client"
        description={deleting ? `"${deleting.name}" will be permanently removed.` : ""}
        footer={
          <>
            <Button variant="ghost" onClick={() => setDeleting(null)}>Cancel</Button>
            <Button
              variant="danger"
              disabled={deleteMutation.isPending}
              onClick={() => deleting && deleteMutation.mutate(deleting.id)}
            >
              <Trash2 className="h-4 w-4" /> {t("cases.caseDeleted")}
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">
          Their cases will remain but will no longer be linked to this client. This action cannot
          be undone.
        </p>
      </Dialog>
    </div>
  );
}

function ClientFormDialog({
  open,
  title,
  description,
  submitLabel,
  isEdit,
  initial,
  onClose,
  onSubmit
}: {
  open: boolean;
  title: string;
  description: string;
  submitLabel: string;
  isEdit: boolean;
  initial: ClientFormState;
  onClose: () => void;
  onSubmit: (form: ClientFormState) => void;
}) {
  const [form, setForm] = useState<ClientFormState>(initial);

  useEffect(() => {
    setForm(initial);
  }, [initial]);

  const set = (k: keyof ClientFormState, v: string) => setForm((f) => ({ ...f, [k]: v }));

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={title}
      description={description}
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!form.name || !form.phone} onClick={() => onSubmit(form)}>{submitLabel}</Button>
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
          <Select value={form.clientType} onChange={(e) => set("clientType", e.target.value)}>
            {clientTypes.map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </Select>
        </Field>
        <Field label="NID">
          <Input value={form.nid} onChange={(e) => set("nid", e.target.value)} />
        </Field>
        <Field label="Company (if corporate)" className="sm:col-span-2">
          <Input value={form.companyName} onChange={(e) => set("companyName", e.target.value)} />
        </Field>
        <Field label="Address" className="sm:col-span-2">
          <Textarea rows={2} value={form.address} onChange={(e) => set("address", e.target.value)} />
        </Field>
        {isEdit ? (
          <label className="sm:col-span-2 inline-flex cursor-pointer items-center gap-2 text-sm text-ink">
            <input
              type="checkbox"
              checked={form.isActive}
              onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
              className="h-4 w-4 accent-primary-700"
            />
            Client is active
          </label>
        ) : null}
      </div>
    </Dialog>
  );
}
