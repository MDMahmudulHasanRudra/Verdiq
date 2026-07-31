"use client";

import { useState, useEffect } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Field, Input } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Loading } from "@/components/ui/loading";
import { configurationService } from "@/lib/services";
import { getErrorMessage } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Cog } from "lucide-react";

const fields = [
  { key: "firmName", label: "Firm Name" },
  { key: "address", label: "Address" },
  { key: "phone", label: "Phone" },
  { key: "email", label: "Email" },
  { key: "website", label: "Website" },
  { key: "timeZone", label: "Time Zone" },
  { key: "currency", label: "Currency" },
  { key: "fiscalYearStart", label: "Fiscal Year Start (month)" }
];

export default function ConfigurationPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [form, setForm] = useState<Record<string, string>>({});
  const [loaded, setLoaded] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ["configuration"],
    queryFn: () => configurationService.getAll()
  });

  useEffect(() => {
    if (data && !loaded) {
      const next: Record<string, string> = {};
      fields.forEach((f) => {
        const v = (data as unknown as Record<string, unknown>)[f.key];
        next[f.key] = v != null ? String(v) : "";
      });
      setForm(next);
      setLoaded(true);
    }
  }, [data, loaded]);

  const saveMutation = useMutation({
    mutationFn: () => configurationService.update(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["configuration"] });
      toast.success("Configuration saved");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  if (isLoading && !loaded) return <Loading label="Loading configuration…" />;

  return (
    <div>
      <PageHeader
        title="Configuration"
        subtitle="Firm-wide settings used across the system."
        actions={
          <Button onClick={() => saveMutation.mutate()} loading={saveMutation.isPending}>
            Save Changes
          </Button>
        }
      />
      <Card>
        <CardHeader title="Chamber Details" />
        <CardContent>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            {fields.map((f) => (
              <Field key={f.key} label={f.label}>
                <Input
                  value={form[f.key] ?? ""}
                  onChange={(e) => setForm((prev) => ({ ...prev, [f.key]: e.target.value }))}
                />
              </Field>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
