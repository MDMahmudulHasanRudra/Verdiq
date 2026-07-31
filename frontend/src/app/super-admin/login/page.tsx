"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Scale } from "lucide-react";
import { superAdminService } from "@/lib/services/super-admin-service";
import { tokenStore, cookieStore } from "@/lib/api";
import { useToast } from "@/components/ui/toast";
import { getErrorMessage } from "@/lib/utils";
import { Field, Input } from "@/components/ui/field";
import { Button } from "@/components/ui/button";

export default function SuperAdminLoginPage() {
  const router = useRouter();
  const toast = useToast();
  const [userId, setUserId] = useState("superadmin");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const data = await superAdminService.login(userId, password);
      if (data.success && data.accessToken && data.refreshToken) {
        tokenStore.saSet(data.accessToken);
        tokenStore.saSetRefresh(data.refreshToken);
        tokenStore.saSetUser(data.admin);
        cookieStore.saAccess();
        toast.success("Welcome back");
        router.replace("/super-admin/dashboard");
      } else {
        toast.error(data.message || "Login failed");
      }
    } catch (err) {
      toast.error("Login failed", getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-950 px-4">
      <div className="w-full max-w-md">
        <div className="mb-8 flex flex-col items-center text-center">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-primary-800 shadow-pop">
            <Scale className="h-8 w-8 text-gold-400" />
          </div>
          <h1 className="mt-4 font-display text-3xl font-bold text-white">Verdiq</h1>
          <p className="mt-1 text-sm text-slate-400">Super Admin Console</p>
        </div>

        <form onSubmit={onSubmit} className="space-y-4 rounded-xl border border-slate-800 bg-slate-900 p-6">
          <Field label="Admin User ID" required>
            <Input
              value={userId}
              onChange={(e) => setUserId(e.target.value)}
              placeholder="superadmin"
              autoComplete="username"
              className="border-slate-700 bg-slate-800 text-white placeholder:text-slate-500"
            />
          </Field>
          <Field label="Password" required>
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              autoComplete="current-password"
              className="border-slate-700 bg-slate-800 text-white placeholder:text-slate-500"
            />
          </Field>
          <Button type="submit" size="lg" className="w-full" loading={loading}>
            Sign in to console
          </Button>
        </form>

        <p className="mt-6 text-center text-xs text-slate-500">
          <a href="/login" className="text-slate-400 hover:text-white">← Back to chamber login</a>
        </p>
      </div>
    </div>
  );
}
