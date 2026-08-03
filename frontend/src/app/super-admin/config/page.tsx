"use client";

import { useEffect, useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useLanguage } from "@/lib/i18n";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Field, Input } from "@/components/ui/field";
import { Loading } from "@/components/ui/loading";
import { superAdminService } from "@/lib/services/super-admin-service";
import { getErrorMessage } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Settings } from "lucide-react";
import type { SystemConfig } from "@/types/super-admin";

export default function SuperAdminConfigPage() {
  const { t } = useLanguage();
  const toast = useToast();
  const qc = useQueryClient();
  const [form, setForm] = useState<SystemConfig | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["super-admin", "config"],
    queryFn: () => superAdminService.config()
  });

  useEffect(() => {
    if (data && !form) setForm(data);
  }, [data, form]);

  const saveMutation = useMutation({
    mutationFn: (input: Partial<SystemConfig>) => superAdminService.updateConfig(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["super-admin", "config"] });
      toast.success(t("superAdmin.config.saved"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  if (isLoading || !form) {
    return <Loading dark label={t("superAdmin.config.loading")} />;
  }

  const update = (key: keyof SystemConfig, value: unknown) => setForm({ ...form, [key]: value });

  return (
    <div>
      <div className="mb-6">
        <h1 className="font-display text-2xl font-bold tracking-tight text-white">{t("superAdmin.systemConfig")}</h1>
        <p className="mt-1 text-sm text-slate-400">{t("superAdmin.config.subtitle")}</p>
      </div>

      <Card className="max-w-2xl border-slate-800 bg-slate-900">
        <div className="flex items-center gap-2 border-b border-slate-800 px-5 py-4">
          <Settings className="h-4 w-4 text-slate-400" />
          <h2 className="font-display text-base font-bold text-white">{t("superAdmin.config.generalSettings")}</h2>
        </div>

        <div className="space-y-5 p-5">
          <ToggleRow
            label={t("superAdmin.config.allowSelfRegistration")}
            description={t("superAdmin.config.allowSelfRegistrationDesc")}
            checked={form.allowSelfRegistration}
            onChange={(v) => update("allowSelfRegistration", v)}
          />
          <ToggleRow
            label={t("superAdmin.config.maintenanceMode")}
            description={t("superAdmin.config.maintenanceModeDesc")}
            checked={form.maintenanceMode}
            onChange={(v) => update("maintenanceMode", v)}
          />
          <ToggleRow
            label={t("superAdmin.config.requireEmailVerification")}
            description={t("superAdmin.config.requireEmailVerificationDesc")}
            checked={form.requireEmailVerification}
            onChange={(v) => update("requireEmailVerification", v)}
          />
          <ToggleRow
            label={t("superAdmin.config.enableAiFeatures")}
            description={t("superAdmin.config.enableAiFeaturesDesc")}
            checked={form.enableAiFeatures}
            onChange={(v) => update("enableAiFeatures", v)}
          />

          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label={t("superAdmin.config.trialDays")}>
              <Input
                type="number"
                className="border-slate-700 bg-slate-800 text-white"
                value={form.trialDays}
                onChange={(e) => update("trialDays", Number(e.target.value))}
              />
            </Field>
            <Field label={t("superAdmin.config.maxLoginAttempts")}>
              <Input
                type="number"
                className="border-slate-700 bg-slate-800 text-white"
                value={form.maxLoginAttempts}
                onChange={(e) => update("maxLoginAttempts", Number(e.target.value))}
              />
            </Field>
            <Field label={t("superAdmin.config.defaultCurrency")}>
              <Input
                className="border-slate-700 bg-slate-800 text-white"
                value={form.defaultCurrency}
                onChange={(e) => update("defaultCurrency", e.target.value)}
              />
            </Field>
          </div>

          <div className="flex justify-end border-t border-slate-800 pt-4">
            <Button onClick={() => saveMutation.mutate(form)} loading={saveMutation.isPending}>
              {t("superAdmin.config.saveChanges")}
            </Button>
          </div>
        </div>
      </Card>
    </div>
  );
}

function ToggleRow({
  label,
  description,
  checked,
  onChange
}: {
  label: string;
  description: string;
  checked: boolean;
  onChange: (v: boolean) => void;
}) {
  return (
    <div className="flex items-center justify-between gap-4">
      <div>
        <p className="text-sm font-medium text-white">{label}</p>
        <p className="text-xs text-slate-400">{description}</p>
      </div>
      <button
        onClick={() => onChange(!checked)}
        className={`relative h-6 w-11 shrink-0 cursor-pointer rounded-full transition-colors ${checked ? "bg-primary-600" : "bg-slate-700"}`}
        aria-label={label}
      >
        <span className={`absolute top-0.5 h-5 w-5 rounded-full bg-white transition-all ${checked ? "left-[22px]" : "left-0.5"}`} />
      </button>
    </div>
  );
}
