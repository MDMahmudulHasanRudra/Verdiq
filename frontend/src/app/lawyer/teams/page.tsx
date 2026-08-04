"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { Loading, EmptyState } from "@/components/ui/loading";
import { teamService } from "@/lib/services";
import { getErrorMessage, initials, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import {
  UsersRound, Plus, Pencil, Trash2, UserPlus, Crown, Shield, User, Mail, Lock, X
} from "lucide-react";
import type { Team, TeamMember } from "@/types/models";

const teamRoles = ["Lead", "Senior", "Member", "Junior"];
const userRoles = ["SeniorLawyer", "JuniorLawyer", "Paralegal", "Intern", "Admin"];

export default function TeamsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [createOpen, setCreateOpen] = useState(false);
  const [detailTeam, setDetailTeam] = useState<Team | null>(null);
  const [editTeam, setEditTeam] = useState<Team | null>(null);
  const [addMemberOpen, setAddMemberOpen] = useState(false);
  const [createUserOpen, setCreateUserOpen] = useState(false);

  const { data: teams, isLoading } = useQuery({
    queryKey: ["teams"],
    queryFn: () => teamService.list()
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ["teams"] });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => teamService.create(input),
    onSuccess: () => { invalidate(); setCreateOpen(false); toast.success("Team created"); },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => teamService.remove(id),
    onSuccess: () => { invalidate(); setDetailTeam(null); toast.success("Team deleted"); },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const addMemberMutation = useMutation({
    mutationFn: ({ teamId, input }: { teamId: string; input: Record<string, unknown> }) =>
      teamService.addMember(teamId, input),
    onSuccess: (_, vars) => {
      invalidate();
      qc.invalidateQueries({ queryKey: ["teams", vars.teamId] });
      setAddMemberOpen(false);
      setCreateUserOpen(false);
      toast.success("Member added");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const removeMemberMutation = useMutation({
    mutationFn: ({ teamId, memberId }: { teamId: string; memberId: string }) =>
      teamService.removeMember(teamId, memberId),
    onSuccess: (_, vars) => {
      invalidate();
      qc.invalidateQueries({ queryKey: ["teams", vars.teamId] });
      toast.success("Member removed");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateRoleMutation = useMutation({
    mutationFn: ({ teamId, memberId, role }: { teamId: string; memberId: string; role: string }) =>
      teamService.updateMemberRole(teamId, memberId, role),
    onSuccess: (_, vars) => {
      invalidate();
      qc.invalidateQueries({ queryKey: ["teams", vars.teamId] });
      toast.success("Role updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title={t("teams.title")}
        subtitle={t("teams.subtitle")}
        actions={
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => setCreateUserOpen(true)}>
              <UserPlus className="h-4 w-4" /> Create User
            </Button>
            <Button onClick={() => setCreateOpen(true)}>
              <Plus className="h-4 w-4" /> New Team
            </Button>
          </div>
        }
      />

      {isLoading ? (
        <Loading />
      ) : teams && teams.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {teams.map((team) => (
            <button
              key={team.id}
              onClick={() => setDetailTeam(team)}
              className="cursor-pointer text-left"
            >
              <Card className="p-5 transition-colors hover:bg-slate-50/50">
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
                        title={`${m.userName} (${m.teamRole})`}
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
            </button>
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

      <CreateUserDialog
        open={createUserOpen}
        onClose={() => setCreateUserOpen(false)}
        onSubmit={(input) => {
          if (!detailTeam) return;
          addMemberMutation.mutate({ teamId: detailTeam.id, input });
        }}
        teams={teams ?? []}
        selectedTeamId={detailTeam?.id}
      />

      {detailTeam && (
        <TeamDetailDialog
          team={detailTeam}
          open={!!detailTeam}
          onClose={() => setDetailTeam(null)}
          onAddMember={() => setAddMemberOpen(true)}
          onRemoveMember={(memberId) => removeMemberMutation.mutate({ teamId: detailTeam.id, memberId })}
          onUpdateRole={(memberId, role) => updateRoleMutation.mutate({ teamId: detailTeam.id, memberId, role })}
          onDelete={() => { if (confirm("Delete this team?")) deleteMutation.mutate(detailTeam.id); }}
        />
      )}

      {addMemberOpen && detailTeam && (
        <AddMemberDialog
          open={addMemberOpen}
          onClose={() => setAddMemberOpen(false)}
          teamId={detailTeam.id}
          onAddExisting={(userId) => addMemberMutation.mutate({ teamId: detailTeam.id, input: { userId } })}
          onCreateNew={(input) => addMemberMutation.mutate({ teamId: detailTeam.id, input })}
        />
      )}
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
  const [form, setForm] = useState({ name: "", description: "" });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Create Team"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!form.name} onClick={() => onSubmit({ name: form.name, description: form.description || null })}>
            Create Team
          </Button>
        </>
      }
    >
      <div className="space-y-4">
        <Field label="Team Name" required>
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} placeholder="e.g. Criminal Defense Unit" />
        </Field>
        <Field label="Description">
          <Textarea rows={2} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}

function CreateUserDialog({
  open,
  onClose,
  onSubmit,
  teams,
  selectedTeamId
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
  teams: Team[];
  selectedTeamId?: string;
}) {
  const [form, setForm] = useState({
    invitedName: "",
    email: "",
    password: "",
    userRole: "JuniorLawyer",
    teamId: selectedTeamId ?? "",
    teamRole: "Member"
  });

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Create New User"
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.invitedName || !form.email || !form.password || !form.teamId}
            onClick={() => onSubmit({
              invitedName: form.invitedName,
              email: form.email,
              password: form.password,
              userRole: form.userRole,
              role: form.teamRole
            })}
          >
            <UserPlus className="h-4 w-4" /> Create & Add to Team
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Full Name" required className="sm:col-span-2">
          <Input value={form.invitedName} onChange={(e) => setForm({ ...form, invitedName: e.target.value })} placeholder="e.g. John Smith" />
        </Field>
        <Field label="Email" required>
          <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} placeholder="user@example.com" />
        </Field>
        <Field label="Password" required>
          <Input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} minLength={6} />
        </Field>
        <Field label="System Role">
          <Select value={form.userRole} onChange={(e) => setForm({ ...form, userRole: e.target.value })}>
            {userRoles.map((r) => <option key={r} value={r}>{r}</option>)}
          </Select>
        </Field>
        <Field label="Team" required>
          <Select value={form.teamId} onChange={(e) => setForm({ ...form, teamId: e.target.value })}>
            <option value="">Select team...</option>
            {teams.map((t) => <option key={t.id} value={t.id}>{t.name}</option>)}
          </Select>
        </Field>
        <Field label="Team Role">
          <Select value={form.teamRole} onChange={(e) => setForm({ ...form, teamRole: e.target.value })}>
            {teamRoles.map((r) => <option key={r} value={r}>{r}</option>)}
          </Select>
        </Field>
      </div>
    </Dialog>
  );
}

function TeamDetailDialog({
  team,
  open,
  onClose,
  onAddMember,
  onRemoveMember,
  onUpdateRole,
  onDelete
}: {
  team: Team;
  open: boolean;
  onClose: () => void;
  onAddMember: () => void;
  onRemoveMember: (memberId: string) => void;
  onUpdateRole: (memberId: string, role: string) => void;
  onDelete: () => void;
}) {
  return (
    <Dialog open={open} onClose={onClose} title={team.name} size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Close</Button>
          <Button variant="danger" onClick={onDelete}><Trash2 className="h-4 w-4" /> Delete Team</Button>
        </>
      }
    >
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm text-ink-muted">{team.description ?? "No description"}</p>
            <p className="mt-1 text-xs text-ink-muted">Created by {team.createdByName} · {formatDate(team.createdAt)}</p>
          </div>
          <Button size="sm" onClick={onAddMember}>
            <UserPlus className="h-4 w-4" /> Add Member
          </Button>
        </div>

        <div>
          <h4 className="mb-3 text-sm font-semibold text-ink">Members ({team.members.length})</h4>
          <div className="space-y-2">
            {team.members.map((m) => (
              <div key={m.id} className="flex items-center justify-between rounded-lg border border-line p-3">
                <div className="flex items-center gap-3">
                  <div className="flex h-9 w-9 items-center justify-center rounded-full bg-primary-50 text-sm font-semibold text-primary-700">
                    {initials(m.userName)}
                  </div>
                  <div>
                    <p className="text-sm font-medium text-ink">{m.userName}</p>
                    <p className="text-xs text-ink-muted">{m.userEmail}</p>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Select
                    value={m.teamRole}
                    onChange={(e) => onUpdateRole(m.id, e.target.value)}
                    className="h-8 w-28 text-xs"
                  >
                    {teamRoles.map((r) => <option key={r} value={r}>{r}</option>)}
                  </Select>
                  {m.teamRole === "Lead" ? (
                    <Crown className="h-4 w-4 text-amber-500" />
                  ) : m.teamRole === "Senior" ? (
                    <Shield className="h-4 w-4 text-blue-500" />
                  ) : null}
                  <button
                    onClick={() => { if (confirm(`Remove ${m.userName} from this team?`)) onRemoveMember(m.id); }}
                    className="cursor-pointer rounded-lg p-1 text-ink-muted hover:bg-red-50 hover:text-red-600"
                  >
                    <X className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
            {team.members.length === 0 ? (
              <p className="py-4 text-center text-sm text-ink-muted">No members yet.</p>
            ) : null}
          </div>
        </div>
      </div>
    </Dialog>
  );
}

function AddMemberDialog({
  open,
  onClose,
  teamId,
  onAddExisting,
  onCreateNew
}: {
  open: boolean;
  onClose: () => void;
  teamId: string;
  onAddExisting: (userId: string) => void;
  onCreateNew: (input: Record<string, unknown>) => void;
}) {
  const [mode, setMode] = useState<"existing" | "new">("new");
  const [form, setForm] = useState({ name: "", email: "", password: "", userRole: "JuniorLawyer", teamRole: "Member" });

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Add Member"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          {mode === "new" ? (
            <Button
              disabled={!form.name || !form.email || !form.password}
              onClick={() => onCreateNew({
                invitedName: form.name,
                email: form.email,
                password: form.password,
                userRole: form.userRole,
                role: form.teamRole
              })}
            >
              <UserPlus className="h-4 w-4" /> Create & Add
            </Button>
          ) : null}
        </>
      }
    >
      <div className="space-y-4">
        <div className="flex gap-2">
          <Button size="sm" variant={mode === "new" ? "subtle" : "ghost"} onClick={() => setMode("new")}>
            <UserPlus className="h-3.5 w-3.5" /> Create New User
          </Button>
        </div>
        {mode === "new" && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <Field label="Full Name" required className="sm:col-span-2">
              <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            </Field>
            <Field label="Email" required>
              <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
            </Field>
            <Field label="Password" required>
              <Input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} minLength={6} />
            </Field>
            <Field label="System Role">
              <Select value={form.userRole} onChange={(e) => setForm({ ...form, userRole: e.target.value })}>
                {userRoles.map((r) => <option key={r} value={r}>{r}</option>)}
              </Select>
            </Field>
            <Field label="Team Role">
              <Select value={form.teamRole} onChange={(e) => setForm({ ...form, teamRole: e.target.value })}>
                {teamRoles.map((r) => <option key={r} value={r}>{r}</option>)}
              </Select>
            </Field>
          </div>
        )}
      </div>
    </Dialog>
  );
}
