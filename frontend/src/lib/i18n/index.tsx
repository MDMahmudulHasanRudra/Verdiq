"use client";

import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import type { Dictionary, Language, TranslationParams } from "./types";
import { en } from "./en";
import { bn } from "./bn";

const STORAGE_KEY = "verdiq-language";

type ResolveFn = (key: string) => string;

interface LanguageContextValue {
  lang: Language;
  setLang: (lang: Language) => void;
  toggleLang: () => void;
  t: ResolveFn;
  dict: Dictionary;
}

const LanguageContext = createContext<LanguageContextValue | null>(null);

function resolvePath(dict: Dictionary, key: string): string | undefined {
  const parts = key.split(".");
  let node: unknown = dict;
  for (const part of parts) {
    if (node === null || typeof node !== "object") return undefined;
    node = (node as Record<string, unknown>)[part];
  }
  return typeof node === "string" ? node : undefined;
}

export function LanguageProvider({ children }: { children: ReactNode }) {
  const [lang, setLangState] = useState<Language>("en");

  useEffect(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved === "en" || saved === "bn") setLangState(saved);
    } catch {
      // ignore storage errors
    }
  }, []);

  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY, lang);
    } catch {
      // ignore storage errors
    }
    document.documentElement.lang = lang;
  }, [lang]);

  const setLang = useCallback((next: Language) => setLangState(next), []);

  const toggleLang = useCallback(() => {
    setLangState((prev) => (prev === "en" ? "bn" : "en"));
  }, []);

  const value = useMemo<LanguageContextValue>(() => {
    const dict = lang === "bn" ? bn : en;
    const t: ResolveFn = (key) => {
      const raw = resolvePath(dict, key);
      if (raw === undefined) return resolvePath(en, key) ?? key;
      return raw;
    };
    return { lang, setLang, toggleLang, t, dict };
  }, [lang, setLang, toggleLang]);

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>;
}

export function useLanguage(): LanguageContextValue {
  const ctx = useContext(LanguageContext);
  if (!ctx) throw new Error("useLanguage must be used within a LanguageProvider");
  return ctx;
}

export function interpolate(template: string, params?: TranslationParams): string {
  if (!params) return template;
  return template.replace(/\{(\w+)\}/g, (match, name: string) =>
    Object.prototype.hasOwnProperty.call(params, name) ? String(params[name]) : match
  );
}
