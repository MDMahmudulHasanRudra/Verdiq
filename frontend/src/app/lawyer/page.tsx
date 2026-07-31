"use client";

import Link from "next/link";
import { useAuthStore } from "@/lib/store/auth-store";
import {
  useDashboardStats,
  useCaseChart,
  useRecentActivities,
  useLawyerProductivity,
  useUpcomingHearings,
  useMyTasks
} from "@/lib/hooks";
import { StatCard } from "@/components/ui/stat-card";
import { BarChart, DonutChart, ChartLegend } from "@/components/ui/charts";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { EmptyState } from "@/components/ui/loading";
import { formatDate, timeAgo, initials, cn } from "@/lib/utils";
import { FolderOpen, Users, CalendarClock, Receipt, TrendingUp, ArrowRight, FileText } from "lucide-react";

const activityIcon = {
  case: FolderOpen,
  hearing: CalendarClock,
  document: FileText
} as const;

export default function LawyerDashboardPage() {
  const user = useAuthStore((s) => s.user);
  const { data: stats, isLoading: statsLoading } = useDashboardStats();
  const { data: chart } = useCaseChart(12);
  const { data: activities } = useRecentActivities(10);
  const { data: productivity } = useLawyerProductivity();
  const { data: hearings } = useUpcomingHearings();
  const { data: tasks } = useMyTasks();

  const donutData = [
    { label: "Active", value: stats?.activeCases ?? 0, color: "#3b5bdb" },
    { label: "Pending", value: stats?.pendingCases ?? 0, color: "#f59e0b" },
    { label: "Closed", value: stats?.closedCases ?? 0, color: "#94a3b8" }
  ].filter((d) => d.value > 0);

  const chartLabels = chart?.map((c) => c.month) ?? [];
  const chartSeries = [
    { name: "Active", color: "#3b5bdb", data: chart?.map((c) => c.active) ?? [] },
    { name: "Closed", color: "#b45309", data: chart?.map((c) => c.closed) ?? [] },
    { name: "Pending", color: "#94a3b8", data: chart?.map((c) => c.pending) ?? [] }
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-2xl font-bold text-ink">
          Welcome back, {user?.fullName?.split(" ")[0] ?? "Counsel"}
        </h1>
        <p className="mt-1 text-sm text-ink-muted">
          {formatDate(new Date().toISOString(), "dddd, D MMMM YYYY")} — here&apos;s what&apos;s happening at the firm.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard
          icon={<FolderOpen className="h-5 w-5" />}
          accent="primary"
          label="Active Cases"
          value={statsLoading ? "—" : stats?.activeCases ?? 0}
          trend={stats?.caseGrowth != null ? `${stats.caseGrowth > 0 ? "+" : ""}${stats.caseGrowth}% MoM` : undefined}
          trendUp={(stats?.caseGrowth ?? 0) >= 0}
        />
        <StatCard
          icon={<Users className="h-5 w-5" />}
          accent="gold"
          label="Clients"
          value={statsLoading ? "—" : stats?.totalClients ?? 0}
          trend={stats?.clientGrowth != null ? `${stats.clientGrowth > 0 ? "+" : ""}${stats.clientGrowth}% MoM` : undefined}
          trendUp={(stats?.clientGrowth ?? 0) >= 0}
        />
        <StatCard
          icon={<CalendarClock className="h-5 w-5" />}
          accent="green"
          label="Hearings Today"
          value={statsLoading ? "—" : stats?.hearingsToday ?? 0}
          trend={`${stats?.upcomingHearings ?? 0} upcoming`}
          trendUp
        />
        <StatCard
          icon={<Receipt className="h-5 w-5" />}
          accent="red"
          label="Closed Cases"
          value={statsLoading ? "—" : stats?.closedCases ?? 0}
          trend={`${stats?.pendingCases ?? 0} pending`}
          trendUp={false}
        />
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
        <Card className="xl:col-span-2">
          <CardHeader
            title="Case Volume (12 months)"
            description="Active, closed and pending cases per month"
            action={
              <Link href="/lawyer/cases" className="inline-flex items-center gap-1 text-sm font-medium text-primary-700 hover:underline">
                View all <ArrowRight className="h-3.5 w-3.5" />
              </Link>
            }
          />
          <CardContent>
            {chart && chart.length > 0 ? (
              <div className="space-y-3">
                <BarChart labels={chartLabels} series={chartSeries} />
                <ChartLegend series={chartSeries} />
              </div>
            ) : (
              <EmptyState title="No case data yet" description="Case filings will appear here once cases are created." />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader title="Cases by Status" />
          <CardContent className="space-y-4">
            {donutData.length > 0 ? (
              <DonutChart data={donutData} />
            ) : (
              <EmptyState title="No cases" description="Create a case to see the breakdown." />
            )}
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-3">
        <Card>
          <CardHeader
            title="Upcoming Hearings"
            action={
              <Link href="/lawyer/hearings" className="inline-flex items-center gap-1 text-sm font-medium text-primary-700 hover:underline">
                All <ArrowRight className="h-3.5 w-3.5" />
              </Link>
            }
          />
          <CardContent className="space-y-3">
            {hearings && hearings.length > 0 ? (
              hearings.slice(0, 5).map((h) => (
                <div key={h.id} className="flex items-start justify-between gap-3 rounded-lg border border-line p-3">
                  <div>
                    <p className="text-sm font-medium text-ink">{h.caseNumber ?? "—"}</p>
                    <p className="mt-0.5 text-xs text-ink-muted">{h.caseTitle ?? h.courtroom}</p>
                    <p className="mt-1 text-xs text-ink-muted">{h.courtroom}{h.judgeName ? ` · ${h.judgeName}` : ""}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-xs font-semibold text-primary-700">{formatDate(h.hearingDate)}</p>
                    <StatusBadge value={h.status} className="mt-1" />
                  </div>
                </div>
              ))
            ) : (
              <EmptyState title="No hearings" description="Scheduled hearings will show here." />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader
            title="My Tasks"
            action={
              <Link href="/lawyer/tasks" className="inline-flex items-center gap-1 text-sm font-medium text-primary-700 hover:underline">
                All <ArrowRight className="h-3.5 w-3.5" />
              </Link>
            }
          />
          <CardContent className="space-y-3">
            {tasks && tasks.length > 0 ? (
              tasks.slice(0, 5).map((t) => (
                <div key={t.id} className="flex items-center justify-between gap-3 rounded-lg border border-line p-3">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium text-ink">{t.title}</p>
                    <p className="mt-0.5 text-xs text-ink-muted">
                      {t.dueDate ? `Due ${formatDate(t.dueDate)}` : "No due date"}
                    </p>
                  </div>
                  <StatusBadge value={t.status} />
                </div>
              ))
            ) : (
              <EmptyState title="No tasks assigned" description="Tasks assigned to you will appear here." />
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader title="Recent Activity" />
          <CardContent className="space-y-1">
            {activities && activities.length > 0 ? (
              activities.map((a) => {
                const Icon = activityIcon[a.type as keyof typeof activityIcon] ?? FileText;
                return (
                  <div key={a.id} className="flex items-start gap-3 rounded-lg px-2 py-2 hover:bg-slate-50">
                    <div className={cn("flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-xs font-semibold", "bg-primary-50 text-primary-700")}>
                      <Icon className="h-4 w-4" />
                    </div>
                    <div className="min-w-0 flex-1">
                      <p className="text-sm text-ink">{a.title}</p>
                      <p className="truncate text-xs text-ink-muted">{a.description}</p>
                      <p className="text-xs text-ink-soft">{timeAgo(a.timestamp)}</p>
                    </div>
                  </div>
                );
              })
            ) : (
              <EmptyState title="No activity yet" description="Firm activity will be logged here." />
            )}
          </CardContent>
        </Card>
      </div>

      {productivity && productivity.length > 0 && (
        <Card>
          <CardHeader title="Lawyer Productivity" action={<TrendingUp className="h-4 w-4 text-ink-soft" />} />
          <CardContent>
            <div className="space-y-4">
              {productivity.map((p) => {
                const closedRate = p.totalCases > 0 ? (p.closedCases / p.totalCases) * 100 : 0;
                return (
                  <div key={p.id} className="flex items-center gap-4">
                    <div className="w-48 truncate text-sm font-medium text-ink">{p.name}</div>
                    <div className="h-2 flex-1 overflow-hidden rounded-full bg-slate-100">
                      <div
                        className="h-full rounded-full bg-gradient-to-r from-primary-700 to-gold-600"
                        style={{ width: `${Math.min(100, closedRate)}%` }}
                      />
                    </div>
                    <div className="w-32 text-right text-xs text-ink-muted">
                      {p.closedCases} closed / {p.totalCases} cases
                    </div>
                    <div className="w-16 text-right text-sm font-semibold text-primary-700">
                      {Math.round(closedRate)}%
                    </div>
                  </div>
                );
              })}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
