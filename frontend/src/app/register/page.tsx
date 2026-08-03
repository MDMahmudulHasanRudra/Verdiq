"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { Scale } from "lucide-react";
import { api, apiGet } from "@/lib/api";
import { performRegister, applyAuthSession, redirectAfterLogin } from "@/lib/auth-actions";
import { useToast } from "@/components/ui/toast";
import { getErrorMessage } from "@/lib/utils";
import { Field, Input, Select } from "@/components/ui/field";
import { Button } from "@/components/ui/button";
import type { Chamber } from "@/types/api";
import { useLanguage } from "@/lib/i18n";

const ROLES = ["SeniorLawyer", "JuniorLawyer", "Assistant", "Accountant"];

export default function RegisterPage() {
  const router = useRouter();
  const { toast } = useToast();
  const { t } = useLanguage();
  const [chambers, setChambers] = useState<Chamber[]>([]);
  const [chamberId, setChamberId] = useState("");
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState("SeniorLawyer");
  const [barCouncilId, setBarCouncilId] = useState("");
  const [loading, setLoading] = useState(false);
  const [loadingChambers, setLoadingChambers] = useState(true);

  useEffect(() => {
    api
      .get<{ data: Chamber[] }>("/chambers")
      .then((res) => {
        setChambers(res.data.data || []);
      })
      .catch(() => toast("error", t("register.couldNotLoadChambers")))
      .finally(() => setLoadingChambers(false));
  }, [toast]);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!chamberId) {
      toast("error", "Please select your chamber");
      return;
    }
    setLoading(true);
    try {
      const data = await performRegister({
        fullName,
        email,
        password,
        confirmPassword: password,
        phone,
        role,
        chamberId,
        barCouncilId: barCouncilId || null
      });
      if (data.accessToken && data.user) {
        applyAuthSession(data);
        redirectAfterLogin(data.user.role, router);
      } else {
        toast("error", data.message || "Registration failed");
      }
    } catch (err) {
      toast("error", "Registration failed", getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-surface px-4 py-10">
      <div className="w-full max-w-lg">
        <div className="mb-8 flex flex-col items-center text-center">
          <div className="flex h-14 w-14 items-center justify-center rounded-2xl bg-primary-800 shadow-pop">
            <Scale className="h-7 w-7 text-gold-400" />
          </div>
          <h1 className="mt-4 font-display text-2xl font-bold text-ink">Create your account</h1>
          <p className="mt-1 text-sm text-ink-muted">
            Join an existing chamber. Already a member?{" "}
            <Link href="/login" className="font-medium text-primary-700 hover:underline">
              Sign in
            </Link>
          </p>
        </div>

        <form onSubmit={onSubmit} className="card space-y-4 p-6">
          <Field label="Chamber" required hint="Select the law chamber you belong to">
            {loadingChambers ? (
              <div className="h-10 animate-pulse rounded-lg bg-slate-200" />
            ) : (
              <Select value={chamberId} onChange={(e) => setChamberId(e.target.value)}>
                <option value="">Select a chamber…</option>
                {chambers.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                    {c.address ? ` — ${c.address}` : ""}
                  </option>
                ))}
              </Select>
            )}
          </Field>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Full name" required>
              <Input value={fullName} onChange={(e) => setFullName(e.target.value)} required />
            </Field>
            <Field label="Role" required>
              <Select value={role} onChange={(e) => setRole(e.target.value)}>
                {ROLES.map((r) => (
                  <option key={r} value={r}>
                    {r}
                  </option>
                ))}
              </Select>
            </Field>
          </div>
          <Field label="Email" required>
            <Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
          </Field>
          <Field label="Phone" required>
            <Input value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="+8801XXXXXXXXX" required />
          </Field>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Password" required hint="Minimum 6 characters">
              <Input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </Field>
            <Field label="Bar Council ID">
              <Input value={barCouncilId} onChange={(e) => setBarCouncilId(e.target.value)} placeholder="BC-2024-001" />
            </Field>
          </div>
          <Button type="submit" size="lg" className="w-full" loading={loading}>
            Create account
          </Button>
        </form>
      </div>
    </div>
  );
}
