"use client";

import { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import {
  Scale, Lock, ArrowRight, Eye, EyeOff,
  CheckCircle, Calendar, FileText, Users, BarChart3, Globe, ChevronDown,
} from "lucide-react";
import { performLogin, applyAuthSession, redirectAfterLogin } from "@/lib/auth-actions";
import { useToast } from "@/components/ui/toast";
import { getErrorMessage } from "@/lib/utils";
import { Field, Input } from "@/components/ui/field";
import { Button } from "@/components/ui/button";
import { useLanguage, interpolate } from "@/lib/i18n";
import { Suspense } from "react";

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const next = searchParams.get("next") || undefined;
  const { error: toastError } = useToast();
  const { t, lang, setLang } = useLanguage();
  const [email, setEmail] = useState("admin@verdiq.com");
  const [password, setPassword] = useState("admin123");
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [focusedField, setFocusedField] = useState<string | null>(null);
  const [langOpen, setLangOpen] = useState(false);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => setMounted(true), 100);
    return () => clearTimeout(timer);
  }, []);

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

  const languages = [
    { code: "en" as const, label: "English", flag: "EN" },
    { code: "bn" as const, label: "বাংলা", flag: "BN" },
  ];
  const currentLang = languages.find((l) => l.code === lang) || languages[0];

  return (
    <div className="flex min-h-screen">
      {/* Left Panel — Branding + Product Preview */}
      <div className="relative hidden w-[55%] overflow-hidden lg:flex lg:flex-col bg-gradient-to-br from-[#0F172A] via-[#162052] to-[#1E3A8A]">
        {/* Subtle grid pattern */}
        <div
          className="absolute inset-0 opacity-[0.04]"
          style={{
            backgroundImage: `linear-gradient(rgba(255,255,255,0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.1) 1px, transparent 1px)`,
            backgroundSize: "60px 60px",
          }}
        />

        {/* Glow orbs */}
        <div className="absolute -left-40 -top-40 h-[500px] w-[500px] rounded-full bg-gold-500/[0.07] blur-[100px]" />
        <div className="absolute -bottom-40 -right-40 h-[500px] w-[500px] rounded-full bg-primary-400/[0.07] blur-[100px]" />

        {/* Content */}
        <div className="relative z-10 flex flex-1 flex-col justify-between px-14 py-12 xl:px-20">
          {/* Top: Logo + Brand */}
          <div
            className={`transition-all duration-700 ${mounted ? "opacity-100 translate-y-0" : "opacity-0 translate-y-4"}`}
          >
            <div className="flex items-center gap-4">
              <div className="flex h-[52px] w-[52px] items-center justify-center rounded-2xl bg-gradient-to-br from-gold-400 to-gold-600 shadow-lg shadow-gold-500/20">
                <Scale className="h-[26px] w-[26px] text-white" />
              </div>
              <div>
                <h1 className="font-display text-[28px] font-bold tracking-tight text-white">
                  Verdiq
                </h1>
              </div>
            </div>
          </div>

          {/* Center: Dashboard Preview */}
          <div
            className={`flex flex-1 items-center justify-center transition-all duration-700 delay-200 ${mounted ? "opacity-100 translate-y-0" : "opacity-0 translate-y-6"}`}
          >
            <div className="relative w-full max-w-[520px]">
              {/* Floating Glass Cards */}
              <div className="absolute -left-6 top-8 z-20 animate-[float_6s_ease-in-out_infinite]">
                <div className="flex items-center gap-2.5 rounded-2xl border border-white/10 bg-white/[0.08] px-4 py-3 shadow-xl backdrop-blur-md">
                  <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-emerald-500/20">
                    <CheckCircle className="h-4 w-4 text-emerald-400" />
                  </div>
                  <div>
                    <p className="text-[11px] font-medium text-white/60">{t("loginPage.hearingsToday")}</p>
                    <p className="text-sm font-bold text-white">12</p>
                  </div>
                </div>
              </div>

              <div className="absolute -right-6 top-20 z-20 animate-[float_6s_ease-in-out_infinite_1s]">
                <div className="flex items-center gap-2.5 rounded-2xl border border-white/10 bg-white/[0.08] px-4 py-3 shadow-xl backdrop-blur-md">
                  <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary-500/20">
                    <Users className="h-4 w-4 text-primary-300" />
                  </div>
                  <div>
                    <p className="text-[11px] font-medium text-white/60">{t("loginPage.activeCases")}</p>
                    <p className="text-sm font-bold text-white">245</p>
                  </div>
                </div>
              </div>

              <div className="absolute -left-4 bottom-16 z-20 animate-[float_6s_ease-in-out_infinite_2s]">
                <div className="flex items-center gap-2.5 rounded-2xl border border-white/10 bg-white/[0.08] px-4 py-3 shadow-xl backdrop-blur-md">
                  <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-gold-500/20">
                    <FileText className="h-4 w-4 text-gold-400" />
                  </div>
                  <div>
                    <p className="text-[11px] font-medium text-white/60">{t("loginPage.aiAssistant")}</p>
                    <p className="text-sm font-bold text-white">{t("loginPage.ready")}</p>
                  </div>
                </div>
              </div>

              <div className="absolute -right-4 bottom-8 z-20 animate-[float_6s_ease-in-out_infinite_3s]">
                <div className="flex items-center gap-2.5 rounded-2xl border border-white/10 bg-white/[0.08] px-4 py-3 shadow-xl backdrop-blur-md">
                  <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-emerald-500/20">
                    <BarChart3 className="h-4 w-4 text-emerald-400" />
                  </div>
                  <div>
                    <p className="text-[11px] font-medium text-white/60">{t("loginPage.uptime")}</p>
                    <p className="text-sm font-bold text-white">99.9%</p>
                  </div>
                </div>
              </div>

              {/* Dashboard Mockup */}
              <div className="relative overflow-hidden rounded-2xl border border-white/[0.08] bg-white/[0.05] shadow-2xl shadow-black/30 backdrop-blur-sm">
                {/* Browser bar */}
                <div className="flex items-center gap-3 border-b border-white/[0.06] bg-white/[0.03] px-5 py-3.5">
                  <div className="flex gap-2">
                    <div className="h-3 w-3 rounded-full bg-white/20" />
                    <div className="h-3 w-3 rounded-full bg-white/20" />
                    <div className="h-3 w-3 rounded-full bg-white/20" />
                  </div>
                  <div className="ml-2 flex-1 rounded-lg bg-white/[0.06] px-4 py-1.5 text-center text-[11px] font-medium text-white/40">
                    app.verdiq.com/dashboard
                  </div>
                </div>

                {/* Dashboard Content */}
                <div className="p-5">
                  {/* Stats Row */}
                  <div className="mb-4 grid grid-cols-3 gap-3">
                    {[
                      { label: t("loginPage.activeCases"), value: "24", delta: `+3 ${t("loginPage.thisWeek")}`, color: "bg-primary-500/20 text-primary-300" },
                      { label: t("loginPage.clients"), value: "156", delta: `+12 ${t("loginPage.thisMonth")}`, color: "bg-emerald-500/20 text-emerald-300" },
                      { label: t("loginPage.revenue"), value: "৳8.4L", delta: `+18% ${t("loginPage.growth")}`, color: "bg-gold-500/20 text-gold-300" },
                    ].map((stat, i) => (
                      <div key={i} className="rounded-xl bg-white/[0.06] p-3.5">
                        <p className="text-[10px] font-medium text-white/40 uppercase tracking-wider">{stat.label}</p>
                        <p className="mt-1 text-xl font-bold text-white">{stat.value}</p>
                        <p className="mt-0.5 text-[10px] font-medium text-emerald-400">{stat.delta}</p>
                      </div>
                    ))}
                  </div>

                  {/* Chart + Upcoming */}
                  <div className="grid grid-cols-5 gap-3">
                    {/* Mini Chart */}
                    <div className="col-span-3 rounded-xl bg-white/[0.06] p-4">
                      <div className="mb-3 flex items-center justify-between">
                        <p className="text-[11px] font-medium text-white/50">{t("loginPage.caseProgress")}</p>
                        <p className="text-[10px] text-white/30">{t("loginPage.last12Months")}</p>
                      </div>
                      <div className="flex items-end gap-[5px] h-20">
                        {[35, 55, 40, 70, 50, 80, 65, 75, 55, 85, 70, 90].map((h, i) => (
                          <div
                            key={i}
                            className="flex-1 rounded-t-sm transition-all duration-300"
                            style={{
                              height: `${h}%`,
                              background: i === 11
                                ? "linear-gradient(to top, #F59E0B, #FBBF24)"
                                : "linear-gradient(to top, rgba(255,255,255,0.12), rgba(255,255,255,0.20))",
                            }}
                          />
                        ))}
                      </div>
                    </div>

                    {/* Upcoming */}
                    <div className="col-span-2 rounded-xl bg-white/[0.06] p-4">
                      <p className="mb-3 text-[11px] font-medium text-white/50">{t("loginPage.upcoming")}</p>
                      <div className="space-y-3">
                        {[
                          { title: "Rahman vs State", time: "10:00 AM", type: t("loginPage.hearing") },
                          { title: "Hasan Contract", time: "2:30 PM", type: t("loginPage.meeting") },
                          { title: "Begum Property", time: "4:00 PM", type: t("loginPage.filing") },
                        ].map((item, i) => (
                          <div key={i} className="flex items-start gap-2.5">
                            <div className="mt-1 h-1.5 w-1.5 flex-shrink-0 rounded-full bg-gold-400" />
                            <div className="min-w-0">
                              <p className="text-[11px] font-medium text-white/90 truncate">{item.title}</p>
                              <p className="text-[10px] text-white/35">{item.time} · {item.type}</p>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>

                  {/* Activity Bar */}
                  <div className="mt-4 flex items-center gap-3 rounded-xl bg-white/[0.04] px-4 py-3">
                    <div className="flex -space-x-2">
                      {["bg-primary-400", "bg-gold-400", "bg-emerald-400"].map((bg, i) => (
                        <div key={i} className={`h-6 w-6 rounded-full ${bg} ring-2 ring-[#0F172A]`} />
                      ))}
                    </div>
                    <p className="text-[11px] text-white/40">
                      <span className="font-medium text-white/60">{interpolate(t("loginPage.teamMembersActive"), { count: 3 })}</span>
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Bottom: Footer */}
          <div
            className={`transition-all duration-700 delay-500 ${mounted ? "opacity-100 translate-y-0" : "opacity-0 translate-y-4"}`}
          >
            <div className="flex items-center gap-6 text-[12px] text-white/30">
              <Link href="#" className="hover:text-white/60 transition-colors duration-150">{t("loginPage.privacy")}</Link>
              <Link href="#" className="hover:text-white/60 transition-colors duration-150">{t("loginPage.terms")}</Link>
              <Link href="#" className="hover:text-white/60 transition-colors duration-150">{t("loginPage.help")}</Link>
              <Link href="#" className="hover:text-white/60 transition-colors duration-150">{t("loginPage.status")}</Link>
              <Link href="#" className="hover:text-white/60 transition-colors duration-150">{t("loginPage.contact")}</Link>
            </div>
          </div>
        </div>
      </div>

      {/* Right Panel — Login Form */}
      <div className="flex flex-1 flex-col bg-[#F8FAFC]">
        {/* Mobile Header */}
        <div className="flex items-center justify-between px-6 py-5 lg:hidden">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br from-gold-400 to-gold-600 shadow-md shadow-gold-500/20">
              <Scale className="h-5 w-5 text-white" />
            </div>
            <span className="font-display text-lg font-bold text-ink">Verdiq</span>
          </div>
          {/* Mobile Lang */}
          <div className="relative">
            <button
              type="button"
              onClick={() => setLangOpen(!langOpen)}
              className="flex items-center gap-2 rounded-xl border border-line bg-white px-3.5 py-2.5 text-sm font-medium text-ink transition-all duration-150 hover:bg-white/80 hover:shadow-sm"
            >
              <Globe className="h-4 w-4 text-ink-muted" />
              <span>{currentLang.flag}</span>
              <ChevronDown className="h-3.5 w-3.5 text-ink-muted" />
            </button>
            {langOpen && (
              <div className="absolute right-0 top-full z-50 mt-2 w-44 overflow-hidden rounded-xl border border-line bg-white shadow-pop">
                {languages.map((l) => (
                  <button
                    key={l.code}
                    type="button"
                    onClick={() => { setLang(l.code); setLangOpen(false); }}
                    className={`flex w-full items-center gap-3 px-4 py-3 text-sm transition-colors duration-150 ${
                      lang === l.code
                        ? "bg-primary-50 font-medium text-primary-700"
                        : "text-ink hover:bg-slate-50"
                    }`}
                  >
                    <span className="font-semibold">{l.flag}</span>
                    <span>{l.label}</span>
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Form Container */}
        <div className="flex flex-1 items-center justify-center px-6 py-12">
          <div className="w-full max-w-[420px]">
            {/* Desktop Header */}
            <div className="mb-10 hidden lg:flex lg:justify-end">
              <div className="relative">
                <button
                  type="button"
                  onClick={() => setLangOpen(!langOpen)}
                  className="flex items-center gap-2 rounded-xl border border-line bg-white px-3.5 py-2.5 text-sm font-medium text-ink transition-all duration-150 hover:bg-white/80 hover:shadow-sm"
                >
                  <Globe className="h-4 w-4 text-ink-muted" />
                  <span>{currentLang.flag}</span>
                  <ChevronDown className="h-3.5 w-3.5 text-ink-muted" />
                </button>
                {langOpen && (
                  <div className="absolute right-0 top-full z-50 mt-2 w-44 overflow-hidden rounded-xl border border-line bg-white shadow-pop">
                    {languages.map((l) => (
                      <button
                        key={l.code}
                        type="button"
                        onClick={() => { setLang(l.code); setLangOpen(false); }}
                        className={`flex w-full items-center gap-3 px-4 py-3 text-sm transition-colors duration-150 ${
                          lang === l.code
                            ? "bg-primary-50 font-medium text-primary-700"
                            : "text-ink hover:bg-slate-50"
                        }`}
                      >
                        <span className="font-semibold">{l.flag}</span>
                        <span>{l.label}</span>
                      </button>
                    ))}
                  </div>
                )}
              </div>
            </div>

            {/* Welcome Text */}
            <div
              className={`mb-10 transition-all duration-500 ${mounted ? "opacity-100 translate-y-0" : "opacity-0 translate-y-3"}`}
            >
              <h2 className="font-display text-[32px] font-bold leading-tight text-ink">
                {t("loginPage.welcomeBack")}
              </h2>
              <p className="mt-3 text-[15px] leading-relaxed text-ink-muted">
                {t("loginPage.subtitle")}
              </p>
            </div>

            {/* Login Card */}
            <div
              className={`rounded-[20px] border border-line/80 bg-white p-8 shadow-[0_1px_3px_rgba(0,0,0,0.04),0_8px_24px_rgba(0,0,0,0.06)] transition-all duration-500 delay-100 ${mounted ? "opacity-100 translate-y-0" : "opacity-0 translate-y-3"}`}
            >
              <form onSubmit={onSubmit} className="space-y-5">
                {/* Email */}
                <Field label={t("login.email")} required>
                  <Input
                    type="email"
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    onFocus={() => setFocusedField("email")}
                    onBlur={() => setFocusedField(null)}
                    placeholder="you@chamber.com"
                    autoComplete="email"
                    className={`h-12 rounded-xl border-line bg-[#F8FAFC] px-4 text-[14px] transition-all duration-150 placeholder:text-ink-soft/60 focus:border-primary-500 focus:bg-white focus:ring-4 focus:ring-primary-500/10 ${
                      focusedField === "email" ? "border-primary-500 bg-white ring-4 ring-primary-500/10" : ""
                    }`}
                  />
                </Field>

                {/* Password */}
                <Field label={t("login.password")} required>
                  <div className="relative">
                    <Input
                      type={showPassword ? "text" : "password"}
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      onFocus={() => setFocusedField("password")}
                      onBlur={() => setFocusedField(null)}
                      placeholder="••••••••"
                      autoComplete="current-password"
                      className={`h-12 rounded-xl border-line bg-[#F8FAFC] px-4 pr-11 text-[14px] transition-all duration-150 placeholder:text-ink-soft/60 focus:border-primary-500 focus:bg-white focus:ring-4 focus:ring-primary-500/10 ${
                        focusedField === "password" ? "border-primary-500 bg-white ring-4 ring-primary-500/10" : ""
                      }`}
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute right-3.5 top-1/2 -translate-y-1/2 rounded-lg p-1 text-ink-soft/50 transition-colors duration-150 hover:text-ink-muted"
                      tabIndex={-1}
                    >
                      {showPassword ? <EyeOff className="h-[18px] w-[18px]" /> : <Eye className="h-[18px] w-[18px]" />}
                    </button>
                  </div>
                </Field>

                {/* Remember + Forgot */}
                <div className="flex items-center justify-between pt-0.5">
                  <label className="flex cursor-pointer items-center gap-2.5">
                    <input
                      type="checkbox"
                      className="h-4 w-4 rounded border-line text-primary-600 focus:ring-primary-500/20"
                    />
                    <span className="text-[13px] text-ink-muted">{t("loginPage.rememberMe")}</span>
                  </label>
                  <Link
                    href="#"
                    className="text-[13px] font-medium text-primary-700 transition-colors duration-150 hover:text-primary-800"
                  >
                    {t("loginPage.forgotPassword")}
                  </Link>
                </div>

                {/* Submit */}
                <div className="pt-2">
                  <Button
                    type="submit"
                    className="group relative h-[52px] w-full rounded-xl bg-gradient-to-r from-primary-700 via-primary-600 to-primary-700 text-[15px] font-semibold text-white shadow-lg shadow-primary-700/25 transition-all duration-200 hover:from-primary-800 hover:via-primary-700 hover:to-primary-800 hover:shadow-xl hover:shadow-primary-700/30 hover:-translate-y-[1px] active:translate-y-0 active:shadow-md"
                    disabled={loading}
                  >
                    <span className="relative z-10 flex items-center justify-center gap-2.5">
                      {loading ? (
                        <>
                          <svg className="h-[18px] w-[18px] animate-spin" viewBox="0 0 24 24" fill="none">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                          </svg>
                          {t("loginPage.loggingIn")}
                        </>
                      ) : (
                        <>
                          {t("login.signIn")}
                          <ArrowRight className="h-4 w-4 transition-transform duration-200 group-hover:translate-x-1" />
                        </>
                      )}
                    </span>
                  </Button>
                </div>
              </form>

            </div>

            {/* Super Admin */}
            <div className="mt-3 text-center">
              <Link
                href="/super-admin/login"
                className="inline-flex items-center gap-1.5 text-[12px] text-ink-soft/60 transition-colors duration-150 hover:text-ink-muted"
              >
                <Lock className="h-3 w-3" />
                {t("loginPage.superAdmin")}
              </Link>
            </div>

            {/* Trust Indicators */}
            <div
              className={`mt-8 flex items-center justify-center gap-6 transition-all duration-500 delay-300 ${mounted ? "opacity-100" : "opacity-0"}`}
            >
              {[
                { icon: CheckCircle, label: t("loginPage.encrypted"), color: "text-emerald-500" },
                { icon: CheckCircle, label: t("loginPage.dailyBackups"), color: "text-emerald-500" },
                { icon: CheckCircle, label: t("loginPage.soc2Ready"), color: "text-emerald-500" },
              ].map((item, i) => (
                <span key={i} className="flex items-center gap-1.5 text-[12px] text-ink-soft/70">
                  <item.icon className={`h-3.5 w-3.5 ${item.color}`} />
                  {item.label}
                </span>
              ))}
            </div>

            {/* Version */}
            <div className="mt-4 text-center">
              <span className="text-[11px] text-ink-soft/40">v1.0.0-beta</span>
            </div>
          </div>
        </div>

        {/* Bottom Bar — Try Demo */}
        <div className="border-t border-line/50 bg-white/50 px-6 py-4">
          <div className="flex items-center justify-center gap-3">
            <button
              type="button"
              onClick={() => { setEmail("admin@verdiq.com"); setPassword("admin123"); }}
              className="inline-flex items-center gap-2 rounded-xl bg-primary-50 px-5 py-2.5 text-[13px] font-medium text-primary-700 transition-all duration-150 hover:bg-primary-100 hover:shadow-sm"
            >
              <Scale className="h-4 w-4" />
              {t("loginPage.tryDemo")}
            </button>
            <Link
              href="/register"
              className="inline-flex items-center gap-2 rounded-xl border border-line bg-white px-5 py-2.5 text-[13px] font-medium text-ink transition-all duration-150 hover:bg-slate-50 hover:shadow-sm"
            >
              {t("loginPage.requestDemo")}
            </Link>
          </div>
        </div>
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
