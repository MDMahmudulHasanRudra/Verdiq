"use client";

import { useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import { Scale } from "lucide-react";
import { performLogin, applyAuthSession, redirectAfterLogin } from "@/lib/auth-actions";
import { useToast } from "@/components/ui/toast";
import { getErrorMessage } from "@/lib/utils";
import { Field, Input } from "@/components/ui/field";
import { Button } from "@/components/ui/button";
import { LanguageSwitcher } from "@/components/layout/language-switcher";
import { useLanguage } from "@/lib/i18n";
import { Suspense } from "react";

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const next = searchParams.get("next") || undefined;
  const { error: toastError } = useToast();
  const { t } = useLanguage();
  const [email, setEmail] = useState("admin@verdiq.com");
  const [password, setPassword] = useState("admin123");
  const [loading, setLoading] = useState(false);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const data = await performLogin(email, password);
      if (data.accessToken && data.user) {
        applyAuthSession(data);
        redirectAfterLogin(data.user.role, router, next);
      } else if (data.message === "2FA required") {
        toastError("Two-factor authentication is enabled");
      } else {
        toastError(data.message || "Login failed");
      }
    } catch (err) {
      toastError("Login failed", getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-surface px-4">
      <div className="w-full max-w-md">
        <div className="mb-8 flex flex-col items-center text-center">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-primary-800 shadow-pop">
            <Scale className="h-8 w-8 text-gold-400" />
          </div>
          <h1 className="mt-4 font-display text-3xl font-bold text-ink">Verdiq</h1>
          <p className="mt-1 text-sm text-ink-muted">{t("login.tagline")}</p>
          <div className="mt-3">
            <LanguageSwitcher />
          </div>
        </div>

        <form onSubmit={onSubmit} className="card space-y-4 p-6">
          <Field label={t("login.email")} required>
            <Input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@chamber.com"
              autoComplete="email"
            />
          </Field>
          <Field label={t("login.password")} required>
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              autoComplete="current-password"
            />
          </Field>
          <div className="flex items-center justify-between text-sm">
            <Link href="/register" className="font-medium text-primary-700 hover:underline">
              {t("login.createChamber")}
            </Link>
            <Link href="/super-admin/login" className="text-ink-muted hover:text-ink">
              {t("login.superAdmin")}
            </Link>
          </div>
          <Button type="submit" size="lg" className="w-full" loading={loading}>
            {t("login.signIn")}
          </Button>
        </form>

        <p className="mt-6 text-center text-xs text-ink-soft">
          Demo: admin@verdiq.com / admin123 · lawyer@verdiq.com / lawyer123
        </p>
      </div>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense>
      <LoginForm />
    </Suspense>
  );
}
