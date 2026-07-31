"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Table } from "@/components/ui/table";
import { Loading, EmptyState } from "@/components/ui/loading";
import { clientPortalService } from "@/lib/services";
import { formatDate } from "@/lib/utils";
import { FolderOpen } from "lucide-react";

export default function ClientCasesPage() {
  const { data, isLoading } = useQuery({
    queryKey: ["client", "cases"],
    queryFn: () => clientPortalService.cases()
  });

  return (
    <div>
      <PageHeader title="My Cases" subtitle="Track the progress of your legal matters." />

      <Card>
        {isLoading ? (
          <Loading />
        ) : data && data.length > 0 ? (
          <Table>
            <thead>
              <tr>
                <th>Case Number</th>
                <th>Title</th>
                <th>Type</th>
                <th>Lawyer</th>
                <th>Next Hearing</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {data.map((c) => (
                <tr key={c.id} className="cursor-pointer">
                  <td className="cursor-pointer">
                    <Link href={`/client/cases/${c.id}`} className="font-medium text-primary-700 hover:underline">
                      {c.caseNumber}
                    </Link>
                  </td>
                  <td className="max-w-64 truncate text-ink">{c.title}</td>
                  <td className="text-ink-muted">{c.caseType}</td>
                  <td className="text-ink-muted">{c.assignedLawyerName}</td>
                  <td className="text-ink-muted">{c.nextHearingDate ? formatDate(c.nextHearingDate) : "—"}</td>
                  <td><StatusBadge value={c.status} /></td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <EmptyState icon={<FolderOpen className="h-10 w-10" />} title="No cases shared" description="Your chamber hasn't shared any cases with you yet." />
        )}
      </Card>
    </div>
  );
}
