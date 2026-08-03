"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Dialog } from "@/components/ui/dialog";
import { Loading, EmptyState } from "@/components/ui/loading";
import { teamService } from "@/lib/services";
import { getErrorMessage, initials } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { UsersRound, Plus } from "lucide-react";

export default function TeamsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [createOpen, setCreateOpen] = useState(false);

  const { data: teams, isLoading } = useQuery({
    queryKey: ["teams"],
    queryFn: () => teamService.list()
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => teamService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["teams"] });
      setCreateOpen(false);
      toast.success("Team created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title={t("teams.title")}
        subtitle={t("teams.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> New Team
          </Button>
        }
      />

      {isLoading ? (
        <Loading />
      ) : teams && teams.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {teams.map((team) => (
            <Card key={team.id} className="p-5">
              <div className="flex items-start justify-between">
                <div>
                  <h3 className="font-display text-lg font-semibold text-ink">{team.name}</h3>
                  <p className="text-xs text-ink-muted">{team.memberCount} members</p>
                </div>
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary-50">
                  <UsersRound className="h-5 w-5 text-primary-700" />
                </div>
              </div>
              <p className="mt-2 line-clamp-2 text-sm text-ink-muted">{team.description ?? "—"}</p>
              <div className="mt-4 border-t border-line-soft pt-3">
                <p className="text-xs text-ink-muted">Created by {team.createdByName}</p>
                <div className="mt-2 flex flex-wrap gap-1.5">
                  {team.members.slice(0, 6).map((m) => (
                    <span
                      key={m.id}
                      className="flex h-7 w-7 items-center justify-center rounded-full bg-gold-50 text-xs font-semibold text-gold-800"
                      title={m.userName}
                    >
                      {initials(m.userName)}
                    </span>
                  ))}
                  {team.members.length > 6 ? (
                    <span className="flex h-7 items-center rounded-full bg-slate-100 px-2 text-xs font-medium text-ink-muted">
                      +{team.members.length - 6}
                    </span>
                  ) : null}
                </div>
              </div>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <EmptyState
            icon={<UsersRound className="h-10 w-10" />}
            title={t("teams.noMembers")}
            description={t("teams.noMembersDesc")}
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> New Team</Button>}
          />
        </Card>
      )}

      <CreateTeamDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function CreateTeamDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const { t } = useLanguage();
  const [form, setForm] = useState({ name: "", specialization: "", description: "" });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("teams.title")}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.name}
            onClick={() =>
              onSubmit({
                name: form.name,
                specialization: form.specialization || null,
                description: form.description || null
              })
            }
          >
            Create Team
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("teams.member")} required>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </Field>
        <Field label={t("teams.role")}>
          <Select value={form.specialization} onChange={(e) => setForm({ ...form, specialization: e.target.value })}>
            <option value="">General</option>
            {["Civil", "Criminal", "Corporate", "Family", "Property", "Labor"].map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </Select>
        </Field>
        <Field label={t("teams.role")} className="sm:col-span-2">
          <Textarea rows={2} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
