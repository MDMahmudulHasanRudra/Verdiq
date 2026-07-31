"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Table, Pagination } from "@/components/ui/table";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useCases } from "@/lib/hooks";
import { caseService } from "@/lib/services";
import { getErrorMessage, formatDate, cn } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Plus, Search, FolderOpen, ArrowUpRight } from "lucide-react";
import type { CreateCaseInput } from "@/types/models";

const statuses = ["", "Active", "Pending", "Closed", "Appeal", "Withdrawn", "BailClosed"];
const priorities = ["", "Low", "Medium", "High", "Urgent"];

export default function CasesPage() {
  const router = useRouter();
  const toast = useToast();
  const qc = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [status, setStatus] = useState("");
  const [priority, setPriority] = useState("");
  const [createOpen, setCreateOpen] = useState(false);

  const { data, isLoading } = useCases({
    page,
    pageSize: 10,
    search: debouncedSearch || undefined,
    status: status || undefined,
    priority: priority || undefined
  });

  const createMutation = useMutation({
    mutationFn: (input: CreateCaseInput) => caseService.create(input),
    onSuccess: (data) => {
      qc.invalidateQueries({ queryKey: ["cases"] });
      setCreateOpen(false);
      toast.success("Case created");
      router.push(`/lawyer/cases/${data.id}`);
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title="Cases"
        subtitle="Manage all firm cases across courts and practice areas."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> New Case
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
            <Input
              placeholder="Search case number, title, court…"
              className="pl-9"
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                window.setTimeout(() => setDebouncedSearch(e.target.value), 350);
              }}
            />
          </div>
          <Select className="sm:w-44" value={status} onChange={(e) => { setStatus(e.target.value); setPage(1); }}>
            <option value="">All statuses</option>
            {statuses.slice(1).map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </Select>
          <Select className="sm:w-40" value={priority} onChange={(e) => { setPriority(e.target.value); setPage(1); }}>
            <option value="">All priorities</option>
            {priorities.slice(1).map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </Select>
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
                  <th>Case Number</th>
                  <th>Title</th>
                  <th>Court</th>
                  <th>Assigned</th>
                  <th>Status</th>
                  <th>Priority</th>
                  <th>Filing Date</th>
                  <th />
                </tr>
              </thead>
              <tbody>
                {data.data.map((c) => (
                  <tr
                    key={c.id}
                    className="cursor-pointer"
                    onClick={() => router.push(`/lawyer/cases/${c.id}`)}
                  >
                    <td className="font-medium text-primary-700">{c.caseNumber}</td>
                    <td className="max-w-64">
                      <p className="truncate font-medium text-ink">{c.title}</p>
                      <p className="truncate text-xs text-ink-muted">{c.caseType}</p>
                    </td>
                    <td className="text-ink-muted">{c.courtName}</td>
                    <td className="text-ink-muted">{c.assignedLawyerName ?? "—"}</td>
                    <td><StatusBadge value={c.status} /></td>
                    <td><StatusBadge value={c.priority} /></td>
                    <td className="text-ink-muted">{formatDate(c.filingDate)}</td>
                    <td className="text-right">
                      <ArrowUpRight className="ml-auto h-4 w-4 text-ink-soft" />
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
            <Pagination
              page={data.page}
              totalPages={data.totalPages}
              totalCount={data.totalCount}
              onChange={setPage}
            />
          </>
        ) : (
          <EmptyState
            icon={<FolderOpen className="h-10 w-10" />}
            title="No cases found"
            description="Create your first case to start tracking filings, hearings and documents."
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> New Case</Button>}
          />
        )}
      </Card>

      <CreateCaseDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateCaseDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: CreateCaseInput) => void;
}) {
  const [form, setForm] = useState<CreateCaseInput>({
    title: "",
    courtName: "",
    caseType: "Civil",
    filingDate: new Date().toISOString().slice(0, 10),
    opponent: "",
    priority: "Medium",
    description: "",
    actsAndSections: "",
    assignedLawyerId: null,
    clientIds: []
  });

  const set = (k: keyof CreateCaseInput, v: string | null) => setForm((f) => ({ ...f, [k]: v }));

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="New Case"
      description="Enter the basic case details to get started."
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.title || !form.courtName}
            onClick={() => onSubmit(form)}
          >
            Create Case
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Case Title" required className="sm:col-span-2">
          <Input value={form.title} onChange={(e) => set("title", e.target.value)} placeholder="e.g. State vs. Rahim Trading Co." />
        </Field>
        <Field label="Court Name" required>
          <Input value={form.courtName} onChange={(e) => set("courtName", e.target.value)} placeholder="e.g. Dhaka District Court" />
        </Field>
        <Field label="Case Type">
          <Select value={form.caseType} onChange={(e) => set("caseType", e.target.value)}>
            {["Civil", "Criminal", "Family", "Commercial", "Constitutional", "Labor", "Property", "Other"].map((t) => (
              <option key={t} value={t}>{t}</option>
            ))}
          </Select>
        </Field>
        <Field label="Filing Date">
          <Input type="date" value={form.filingDate} onChange={(e) => set("filingDate", e.target.value)} />
        </Field>
        <Field label="Priority">
          <Select value={form.priority ?? "Medium"} onChange={(e) => set("priority", e.target.value)}>
            {priorities.slice(1).map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </Select>
        </Field>
        <Field label="Opponent" className="sm:col-span-2">
          <Input value={form.opponent ?? ""} onChange={(e) => set("opponent", e.target.value)} placeholder="Opposing party name" />
        </Field>
        <Field label="Acts & Sections" className="sm:col-span-2">
          <Input value={form.actsAndSections ?? ""} onChange={(e) => set("actsAndSections", e.target.value)} placeholder="e.g. Penal Code 1860, s. 420" />
        </Field>
        <Field label="Description" className="sm:col-span-2">
          <textarea
            className="w-full rounded-lg border border-line bg-card px-3 py-2 text-sm text-ink focus:border-primary-600 focus:shadow-glow focus:outline-none"
            rows={3}
            value={form.description ?? ""}
            onChange={(e) => set("description", e.target.value)}
          />
        </Field>
      </div>
    </Dialog>
  );
}
