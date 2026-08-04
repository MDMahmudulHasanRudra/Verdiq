"use client";

import { useState } from "react";
import { Download, ExternalLink, X } from "lucide-react";
import { Dialog } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { API_URL, downloadBlob } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { getErrorMessage } from "@/lib/utils";
import { documentService } from "@/lib/services";

export function DocumentPreview({
  documentId,
  fileName,
  fileType,
  open,
  onClose
}: {
  documentId: string;
  fileName: string;
  fileType: string;
  open: boolean;
  onClose: () => void;
}) {
  const toast = useToast();
  const [loading, setLoading] = useState(false);

  const isImage = fileType?.startsWith("image/");
  const isPdf = fileType === "application/pdf";
  const isPreviewable = isImage || isPdf;

  const handleDownload = async () => {
    try {
      setLoading(true);
      const blob = await documentService.download(documentId);
      downloadBlob(blob, fileName);
    } catch (e) {
      toast.error(getErrorMessage(e));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={fileName}
      description={isPreviewable ? "Preview" : "No preview available"}
      size="lg"
      footer={
        <>
          <Button variant="outline" onClick={handleDownload} disabled={loading}>
            <Download className="h-4 w-4" /> Download
          </Button>
          <Button variant="ghost" onClick={onClose}>Close</Button>
        </>
      }
    >
      {isPreviewable ? (
        <div className="flex items-center justify-center rounded-lg bg-slate-50 p-2">
          {isImage ? (
            <img
              src={`${API_URL}/documents/download/${documentId}`}
              alt={fileName}
              className="max-h-[60vh] rounded-lg object-contain"
            />
          ) : isPdf ? (
            <iframe
              src={`${API_URL}/documents/download/${documentId}`}
              className="h-[60vh] w-full rounded-lg border-0"
              title={fileName}
            />
          ) : null}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <p className="text-sm text-ink-muted">Preview not available for this file type.</p>
          <Button variant="outline" onClick={handleDownload} disabled={loading} className="mt-4">
            <Download className="h-4 w-4" /> Download to view
          </Button>
        </div>
      )}
    </Dialog>
  );
}
