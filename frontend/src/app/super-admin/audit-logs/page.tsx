"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/field";
import { Table, Pagination } from "@/components/ui/table";
import { Loading, EmptyState } from "@/components/ui/loading";
import { superAdminService } from "@/lib/services/super-admin-service";
import { formatDateTime } from "@/lib/utils";
import { ScrollText } from "lucide-react";

export default function SuperAdminAuditLogsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");

  const { data, isLoading } = useQuery({
    queryKey: ["super-admin", "audit-logs", page],
    queryFn: () => superAdminService.auditLogs(page, 50)
  });

  const filtered = (data ?? []).filter(
    (l) => !search || l.userName.toLowerCase().includes(search.toLowerCase()) || l.action.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div>
      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="font-display text-2xl font-bold tracking-tight text-white">Audit Logs</h1>
          <p className="mt-1 text-sm text-slate-400">Platform-wide activity trail.</p>
        </div>
        <Input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Filter by user or action…"
          className="w-full max-w-xs border-slate-700 bg-slate-800 text-white placeholder:text-slate-500"
        />
      </div>

      <Card className="border-slate-800 bg-slate-900">
        {isLoading ? (
          <Loading dark />
        ) : filtered.length > 0 ? (
          <>
            <Table className="dark-table">
              <thead>
                <tr>
                  <th>User</th>
                  <th>Action</th>
                  <th>Entity</th>
                  <th>IP</th>
                  <th>Timestamp</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((l) => (
                  <tr key={l.id}>
                    <td>
                      <p className="font-medium text-white">{l.userName}</p>
                      <p className="text-xs text-slate-400">{l.userId}</p>
                    </td>
                    <td>
                      <span className="text-slate-200">{l.actionLabel ?? l.action}</span>
                      {l.changes?.length ? (
                        <p className="text-xs text-slate-400">{l.changes.map((c) => c.field).join(", ")}</p>
                      ) : null}
                    </td>
                    <td className="text-slate-400">{l.entity}</td>
                    <td className="text-slate-400">{l.ipAddress || "—"}</td>
                    <td className="text-slate-400">{formatDateTime(l.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
            <Pagination page={page} totalPages={Math.ceil(filtered.length / 50)} onChange={setPage} />
          </>
        ) : (
          <EmptyState dark icon={<ScrollText className="h-10 w-10" />} title="No audit logs" description="Activity logs will appear here as events occur." />
        )}
      </Card>
    </div>
  );
}
