import Link from "next/link";
import { FileSearch, Home } from "lucide-react";

export default function NotFound() {
  return (
    <div className="flex min-h-[70vh] items-center justify-center px-6 py-16">
      <div className="flex max-w-md flex-col items-center text-center">
        <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-gold-50 text-gold-700">
          <FileSearch className="h-8 w-8" />
        </div>
        <p className="mt-6 text-sm font-semibold uppercase tracking-widest text-gold-700">404</p>
        <h1 className="mt-2 font-serif text-2xl font-semibold text-ink">Page not found</h1>
        <p className="mt-2 text-sm leading-relaxed text-ink-muted">
          The page you&apos;re looking for doesn&apos;t exist or may have been moved.
        </p>
        <Link
          href="/lawyer/dashboard"
          className="mt-6 inline-flex items-center gap-2 rounded-lg bg-ink px-4 py-2.5 text-sm font-medium text-white transition-colors hover:bg-ink/90"
        >
          <Home className="h-4 w-4" />
          Back to dashboard
        </Link>
      </div>
    </div>
  );
}
