"use client";

import { useState } from "react";
import { useSearchParams } from "next/navigation";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { documentService } from "@/lib/services";
import { useCases } from "@/lib/hooks";
import { getErrorMessage, formatDateTime, API_URL } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { FileText, Upload, Download, Trash2, Eye, Pencil, X } from "lucide-react";
import type { Document } from "@/types/models";

const categories = ["Pleadings", "Evidence", "Court Orders", "Correspondence", "Contracts", "Fees", "Other"];

export default function DocumentsPage() {
  const params = useSearchParams();
  const initialCaseId = params.get("caseId") ?? "";
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [category, setCategory] = useState("");
  const [uploadOpen, setUploadOpen] = useState(false);
  const [search, setSearch] = useState("");
  const [editDoc, setEditDoc] = useState<Document | null>(null);
  const [deleteDoc, setDeleteDoc] = useState<Document | null>(null);
  const [previewDoc, setPreviewDoc] = useState<Document | null>(null);

  const { data: documents, isLoading } = useQuery({
    queryKey: ["documents", category, initialCaseId, search],
    queryFn: () =>
      documentService.list({
        category: category || undefined,
        caseId: initialCaseId || undefined,
        search: search || undefined
      })
  });

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ["documents"] });
    qc.invalidateQueries({ queryKey: ["case"] });
  };

  const uploadMutation = useMutation({
    mutationFn: ({ file, caseId, docCategory }: { file: File; caseId: string; docCategory: string }) =>
      documentService.upload(file, caseId, docCategory),
    onSuccess: () => {
      invalidate();
      setUploadOpen(false);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: Record<string, unknown> }) =>
      documentService.update(id, input),
    onSuccess: () => {
      invalidate();
      setEditDoc(null);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => documentService.remove(id),
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
        title={t("documents.title")}
        subtitle={t("documents.subtitle")}
        actions={
          <Button onClick={() => setUploadOpen(true)}>
            <Upload className="h-4 w-4" /> {t("documents.uploadDocument")}
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <div className="flex flex-col gap-3 sm:flex-row">
          <Select className="sm:w-56" value={category} onChange={(e) => setCategory(e.target.value)}>
            <option value="">{t("common.all")}</option>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
          <div className="relative flex-1">
            <Input
              placeholder={`${t("common.search")}…`}
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          {initialCaseId ? (
            <div className="flex items-center gap-2 rounded-lg bg-primary-50 px-3 py-2 text-xs text-primary-800">
              {initialCaseId.slice(0, 8)}…
            </div>
          ) : null}
        </div>
      </Card>

      <Card>
        {isLoading ? (
          <Loading />
        ) : documents && documents.length > 0 ? (
          <table className="table-base">
            <thead>
              <tr>
                <th>{t("documents.name")}</th>
                <th>{t("legalDatabase.category")}</th>
                <th>{t("invoices.case")}</th>
                <th>{t("documents.size")}</th>
                <th>{t("documents.versions")}</th>
                <th>{t("documents.uploaded")}</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {documents.map((d) => (
                <tr key={d.id}>
                  <td>
                    <div className="flex items-center gap-3">
                      <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-50">
                        <FileText className="h-4 w-4 text-primary-700" />
                      </div>
                      <div>
                        <p className="font-medium text-ink">{d.originalFileName ?? d.fileName}</p>
                        <p className="text-xs text-ink-muted">{d.fileType}</p>
                      </div>
                    </div>
                  </td>
                  <td className="text-ink-muted">{d.category}</td>
                  <td className="max-w-40 truncate text-ink-muted">{d.caseTitle ?? d.caseId.slice(0, 8)}</td>
                  <td className="text-ink-muted">
                    {d.fileSize ? `${(d.fileSize / 1024).toFixed(0)} KB` : "—"}
                  </td>
                  <td><StatusBadge value={`v${d.version}`} /></td>
                  <td className="text-ink-muted">
                    {formatDateTime(d.createdAt)}
                    <p className="text-xs">{d.uploadedByName}</p>
                  </td>
                  <td>
                    <div className="flex items-center justify-end gap-1">
                      <button
                        className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                        aria-label={t("common.preview")}
                        title={t("common.preview")}
                        onClick={() => setPreviewDoc(d)}
                      >
                        <Eye className="h-4 w-4" />
                      </button>
                      <a
                        className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                        aria-label={t("common.download")}
                        title={t("common.download")}
                        href={`${API_URL}/documents/download/${d.id}`}
                        target="_blank"
                        rel="noreferrer"
                      >
                        <Download className="h-4 w-4" />
                      </a>
                      <button
                        onClick={() => setEditDoc(d)}
                        className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                        aria-label={t("common.edit")}
                      >
                        <Pencil className="h-4 w-4" />
                      </button>
                      <button
                        onClick={() => setDeleteDoc(d)}
                        className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
                        aria-label={t("common.delete")}
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <EmptyState
            icon={<FileText className="h-10 w-10" />}
            title={t("documents.noDocuments")}
            description={t("documents.noDocumentsDesc")}
            action={
              <Button onClick={() => setUploadOpen(true)}>
                <Upload className="h-4 w-4" /> {t("documents.uploadDocument")}
              </Button>
            }
          />
        )}
      </Card>

      <UploadDialog
        open={uploadOpen}
        onClose={() => setUploadOpen(false)}
        initialCaseId={initialCaseId}
        onSubmit={(v) => uploadMutation.mutate(v)}
      />

      <EditDocumentDialog
        open={!!editDoc}
        document={editDoc}
        onClose={() => setEditDoc(null)}
        onSubmit={(input) => editDoc && updateMutation.mutate({ id: editDoc.id, input })}
      />

      <PreviewDialog open={!!previewDoc} document={previewDoc} onClose={() => setPreviewDoc(null)} />

      <Dialog
        open={!!deleteDoc}
        onClose={() => setDeleteDoc(null)}
        title={t("common.delete")}
        size="sm"
        footer={
          <>
            <Button variant="ghost" onClick={() => setDeleteDoc(null)}>{t("common.cancel")}</Button>
            <Button variant="danger" onClick={() => deleteDoc && deleteMutation.mutate(deleteDoc.id)}>
              {t("common.delete")}
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">
          {t("documents.deleteConfirm")} <span className="font-medium text-ink">{deleteDoc?.originalFileName ?? deleteDoc?.fileName}</span>
        </p>
      </Dialog>
    </div>
  );
}

function UploadDialog({
  open,
  onClose,
  initialCaseId,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  initialCaseId: string;
  onSubmit: (v: { file: File; caseId: string; docCategory: string }) => void;
}) {
  const { t } = useLanguage();
  const [file, setFile] = useState<File | null>(null);
  const [caseId, setCaseId] = useState(initialCaseId);
  const [docCategory, setDocCategory] = useState("Evidence");

  const { data: casesData } = useCases({ pageSize: 100 });
  const cases = casesData?.data ?? [];

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("documents.uploadDocument")}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button disabled={!file || !caseId} onClick={() => file && onSubmit({ file, caseId, docCategory })}>
            {t("common.upload")}
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        <Field label={t("documents.name")} required>
          <input
            type="file"
            className="w-full text-sm text-ink file:mr-4 file:cursor-pointer file:rounded-lg file:border-0 file:bg-primary-50 file:px-4 file:py-2 file:text-sm file:font-medium file:text-primary-800 file:transition-colors hover:file:bg-primary-100"
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          />
        </Field>
        <Field label={t("invoices.case")} required>
          <Select value={caseId} onChange={(e) => setCaseId(e.target.value)}>
            <option value="">{t("common.select")}</option>
            {cases.map((c) => (
              <option key={c.id} value={c.id}>
                {c.caseNumber} — {c.title}
              </option>
            ))}
          </Select>
        </Field>
        <Field label={t("legalDatabase.category")}>
          <Select value={docCategory} onChange={(e) => setDocCategory(e.target.value)}>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
        </Field>
      </div>
    </Dialog>
  );
}

function EditDocumentDialog({
  open,
  document,
  onClose,
  onSubmit
}: {
  open: boolean;
  document: Document | null;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const { t } = useLanguage();
  const [category, setCategory] = useState(document?.category ?? "Evidence");
  const [description, setDescription] = useState(document?.description ?? "");
  const [tags, setTags] = useState(document?.tags ?? "");

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("common.edit")}
      description={document?.originalFileName ?? document?.fileName}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button disabled={!category} onClick={() => onSubmit({ category, description: description || null, tags: tags || null })}>
            {t("common.save")}
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        <Field label={t("legalDatabase.category")}>
          <Select value={category} onChange={(e) => setCategory(e.target.value)}>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
        </Field>
        <Field label={t("invoices.description")}>
          <Textarea rows={2} value={description} onChange={(e) => setDescription(e.target.value)} />
        </Field>
        <Field label={t("legalDatabase.keywords")}>
          <Input value={tags} onChange={(e) => setTags(e.target.value)} placeholder="comma, separated" />
        </Field>
      </div>
    </Dialog>
  );
}

function PreviewDialog({
  open,
  document,
  onClose
}: {
  open: boolean;
  document: Document | null;
  onClose: () => void;
}) {
  const { t } = useLanguage();

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("common.preview")}
      description={document?.originalFileName ?? document?.fileName}
      size="xl"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.close")}</Button>
          <a href={`${API_URL}/documents/download/${document?.id}`} target="_blank" rel="noreferrer">
            <Button>{t("common.download")}</Button>
          </a>
        </>
      }
    >
      {document ? (
        <div className="flex h-[60vh] flex-col items-center justify-center rounded-lg border border-line bg-surface text-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-xl bg-primary-50 mb-4">
            <FileText className="h-7 w-7 text-primary-700" />
          </div>
          <p className="text-sm font-medium text-ink">{document.originalFileName ?? document.fileName}</p>
          <p className="mt-1 text-xs text-ink-muted">
            {document.fileType} · {document.category} · v{document.version}
          </p>
          <a
            href={`${API_URL}/documents/preview/${document.id}`}
            target="_blank"
            rel="noreferrer"
            className="mt-4 cursor-pointer rounded-lg bg-primary-700 px-4 py-2 text-sm font-medium text-white hover:bg-primary-800"
          >
            {t("common.preview")}
          </a>
          <p className="mt-2 flex items-center gap-1 text-xs text-ink-muted">
            <X className="h-3 w-3" /> {t("documents.versions")}: {document.versionCount}
          </p>
        </div>
      ) : null}
    </Dialog>
  );
}