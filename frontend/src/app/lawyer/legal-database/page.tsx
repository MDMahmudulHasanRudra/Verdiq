"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { legalSectionService, legalDocumentService } from "@/lib/services";
import { getErrorMessage } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { Tabs } from "@/components/ui/tabs";
import {
  BookOpen,
  Plus,
  Search,
  ChevronDown,
  ChevronRight,
  FilePlus,
  FileText,
  Pencil,
  Trash2,
  ScrollText,
  Scale
} from "lucide-react";
import type { LegalSection, LegalDocument } from "@/types/models";

const categories = ["Civil", "Criminal", "Property", "Family", "Commercial", "Constitutional", "Labor"];
const docCategories = ["Law", "Judgment", "Regulation", "Ordinance", "Rule", "Other"];

function useProcedures(sectionId: string) {
  return useQuery({
    queryKey: ["legal-sections", sectionId, "procedures"],
    queryFn: () => legalSectionService.procedures(sectionId),
    enabled: !!sectionId
  });
}

function SectionCard({
  section,
  onAddProcedure,
  onEdit,
  onDelete
}: {
  section: LegalSection;
  onAddProcedure: (sectionId: string, input: Record<string, unknown>) => void;
  onEdit: (section: LegalSection) => void;
  onDelete: (section: LegalSection) => void;
}) {
  const { t } = useLanguage();
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
        <div className="flex items-center gap-2">
          <span className="rounded-full bg-primary-50 px-2.5 py-0.5 text-xs font-medium text-primary-800">
            {section.procedureCount} {t("legalDatabase.content")}
          </span>
          <button
            onClick={(e) => {
              e.stopPropagation();
              onEdit(section);
            }}
            className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
            aria-label={t("common.edit")}
          >
            <Pencil className="h-4 w-4" />
          </button>
          <button
            onClick={(e) => {
              e.stopPropagation();
              onDelete(section);
            }}
            className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
            aria-label={t("common.delete")}
          >
            <Trash2 className="h-4 w-4" />
          </button>
        </div>
      </button>
      {open ? (
        <div className="border-t border-line-soft px-5 py-4">
          {section.description ? <p className="mb-4 text-sm text-ink-muted">{section.description}</p> : null}
          <div className="flex items-center justify-between">
            <h4 className="text-sm font-semibold text-ink">{t("legalDatabase.content")}</h4>
            <Button
              size="sm"
              variant="subtle"
              onClick={() => onAddProcedure(section.id, { title: "New step", description: "Describe the step" })}
            >
              <FilePlus className="h-3.5 w-3.5" /> {t("common.add")}
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
              <p className="text-sm text-ink-muted">{t("legalDatabase.noDocuments")}</p>
            )}
          </div>
        </div>
      ) : null}
    </Card>
  );
}

export default function LegalDatabasePage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [tab, setTab] = useState("sections");
  const [category, setCategory] = useState("");
  const [search, setSearch] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [editSection, setEditSection] = useState<LegalSection | null>(null);
  const [deleteSection, setDeleteSection] = useState<LegalSection | null>(null);
  const [createDocOpen, setCreateDocOpen] = useState(false);
  const [editDoc, setEditDoc] = useState<LegalDocument | null>(null);
  const [deleteDoc, setDeleteDoc] = useState<LegalDocument | null>(null);
  const [docSearch, setDocSearch] = useState("");

  const { data: sections, isLoading } = useQuery({
    queryKey: ["legal-sections", category, search],
    queryFn: () => legalSectionService.list({ category: category || undefined, search: search || undefined })
  });

  const { data: docsData, isLoading: docsLoading } = useQuery({
    queryKey: ["legal-documents", docSearch],
    queryFn: () => legalDocumentService.list({ pageSize: 100 })
  });
  const documents = docsData?.data ?? [];
  const filteredDocs = docSearch.trim()
    ? documents.filter((d) => (d.title + " " + (d.citation ?? "") + " " + (d.keywords ?? "")).toLowerCase().includes(docSearch.trim().toLowerCase()))
    : documents;

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["legal-sections"] });
    qc.invalidateQueries({ queryKey: ["legal-documents"] });
  };

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => legalSectionService.create(input),
    onSuccess: () => {
      invalidate();
      setCreateOpen(false);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateSectionMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: Record<string, unknown> }) =>
      legalSectionService.update(id, input),
    onSuccess: () => {
      invalidate();
      setEditSection(null);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteSectionMutation = useMutation({
    mutationFn: (id: string) => legalSectionService.remove(id),
    onSuccess: () => {
      invalidate();
      setDeleteSection(null);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const addProcedure = useMutation({
    mutationFn: ({ sectionId, input }: { sectionId: string; input: Record<string, unknown> }) =>
      legalSectionService.createProcedure(sectionId, input),
    onSuccess: () => {
      invalidate();
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const createDocMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => legalDocumentService.create(input),
    onSuccess: () => {
      invalidate();
      setCreateDocOpen(false);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateDocMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: Record<string, unknown> }) =>
      legalDocumentService.update(id, input),
    onSuccess: () => {
      invalidate();
      setEditDoc(null);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteDocMutation = useMutation({
    mutationFn: (id: string) => legalDocumentService.remove(id),
    onSuccess: () => {
      invalidate();
      setDeleteDoc(null);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title={t("legalDatabase.title")}
        subtitle={t("legalDatabase.subtitle")}
      />

      <div className="mb-4">
        <Tabs
          tabs={[
            { value: "sections", label: t("legalDatabase.laws"), icon: <Scale className="h-4 w-4" /> },
            { value: "documents", label: t("legalDatabase.judgments"), icon: <ScrollText className="h-4 w-4" /> }
          ]}
          value={tab}
          onChange={setTab}
        />
      </div>

      {tab === "sections" ? (
        <>
          <Card className="mb-4 p-4">
            <div className="flex flex-col gap-3 sm:flex-row">
              <div className="relative flex-1">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
                <Input
                  className="pl-9"
                  placeholder={t("legalDatabase.searchHint")}
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
              </div>
              <Select className="sm:w-48" value={category} onChange={(e) => setCategory(e.target.value)}>
                <option value="">{t("common.all")}</option>
                {categories.map((c) => (
                  <option key={c} value={c}>{c}</option>
                ))}
              </Select>
              <Button onClick={() => setCreateOpen(true)}>
                <Plus className="h-4 w-4" /> {t("legalDatabase.addDocument")}
              </Button>
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
                  onEdit={(section) => setEditSection(section)}
                  onDelete={(section) => setDeleteSection(section)}
                />
              ))}
            </div>
          ) : (
            <Card>
              <EmptyState
                icon={<BookOpen className="h-10 w-10" />}
                title={t("legalDatabase.noDocuments")}
                description={t("legalDatabase.noDocumentsDesc")}
                action={
                  <Button onClick={() => setCreateOpen(true)}>
                    <Plus className="h-4 w-4" /> {t("legalDatabase.addDocument")}
                  </Button>
                }
              />
            </Card>
          )}
        </>
      ) : (
        <>
          <Card className="mb-4 p-4">
            <div className="flex flex-col gap-3 sm:flex-row">
              <div className="relative flex-1">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
                <Input
                  className="pl-9"
                  placeholder={t("legalDatabase.searchHint")}
                  value={docSearch}
                  onChange={(e) => setDocSearch(e.target.value)}
                />
              </div>
              <Button onClick={() => setCreateDocOpen(true)}>
                <Plus className="h-4 w-4" /> {t("legalDatabase.addDocument")}
              </Button>
            </div>
          </Card>

          <Card>
            {docsLoading ? (
              <Loading />
            ) : filteredDocs && filteredDocs.length > 0 ? (
              <table className="table-base">
                <thead>
                  <tr>
                    <th>{t("legalDatabase.titleLabel")}</th>
                    <th>{t("legalDatabase.category")}</th>
                    <th>{t("legalDatabase.citation")}</th>
                    <th>{t("legalDatabase.year")}</th>
                    <th className="text-right">{t("common.actions")}</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredDocs.map((d) => (
                    <tr key={d.id}>
                      <td className="font-medium text-ink">
                        <div className="flex items-center gap-2">
                          <FileText className="h-4 w-4 text-ink-muted" />
                          {d.title}
                        </div>
                      </td>
                      <td className="text-ink-muted">{d.category}</td>
                      <td className="text-ink-muted">{d.citation ?? "—"}</td>
                      <td className="text-ink-muted">{d.year ?? "—"}</td>
                      <td>
                        <div className="flex items-center justify-end gap-1">
                          <Button variant="ghost" size="icon" onClick={() => setEditDoc(d)} aria-label={t("common.edit")}>
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="icon" onClick={() => setDeleteDoc(d)} aria-label={t("common.delete")}>
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
                icon={<ScrollText className="h-10 w-10" />}
                title={t("legalDatabase.noDocuments")}
                description={t("legalDatabase.noDocumentsDesc")}
                action={
                  <Button onClick={() => setCreateDocOpen(true)}>
                    <Plus className="h-4 w-4" /> {t("legalDatabase.addDocument")}
                  </Button>
                }
              />
            )}
          </Card>
        </>
      )}

      <SectionFormDialog
        open={createOpen}
        title={t("legalDatabase.addDocument")}
        submitLabel={t("common.create")}
        onClose={() => setCreateOpen(false)}
        onSubmit={(v) => createMutation.mutate(v)}
      />

      <SectionFormDialog
        open={!!editSection}
        editing={editSection}
        title={t("common.edit")}
        submitLabel={t("common.save")}
        onClose={() => setEditSection(null)}
        onSubmit={(v) => editSection && updateSectionMutation.mutate({ id: editSection.id, input: v })}
      />

      <LegalDocFormDialog
        open={createDocOpen}
        title={t("legalDatabase.addDocument")}
        submitLabel={t("common.create")}
        onClose={() => setCreateDocOpen(false)}
        onSubmit={(v) => createDocMutation.mutate(v)}
      />

      <LegalDocFormDialog
        open={!!editDoc}
        editing={editDoc}
        title={t("common.edit")}
        submitLabel={t("common.save")}
        onClose={() => setEditDoc(null)}
        onSubmit={(v) => editDoc && updateDocMutation.mutate({ id: editDoc.id, input: v })}
      />

      <Dialog
        open={!!deleteSection}
        onClose={() => setDeleteSection(null)}
        title={t("common.delete")}
        size="sm"
        footer={
          <>
            <Button variant="ghost" onClick={() => setDeleteSection(null)}>{t("common.cancel")}</Button>
            <Button variant="danger" onClick={() => deleteSection && deleteSectionMutation.mutate(deleteSection.id)}>
              {t("common.delete")}
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">
          {t("documents.deleteConfirm")} <span className="font-medium text-ink">{deleteSection?.sectionTitle}</span>
        </p>
      </Dialog>

      <Dialog
        open={!!deleteDoc}
        onClose={() => setDeleteDoc(null)}
        title={t("common.delete")}
        size="sm"
        footer={
          <>
            <Button variant="ghost" onClick={() => setDeleteDoc(null)}>{t("common.cancel")}</Button>
            <Button variant="danger" onClick={() => deleteDoc && deleteDocMutation.mutate(deleteDoc.id)}>
              {t("common.delete")}
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">
          {t("documents.deleteConfirm")} <span className="font-medium text-ink">{deleteDoc?.title}</span>
        </p>
      </Dialog>
    </div>
  );
}

function SectionFormDialog({
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
  editing?: LegalSection | null;
  title: string;
  submitLabel: string;
}) {
  const { t } = useLanguage();
  const [form, setForm] = useState(() => ({
    title: editing?.sectionTitle ?? "",
    sectionCode: editing?.sectionCode ?? "",
    actName: editing?.lawName ?? "",
    category: editing?.category ?? "Civil",
    content: editing?.description ?? ""
  }));
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={title}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button disabled={!form.title} onClick={() => onSubmit(form)}>{submitLabel}</Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("legalDatabase.titleLabel")} required className="sm:col-span-2">
          <Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} placeholder="e.g. Section 420, Penal Code 1860" />
        </Field>
        <Field label={t("legalDatabase.citation")}>
          <Input value={form.sectionCode} onChange={(e) => setForm({ ...form, sectionCode: e.target.value })} />
        </Field>
        <Field label={t("legalDatabase.content")}>
          <Input value={form.actName} onChange={(e) => setForm({ ...form, actName: e.target.value })} />
        </Field>
        <Field label={t("legalDatabase.category")}>
          <Select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
        </Field>
        <Field label={t("common.notes")} className="sm:col-span-2">
          <Textarea rows={4} value={form.content} onChange={(e) => setForm({ ...form, content: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}

function LegalDocFormDialog({
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
  editing?: LegalDocument | null;
  title: string;
  submitLabel: string;
}) {
  const { t } = useLanguage();
  const [form, setForm] = useState(() => ({
    title: editing?.title ?? "",
    category: editing?.category ?? "Judgment",
    content: editing?.content ?? "",
    citation: editing?.citation ?? "",
    judgeName: editing?.judgeName ?? "",
    keywords: editing?.keywords ?? "",
    year: editing?.year ? String(editing.year) : ""
  }));
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={title}
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button disabled={!form.title} onClick={() =>
            onSubmit({
              ...form,
              year: form.year ? Number(form.year) : null,
              citation: form.citation || null,
              judgeName: form.judgeName || null,
              keywords: form.keywords || null
            })
          }>
            {submitLabel}
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("legalDatabase.titleLabel")} required className="sm:col-span-2">
          <Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
        </Field>
        <Field label={t("legalDatabase.category")}>
          <Select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
            {docCategories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
        </Field>
        <Field label={t("legalDatabase.citation")}>
          <Input value={form.citation} onChange={(e) => setForm({ ...form, citation: e.target.value })} />
        </Field>
        <Field label={t("legalDatabase.year")}>
          <Input type="number" value={form.year} onChange={(e) => setForm({ ...form, year: e.target.value })} />
        </Field>
        <Field label="Judge">
          <Input value={form.judgeName} onChange={(e) => setForm({ ...form, judgeName: e.target.value })} />
        </Field>
        <Field label={t("legalDatabase.keywords")} className="sm:col-span-2">
          <Input value={form.keywords} onChange={(e) => setForm({ ...form, keywords: e.target.value })} />
        </Field>
        <Field label={t("legalDatabase.content")} className="sm:col-span-2">
          <Textarea rows={5} value={form.content} onChange={(e) => setForm({ ...form, content: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}