"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { templateService } from "@/lib/services";
import { getErrorMessage, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { ScrollText, Plus, Copy } from "lucide-react";

const categories = ["Pleading", "Notice", "Petition", "Affidavit", "Deed", "Letter", "Other"];

export default function TemplatesPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [category, setCategory] = useState("");
  const [createOpen, setCreateOpen] = useState(false);

  const { data: templates, isLoading } = useQuery({
    queryKey: ["templates", category],
    queryFn: () => templateService.list(category || undefined)
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => templateService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["templates"] });
      setCreateOpen(false);
      toast.success("Template created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title="Document Templates"
        subtitle="Reusable templates with variable substitution for common filings."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> New Template
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <Select className="sm:w-48" value={category} onChange={(e) => setCategory(e.target.value)}>
          <option value="">All categories</option>
          {categories.map((c) => (
            <option key={c} value={c}>{c}</option>
          ))}
        </Select>
      </Card>

      {isLoading ? (
        <Loading />
      ) : templates && templates.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {templates.map((t) => (
            <Card key={t.id} className="flex flex-col p-5">
              <div className="flex items-start justify-between">
                <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-50">
                  <ScrollText className="h-5 w-5 text-primary-700" />
                </div>
                <span className="rounded-full bg-gold-50 px-2.5 py-0.5 text-xs font-medium text-gold-800">
                  {t.category}
                </span>
              </div>
              <h3 className="mt-3 font-display text-lg font-semibold text-ink">{t.title}</h3>
              <p className="mt-1 line-clamp-2 flex-1 text-sm text-ink-muted">{t.description ?? t.content?.slice(0, 120) ?? "—"}</p>
              <div className="mt-4 flex items-center justify-between border-t border-line-soft pt-3">
                <p className="text-xs text-ink-muted">Created {formatDate(t.createdAt)}</p>
                <Button size="sm" variant="outline">
                  <Copy className="h-3.5 w-3.5" /> Use
                </Button>
              </div>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <EmptyState
            icon={<ScrollText className="h-10 w-10" />}
            title="No templates"
            description="Create templates to speed up routine drafting."
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> New Template</Button>}
          />
        </Card>
      )}

      <CreateTemplateDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateTemplateDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({ title: "", description: "", category: "Pleading", content: "" });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="New Template"
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!form.title || !form.content} onClick={() => onSubmit(form)}>Create Template</Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Title" required className="sm:col-span-2">
          <Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
        </Field>
        <Field label="Description" className="sm:col-span-2">
          <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
        <Field label="Category">
          <Select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
        </Field>
        <Field label="Content" required className="sm:col-span-2" hint="Use {{variables}} like {{clientName}} for substitution.">
          <Textarea rows={8} value={form.content} onChange={(e) => setForm({ ...form, content: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
