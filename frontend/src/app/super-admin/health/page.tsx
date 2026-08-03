"use client";

import { useQuery } from "@tanstack/react-query";
import { Card } from "@/components/ui/card";
import { StatCard } from "@/components/ui/stat-card";
import { Loading } from "@/components/ui/loading";
import { superAdminService } from "@/lib/services/super-admin-service";
import { formatCurrency, formatDate } from "@/lib/utils";
import { useLanguage } from "@/lib/i18n";
import { Activity, Database, Users, FolderOpen, HardDrive, Clock } from "lucide-react";
import type { SystemHealth } from "@/types/super-admin";

export default function SuperAdminHealthPage() {
  const { t } = useLanguage();
  const { data, isLoading } = useQuery({
    queryKey: ["super-admin", "health"],
    queryFn: () => superAdminService.health()
  });

  if (isLoading || !data) {
    return <Loading dark label={t("superAdmin.health.checkingHealth")} />;
  }

  return (
    <div>
      <div className="mb-6">
        <h1 className="font-display text-2xl font-bold tracking-tight text-white">{t("superAdmin.health.title")}</h1>
        <p className="mt-1 text-sm text-slate-400">{t("superAdmin.health.subtitle")}</p>
      </div>

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard
          label={t("superAdmin.health.apiStatus")}
          value={<span className="flex items-center gap-2"><Dot status={data.status} /> {data.status}</span>}
          icon={<Activity className="h-5 w-5" />}
          accent={data.status === "Healthy" ? "green" : "red"}
        />
        <StatCard
          label={t("superAdmin.health.database")}
          value={<span className="flex items-center gap-2"><Dot status={data.databaseStatus} /> {data.databaseStatus}</span>}
          icon={<Database className="h-5 w-5" />}
          accent={data.databaseStatus === "Healthy" ? "green" : "red"}
        />
        <StatCard label={t("superAdmin.health.activeConnections")} value={data.activeConnections} icon={<Users className="h-5 w-5" />} accent="blue" />
        <StatCard label={t("superAdmin.health.uptime")} value={data.uptime} icon={<Clock className="h-5 w-5" />} accent="gold" />
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <Card className="border-slate-800 bg-slate-900">
            <div className="border-b border-slate-800 px-5 py-4">
              <h2 className="font-display text-base font-bold text-white">{t("superAdmin.health.platformCounts")}</h2>
            </div>
            <dl className="grid grid-cols-2 gap-x-6 gap-y-4 p-5 sm:grid-cols-3">
              <Metric label={t("superAdmin.health.chambers")} value={data.totalChambers} />
              <Metric label={t("superAdmin.users.title")} value={data.totalUsers} />
              <Metric label={t("superAdmin.health.cases")} value={data.totalCases} />
              <Metric label={t("superAdmin.health.activeSubscriptions")} value={data.activeSubscriptions} />
              <Metric label={t("superAdmin.health.monthlyRevenue")} value={formatCurrency(data.monthlyRevenue)} />
              <Metric label={t("superAdmin.health.storageUsed")} value={formatBytes(data.storageUsedBytes)} />
            </dl>
          </Card>

          <Card className="border-slate-800 bg-slate-900">
            <div className="border-b border-slate-800 px-5 py-4">
              <h2 className="font-display text-base font-bold text-white">{t("superAdmin.health.storageBackup")}</h2>
            </div>
            <dl className="space-y-3 p-5 text-sm">
              <div className="flex justify-between gap-3">
                <dt className="flex items-center gap-2 text-slate-400"><Database className="h-4 w-4" /> {t("superAdmin.health.databaseSize")}</dt>
                <dd className="font-medium text-white">{formatBytes(data.databaseSizeBytes)}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="flex items-center gap-2 text-slate-400"><HardDrive className="h-4 w-4" /> {t("superAdmin.health.storageUsedBytes")}</dt>
                <dd className="font-medium text-white">{formatBytes(data.storageUsedBytes)}</dd>
              </div>
              <div className="flex justify-between gap-3">
                <dt className="text-slate-400">{t("superAdmin.health.lastBackup")}</dt>
                <dd className="font-medium text-white">{data.lastBackup ? formatDate(data.lastBackup) : t("superAdmin.health.never")}</dd>
              </div>
            </dl>
          </Card>
        </div>

        <Card className="border-slate-800 bg-slate-900">
          <div className="border-b border-slate-800 px-5 py-4">
            <h2 className="font-display text-base font-bold text-white">{t("superAdmin.health.activeAlerts")}</h2>
          </div>
          {data.activeAlerts.length > 0 ? (
            <ul className="space-y-2 p-5">
              {data.activeAlerts.map((a, i) => (
                <li key={i} className="rounded-lg border border-red-500/30 bg-red-500/10 px-3 py-2 text-sm text-red-300">
                  {a}
                </li>
              ))}
            </ul>
          ) : (
            <p className="p-5 text-sm text-slate-400">{t("superAdmin.health.noAlerts")}</p>
          )}
        </Card>
      </div>
    </div>
  );
}

function Dot({ status }: { status: string }) {
  const healthy = status === "Healthy";
  return <span className={`inline-block h-2.5 w-2.5 rounded-full ${healthy ? "bg-emerald-400" : "bg-red-400"}`} />;
}

function Metric({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <p className="text-xs text-slate-400">{label}</p>
      <p className="mt-0.5 font-display text-lg font-bold text-white">{value}</p>
    </div>
  );
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  if (bytes < 1024 * 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  return `${(bytes / (1024 * 1024 * 1024)).toFixed(2)} GB`;
}
