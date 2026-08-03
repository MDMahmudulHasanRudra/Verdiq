"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Scale, X } from "lucide-react";
import { navGroups } from "@/components/layout/nav-config";
import { useAuthStore } from "@/lib/store/auth-store";
import { useLanguage } from "@/lib/i18n";
import { cn } from "@/lib/utils";

export function Sidebar({ open, onClose }: { open: boolean; onClose: () => void }) {
  const pathname = usePathname();
  const user = useAuthStore((s) => s.user);
  const { t } = useLanguage();
  const modules = user?.modules?.length ? user.modules : null;

  const filteredGroups = navGroups
    .map((g) => ({
      ...g,
      items: modules ? g.items.filter((i) => !i.module || modules.includes(i.module)) : g.items
    }))
    .filter((g) => g.items.length > 0);

  return (
    <>
      {open ? (
        <div className="fixed inset-0 z-40 bg-slate-900/40 lg:hidden" onClick={onClose} />
      ) : null}
      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-50 flex w-64 flex-col border-r border-line bg-card transition-transform duration-200 lg:translate-x-0",
          open ? "translate-x-0" : "-translate-x-full"
        )}
      >
        <div className="flex h-16 items-center justify-between border-b border-line px-5">
          <Link href="/lawyer" className="flex items-center gap-2.5" onClick={onClose}>
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary-800">
              <Scale className="h-5 w-5 text-gold-400" />
            </div>
            <div>
              <p className="font-display text-lg font-bold leading-none text-ink">Verdiq</p>
              <p className="mt-0.5 text-[10px] font-medium uppercase tracking-widest text-ink-muted">
                Law Chamber
              </p>
            </div>
          </Link>
          <button onClick={onClose} className="cursor-pointer text-ink-muted lg:hidden" aria-label="Close menu">
            <X className="h-5 w-5" />
          </button>
        </div>

        <nav className="flex-1 space-y-5 overflow-y-auto px-3 py-4">
          {filteredGroups.map((group) => (
            <div key={group.key}>
              <p className="px-3 pb-1.5 text-[10px] font-semibold uppercase tracking-widest text-ink-soft">
                {t(group.i18nKey)}
              </p>
              <div className="space-y-0.5">
                {group.items.map((item) => {
                  const active =
                    item.href === "/lawyer"
                      ? pathname === "/lawyer"
                      : pathname.startsWith(item.href);
                  const Icon = item.icon;
                  return (
                    <Link
                      key={item.href}
                      href={item.href}
                      onClick={onClose}
                      className={cn(
                        "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors duration-100",
                        active
                          ? "bg-primary-50 text-primary-800"
                          : "text-ink-muted hover:bg-slate-50 hover:text-ink"
                      )}
                    >
                      <Icon className={cn("h-4.5 w-4.5 h-[18px] w-[18px]", active ? "text-primary-700" : "text-ink-soft")} />
                      {t(item.i18nKey)}
                    </Link>
                  );
                })}
              </div>
            </div>
          ))}
        </nav>

        <div className="border-t border-line px-5 py-4">
          <p className="text-xs text-ink-muted">© {new Date().getFullYear()} Verdiq</p>
        </div>
      </aside>
    </>
  );
}
