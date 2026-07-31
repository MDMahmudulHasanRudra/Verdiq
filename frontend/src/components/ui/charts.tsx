"use client";

import { cn } from "@/lib/utils";

interface Series {
  name: string;
  color: string;
  data: number[];
}

export function BarChart({
  labels,
  series,
  height = 260,
  className
}: {
  labels: string[];
  series: Series[];
  height?: number;
  className?: string;
}) {
  const max = Math.max(1, ...series.flatMap((s) => s.data));
  const barGroup = 28 + (series.length - 1) * 14;
  const padding = 28;
  const innerWidth = 800 - padding * 2;
  const groupWidth = innerWidth / Math.max(labels.length, 1);

  return (
    <div className={cn("w-full overflow-x-auto", className)}>
      <svg viewBox={`0 0 800 ${height}`} className="h-auto w-full min-w-[520px]" role="img">
        {/* grid lines */}
        {[0, 0.25, 0.5, 0.75, 1].map((f) => (
          <g key={f}>
            <line
              x1={padding}
              x2={800 - padding}
              y1={10 + f * (height - 40)}
              y2={10 + f * (height - 40)}
              stroke="#e2e8f0"
              strokeWidth={1}
            />
            <text
              x={padding - 8}
              y={14 + f * (height - 40)}
              textAnchor="end"
              fontSize={10}
              fill="#94a3b8"
            >
              {Math.round(max * (1 - f))}
            </text>
          </g>
        ))}
        {labels.map((label, i) => {
          const x = padding + i * groupWidth + (groupWidth - barGroup) / 2;
          return (
            <g key={label}>
              {series.map((s, si) => {
                const barW = 12;
                const bx = x + si * (barW + 4);
                const h = (s.data[i] / max) * (height - 50);
                return (
                  <rect
                    key={s.name}
                    x={bx}
                    y={height - 30 - h}
                    width={barW}
                    height={Math.max(h, s.data[i] > 0 ? 2 : 0)}
                    rx={3}
                    fill={s.color}
                  >
                    <title>{`${label} · ${s.name}: ${s.data[i]}`}</title>
                  </rect>
                );
              })}
              <text
                x={x + barGroup / 2}
                y={height - 12}
                textAnchor="middle"
                fontSize={10}
                fill="#64748b"
              >
                {label}
              </text>
            </g>
          );
        })}
      </svg>
    </div>
  );
}

export function ChartLegend({ series }: { series: Series[] }) {
  return (
    <div className="flex flex-wrap items-center gap-4">
      {series.map((s) => (
        <span key={s.name} className="inline-flex items-center gap-1.5 text-xs text-ink-muted">
          <span className="h-2.5 w-2.5 rounded-sm" style={{ background: s.color }} />
          {s.name}
        </span>
      ))}
    </div>
  );
}

export function DonutChart({
  data,
  size = 180,
  thickness = 22
}: {
  data: { label: string; value: number; color: string }[];
  size?: number;
  thickness?: number;
}) {
  const total = Math.max(1, data.reduce((acc, d) => acc + d.value, 0));
  const radius = (size - thickness) / 2;
  const circumference = 2 * Math.PI * radius;
  let offset = 0;

  return (
    <div className="flex items-center gap-6">
      <svg width={size} height={size} className="shrink-0">
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          stroke="#f1f5f9"
          strokeWidth={thickness}
        />
        {data.map((d) => {
          if (d.value <= 0) return null;
          const fraction = d.value / total;
          const dash = fraction * circumference;
          const el = (
            <circle
              key={d.label}
              cx={size / 2}
              cy={size / 2}
              r={radius}
              fill="none"
              stroke={d.color}
              strokeWidth={thickness}
              strokeDasharray={`${dash} ${circumference - dash}`}
              strokeDashoffset={-offset}
              strokeLinecap="butt"
            />
          );
          offset += dash;
          return el;
        })}
        <text
          x="50%"
          y="47%"
          textAnchor="middle"
          className="fill-ink font-semibold"
          fontSize={Math.round(size / 8)}
        >
          {total}
        </text>
        <text x="50%" y="58%" textAnchor="middle" fontSize={Math.round(size / 18)} className="fill-slate-400">
          total
        </text>
      </svg>
      <div className="space-y-2">
        {data.map((d) => (
          <div key={d.label} className="flex items-center gap-2 text-sm">
            <span className="h-2.5 w-2.5 rounded-sm" style={{ background: d.color }} />
            <span className="text-ink-muted">{d.label}</span>
            <span className="font-semibold text-ink">{d.value}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
