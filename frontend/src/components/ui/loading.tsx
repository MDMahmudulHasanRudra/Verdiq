"use client";

import { Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";

export function Spinner({ className }: { className?: string }) {
  return <Loader2 className={cn("h-5 w-5 animate-spin text-primary-700", className)} />;
}

export function Loading({ label = "Loading...", dark = false }: { label?: string; dark?: boolean }) {
  return (
    <div className={cn("flex h-48 flex-col items-center justify-center gap-3", dark ? "text-slate-400" : "text-ink-muted")}>
      <Spinner className={cn("h-7 w-7", dark && "text-slate-300")} />
      <p className="text-sm">{label}</p>
    </div>
  );
}

export function EmptyState({
  icon,
  title,
  description,
  action,
  dark = false
}: {
  icon?: React.ReactNode;
  title: string;
  description?: string;
  action?: React.ReactNode;
  dark?: boolean;
}) {
  return (
    <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
      {icon ? <div className={cn("mb-4", dark ? "text-slate-500" : "text-ink-soft")}>{icon}</div> : null}
      <h3 className={cn("font-display text-lg font-semibold", dark ? "text-white" : "text-ink")}>{title}</h3>
      {description ? <p className={cn("mt-1 max-w-sm text-sm", dark ? "text-slate-400" : "text-ink-muted")}>{description}</p> : null}
      {action ? <div className="mt-4">{action}</div> : null}
    </div>
  );
}
