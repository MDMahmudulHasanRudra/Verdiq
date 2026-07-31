"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Scale, Bell, LogOut, Menu, X } from "lucide-react";
import { useAuthStore } from "@/lib/store/auth-store";
import { performLogout } from "@/lib/auth-actions";
import { clientPortalService } from "@/lib/services";
import { useQuery } from "@tanstack/react-query";
import { useToast } from "@/components/ui/toast";
import { Loading } from "@/components/ui/loading";
import { cn } from "@/lib/utils";

const navItems = [
  { label: "Dashboard", href: "/client" },
  { label: "My Cases", href: "/client/cases" },
  { label: "Hearings", href: "/client/hearings" },
  { label: "Documents", href: "/client/documents" },
  { label: "Invoices", href: "/client/invoices" },
  { label: "Messages", href: "/client/messages" }
];

export default function ClientLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const toast = useToast();
  const user = useAuthStore((s) => s.user);
  const loading = useAuthStore((s) => s.loading);
  const [menuOpen, setMenuOpen] = useState(false);

  const { data: profile } = useQuery({
    queryKey: ["client", "profile"],
    queryFn: () => clientPortalService.profile(),
    enabled: !!user
  });

  const { data: unread = 0 } = useQuery({
    queryKey: ["client", "unread"],
    queryFn: () => clientPortalService.unreadCount(),
    enabled: !!user
  });

  useEffect(() => {
    if (!loading && !user) router.replace("/login");
  }, [loading, user, router]);

  if (loading || !user) {
    return <Loading label="Loading your portal…" />;
  }

  const onLogout = () => {
    performLogout();
    toast.success("Signed out");
    router.replace("/login");
  };

  return (
    <div className="min-h-screen bg-surface">
      <header className="sticky top-0 z-30 border-b border-line bg-card/90 backdrop-blur">
        <div className="mx-auto flex h-16 max-w-6xl items-center gap-3 px-4 sm:px-6">
          <button
            className="cursor-pointer rounded-lg p-2 text-ink-muted hover:bg-slate-100 lg:hidden"
            onClick={() => setMenuOpen((v) => !v)}
            aria-label="Toggle menu"
          >
            {menuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </button>
          <Link href="/client" className="flex items-center gap-2.5">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-800">
              <Scale className="h-5 w-5 text-gold-400" />
            </div>
            <div className="hidden sm:block">
              <p className="font-display text-lg font-bold leading-none text-ink">Verdiq</p>
              <p className="mt-0.5 text-[10px] font-medium uppercase tracking-widest text-ink-muted">Client Portal</p>
            </div>
          </Link>

          <nav className="ml-auto hidden items-center gap-1 lg:flex">
            {navItems.map((item) => {
              const active = item.href === "/client" ? pathname === "/client" : pathname.startsWith(item.href);
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={cn(
                    "rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                    active ? "bg-primary-50 text-primary-800" : "text-ink-muted hover:bg-slate-50 hover:text-ink"
                  )}
                >
                  {item.label}
                </Link>
              );
            })}
          </nav>

          <div className="ml-auto flex items-center gap-1.5 lg:ml-2">
            <Link
              href="/client/messages"
              className="relative cursor-pointer rounded-lg p-2 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
              aria-label="Messages"
            >
              <Bell className="h-5 w-5" />
              {unread > 0 ? (
                <span className="absolute right-1 top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
                  {unread > 9 ? "9+" : unread}
                </span>
              ) : null}
            </Link>
            <button
              onClick={onLogout}
              className="cursor-pointer rounded-lg p-2 text-ink-muted transition-colors hover:bg-slate-100 hover:text-red-500"
              aria-label="Sign out"
            >
              <LogOut className="h-5 w-5" />
            </button>
          </div>
        </div>

        {menuOpen ? (
          <nav className="border-t border-line bg-card px-4 py-2 lg:hidden">
            {navItems.map((item) => {
              const active = item.href === "/client" ? pathname === "/client" : pathname.startsWith(item.href);
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  onClick={() => setMenuOpen(false)}
                  className={cn(
                    "block rounded-lg px-3 py-2 text-sm font-medium",
                    active ? "bg-primary-50 text-primary-800" : "text-ink-muted hover:bg-slate-50 hover:text-ink"
                  )}
                >
                  {item.label}
                </Link>
              );
            })}
          </nav>
        ) : null}
      </header>

      <main className="mx-auto max-w-6xl px-4 py-6 sm:px-6">
        {profile?.chamberName ? (
          <p className="mb-6 text-center text-xs text-ink-muted">Served by {profile.chamberName}</p>
        ) : null}
        {children}
      </main>
    </div>
  );
}
