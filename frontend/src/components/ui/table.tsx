"use client";

import type { ReactNode, TableHTMLAttributes } from "react";
import { cn } from "@/lib/utils";

export function Table({ className, ...props }: TableHTMLAttributes<HTMLTableElement>) {
  return <table className={cn("table-base", className)} {...props} />;
}

export function Th({
  children,
  className,
  sortable,
  onClick
}: {
  children: ReactNode;
  className?: string;
  sortable?: boolean;
  onClick?: () => void;
}) {
  return (
    <th className={cn(className)}>
      {sortable ? (
        <button
          onClick={onClick}
          className="inline-flex cursor-pointer items-center gap-1 uppercase tracking-wide transition-colors hover:text-ink"
        >
          {children}
        </button>
      ) : (
        children
      )}
    </th>
  );
}

export function Pagination({
  page,
  totalPages,
  onChange,
  totalCount
}: {
  page: number;
  totalPages: number;
  onChange: (page: number) => void;
  totalCount?: number;
}) {
  if (totalPages <= 1 && totalCount === undefined) return null;
  if (totalPages <= 1) return null;
  return (
    <div className="flex items-center justify-between px-5 py-3">
      <p className="text-xs text-ink-muted">
        {totalCount !== undefined ? `${totalCount} items · ` : ""}Page {page} of {totalPages}
      </p>
      <div className="flex items-center gap-1">
        <button
          disabled={page <= 1}
          onClick={() => onChange(page - 1)}
          className="cursor-pointer rounded-lg border border-line px-3 py-1.5 text-xs font-medium text-ink transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
        >
          Previous
        </button>
        <button
          disabled={page >= totalPages}
          onClick={() => onChange(page + 1)}
          className="cursor-pointer rounded-lg border border-line px-3 py-1.5 text-xs font-medium text-ink transition-colors hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-50"
        >
          Next
        </button>
      </div>
    </div>
  );
}
