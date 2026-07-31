"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Field, Input, Select } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { subscriptionService } from "@/lib/services";
import { getErrorMessage, formatDate } from "@/lib/utils";
import { useAuthStore } from "@/lib/store/auth-store";
import { useToast } from "@/components/ui/toast";
import { Crown, User, LogOut, Building2 } from "lucide-react";
import { performLogout } from "@/lib/auth-actions";
import { useRouter } from "next/navigation";
import { StatusBadge } from "@/components/ui/badge";

export default function SettingsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const router = useRouter();
  const user = useAuthStore((s) => s.user);
  const [plan, setPlan] = useState("");

  const { data: sub, isLoading } = useQuery({
    queryKey: ["subscription", "my"],
    queryFn: () => subscriptionService.my()
  });

  const changePlan = useMutation({
    mutationFn: (p: string) => subscriptionService.changePlan(p),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["subscription"] });
      toast.success("Plan updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader title="Settings" subtitle="Your account and firm subscription." />

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader title="Your Account" />
          <CardContent>
            <div className="flex items-center gap-4">
              <div className="flex h-14 w-14 items-center justify-center rounded-full bg-primary-700 text-lg font-bold text-white">
                {(user?.fullName ?? "VQ").split(" ").map((p) => p[0]).slice(0, 2).join("")}
              </div>
              <div>
                <p className="font-display text-lg font-semibold text-ink">{user?.fullName}</p>
                <p className="text-sm text-ink-muted">{user?.email}</p>
                <p className="text-xs text-ink-muted">{user?.role} · Chamber {user?.chamberId?.slice(0, 8)}</p>
              </div>
            </div>
            <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2">
              <Field label="Phone">
                <Input defaultValue={user?.phone ?? ""} />
              </Field>
              <Field label="Bar Council ID">
                <Input defaultValue={user?.barCouncilId ?? ""} />
              </Field>
            </div>
          </CardContent>
        </Card>

        <div className="space-y-6">
          <Card>
            <CardHeader title="Subscription" action={<Crown className="h-4 w-4 text-gold-600" />} />
            <CardContent>
              {isLoading ? (
                <Loading />
              ) : sub ? (
                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-ink-muted">Plan</span>
                    <StatusBadge value={sub.plan} />
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-ink-muted">Status</span>
                    <StatusBadge value={sub.status} />
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-sm text-ink-muted">Renews</span>
                    <span className="text-sm font-medium text-ink">{formatDate(sub.currentPeriodEnd)}</span>
                  </div>
                  <div className="pt-2">
                    <Select value={plan} onChange={(e) => setPlan(e.target.value)}>
                      <option value="">Change plan…</option>
                      <option value="Free">Free</option>
                      <option value="Pro">Pro</option>
                      <option value="Chamber">Chamber</option>
                    </Select>
                    {plan ? (
                      <Button
                        size="sm"
                        variant="gold"
                        className="mt-2 w-full"
                        onClick={() => changePlan.mutate(plan)}
                      >
                        Switch to {plan}
                      </Button>
                    ) : null}
                  </div>
                </div>
              ) : (
                <p className="text-sm text-ink-muted">No subscription found.</p>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader title="Session" />
            <CardContent className="space-y-2">
              <Button variant="outline" className="w-full" onClick={() => router.push("/lawyer/profile")}>
                <User className="h-4 w-4" /> View Profile
              </Button>
              <Button
                variant="outline"
                className="w-full"
                onClick={async () => {
                  await performLogout();
                  router.replace("/login");
                }}
              >
                <LogOut className="h-4 w-4" /> Sign Out
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
