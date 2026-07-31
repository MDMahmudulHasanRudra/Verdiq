"use client";

import type { ReactNode } from "react";
import { Card } from "@/components/ui/card";
import { cn } from "@/lib/utils";

export function StatCard({
  label,
  value,
  icon,
  trend,
  trendUp,
  accent = "primary",
  loading
}: {
  label: string;
  value: ReactNode;
  icon?: ReactNode;
  trend?: string;
  trendUp?: boolean;
  accent?: "primary" | "gold" | "green" | "red" | "blue";
  loading?: boolean;
}) {
  const accents = {
    primary: "bg-primary-50 text-primary-700",
    gold: "bg-gold-50 text-gold-700",
    green: "bg-emerald-50 text-emerald-600",
    red: "bg-red-50 text-red-600",
    blue: "bg-blue-50 text-blue-600"
  };
  return (
    <Card className="p-5">
      <div className="flex items-start justify-between">
        <div className="min-w-0">
          <p className="text-sm font-medium text-ink-muted">{label}</p>
          {loading ? (
            <div className="mt-2 h-8 w-20 animate-pulse rounded bg-slate-200" />
          ) : (
            <p className="mt-1 truncate font-display text-2xl font-bold text-ink">{value}</p>
          )}
          {trend ? (
            <p className={cn("mt-1 text-xs font-medium", trendUp ? "text-emerald-600" : "text-red-500")}>{trend}</p>
          ) : null}
        </div>
        {icon ? <div className={cn("rounded-lg p-2.5", accents[accent])}>{icon}</div> : null}
      </div>
    </Card>
  );
}
