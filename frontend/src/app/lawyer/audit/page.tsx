"use client";

import { useQuery } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Loading, EmptyState } from "@/components/ui/loading";
import { StatusBadge } from "@/components/ui/badge";
import { auditService } from "@/lib/services";
import { timeAgo } from "@/lib/utils";
import { useLanguage } from "@/lib/i18n";
import { ScrollText } from "lucide-react";

export default function AuditPage() {
  const { t } = useLanguage();
  const { data, isLoading } = useQuery({
    queryKey: ["audit-logs"],
    queryFn: () => auditService.logs()
  });

  const logs = (data as Record<string, unknown>)?.logs ?? [];

  return (
    <div>
      <PageHeader title={t("audit.title")} subtitle={t("audit.subtitle")} />
      <Card>
        <CardHeader title={`${t("audit.title")} (${Array.isArray(logs) ? logs.length : 0})`} />
        <CardContent>
          {isLoading ? (
            <Loading />
          ) : !Array.isArray(logs) || logs.length === 0 ? (
            <EmptyState icon={<ScrollText className="h-10 w-10" />} title={t("audit.noLogs")} description={t("audit.noLogsDesc")} />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-line-soft text-left text-xs font-medium text-ink-soft uppercase tracking-wider">
                    <th className="px-4 py-3">{t("audit.user")}</th>
                    <th className="px-4 py-3">{t("audit.action")}</th>
                    <th className="px-4 py-3">{t("audit.entity")}</th>
                    <th className="px-4 py-3">{t("audit.details")}</th>
                    <th className="px-4 py-3">{t("audit.when")}</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-line-soft">
                  {logs.map((l: Record<string, unknown>) => (
                    <tr key={String(l.id)} className="hover:bg-slate-50">
                      <td className="px-4 py-3 font-medium text-ink">{String(l.user_name ?? l.userName ?? "—")}</td>
                      <td className="px-4 py-3"><StatusBadge value={String(l.action)} /></td>
                      <td className="px-4 py-3 text-ink-muted">{String(l.entity ?? "—")}</td>
                      <td className="max-w-sm truncate px-4 py-3 text-ink-muted">{String(l.details ?? "—")}</td>
                      <td className="px-4 py-3 text-xs text-ink-soft">{l.created_at ? timeAgo(String(l.created_at)) : "—"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
