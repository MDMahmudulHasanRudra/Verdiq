"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { legalSectionService } from "@/lib/services";
import { getErrorMessage } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { BookOpen, Plus, Search, ChevronDown, ChevronRight, FilePlus } from "lucide-react";
import { useQuery as useReactQuery } from "@tanstack/react-query";
import type { LegalProcedure } from "@/types/models";

function useProcedures(sectionId: string) {
  return useReactQuery({
    queryKey: ["legal-sections", sectionId, "procedures"],
    queryFn: () => legalSectionService.procedures(sectionId),
    enabled: !!sectionId
  });
}

function SectionCard({
  section,
  onAddProcedure
}: {
  section: import("@/types/models").LegalSection;
  onAddProcedure: (sectionId: string, input: Record<string, unknown>) => void;
}) {
  const [open, setOpen] = useState(false);
  const { data: procedures } = useProcedures(open ? section.id : "");
  return (
    <Card>
      <button
        className="flex w-full cursor-pointer items-center justify-between gap-3 px-5 py-4 text-left"
        onClick={() => setOpen((v) => !v)}
      >
        <div className="flex items-center gap-3">
          {open ? (
            <ChevronDown className="h-4 w-4 text-ink-soft" />
          ) : (
            <ChevronRight className="h-4 w-4 text-ink-soft" />
          )}
          <div>
            <p className="font-medium text-ink">{section.sectionTitle}</p>
            <p className="text-xs text-ink-muted">{section.lawName} · {section.category ?? "General"} · {section.sectionCode}</p>
          </div>
        </div>
        <span className="rounded-full bg-primary-50 px-2.5 py-0.5 text-xs font-medium text-primary-800">
          {section.procedureCount} procedures
        </span>
      </button>
      {open ? (
        <div className="border-t border-line-soft px-5 py-4">
          {section.description ? <p className="mb-4 text-sm text-ink-muted">{section.description}</p> : null}
          <div className="flex items-center justify-between">
            <h4 className="text-sm font-semibold text-ink">Procedure Checklist</h4>
            <Button
              size="sm"
              variant="subtle"
              onClick={() => onAddProcedure(section.id, { title: "New step", description: "Describe the step" })}
            >
              <FilePlus className="h-3.5 w-3.5" /> Add Step
            </Button>
          </div>
          <div className="mt-3 space-y-2">
            {procedures && procedures.length > 0 ? (
              procedures.map((p) => (
                <div key={p.id} className="flex items-center justify-between rounded-lg bg-slate-50 px-3 py-2">
                  <div>
                    <p className="text-sm text-ink">{p.title}</p>
                    {p.description ? <p className="text-xs text-ink-muted">{p.description}</p> : null}
                  </div>
                  <p className="text-xs text-ink-soft">{p.stepNumber}</p>
                </div>
              ))
            ) : (
              <p className="text-sm text-ink-muted">No steps yet.</p>
            )}
          </div>
        </div>
      ) : null}
    </Card>
  );
}

const categories = ["Civil", "Criminal", "Property", "Family", "Commercial", "Constitutional", "Labor"];

export default function LegalDatabasePage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [category, setCategory] = useState("");
  const [search, setSearch] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [expanded, setExpanded] = useState<Record<string, boolean>>({});

  const { data: sections, isLoading } = useQuery({
    queryKey: ["legal-sections", category, search],
    queryFn: () => legalSectionService.list({ category: category || undefined, search: search || undefined })
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => legalSectionService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["legal-sections"] });
      setCreateOpen(false);
      toast.success("Section added");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const addProcedure = useMutation({
    mutationFn: ({ sectionId, input }: { sectionId: string; input: Record<string, unknown> }) =>
      legalSectionService.createProcedure(sectionId, input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["legal-sections"] });
      toast.success("Procedure added");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title="Legal Database"
        subtitle="Searchable library of legal sections and procedure checklists."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> Add Section
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <div className="flex flex-col gap-3 sm:flex-row">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
            <Input
              className="pl-9"
              placeholder="Search sections, acts, procedures…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          <Select className="sm:w-48" value={category} onChange={(e) => setCategory(e.target.value)}>
            <option value="">All categories</option>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
        </div>
      </Card>

      {isLoading ? (
        <Loading />
      ) : sections && sections.length > 0 ? (
        <div className="space-y-3">
          {sections.map((s) => (
            <SectionCard
              key={s.id}
              section={s}
              onAddProcedure={(sectionId, input) => addProcedure.mutate({ sectionId, input })}
            />
          ))}
        </div>
      ) : (
        <Card>
          <EmptyState
            icon={<BookOpen className="h-10 w-10" />}
            title="No legal sections"
            description="Add Bangladesh statutes and procedure checklists to power case workflows."
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> Add Section</Button>}
          />
        </Card>
      )}

      <CreateSectionDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateSectionDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({ title: "", actName: "", category: "Civil", content: "" });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Add Legal Section"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!form.title} onClick={() => onSubmit(form)}>Add Section</Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Title" required className="sm:col-span-2">
          <Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} placeholder="e.g. Section 420, Penal Code 1860" />
        </Field>
        <Field label="Act Name">
          <Input value={form.actName} onChange={(e) => setForm({ ...form, actName: e.target.value })} />
        </Field>
        <Field label="Category">
          <Select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
        </Field>
        <Field label="Content" className="sm:col-span-2">
          <Textarea rows={4} value={form.content} onChange={(e) => setForm({ ...form, content: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
