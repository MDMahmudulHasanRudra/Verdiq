"use client";

import { useQuery } from "@tanstack/react-query";
import { useParams } from "next/navigation";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Loading, EmptyState } from "@/components/ui/loading";
import { clientPortalService } from "@/lib/services";
import { formatDate } from "@/lib/utils";
import { FileText, CalendarClock } from "lucide-react";

export default function ClientCaseDetailPage() {
  const params = useParams<{ id: string }>();
  const { data, isLoading } = useQuery({
    queryKey: ["client", "case", params.id],
    queryFn: () => clientPortalService.caseDetail(params.id)
  });

  if (isLoading || !data) {
    return <Loading label="Loading case details…" />;
  }

  return (
    <div>
      <PageHeader title={data.caseNumber} subtitle={data.title} />

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-1">
          <Card className="p-5">
            <h2 className="mb-4 font-display text-base font-bold text-ink">Case Information</h2>
            <dl className="space-y-3 text-sm">
              <div className="flex justify-between gap-3">
                <dt className="text-ink-muted">Court</dt>
                <dd className="text-right font-medium text-ink">{data.courtName}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="text-ink-muted">Type</dt>
                <dd className="text-right font-medium text-ink">{data.caseType}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="text-ink-muted">Opponent</dt>
                <dd className="text-right font-medium text-ink">{data.opponent}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="text-ink-muted">Filing Date</dt>
                <dd className="text-right font-medium text-ink">{formatDate(data.filingDate)}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="text-ink-muted">Status</dt>
                <dd><StatusBadge value={data.status} /></dd>
              </div>
            </dl>
          </Card>

          <Card className="p-5">
            <h2 className="mb-4 font-display text-base font-bold text-ink">Your Lawyer</h2>
            <p className="text-sm font-semibold text-ink">{data.assignedLawyerName}</p>
            <p className="mt-1 text-xs text-ink-muted">{data.assignedLawyerEmail}</p>
            <p className="text-xs text-ink-muted">{data.assignedLawyerPhone}</p>
          </Card>
        </div>

        <Card className="lg:col-span-2">
          <div className="border-b border-line px-5 py-4">
            <h2 className="font-display text-base font-bold text-ink">Case Timeline</h2>
          </div>
          {data.timeline.length > 0 ? (
            <ol className="space-y-0 p-5">
              {data.timeline.map((t, i) => (
                <li key={t.id} className="relative pb-6 pl-8 last:pb-0">
                  {i < data.timeline.length - 1 ? <span className="absolute left-2 top-2 h-full w-px bg-line" /> : null}
                  <span className="absolute left-0 top-1 flex h-4 w-4 items-center justify-center rounded-full bg-primary-100">
                    {t.type === "Hearing" ? (
                      <CalendarClock className="h-2.5 w-2.5 text-primary-700" />
                    ) : (
                      <FileText className="h-2.5 w-2.5 text-primary-700" />
                    )}
                  </span>
                  <p className="text-sm font-medium text-ink">{t.description}</p>
                  <p className="text-xs text-ink-muted">{t.type} · {formatDate(t.timestamp)}</p>
                </li>
              ))}
            </ol>
          ) : (
            <EmptyState icon={<FileText className="h-8 w-8" />} title="No activity yet" description="Updates to your case will appear here." />
          )}
        </Card>
      </div>
    </div>
  );
}
