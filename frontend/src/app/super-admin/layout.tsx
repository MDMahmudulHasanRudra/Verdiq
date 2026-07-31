"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Scale, X, ShieldAlert, LayoutDashboard, Building2, Users, BadgeCheck, KeyRound, ScrollText, Settings, Activity, LogOut } from "lucide-react";
import { tokenStore, cookieStore } from "@/lib/api";
import { Loading } from "@/components/ui/loading";
import { cn } from "@/lib/utils";

const navItems = [
  { label: "Dashboard", href: "/super-admin/dashboard", icon: LayoutDashboard },
  { label: "Chambers", href: "/super-admin/chambers", icon: Building2 },
  { label: "Users", href: "/super-admin/users", icon: Users },
  { label: "Subscriptions", href: "/super-admin/subscriptions", icon: BadgeCheck },
  { label: "Permissions", href: "/super-admin/permissions", icon: KeyRound },
  { label: "Audit Logs", href: "/super-admin/audit-logs", icon: ScrollText },
  { label: "System Config", href: "/super-admin/config", icon: Settings },
  { label: "System Health", href: "/super-admin/health", icon: Activity }
];

export default function SuperAdminLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (!tokenStore.saGet()) {
      router.replace("/super-admin/login");
      return;
    }
    setReady(true);
  }, [router]);

  if (!ready) {
    return <Loading label="Loading console…" />;
  }

  const admin = tokenStore.saGetUser<{ name?: string; userId?: string; role?: string }>();

  const onLogout = () => {
    tokenStore.saClear();
    cookieStore.clearSaAccess();
    router.replace("/super-admin/login");
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      {sidebarOpen ? (
        <div className="fixed inset-0 z-40 bg-black/50 lg:hidden" onClick={() => setSidebarOpen(false)} />
      ) : null}
      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-50 flex w-64 flex-col border-r border-slate-800 bg-slate-900 transition-transform duration-200 lg:translate-x-0",
          sidebarOpen ? "translate-x-0" : "-translate-x-full"
        )}
      >
        <div className="flex h-16 items-center justify-between border-b border-slate-800 px-5">
          <Link href="/super-admin/dashboard" className="flex items-center gap-2.5">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-800">
              <Scale className="h-5 w-5 text-gold-400" />
            </div>
            <div>
              <p className="font-display text-lg font-bold leading-none text-white">Verdiq</p>
              <p className="mt-0.5 text-[10px] font-medium uppercase tracking-widest text-slate-400">Super Admin</p>
            </div>
          </Link>
          <button onClick={() => setSidebarOpen(false)} className="cursor-pointer text-slate-400 lg:hidden" aria-label="Close menu">
            <X className="h-5 w-5" />
          </button>
        </div>

        <nav className="flex-1 space-y-1 overflow-y-auto px-3 py-4">
          {navItems.map((item) => {
            const active = pathname.startsWith(item.href);
            const Icon = item.icon;
            return (
              <Link
                key={item.href}
                href={item.href}
                onClick={() => setSidebarOpen(false)}
                className={cn(
                  "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors duration-100",
                  active ? "bg-primary-900/60 text-primary-200" : "text-slate-400 hover:bg-slate-800 hover:text-slate-100"
                )}
              >
                <Icon className="h-[18px] w-[18px]" />
                {item.label}
              </Link>
            );
          })}
        </nav>

        <div className="space-y-2 border-t border-slate-800 px-5 py-4">
          <div className="flex items-center gap-2.5">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-slate-800">
              <ShieldAlert className="h-4 w-4 text-gold-400" />
            </div>
            <div className="min-w-0">
              <p className="truncate text-sm font-medium text-white">{admin?.name ?? "Super Admin"}</p>
              <p className="truncate text-xs text-slate-400">{admin?.userId}</p>
            </div>
          </div>
          <button
            onClick={onLogout}
            className="flex w-full cursor-pointer items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-slate-400 transition-colors hover:bg-slate-800 hover:text-red-400"
          >
            <LogOut className="h-4 w-4" /> Sign out
          </button>
        </div>
      </aside>

      <div className="flex min-h-screen flex-col lg:pl-64">
        <header className="sticky top-0 z-30 flex h-16 items-center gap-3 border-b border-slate-800 bg-slate-950/90 px-4 backdrop-blur sm:px-6">
          <button
            onClick={() => setSidebarOpen(true)}
            className="cursor-pointer rounded-lg p-2 text-slate-400 hover:bg-slate-800 lg:hidden"
            aria-label="Open menu"
          >
            <Scale className="h-5 w-5" />
          </button>
          <p className="text-sm text-slate-400">Console</p>
          <div className="ml-auto flex items-center gap-2 rounded-full border border-gold-500/30 bg-gold-500/10 px-3 py-1 text-xs font-medium text-gold-400">
            <Activity className="h-3.5 w-3.5" /> Platform-wide access
          </div>
        </header>
        <main className="flex-1 px-4 py-6 sm:px-6 lg:px-8">{children}</main>
      </div>
    </div>
  );
}
