"use client";

import { useState } from "react";
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

export default function ConfigPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [form, setForm] = useState<Record<string, string>>({});

  const { data: config, isLoading } = useQuery({
    queryKey: ["config"],
    queryFn: () => configurationService.getAll()
  });

  const save = useMutation({
    mutationFn: () => configurationService.update(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["config"] });
      toast.success(t("configuration.configSaved"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  if (isLoading) return <Loading />;

  const cfg = (config?.settings?.general ?? {}) as Record<string, string>;

  const fields: { key: string; label: string; type?: "text" | "email" | "number" | "select"; options?: string[]; placeholder?: string }[] = [
    { key: "firmName", label: t("configuration.firmName"), placeholder: "Your firm" },
    { key: "address", label: t("configuration.address"), placeholder: "Street, city" },
    { key: "phone", label: t("configuration.phone"), type: "text", placeholder: "+880…" },
    { key: "email", label: t("configuration.email"), type: "email", placeholder: "firm@example.com" },
    { key: "website", label: t("configuration.website"), placeholder: "https://" },
    { key: "timeZone", label: t("configuration.timeZone"), type: "select", options: timezones },
    { key: "currency", label: t("configuration.currency"), type: "select", options: currencies },
    { key: "fiscalYearStart", label: t("configuration.fiscalYearStart"), type: "select", options: months }
  ];

  return (
    <div>
      <PageHeader title={t("configuration.title")} subtitle={t("configuration.subtitle")} />
      <Card>
        <CardHeader title={t("configuration.chamberDetails")} />
        <CardContent>
          <div className="space-y-4">
            {fields.map((f) => (
              <Field key={f.key} label={f.label}>
                {f.type === "select" ? (
                  <Select
                    value={form[f.key] ?? cfg[f.key] ?? ""}
                    onChange={(e) => setForm((p) => ({ ...p, [f.key]: e.target.value }))}
                  >
                    {f.options?.map((o) => <option key={o} value={o}>{o}</option>)}
                  </Select>
                ) : (
                  <Input
                    type={f.type ?? "text"}
                    placeholder={f.placeholder}
                    value={form[f.key] ?? cfg[f.key] ?? ""}
                    onChange={(e) => setForm((p) => ({ ...p, [f.key]: e.target.value }))}
                  />
                )}
              </Field>
            ))}
          </div>
          <div className="mt-6 flex justify-end">
            <Button onClick={() => save.mutate()} disabled={save.isPending || Object.keys(form).length === 0}>
              {save.isPending ? "…" : t("configuration.saveChanges")}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
