"use client";

import { useQuery } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Table } from "@/components/ui/table";
import { Loading, EmptyState } from "@/components/ui/loading";
import { clientPortalService } from "@/lib/services";
import { formatDateTime } from "@/lib/utils";
import { CalendarClock } from "lucide-react";

export default function ClientHearingsPage() {
  const { data, isLoading } = useQuery({
    queryKey: ["client", "hearings"],
    queryFn: () => clientPortalService.hearings()
  });

  return (
    <div>
      <PageHeader title="Hearings" subtitle="Your scheduled court appearances." />

      <Card>
        {isLoading ? (
          <Loading />
        ) : data && data.length > 0 ? (
          <Table>
            <thead>
              <tr>
                <th>Case</th>
                <th>Hearing Date</th>
                <th>Courtroom</th>
                <th>Judge</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {data.map((h) => (
                <tr key={h.id}>
                  <td>
                    <p className="font-medium text-primary-700">{h.caseNumber}</p>
                    <p className="truncate text-xs text-ink-muted">{h.caseTitle}</p>
                  </td>
                  <td>{formatDateTime(h.hearingDate)}</td>
                  <td className="text-ink-muted">{h.courtroom ?? "—"}</td>
                  <td className="text-ink-muted">{h.judgeName ?? "—"}</td>
                  <td><StatusBadge value={h.status} /></td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <EmptyState icon={<CalendarClock className="h-10 w-10" />} title="No hearings" description="Your chamber hasn't scheduled any hearings for you yet." />
        )}
      </Card>
    </div>
  );
}
