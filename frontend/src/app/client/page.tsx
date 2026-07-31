"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { StatCard } from "@/components/ui/stat-card";
import { StatusBadge } from "@/components/ui/badge";
import { Table } from "@/components/ui/table";
import { Loading, EmptyState } from "@/components/ui/loading";
import { clientPortalService } from "@/lib/services";
import { formatDate } from "@/lib/utils";
import { FolderOpen, CalendarClock, Receipt, FileText, TrendingUp } from "lucide-react";

export default function ClientDashboardPage() {
  const { data, isLoading } = useQuery({
    queryKey: ["client", "dashboard"],
    queryFn: () => clientPortalService.dashboard()
  });

  if (isLoading || !data) {
    return <Loading label="Loading your dashboard…" />;
  }

  return (
    <div>
      <PageHeader title="Client Dashboard" subtitle="Your case updates at a glance." />

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Active Cases" value={data.activeCases} icon={<FolderOpen className="h-5 w-5" />} accent="primary" />
        <StatCard label="Upcoming Hearings" value={data.upcomingHearings} icon={<CalendarClock className="h-5 w-5" />} accent="blue" />
        <StatCard label="Pending Invoices" value={data.pendingInvoices} icon={<Receipt className="h-5 w-5" />} accent="red" />
        <StatCard
          label="Outstanding Balance"
          value={formatCurrency(data.outstandingBalance)}
          icon={<TrendingUp className="h-5 w-5" />}
          accent="gold"
        />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <Card>
          <div className="border-b border-line px-5 py-4">
            <h2 className="font-display text-base font-bold text-ink">Recent Cases</h2>
          </div>
          {data.recentCases.length > 0 ? (
            <Table>
              <thead>
                <tr>
                  <th>Case</th>
                  <th>Status</th>
                  <th>Next Hearing</th>
                </tr>
              </thead>
              <tbody>
                {data.recentCases.map((c) => (
                  <tr key={c.id} className="cursor-pointer">
                    <td className="cursor-pointer" onClick={() => (window.location.href = `/client/cases/${c.id}`)}>
                      <p className="font-medium text-primary-700">{c.caseNumber}</p>
                      <p className="truncate text-xs text-ink-muted">{c.title}</p>
                    </td>
                    <td><StatusBadge value={c.status} /></td>
                    <td className="text-ink-muted">{c.nextHearingDate ? formatDate(c.nextHearingDate) : "—"}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
          ) : (
            <EmptyState icon={<FolderOpen className="h-8 w-8" />} title="No cases yet" description="Your chamber hasn't shared any cases with you yet." />
          )}
        </Card>

        <div className="grid grid-cols-1 gap-6">
          <Card>
            <div className="border-b border-line px-5 py-4">
              <h2 className="font-display text-base font-bold text-ink">Upcoming Hearings</h2>
            </div>
            {data.upcomingHearingList.length > 0 ? (
              <ul className="divide-y divide-line-soft">
                {data.upcomingHearingList.map((h) => (
                  <li key={h.id} className="flex items-center justify-between gap-3 px-5 py-3">
                    <div>
                      <p className="text-sm font-medium text-ink">{h.caseTitle}</p>
                      <p className="text-xs text-ink-muted">{h.caseNumber}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-medium text-primary-700">{formatDate(h.hearingDate)}</p>
                      <p className="text-xs text-ink-muted">{h.status}</p>
                    </div>
                  </li>
                ))}
              </ul>
            ) : (
              <EmptyState icon={<CalendarClock className="h-8 w-8" />} title="No hearings" description="No upcoming hearings scheduled." />
            )}
          </Card>

          <Card>
            <div className="border-b border-line px-5 py-4">
              <h2 className="font-display text-base font-bold text-ink">Recent Invoices</h2>
            </div>
            {data.recentInvoices.length > 0 ? (
              <ul className="divide-y divide-line-soft">
                {data.recentInvoices.map((inv) => (
                  <li key={inv.id} className="flex items-center justify-between gap-3 px-5 py-3">
                    <div>
                      <p className="text-sm font-medium text-ink">{inv.invoiceNumber}</p>
                      <p className="text-xs text-ink-muted">{inv.description ?? inv.status}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-semibold text-ink">{formatCurrency(inv.amount)}</p>
                      <p className="text-xs text-ink-muted">{inv.status}</p>
                    </div>
                  </li>
                ))}
              </ul>
            ) : (
              <EmptyState icon={<Receipt className="h-8 w-8" />} title="No invoices" description="You have no invoices yet." />
            )}
          </Card>
        </div>
      </div>
    </div>
  );
}

function formatCurrency(n: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "BDT", maximumFractionDigits: 0 }).format(n);
}
