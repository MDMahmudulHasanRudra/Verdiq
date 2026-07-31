"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { Card } from "@/components/ui/card";
import { StatCard } from "@/components/ui/stat-card";
import { StatusBadge } from "@/components/ui/badge";
import { Table } from "@/components/ui/table";
import { Loading } from "@/components/ui/loading";
import { superAdminService } from "@/lib/services/super-admin-service";
import { formatCurrency } from "@/lib/utils";
import { Building2, Users, FolderOpen, BadgeCheck, TrendingUp, AlertTriangle } from "lucide-react";

export default function SuperAdminDashboardPage() {
  const { data, isLoading } = useQuery({
    queryKey: ["super-admin", "dashboard"],
    queryFn: () => superAdminService.dashboard()
  });

  if (isLoading || !data) {
    return <Loading label="Loading platform metrics…" />;
  }

  return (
    <div>
      <div className="mb-6">
        <h1 className="font-display text-2xl font-bold tracking-tight text-white">Platform Dashboard</h1>
        <p className="mt-1 text-sm text-slate-400">Overview of every chamber on Verdiq.</p>
      </div>

      {data.alerts.length > 0 ? (
        <div className="mb-6 space-y-2">
          {data.alerts.map((a, i) => (
            <div key={i} className="flex items-center gap-2 rounded-lg border border-gold-500/30 bg-gold-500/10 px-4 py-2.5 text-sm text-gold-300">
              <AlertTriangle className="h-4 w-4 shrink-0" /> {a}
            </div>
          ))}
        </div>
      ) : null}

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Total Chambers" value={data.totalChambers} icon={<Building2 className="h-5 w-5" />} accent="primary" />
        <StatCard label="Total Users" value={data.totalUsers} icon={<Users className="h-5 w-5" />} accent="blue" />
        <StatCard label="Total Cases" value={data.totalCases} icon={<FolderOpen className="h-5 w-5" />} accent="green" />
        <StatCard label="Active Subscriptions" value={data.activeSubscriptions} icon={<BadgeCheck className="h-5 w-5" />} accent="gold" />
      </div>

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Monthly Revenue" value={formatCurrency(data.monthlyRevenue)} icon={<TrendingUp className="h-5 w-5" />} accent="green" />
        <StatCard label="New Chambers This Month" value={data.newChambersThisMonth} icon={<Building2 className="h-5 w-5" />} accent="blue" />
        <StatCard label="New Users This Month" value={data.newUsersThisMonth} icon={<Users className="h-5 w-5" />} accent="primary" />
        <StatCard label="Expired Subscriptions" value={data.expiredSubscriptions} icon={<AlertTriangle className="h-5 w-5" />} accent="red" />
      </div>

      <Card className="border-slate-800 bg-slate-900">
        <div className="flex items-center justify-between border-b border-slate-800 px-5 py-4">
          <h2 className="font-display text-base font-bold text-white">Recent Chambers</h2>
          <Link href="/super-admin/chambers" className="text-sm font-medium text-primary-300 hover:underline">
            View all →
          </Link>
        </div>
        {data.chambers.length > 0 ? (
          <Table className="dark-table">
            <thead>
              <tr>
                <th>Chamber</th>
                <th>Plan</th>
                <th>Users</th>
                <th>Cases</th>
                <th>Revenue</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {data.chambers.slice(0, 8).map((c) => (
                <tr key={c.id}>
                  <td>
                    <p className="font-medium text-white">{c.name}</p>
                    <p className="text-xs text-slate-400">{c.address ?? "—"}</p>
                  </td>
                  <td><StatusBadge value={c.subscriptionPlan} /></td>
                  <td className="text-slate-300">{c.usersCount}</td>
                  <td className="text-slate-300">{c.casesCount}</td>
                  <td className="text-slate-300">{formatCurrency(c.totalRevenue)}</td>
                  <td><StatusBadge value={c.isActive ? "Active" : "Inactive"} /></td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <p className="p-6 text-sm text-slate-400">No chambers registered yet.</p>
        )}
      </Card>
    </div>
  );
}
