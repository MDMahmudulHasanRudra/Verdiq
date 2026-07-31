"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams } from "next/navigation";
import { PageHeader } from "@/components/ui/page-header";
import { Input } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Loading, EmptyState } from "@/components/ui/loading";
import { StatusBadge } from "@/components/ui/badge";
import { searchService } from "@/lib/services";
import { Search as SearchIcon, FolderOpen, Users, FileText, Scale } from "lucide-react";

const typeIcons: Record<string, React.ComponentType<{ className?: string }>> = {
  case: FolderOpen,
  client: Users,
  document: FileText,
  legal: Scale
};

export default function SearchPage() {
  const router = useRouter();
  const params = useSearchParams();
  const [q, setQ] = useState(params.get("q") ?? "");

  const { data, isLoading } = useQuery({
    queryKey: ["search", q],
    queryFn: () => searchService.all(q),
    enabled: q.trim().length > 0
  });

  const results = data?.results ?? [];

  return (
    <div>
      <PageHeader title="Search" subtitle="Find cases, clients, documents and more across the firm." />
      <Card className="mb-6 p-4">
        <div className="relative">
          <SearchIcon className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
          <Input
            autoFocus
            className="pl-9"
            placeholder="Search the whole firm…"
            value={q}
            onChange={(e) => setQ(e.target.value)}
          />
        </div>
      </Card>

      {q.trim().length === 0 ? (
        <Card>
          <EmptyState icon={<SearchIcon className="h-10 w-10" />} title="Type to search" description="Search case numbers, client names, document titles and legal sections." />
        </Card>
      ) : isLoading ? (
        <Loading label="Searching…" />
      ) : results.length > 0 ? (
        <Card>
          <CardHeader title={`Results (${data?.totalCount ?? results.length})`} />
          <CardContent className="divide-y divide-line-soft">
            {results.map((r) => {
              const type = String(r.type).toLowerCase();
              const Icon = typeIcons[type] ?? FolderOpen;
              return (
                <button
                  key={r.id}
                  onClick={() => {
                    if (r.url.startsWith("/")) router.push(r.url);
                    else if (r.url) window.open(r.url, "_blank");
                  }}
                  className="flex w-full cursor-pointer items-center gap-4 px-2 py-3 text-left transition-colors hover:bg-slate-50"
                >
                  <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-50">
                    <Icon className="h-4 w-4 text-primary-700" />
                  </div>
                  <div className="flex-1">
                    <p className="text-sm font-medium text-ink">{r.title}</p>
                    <p className="text-xs text-ink-muted">{r.subtitle}</p>
                  </div>
                  <StatusBadge value={r.status || type} />
                </button>
              );
            })}
          </CardContent>
        </Card>
      ) : (
        <Card>
          <EmptyState title="No results" description={`Nothing matched "${q}".`} />
        </Card>
      )}
    </div>
  );
}
