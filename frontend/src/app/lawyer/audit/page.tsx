"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Table, Pagination } from "@/components/ui/table";
import { Loading, EmptyState } from "@/components/ui/loading";
import { auditService } from "@/lib/services";
import { formatDateTime } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { ShieldCheck } from "lucide-react";
import type { PagedResponse } from "@/types/api";

export default function AuditPage() {
  const toast = useToast();
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ["audit", "logs", page],
    queryFn: () => auditService.logs({ page, pageSize: 20 })
  });

  const logs = ((data as PagedResponse<Record<string, unknown>> | undefined)?.data ??
    (data as unknown as Record<string, unknown>[] | undefined) ??
    []) as Record<string, unknown>[];

  return (
    <div>
      <PageHeader title="Audit Logs" subtitle="A trail of actions across the firm." />
      <Card>
        {isLoading ? (
          <Loading />
        ) : logs.length > 0 ? (
          <>
            <Table>
              <thead>
                <tr>
                  <th>User</th>
                  <th>Action</th>
                  <th>Entity</th>
                  <th>Details</th>
                  <th>When</th>
                </tr>
              </thead>
              <tbody>
                {logs.map((l) => (
                  <tr key={String(l.id)}>
                    <td className="font-medium text-ink">{String(l.userName ?? l.user ?? "—")}</td>
                    <td className="text-ink-muted">{String(l.action ?? l.actionType ?? "—")}</td>
                    <td className="text-ink-muted">{String(l.entity ?? l.entityName ?? "—")}</td>
                    <td className="max-w-64 truncate text-ink-muted">{String(l.description ?? l.details ?? "—")}</td>
                    <td className="text-ink-muted">{formatDateTime(String(l.createdAt ?? l.timestamp ?? ""))}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
            <Pagination
              page={page}
              totalPages={(data as PagedResponse<Record<string, unknown>> | undefined)?.totalPages ?? 1}
              totalCount={(data as PagedResponse<Record<string, unknown>> | undefined)?.totalCount}
              onChange={setPage}
            />
          </>
        ) : (
          <EmptyState icon={<ShieldCheck className="h-10 w-10" />} title="No audit logs yet" description="Actions will be recorded here as the team works." />
        )}
      </Card>
    </div>
  );
}
