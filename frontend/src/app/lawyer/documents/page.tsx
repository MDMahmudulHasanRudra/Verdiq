"use client";

import { useState } from "react";
import { useSearchParams } from "next/navigation";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { documentService } from "@/lib/services";
import { useCases } from "@/lib/hooks";
import { getErrorMessage, formatDateTime, API_URL } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { FileText, Upload, Download, Trash2 } from "lucide-react";

const categories = ["Pleadings", "Evidence", "Court Orders", "Correspondence", "Contracts", "Fees", "Other"];

export default function DocumentsPage() {
  const params = useSearchParams();
  const initialCaseId = params.get("caseId") ?? "";
  const toast = useToast();
  const qc = useQueryClient();
  const [category, setCategory] = useState("");
  const [uploadOpen, setUploadOpen] = useState(false);
  const [search, setSearch] = useState("");

  const { data: documents, isLoading } = useQuery({
    queryKey: ["documents", category, initialCaseId, search],
    queryFn: () =>
      documentService.list({
        category: category || undefined,
        caseId: initialCaseId || undefined,
        search: search || undefined
      })
  });

  const uploadMutation = useMutation({
    mutationFn: ({ file, caseId, docCategory }: { file: File; caseId: string; docCategory: string }) =>
      documentService.upload(file, caseId, docCategory),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["documents"] });
      qc.invalidateQueries({ queryKey: ["case"] });
      setUploadOpen(false);
      toast.success("Document uploaded");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => documentService.remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["documents"] });
      toast.success("Document deleted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title="Documents"
        subtitle="Organize case files, pleadings and evidence."
        actions={
          <Button onClick={() => setUploadOpen(true)}>
            <Upload className="h-4 w-4" /> Upload
          </Button>
        }
      />

      <Card className="mb-4 p-4">
        <div className="flex flex-col gap-3 sm:flex-row">
          <Select className="sm:w-56" value={category} onChange={(e) => setCategory(e.target.value)}>
            <option value="">All categories</option>
            {categories.map((c) => (
              <option key={c} value={c}>{c}</option>
            ))}
          </Select>
          <div className="relative flex-1">
            <Input
              placeholder="Search file, case number or title…"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
          {initialCaseId ? (
            <div className="flex items-center gap-2 rounded-lg bg-primary-50 px-3 py-2 text-xs text-primary-800">
              Filtered by case {initialCaseId.slice(0, 8)}…
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
                <th>Name</th>
                <th>Category</th>
                <th>Case</th>
                <th>Size</th>
                <th>Version</th>
                <th>Uploaded</th>
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
                    <div className="flex items-center gap-1">
                      <a
                        className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                        aria-label="Download"
                        title="Download"
                        href={`${API_URL}/documents/download/${d.id}`}
                        target="_blank"
                        rel="noreferrer"
                      >
                        <Download className="h-4 w-4" />
                      </a>
                      <button
                        onClick={() => deleteMutation.mutate(d.id)}
                        className="cursor-pointer rounded-lg p-1.5 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
                        aria-label="Delete"
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
            title="No documents"
            description="Upload pleadings, evidence and correspondence."
            action={<Button onClick={() => setUploadOpen(true)}><Upload className="h-4 w-4" /> Upload</Button>}
          />
        )}
      </Card>

      <UploadDialog
        open={uploadOpen}
        onClose={() => setUploadOpen(false)}
        initialCaseId={initialCaseId}
        onSubmit={(v) => uploadMutation.mutate(v)}
      />
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
  const [file, setFile] = useState<File | null>(null);
  const [caseId, setCaseId] = useState(initialCaseId);
  const [docCategory, setDocCategory] = useState("Evidence");

  const { data: casesData } = useCases({ pageSize: 100 });
  const cases = casesData?.data ?? [];

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Upload Document"
      description="Attach a file to a case. Documents are stored securely and visible from the case page."
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!file || !caseId} onClick={() => file && onSubmit({ file, caseId, docCategory })}>
            Upload
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        <Field label="File" required>
          <input
            type="file"
            className="w-full text-sm text-ink file:mr-4 file:cursor-pointer file:rounded-lg file:border-0 file:bg-primary-50 file:px-4 file:py-2 file:text-sm file:font-medium file:text-primary-800 file:transition-colors hover:file:bg-primary-100"
            onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          />
        </Field>
        <Field label="Case" required>
          <Select value={caseId} onChange={(e) => setCaseId(e.target.value)}>
            <option value="">Select a case…</option>
            {cases.map((c) => (
              <option key={c.id} value={c.id}>
                {c.caseNumber} — {c.title}
              </option>
            ))}
          </Select>
        </Field>
        <Field label="Category">
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
