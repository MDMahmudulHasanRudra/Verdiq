"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input, Select, Field } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/badge";
import { Table } from "@/components/ui/table";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { superAdminService } from "@/lib/services/super-admin-service";
import { getErrorMessage, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { Users, Plus, KeyRound } from "lucide-react";
import type { SuperAdminUser } from "@/types/super-admin";

export default function SuperAdminUsersPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [createOpen, setCreateOpen] = useState(false);
  const [resetTarget, setResetTarget] = useState<SuperAdminUser | null>(null);

  const { data: users, isLoading } = useQuery({
    queryKey: ["super-admin", "users"],
    queryFn: () => superAdminService.users()
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => superAdminService.createUser(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["super-admin", "users"] });
      setCreateOpen(false);
      toast.success(t("superAdmin.users.userCreated"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const toggleMutation = useMutation({
    mutationFn: (id: string) => superAdminService.toggleUserStatus(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["super-admin", "users"] });
      toast.success(t("superAdmin.users.statusToggled"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const resetMutation = useMutation({
    mutationFn: (v: { id: string; password: string }) => superAdminService.resetPassword(v.id, v.password),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["super-admin", "users"] });
      setResetTarget(null);
      toast.success(t("superAdmin.users.passwordReset"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="font-display text-2xl font-bold tracking-tight text-white">{t("superAdmin.users.title")}</h1>
          <p className="mt-1 text-sm text-slate-400">{t("superAdmin.users.subtitle")}</p>
        </div>
        <Button onClick={() => setCreateOpen(true)}>
          <Plus className="h-4 w-4" /> {t("superAdmin.users.createUser")}
        </Button>
      </div>

      <Card className="border-slate-800 bg-slate-900">
        {isLoading ? (
          <Loading dark />
        ) : users && users.length > 0 ? (
          <Table className="dark-table">
            <thead>
              <tr>
                <th>{t("superAdmin.users.user")}</th>
                <th>{t("superAdmin.chambers.title")}</th>
                <th>{t("superAdmin.users.role")}</th>
                <th>{t("superAdmin.users.plan")}</th>
                <th>{t("superAdmin.users.joined")}</th>
                <th>{t("superAdmin.users.status")}</th>
                <th className="text-right">{t("superAdmin.users.actions")}</th>
              </tr>
            </thead>
            <tbody>
              {users.map((u: SuperAdminUser) => (
                <tr key={u.id}>
                  <td>
                    <p className="font-medium text-white">{u.fullName}</p>
                    <p className="text-xs text-slate-400">{u.email}</p>
                  </td>
                  <td className="max-w-40 truncate text-slate-300">{u.chamberName}</td>
                  <td><StatusBadge value={u.role} /></td>
                  <td><StatusBadge value={u.subscriptionPlan} /></td>
                  <td className="text-slate-400">{formatDate(u.createdAt)}</td>
                  <td><StatusBadge value={u.isActive ? "Active" : "Inactive"} /></td>
                  <td className="text-right">
                    <div className="flex items-center justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-primary-300 hover:bg-primary-500/10 hover:text-primary-200"
                        onClick={() => setResetTarget(u)}
                      >
                        <KeyRound className="h-3.5 w-3.5" /> {t("superAdmin.users.resetPassword")}
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        className="text-slate-300 hover:bg-slate-700/40 hover:text-white"
                        onClick={() => toggleMutation.mutate(u.id)}
                        loading={toggleMutation.isPending && toggleMutation.variables === u.id}
                      >
                        {u.isActive ? t("superAdmin.users.suspend") : t("superAdmin.users.activate")}
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <EmptyState dark icon={<Users className="h-10 w-10" />} title={t("superAdmin.users.noUsers")} description={t("superAdmin.users.noUsersDesc")} />
        )}
      </Card>

      <CreateUserDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
      <ResetPasswordDialog user={resetTarget} onClose={() => setResetTarget(null)} onSubmit={(password) => resetTarget && resetMutation.mutate({ id: resetTarget.id, password })} />
    </div>
  );
}

function CreateUserDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const { t } = useLanguage();
  const [form, setForm] = useState({ fullName: "", email: "", password: "", role: "Owner", chamberId: "" });
  const isFormValid = form.fullName && form.email && form.password;

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={t("superAdmin.users.createUser")}
      description={t("superAdmin.users.provisionUser")}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button disabled={!isFormValid} onClick={() => onSubmit(form)}>{t("superAdmin.users.createUser")}</Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("superAdmin.users.fullName")} required>
          <Input value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
        </Field>
        <Field label={t("superAdmin.users.email")} required>
          <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
        </Field>
        <Field label={t("superAdmin.users.password")} required>
          <Input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
        </Field>
        <Field label={t("superAdmin.users.role")}>
          <Select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })}>
            <option value="Owner">Owner</option>
            <option value="SeniorLawyer">Senior Lawyer</option>
            <option value="JuniorLawyer">Junior Lawyer</option>
            <option value="Assistant">Assistant</option>
            <option value="Accountant">Accountant</option>
            <option value="Client">Client</option>
          </Select>
        </Field>
        <Field label={t("superAdmin.users.chamberId")} className="sm:col-span-2">
          <Input value={form.chamberId} onChange={(e) => setForm({ ...form, chamberId: e.target.value })} placeholder={t("superAdmin.users.chamberIdPlaceholder")} />
        </Field>
      </div>
    </Dialog>
  );
}

function ResetPasswordDialog({
  user,
  onClose,
  onSubmit
}: {
  user: SuperAdminUser | null;
  onClose: () => void;
  onSubmit: (password: string) => void;
}) {
  const { t } = useLanguage();
  const [password, setPassword] = useState("");

  return (
    <Dialog
      open={!!user}
      onClose={onClose}
      title={t("superAdmin.users.resetPassword")}
      description={user ? `${t("superAdmin.users.resetPasswordFor")} ${user.fullName}.` : ""}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button disabled={!password} onClick={() => onSubmit(password)}>{t("superAdmin.users.resetPassword")}</Button>
        </>
      }
    >
      <Field label={t("superAdmin.users.newPassword")} required>
        <Input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
      </Field>
    </Dialog>
  );
}
