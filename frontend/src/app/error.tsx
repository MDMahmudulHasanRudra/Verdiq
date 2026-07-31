"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { AlertTriangle, RefreshCw, Home, ChevronDown, Database } from "lucide-react";

export default function ErrorPage({
  error,
  reset
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  const [showDetails, setShowDetails] = useState(false);

  useEffect(() => {
    console.error("[Verdiq] page error:", error);
  }, [error]);

  const message = error?.message || "Something went wrong while loading this page.";

  const backendHint = /network|ERR_CONNECTION|ECONNREFUSED|timeout|request failed|5000|failed to fetch/i.test(
    message
  );

  return (
    <div className="flex min-h-[70vh] items-center justify-center px-6 py-16">
      <div className="w-full max-w-md">
        <div className="flex flex-col items-center text-center">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-red-50 text-red-500">
            <AlertTriangle className="h-8 w-8" />
          </div>
          <h1 className="mt-6 font-serif text-2xl font-semibold text-ink">
            This page hit an error
          </h1>
          <p className="mt-2 text-sm leading-relaxed text-ink-muted">
            We couldn&apos;t load what you asked for. Try again, or go back to the dashboard.
          </p>

          <div className="mt-6 flex items-center gap-3">
            <button
              onClick={reset}
              className="inline-flex cursor-pointer items-center gap-2 rounded-lg bg-ink px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-ink/90"
            >
              <RefreshCw className="h-4 w-4" />
              Try again
            </button>
            <Link
              href="/lawyer/dashboard"
              className="inline-flex items-center gap-2 rounded-lg border border-line px-4 py-2.5 text-sm font-medium text-ink transition-colors hover:bg-slate-50"
            >
              <Home className="h-4 w-4" />
              Dashboard
            </Link>
          </div>

          {backendHint ? (
            <div className="mt-6 flex w-full items-start gap-3 rounded-xl border border-amber-200 bg-amber-50 p-4 text-left">
              <Database className="mt-0.5 h-5 w-5 shrink-0 text-amber-600" />
              <div>
                <p className="text-sm font-semibold text-amber-900">
                  Backend server looks unreachable
                </p>
                <p className="mt-1 text-xs leading-relaxed text-amber-800">
                  The data for this module is served by the API. Start it with{" "}
                  <code className="rounded bg-amber-100 px-1 py-0.5 text-[11px] font-mono">
                    dotnet run
                  </code>{" "}
                  inside <code className="rounded bg-amber-100 px-1 py-0.5 text-[11px] font-mono">backend/Verdiq.API</code>,
                  then refresh.
                </p>
              </div>
            </div>
          ) : null}

          <button
            onClick={() => setShowDetails((v) => !v)}
            className="mt-4 inline-flex cursor-pointer items-center gap-1 text-xs font-medium text-ink-soft transition-colors hover:text-ink"
          >
            Error details
            <ChevronDown className={`h-3.5 w-3.5 transition-transform ${showDetails ? "rotate-180" : ""}`} />
          </button>

          {showDetails ? (
            <pre className="mt-3 w-full overflow-x-auto rounded-lg border border-line bg-slate-50 p-3 text-left text-[11px] leading-relaxed text-ink-muted">
              {message}
              {error?.digest ? `\n\ndigest: ${error.digest}` : ""}
            </pre>
          ) : null}
        </div>
      </div>
    </div>
  );
}
