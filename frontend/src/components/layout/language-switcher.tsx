"use client";

import { Languages } from "lucide-react";
import { useLanguage } from "@/lib/i18n";

export function LanguageSwitcher({ compact = false }: { compact?: boolean }) {
  const { lang, toggleLang } = useLanguage();

  return (
    <button
      type="button"
      onClick={toggleLang}
      title={lang === "en" ? "বাংলা" : "English"}
      className="flex cursor-pointer items-center gap-1.5 rounded-lg px-2 py-1.5 text-sm font-medium text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
    >
      <Languages className="h-4 w-4" />
      {!compact && <span className="uppercase">{lang}</span>}
    </button>
  );
}
