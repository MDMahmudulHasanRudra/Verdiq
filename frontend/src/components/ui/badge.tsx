"use client";

import { cn } from "@/lib/utils";

type Tone = "default" | "primary" | "gold" | "green" | "red" | "amber" | "blue" | "slate" | "purple";

const tones: Record<Tone, string> = {
  default: "bg-slate-100 text-slate-700",
  primary: "bg-primary-50 text-primary-800",
  gold: "bg-gold-50 text-gold-800",
  green: "bg-emerald-50 text-emerald-700",
  red: "bg-red-50 text-red-700",
  amber: "bg-amber-50 text-amber-700",
  blue: "bg-blue-50 text-blue-700",
  slate: "bg-slate-100 text-slate-600",
  purple: "bg-purple-50 text-purple-700"
};

export function toneFor(value: string | null | undefined): Tone {
  const v = (value || "").toLowerCase();
  if (/active|granted|paid|completed|done|approved|present|success|active|open|won/.test(v)) return "green";
  if (/pending|scheduled|draft|trial|running|unreconciled/.test(v)) return "gold";
  if (/closed|inactive|revoked|cancelled|forfeited|canceled|disposed|absent/.test(v)) return "slate";
  if (/overdue|expired|critical|urgent|failed|rejected|high|blacklist/.test(v)) return "red";
  if (/in.?progress|adjourned|appeal|resigned|terminated/.test(v)) return "amber";
  if (/client|junior|assistant|medium/.test(v)) return "blue";
  if (/senior|owner|admin|super/.test(v)) return "purple";
  return "default";
}

export function Badge({
  children,
  tone,
  className
}: {
  children: React.ReactNode;
  tone?: Tone;
  className?: string;
}) {
  return (
    <span className={cn("inline-flex items-center gap-1 rounded-full px-2.5 py-0.5 text-xs font-medium", tones[tone || "default"], className)}>
      {children}
    </span>
  );
}

export function StatusBadge({ value, className }: { value: string | null | undefined; className?: string }) {
  return (
    <Badge tone={toneFor(value)} className={className}>
      {value || "—"}
    </Badge>
  );
}
