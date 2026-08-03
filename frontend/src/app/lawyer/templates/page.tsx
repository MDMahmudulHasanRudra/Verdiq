"use client";

import { useMemo, useState } from "react";
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
import { useLanguage } from "@/lib/i18n";
import { ScrollText, Plus, Copy, FileText } from "lucide-react";
import type { Template } from "@/types/models";

const categories = ["Pleading", "Notice", "Petition", "Affidavit", "Deed", "Letter", "Other"];

function extractVariables(content: string): string[] {
  const matches = content.match(/\{\{\s*([a-zA-Z0-9_]+)\s*\}\}/g) ?? [];
  return Array.from(new Set(matches.map((m) => m.replace(/[{}]/g, "").trim())));
}

export default function TemplatesPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [category, setCategory] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [useTemplate, setUseTemplate] = useState<Template | null>(null);

  const { data: templates, isLoading } = useQuery({
    queryKey: ["templates", category],
    queryFn: () => templateService.list(category || undefined)
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => templateService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["templates"] });
      setCreateOpen(false);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title={t("templates.title")}
        subtitle={t("templates.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("templates.addTemplate")}
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <Select className="sm:w-48" value={category} onChange={(e) => setCategory(e.target.value)}>
          <option value="">{t("common.all")}</option>
          {categories.map((c) => (
            <option key={c} value={c}>{c}</option>
          ))}
        </Select>
      </Card>

      {isLoading ? (
        <Loading />
      ) : templates && templates.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {templates.map((tpl) => (
            <Card key={tpl.id} className="flex flex-col p-5">
              <div className="flex items-start justify-between">
                <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary-50">
                  <ScrollText className="h-5 w-5 text-primary-700" />
                </div>
                <span className="rounded-full bg-gold-50 px-2.5 py-0.5 text-xs font-medium text-gold-800">
                  {tpl.category}
                </span>
              </div>
              <h3 className="mt-3 font-display text-lg font-semibold text-ink">{tpl.title}</h3>
              <p className="mt-1 line-clamp-2 flex-1 text-sm text-ink-muted">{tpl.description ?? tpl.content?.slice(0, 120) ?? "—"}</p>
              {extractVariables(tpl.content ?? "").length > 0 ? (
                <p className="mt-2 text-xs text-ink-muted">
                  {t("templates.variables")}: {extractVariables(tpl.content ?? "").join(", ")}
                </p>
              ) : null}
              <div className="mt-4 flex items-center justify-between border-t border-line-soft pt-3">
                <p className="text-xs text-ink-muted">{t("common.date")}: {formatDate(tpl.createdAt)}</p>
                <Button size="sm" variant="outline" onClick={() => setUseTemplate(tpl)}>
                  <Copy className="h-3.5 w-3.5" /> {t("templates.use")}
                </Button>
              </div>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <EmptyState
            icon={<ScrollText className="h-10 w-10" />}
            title={t("templates.noTemplates")}
            description={t("templates.noTemplatesDesc")}
            action={
              <Button onClick={() => setCreateOpen(true)}>
                <Plus className="h-4 w-4" /> {t("templates.addTemplate")}
              </Button>
            }
          />
        </Card>
      )}

      <CreateTemplateDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
      <UseTemplateDialog open={!!useTemplate} template={useTemplate} onClose={() => setUseTemplate(null)} />
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
  const { t } = useLanguage();
  const [form, setForm] = useState({ title: "", description: "", category: "Pleading", content: "" });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("templates.addTemplate")}
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button disabled={!form.title || !form.content} onClick={() => onSubmit(form)}>{t("common.create")}</Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("legalDatabase.titleLabel")} required className="sm:col-span-2">
          <Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
        </Field>
        <Field label={t("invoices.description")} className="sm:col-span-2">
          <Input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
        <Field label={t("templates.category")}>
          <Select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
        </Field>
        <Field label={t("legalDatabase.content")} required className="sm:col-span-2">
          <Textarea rows={8} value={form.content} onChange={(e) => setForm({ ...form, content: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}

function UseTemplateDialog({
  open,
  template,
  onClose
}: {
  open: boolean;
  template: Template | null;
  onClose: () => void;
}) {
  const { t } = useLanguage();
  const toast = useToast();
  const variables = useMemo(() => (template ? extractVariables(template.content ?? "") : []), [template]);
  const [values, setValues] = useState<Record<string, string>>({});
  const [rendered, setRendered] = useState<string | null>(null);

  const renderMutation = useMutation({
    mutationFn: (input: Record<string, string>) => templateService.render(template!.id, input),
    onSuccess: (data) => {
      setRendered(data);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  if (!template) return null;

  return (
    <Dialog
      open={open}
      onClose={() => {
        onClose();
        setRendered(null);
        setValues({});
      }}
      title={`${t("templates.use")} — ${template.title}`}
      size="xl"
      footer={
        <>
          <Button variant="ghost" onClick={() => { onClose(); setRendered(null); setValues({}); }}>{t("common.close")}</Button>
          {variables.length > 0 && !rendered ? (
            <Button
              disabled={Object.keys(values).length < (variables?.length ?? 0) || !variables}
              onClick={() => renderMutation.mutate(values)}
            >
              {t("common.next")}
            </Button>
          ) : null}
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <div className="space-y-4">
          <div className="flex items-center gap-2">
            <FileText className="h-4 w-4 text-ink-muted" />
            <h3 className="text-sm font-semibold text-ink">{t("templates.variables")}</h3>
          </div>
          {variables && variables.length > 0 ? (
            variables.map((v) => (
              <Field key={v} label={v}>
                <Input
                  value={values[v] ?? ""}
                  onChange={(e) => setValues((prev) => ({ ...prev, [v]: e.target.value }))}
                  placeholder={v}
                />
              </Field>
            ))
          ) : (
            <p className="text-sm text-ink-muted">{t("common.noResults")}</p>
          )}
        </div>
        <div>
          <div className="mb-2 flex items-center gap-2">
            <ScrollText className="h-4 w-4 text-ink-muted" />
            <h3 className="text-sm font-semibold text-ink">{t("templates.previewDoc")}</h3>
          </div>
          <div className="max-h-[55vh] overflow-y-auto rounded-lg border border-line bg-surface p-4 text-sm text-ink whitespace-pre-wrap">
            {rendered ?? template.content}
          </div>
        </div>
      </div>
    </Dialog>
  );
}