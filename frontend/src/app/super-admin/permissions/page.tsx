"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Select } from "@/components/ui/field";
import { Loading, EmptyState } from "@/components/ui/loading";
import { superAdminService } from "@/lib/services/super-admin-service";
import { getErrorMessage } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { KeyRound } from "lucide-react";
import type { Permission } from "@/types/super-admin";

const ROLE_OPTIONS = ["SuperAdmin", "Owner", "SeniorLawyer", "JuniorLawyer", "Assistant", "Accountant", "Client"];

export default function SuperAdminPermissionsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [role, setRole] = useState("SeniorLawyer");

  const { data: permissions, isLoading: permsLoading } = useQuery({
    queryKey: ["super-admin", "permissions"],
    queryFn: () => superAdminService.permissions()
  });

  const { data: rolePerms } = useQuery({
    queryKey: ["super-admin", "role-permissions"],
    queryFn: () => superAdminService.rolePermissions()
  });

  const assignMutation = useMutation({
    mutationFn: (permissionIds: string[]) => superAdminService.assignRolePermissions(role, permissionIds),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["super-admin", "role-permissions"] });
      toast.success(`Updated permissions for ${role}`);
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const current = rolePerms?.find((rp) => rp.role === role);
  const currentIds = new Set((current?.permissions ?? []).map((p) => p.id));

  const toggle = (id: string) => {
    const next = new Set(currentIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    assignMutation.mutate(Array.from(next));
  };

  const grouped: Record<string, Permission[]> = {};
  (permissions ?? []).forEach((p) => {
    (grouped[p.module] = grouped[p.module] || []).push(p);
  });

  return (
    <div>
      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="font-display text-2xl font-bold tracking-tight text-white">Permissions</h1>
          <p className="mt-1 text-sm text-slate-400">Control what each role can do across the platform.</p>
        </div>
        <Select className="w-52 border-slate-700 bg-slate-800 text-white" value={role} onChange={(e) => setRole(e.target.value)}>
          {ROLE_OPTIONS.map((r) => (
            <option key={r} value={r}>{r}</option>
          ))}
        </Select>
      </div>

      {permsLoading ? (
        <Card className="border-slate-800 bg-slate-900"><Loading dark /></Card>
      ) : Object.keys(grouped).length > 0 ? (
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
          {Object.entries(grouped).map(([module, perms]) => (
            <Card key={module} className="border-slate-800 bg-slate-900">
              <div className="border-b border-slate-800 px-5 py-4">
                <h2 className="font-display text-base font-bold text-white">{module}</h2>
              </div>
              <ul className="divide-y divide-slate-800/60">
                {perms.map((p) => (
                  <li key={p.id} className="flex items-center justify-between gap-3 px-5 py-3">
                    <div>
                      <p className="text-sm font-medium text-white">{p.name}</p>
                      <p className="text-xs text-slate-400">{p.description}</p>
                    </div>
                    <button
                      onClick={() => toggle(p.id)}
                      className={`relative h-6 w-11 shrink-0 cursor-pointer rounded-full transition-colors ${
                        currentIds.has(p.id) ? "bg-primary-600" : "bg-slate-700"
                      }`}
                      aria-label={`Toggle ${p.name}`}
                    >
                      <span
                        className={`absolute top-0.5 h-5 w-5 rounded-full bg-white transition-all ${
                          currentIds.has(p.id) ? "left-[22px]" : "left-0.5"
                        }`}
                      />
                    </button>
                  </li>
                ))}
              </ul>
            </Card>
          ))}
        </div>
      ) : (
        <Card className="border-slate-800 bg-slate-900">
          <EmptyState dark icon={<KeyRound className="h-10 w-10" />} title="No permissions" description="Seed permissions in the system to configure roles." />
        </Card>
      )}

      {assignMutation.isPending ? (
        <p className="mt-4 text-sm text-slate-400">Saving permission changes…</p>
      ) : (
        <p className="mt-4 flex items-center gap-2 text-xs text-slate-500">
          <Button variant="ghost" size="sm" className="text-slate-300" onClick={() => assignMutation.mutate(Array.from(currentIds))}>
            Save current role
          </Button>
        </p>
      )}
    </div>
  );
}
