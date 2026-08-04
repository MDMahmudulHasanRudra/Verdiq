"use client";

import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { useToast } from "@/components/ui/toast";
import { configurationService } from "@/lib/services";
import { getErrorMessage } from "@/lib/utils";
import { useLanguage } from "@/lib/i18n";

const timezones = ["Asia/Kolkata", "Asia/Dhaka", "Asia/Karachi", "Asia/Colombo", "Asia/Kathmandu", "Asia/Thimphu", "Asia/Yangon", "Asia/Singapore", "Asia/Dubai", "Europe/London", "America/New_York"];
const currencies = ["BDT", "INR", "PKR", "LKR", "NPR", "SGD", "USD", "GBP", "EUR"];
const months = Array.from({ length: 12 }, (_, i) => new Date(0, i).toLocaleString("en", { month: "long" }));
const reminderChannels = ["email", "sms", "whatsapp"];
const storageProviders = ["local", "s3", "cloudinary", "google-drive"];
const caseStatuses = ["Active", "Pending", "Closed", "Appeal", "Withdrawn"];

export default function ConfigPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [form, setForm] = useState<Record<string, unknown>>({});

  const { data: config, isLoading } = useQuery({
    queryKey: ["config"],
    queryFn: () => configurationService.getAll()
  });

  const sections = useMemo(() => {
    const general = ((config?.settings?.general ?? {}) as Record<string, unknown>);
    const branding = ((config?.settings?.branding ?? {}) as Record<string, unknown>);
    const billing = ((config?.settings?.billing ?? {}) as Record<string, unknown>);
    const communications = ((config?.settings?.communications ?? {}) as Record<string, unknown>);
    const integrations = ((config?.settings?.integrations ?? {}) as Record<string, unknown>);
    const securitySession = ((config?.settings?.securitySession ?? {}) as Record<string, unknown>);
    const dataRetention = ((config?.settings?.dataRetention ?? {}) as Record<string, unknown>);
    const workflow = ((config?.settings?.workflow ?? {}) as Record<string, unknown>);
    return { general, branding, billing, communications, integrations, securitySession, dataRetention, workflow };
  }, [config]);

  const save = useMutation({
    mutationFn: () => configurationService.update(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["config"] });
      toast.success(t("configuration.configSaved"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  if (isLoading) return <Loading />;

  const updateField = (section: string, key: string, value: string | boolean | number) => {
    setForm((prev) => ({
      ...prev,
      [section]: {
        ...((prev[section] as Record<string, unknown>) ?? {}),
        [key]: value
      }
    }));
  };

  const renderInput = (section: string, key: string, label: string, value: unknown, type: "text" | "email" | "number" | "select" | "checkbox" = "text", options?: string[]) => {
    const currentValue = (form[section] as Record<string, unknown> | undefined)?.[key] ?? (sections[section as keyof typeof sections]?.[key] ?? "");
    if (type === "checkbox") {
      return (
        <label className="flex items-center gap-3 rounded-lg border border-line bg-card px-3 py-3 text-sm text-ink">
          <input
            type="checkbox"
            checked={Boolean(currentValue)}
            onChange={(e) => updateField(section, key, e.target.checked)}
          />
          <span>{label}</span>
        </label>
      );
    }

    return (
      <Field key={`${section}-${key}`} label={label}>
        {type === "select" ? (
          <Select value={String(currentValue ?? "")} onChange={(e) => updateField(section, key, e.target.value)}>
            {options?.map((o) => <option key={o} value={o}>{o}</option>)}
          </Select>
        ) : (
          <Input
            type={type}
            value={String(currentValue ?? "")}
            onChange={(e) => updateField(section, key, type === "number" ? Number(e.target.value) : e.target.value)}
          />
        )}
      </Field>
    );
  };

  return (
    <div>
      <PageHeader title={t("configuration.title")} subtitle={t("configuration.subtitle")} />
      <div className="space-y-6">
        <Card>
          <CardHeader title={t("configuration.organization")} />
          <CardContent className="grid gap-4 md:grid-cols-2">
            {renderInput("general", "companyName", t("configuration.firmName"), sections.general.companyName, "text")}
            {renderInput("general", "companyNameBn", t("configuration.appNameBn"), sections.general.companyNameBn, "text")}
            {renderInput("general", "address", t("configuration.address"), sections.general.address, "text")}
            {renderInput("general", "phone", t("configuration.phone"), sections.general.phone, "text")}
            {renderInput("general", "email", t("configuration.email"), sections.general.email, "email")}
            {renderInput("general", "website", t("configuration.website"), sections.general.website, "text")}
            {renderInput("general", "timezone", t("configuration.timeZone"), sections.general.timezone, "select", timezones)}
            {renderInput("general", "currency", t("configuration.currency"), sections.general.currency, "select", currencies)}
            {renderInput("general", "fiscalYearStart", t("configuration.fiscalYearStart"), sections.general.fiscalYearStart, "select", months)}
            {renderInput("general", "language", "Default Language", sections.general.language, "select", ["en", "bn"])}
          </CardContent>
        </Card>

        <Card>
          <CardHeader title={t("configuration.branding")} />
          <CardContent className="grid gap-4 md:grid-cols-2">
            {renderInput("branding", "appName", t("configuration.appName"), sections.branding.appName, "text")}
            {renderInput("branding", "appNameBn", t("configuration.appNameBn"), sections.branding.appNameBn, "text")}
            {renderInput("branding", "themeColor", t("configuration.themeColor"), sections.branding.themeColor, "text")}
            {renderInput("branding", "accentColor", t("configuration.accentColor"), sections.branding.accentColor, "text")}
            {renderInput("branding", "showBranding", t("configuration.showBranding"), sections.branding.showBranding, "checkbox")}
          </CardContent>
        </Card>

        <Card>
          <CardHeader title={t("configuration.billing")} />
          <CardContent className="grid gap-4 md:grid-cols-2">
            {renderInput("billing", "taxRatePercent", t("configuration.taxRatePercent"), sections.billing.taxRatePercent, "number")}
            {renderInput("billing", "invoiceDueDays", t("configuration.invoiceDueDays"), sections.billing.invoiceDueDays, "number")}
            {renderInput("billing", "lateFeePercent", t("configuration.lateFeePercent"), sections.billing.lateFeePercent, "number")}
          </CardContent>
        </Card>

        <Card>
          <CardHeader title={t("configuration.communications")} />
          <CardContent className="grid gap-4 md:grid-cols-2">
            {renderInput("communications", "defaultReminderChannel", t("configuration.defaultReminderChannel"), sections.communications.defaultReminderChannel, "select", reminderChannels)}
            {renderInput("communications", "allowEmail", t("configuration.allowEmail"), sections.communications.allowEmail, "checkbox")}
            {renderInput("communications", "allowSms", t("configuration.allowSms"), sections.communications.allowSms, "checkbox")}
            {renderInput("communications", "allowWhatsApp", t("configuration.allowWhatsApp"), sections.communications.allowWhatsApp, "checkbox")}
          </CardContent>
        </Card>

        <Card>
          <CardHeader title={t("configuration.integrations")} />
          <CardContent className="grid gap-4 md:grid-cols-2">
            {renderInput("integrations", "googleDriveEnabled", t("configuration.googleDriveEnabled"), sections.integrations.googleDriveEnabled, "checkbox")}
            {renderInput("integrations", "storageProvider", t("configuration.storageProvider"), sections.integrations.storageProvider, "select", storageProviders)}
          </CardContent>
        </Card>

        <Card>
          <CardHeader title={t("configuration.security")} />
          <CardContent className="grid gap-4 md:grid-cols-2">
            {renderInput("securitySession", "enableMfa", t("configuration.enableMfa"), sections.securitySession.enableMfa, "checkbox")}
            {renderInput("securitySession", "sessionTimeoutMinutes", t("configuration.sessionTimeoutMinutes"), sections.securitySession.sessionTimeoutMinutes, "number")}
            {renderInput("securitySession", "maxLoginAttempts", t("configuration.maxLoginAttempts"), sections.securitySession.maxLoginAttempts, "number")}
            {renderInput("securitySession", "lockoutDurationMinutes", t("configuration.lockoutDurationMinutes"), sections.securitySession.lockoutDurationMinutes, "number")}
          </CardContent>
        </Card>

        <Card>
          <CardHeader title={t("configuration.retention")} />
          <CardContent className="grid gap-4 md:grid-cols-2">
            {renderInput("dataRetention", "archiveAfterDays", t("configuration.archiveAfterDays"), sections.dataRetention.archiveAfterDays, "number")}
            {renderInput("dataRetention", "autoDeleteAfterDays", t("configuration.autoDeleteAfterDays"), sections.dataRetention.autoDeleteAfterDays, "number")}
            {renderInput("dataRetention", "retainAuditLogsDays", t("configuration.retainAuditLogsDays"), sections.dataRetention.retainAuditLogsDays, "number")}
          </CardContent>
        </Card>

        <Card>
          <CardHeader title={t("configuration.workflow")} />
          <CardContent className="grid gap-4 md:grid-cols-2">
            {renderInput("workflow", "autoCaseNumbering", t("configuration.autoCaseNumbering"), sections.workflow.autoCaseNumbering, "checkbox")}
            {renderInput("workflow", "requireWorkflowNotes", t("configuration.requireWorkflowNotes"), sections.workflow.requireWorkflowNotes, "checkbox")}
            {renderInput("workflow", "allowCaseReopen", t("configuration.allowCaseReopen"), sections.workflow.allowCaseReopen, "checkbox")}
            {renderInput("workflow", "defaultCaseStatus", t("configuration.defaultCaseStatus"), sections.workflow.defaultCaseStatus, "select", caseStatuses)}
          </CardContent>
        </Card>

        <div className="flex justify-end">
          <Button onClick={() => save.mutate()} disabled={save.isPending || Object.keys(form).length === 0}>
            {save.isPending ? "…" : t("configuration.saveChanges")}
          </Button>
        </div>
      </div>
    </div>
  );
}
