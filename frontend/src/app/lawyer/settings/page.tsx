"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { getErrorMessage } from "@/lib/utils";
import { subscriptionService, configurationService } from "@/lib/services";
import { User, CreditCard, LogOut, ExternalLink } from "lucide-react";

const plans: Record<string, { label: string; next: string }> = {
  free: { label: "Free", next: "Starter" },
  starter: { label: "Starter", next: "Pro" },
  pro: { label: "Pro", next: "Firm" },
  firm: { label: "Firm", next: "Firm" }
};

export default function SettingsPage() {
  const { t } = useLanguage();
  const toast = useToast();
  const router = useRouter();
  const qc = useQueryClient();

  const { data: profile, isLoading: profileLoading } = useQuery({
    queryKey: ["profile"],
    queryFn: () => configurationService.getAll()
  });

  const { data: subscription } = useQuery({
    queryKey: ["subscription"],
    queryFn: () => subscriptionService.my()
  });

  const switchPlan = useMutation({
    mutationFn: (plan: string) => subscriptionService.changePlan(plan),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["subscription"] });
      toast.success(t("settings.planUpdated"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const handleSignOut = async () => {
    router.push("/login");
    router.refresh();
  };

  if (profileLoading) return <Loading />;

  const cfg = (profile?.settings?.general ?? {}) as Record<string, string>;
  const currentPlan = subscription?.plan ?? "free";
  const planInfo = plans[currentPlan] ?? plans.free;
  const isMax = currentPlan === "firm";

  return (
    <div>
      <PageHeader title={t("settings.title")} subtitle={t("settings.subtitle")} />

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader title={t("settings.yourAccount")} />
          <CardContent className="space-y-4">
            <div className="flex items-center gap-4">
              <div className="flex h-16 w-16 items-center justify-center rounded-full bg-primary-50">
                <User className="h-8 w-8 text-primary-700" />
              </div>
              <div>
                <p className="text-lg font-semibold text-ink">{cfg.firmName ?? "—"}</p>
                <p className="text-sm text-ink-muted">{cfg.email ?? "—"}</p>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <p className="font-medium text-ink">{t("settings.phone")}</p>
                <p className="text-ink-muted">{cfg.phone ?? "—"}</p>
              </div>
              <div>
                <p className="font-medium text-ink">{t("settings.barCouncilId")}</p>
                <p className="text-ink-muted">{cfg.barCouncilId ?? "—"}</p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader title={t("settings.subscription")} />
          <CardContent className="space-y-4">
            <div className="flex items-center gap-3">
              <CreditCard className="h-5 w-5 text-primary-600" />
              <div>
                <p className="text-sm font-medium text-ink">{t("settings.plan")}</p>
                <p className="text-lg font-semibold text-ink">{planInfo.label}</p>
              </div>
              <Badge>{subscription?.status ?? "active"}</Badge>
            </div>
            {subscription?.currentPeriodEnd && (
              <p className="text-sm text-ink-muted">{t("settings.renews")}: {new Date(subscription.currentPeriodEnd).toLocaleDateString()}</p>
            )}
            {!isMax && (
              <Button variant="outline" onClick={() => switchPlan.mutate(planInfo.next)} disabled={switchPlan.isPending}>
                {switchPlan.isPending ? "…" : `${t("settings.switchTo")} ${planInfo.next}`}
              </Button>
            )}
          </CardContent>
        </Card>
      </div>

      <Card className="mt-6">
        <CardHeader title={t("settings.session")} />
        <CardContent className="flex gap-3">
          <Button variant="outline" onClick={() => window.open("https://dashboard.stripe.com", "_blank")}>
            <ExternalLink className="mr-2 h-4 w-4" /> Stripe Dashboard
          </Button>
          <Button variant="danger" onClick={handleSignOut}>
            <LogOut className="mr-2 h-4 w-4" /> {t("settings.signOut")}
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
