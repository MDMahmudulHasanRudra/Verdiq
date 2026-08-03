"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { StatusBadge } from "@/components/ui/badge";
import { Table } from "@/components/ui/table";
import { Loading, EmptyState } from "@/components/ui/loading";
import { superAdminService } from "@/lib/services/super-admin-service";
import { getErrorMessage, formatCurrency, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { BadgeCheck } from "lucide-react";
import type { SuperAdminSubscription } from "@/types/super-admin";

export default function SuperAdminSubscriptionsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();

  const { data, isLoading } = useQuery({
    queryKey: ["super-admin", "subscriptions"],
    queryFn: () => superAdminService.subscriptions()
  });

  const updateMutation = useMutation({
    mutationFn: (v: { id: string; input: Record<string, unknown> }) =>
      superAdminService.updateUserSubscription(v.id, v.input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["super-admin", "subscriptions"] });
      toast.success(t("superAdmin.subscriptions.subscriptionUpdated"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <div className="mb-6">
        <h1 className="font-display text-2xl font-bold tracking-tight text-white">{t("superAdmin.subscriptions.title")}</h1>
        <p className="mt-1 text-sm text-slate-400">{t("superAdmin.subscriptions.subtitle")}</p>
      </div>

      <Card className="border-slate-800 bg-slate-900">
        {isLoading ? (
          <Loading dark />
        ) : data && data.length > 0 ? (
          <Table className="dark-table">
            <thead>
              <tr>
                <th>{t("superAdmin.subscriptions.chamber")}</th>
                <th>{t("superAdmin.subscriptions.owner")}</th>
                <th>{t("superAdmin.subscriptions.plan")}</th>
                <th>{t("superAdmin.subscriptions.period")}</th>
                <th>{t("common.status")}</th>
                <th className="text-right">{t("superAdmin.subscriptions.actions")}</th>
              </tr>
            </thead>
            <tbody>
              {data.map((s: SuperAdminSubscription) => (
                <tr key={s.id}>
                  <td>
                    <p className="font-medium text-white">{s.chamberName}</p>
                    <p className="text-xs text-slate-400">{s.chamberId}</p>
                  </td>
                  <td className="text-slate-300">{s.userFullName}</td>
                  <td><StatusBadge value={s.plan} /></td>
                  <td className="text-slate-400">
                    {formatDate(s.currentPeriodStart)} → {formatDate(s.currentPeriodEnd)}
                  </td>
                  <td><StatusBadge value={s.status} /></td>
                  <td className="text-right">
                    <div className="flex items-center justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-primary-300 hover:bg-primary-500/10 hover:text-primary-200"
                        onClick={() =>
                          updateMutation.mutate({
                            id: s.id,
                            input: { plan: s.plan === "Pro" ? "Chamber" : "Pro", status: "Active" }
                          })
                        }
                      >
                        {s.plan === "Pro" ? t("superAdmin.subscriptions.upgradeToChamber") : t("superAdmin.subscriptions.downgradeToPro")}
                      </Button>
                      {s.cancelAtPeriodEnd ? (
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-slate-300 hover:bg-slate-700/40 hover:text-white"
                          onClick={() => updateMutation.mutate({ id: s.id, input: { cancelAtPeriodEnd: false } })}
                        >
                          {t("superAdmin.subscriptions.reinstate")}
                        </Button>
                      ) : null}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <EmptyState dark icon={<BadgeCheck className="h-10 w-10" />} title={t("superAdmin.subscriptions.noSubscriptions")} description={t("superAdmin.subscriptions.noSubscriptionsDesc")} />
        )}
      </Card>
    </div>
  );
}
