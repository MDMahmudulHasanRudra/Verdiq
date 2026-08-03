"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { Bell, Menu, Search } from "lucide-react";
import { Avatar } from "@/components/ui/avatar";
import { Dropdown, DropdownItem } from "@/components/ui/dropdown";
import { LanguageSwitcher } from "@/components/layout/language-switcher";
import { useAuthStore } from "@/lib/store/auth-store";
import { performLogout } from "@/lib/auth-actions";
import { apiGet } from "@/lib/api";
import { useQuery } from "@tanstack/react-query";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";

export function Header({ onOpenSidebar }: { onOpenSidebar: () => void }) {
  const router = useRouter();
  const user = useAuthStore((s) => s.user);
  const { success } = useToast();
  const { t } = useLanguage();
  const [query, setQuery] = useState("");

  const { data: unread = 0 } = useQuery({
    queryKey: ["notifications", "unread"],
    queryFn: () => apiGet<number>("/notifications/unread-count")
  });

  const onLogout = () => {
    performLogout();
    success("Signed out");
    router.replace("/login");
  };

  const onSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (query.trim()) router.push(`/lawyer/search?q=${encodeURIComponent(query.trim())}`);
  };

  return (
    <header className="sticky top-0 z-30 flex h-16 items-center gap-3 border-b border-line bg-card/90 px-4 backdrop-blur sm:px-6">
      <button
        onClick={onOpenSidebar}
        className="cursor-pointer rounded-lg p-2 text-ink-muted hover:bg-slate-100 lg:hidden"
        aria-label={t("header.openMenu")}
      >
        <Menu className="h-5 w-5" />
      </button>

      <form onSubmit={onSearch} className="relative hidden flex-1 max-w-md sm:block">
        <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-ink-soft" />
        <input
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          placeholder={t("nav.searchPlaceholder")}
          className="h-10 w-full rounded-lg border border-line bg-surface pl-9 pr-3 text-sm text-ink placeholder:text-ink-soft focus:border-primary-600 focus:outline-none"
        />
      </form>

      <div className="ml-auto flex items-center gap-1.5">
        <LanguageSwitcher />
        <Link
          href="/lawyer/notifications"
          className="relative cursor-pointer rounded-lg p-2 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
          aria-label={t("header.notifications")}
        >
          <Bell className="h-5 w-5" />
          {unread > 0 ? (
            <span className="absolute right-1 top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
              {unread > 9 ? "9+" : unread}
            </span>
          ) : null}
        </Link>

        <Dropdown
          align="right"
          trigger={
            <button className="flex cursor-pointer items-center gap-2 rounded-lg p-1.5 transition-colors hover:bg-slate-100">
              <Avatar name={user?.fullName} src={user?.avatarUrl} size="sm" />
              <span className="hidden text-left sm:block">
                <span className="block max-w-[140px] truncate text-sm font-medium text-ink">
                  {user?.fullName}
                </span>
                <span className="block text-xs text-ink-muted">{user?.role}</span>
              </span>
            </button>
          }
        >
          {(close) => (
            <>
              <div className="border-b border-line-soft px-3 py-2">
                <p className="text-sm font-medium text-ink">{user?.fullName}</p>
                <p className="truncate text-xs text-ink-muted">{user?.email}</p>
                <p className="mt-0.5 text-xs text-ink-muted">{user?.chamberName}</p>
              </div>
              <DropdownItem
                onClick={() => {
                  close();
                  router.push("/lawyer/settings");
                }}
              >
                {t("nav.profileSettings")}
              </DropdownItem>
              {user?.role === "Owner" ? (
                <DropdownItem
                  onClick={() => {
                    close();
                    router.push("/admin");
                  }}
                >
                  {t("nav.adminPanel")}
                </DropdownItem>
              ) : null}
              <DropdownItem
                onClick={() => {
                  close();
                  router.push("/lawyer/configuration");
                }}
              >
                {t("nav.chamberConfig")}
              </DropdownItem>
              <DropdownItem
                danger
                onClick={() => {
                  close();
                  onLogout();
                }}
              >
                {t("nav.signOut")}
              </DropdownItem>
            </>
          )}
        </Dropdown>
      </div>
    </header>
  );
}
