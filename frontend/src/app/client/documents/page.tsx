"use client";

import { useQuery } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Table } from "@/components/ui/table";
import { Loading, EmptyState } from "@/components/ui/loading";
import { clientPortalService } from "@/lib/services";
import { formatDate } from "@/lib/utils";
import { FileText } from "lucide-react";

export default function ClientDocumentsPage() {
  const { data, isLoading } = useQuery({
    queryKey: ["client", "documents"],
    queryFn: () => clientPortalService.documents()
  });

  return (
    <div>
      <PageHeader title="Documents" subtitle="Files your chamber has shared with you." />

      <Card>
        {isLoading ? (
          <Loading />
        ) : data && data.length > 0 ? (
          <Table>
            <thead>
              <tr>
                <th>File</th>
                <th>Case</th>
                <th>Category</th>
                <th>Size</th>
                <th>Shared On</th>
              </tr>
            </thead>
            <tbody>
              {data.map((d) => (
                <tr key={d.id}>
                  <td>
                    <p className="font-medium text-ink">{d.fileName}</p>
                    <p className="text-xs text-ink-muted">{d.uploadedByName}</p>
                  </td>
                  <td className="max-w-56 truncate text-ink-muted">{d.caseTitle}</td>
                  <td><span className="text-ink-muted">{d.category}</span></td>
                  <td className="text-ink-muted">{formatBytes(d.fileSize)}</td>
                  <td className="text-ink-muted">{formatDate(d.createdAt)}</td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <EmptyState icon={<FileText className="h-10 w-10" />} title="No documents shared" description="Files shared by your chamber will appear here." />
        )}
      </Card>
    </div>
  );
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
