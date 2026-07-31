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
import { UsersRound, Plus } from "lucide-react";

export default function TeamsPage() {
  const toast = useToast();
  const qc = useQueryClient();
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
        title="Teams"
        subtitle="Organize lawyers and staff into practice teams."
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
          {teams.map((t) => (
            <Card key={t.id} className="p-5">
              <div className="flex items-start justify-between">
                <div>
                  <h3 className="font-display text-lg font-semibold text-ink">{t.name}</h3>
                  <p className="text-xs text-ink-muted">{t.memberCount} members</p>
                </div>
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-primary-50">
                  <UsersRound className="h-5 w-5 text-primary-700" />
                </div>
              </div>
              <p className="mt-2 line-clamp-2 text-sm text-ink-muted">{t.description ?? "—"}</p>
              <div className="mt-4 border-t border-line-soft pt-3">
                <p className="text-xs text-ink-muted">Created by {t.createdByName}</p>
                <div className="mt-2 flex flex-wrap gap-1.5">
                  {t.members.slice(0, 6).map((m) => (
                    <span
                      key={m.id}
                      className="flex h-7 w-7 items-center justify-center rounded-full bg-gold-50 text-xs font-semibold text-gold-800"
                      title={m.userName}
                    >
                      {initials(m.userName)}
                    </span>
                  ))}
                  {t.members.length > 6 ? (
                    <span className="flex h-7 items-center rounded-full bg-slate-100 px-2 text-xs font-medium text-ink-muted">
                      +{t.members.length - 6}
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
            title="No teams"
            description="Create practice teams to group lawyers by specialization."
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
  const [form, setForm] = useState({ name: "", specialization: "", description: "" });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="New Team"
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
        <Field label="Team Name" required>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </Field>
        <Field label="Specialization">
          <Select value={form.specialization} onChange={(e) => setForm({ ...form, specialization: e.target.value })}>
            <option value="">General</option>
            {["Civil", "Criminal", "Corporate", "Family", "Property", "Labor"].map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </Select>
        </Field>
        <Field label="Description" className="sm:col-span-2">
          <Textarea rows={2} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}
