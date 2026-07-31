"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { StatCard } from "@/components/ui/stat-card";
import { Button } from "@/components/ui/button";
import { Input, Select, Field } from "@/components/ui/field";
import { StatusBadge } from "@/components/ui/badge";
import { Table } from "@/components/ui/table";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { adminService } from "@/lib/services";
import { getErrorMessage, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Users, Activity, DollarSign, ShieldAlert, Plus } from "lucide-react";
import type { AdminUser } from "@/types/models";

export default function AdminPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [search, setSearch] = useState("");
  const [createOpen, setCreateOpen] = useState(false);

  const { data: users, isLoading } = useQuery({
    queryKey: ["admin", "users", search],
    queryFn: () => adminService.users(search || undefined)
  });

  const { data: stats } = useQuery({
    queryKey: ["admin", "stats"],
    queryFn: () => adminService.systemStats()
  });

  const s = (stats as Record<string, unknown> | undefined) ?? {};

  const toggleStatus = useMutation({
    mutationFn: (id: string) => adminService.toggleStatus(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "users"] });
      toast.success("User status updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const createUser = useMutation({
    mutationFn: (input: Record<string, unknown>) => adminService.createUser(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["admin", "users"] });
      setCreateOpen(false);
      toast.success("User created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title="Admin"
        subtitle="Manage chamber users and monitor system health."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> Add User
          </Button>
        }
      />

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard label="Total Users" value={Number(s.totalUsers ?? 0)} icon={<Users className="h-5 w-5" />} accent="primary" />
        <StatCard label="Active Cases" value={Number(s.totalCases ?? 0)} icon={<Activity className="h-5 w-5" />} accent="blue" />
        <StatCard label="Monthly Revenue" value={formatCurrency(Number(s.monthlyRevenue ?? 0))} icon={<DollarSign className="h-5 w-5" />} accent="green" />
        <StatCard label="Inactive Users" value={Number(s.inactiveUsers ?? 0)} icon={<ShieldAlert className="h-5 w-5" />} accent="red" />
      </div>

      <Card>
        <div className="flex flex-col gap-3 border-b border-line p-4 sm:flex-row sm:items-center sm:justify-between">
          <div className="flex flex-1 flex-wrap items-center gap-2">
            <Input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search by name or email…"
              className="w-full max-w-xs"
            />
          </div>
          <p className="text-sm text-ink-muted">{users?.length ?? 0} users</p>
        </div>

        {isLoading ? (
          <Loading />
        ) : users && users.length > 0 ? (
          <Table>
            <thead>
              <tr>
                <th>User</th>
                <th>Role</th>
                <th>Cases</th>
                <th>Joined</th>
                <th>Status</th>
                <th className="text-right">Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((u: AdminUser) => (
                <tr key={u.id}>
                  <td>
                    <p className="font-medium text-ink">{u.fullName}</p>
                    <p className="text-xs text-ink-muted">{u.email}</p>
                  </td>
                  <td><StatusBadge value={u.role} /></td>
                  <td className="text-ink-muted">{u.casesCount}</td>
                  <td className="text-ink-muted">{formatDate(u.createdAt)}</td>
                  <td><StatusBadge value={u.isActive ? "Active" : "Inactive"} /></td>
                  <td className="text-right">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => toggleStatus.mutate(u.id)}
                      loading={toggleStatus.isPending && toggleStatus.variables === u.id}
                    >
                      {u.isActive ? "Deactivate" : "Activate"}
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <EmptyState
            icon={<Users className="h-10 w-10" />}
            title="No users found"
            description="Add a team member to your chamber to get started."
          />
        )}
      </Card>

      <CreateUserDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createUser.mutate(v)} />
    </div>
  );
}

function formatCurrency(n: number) {
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "BDT", maximumFractionDigits: 0 }).format(n);
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
  const [form, setForm] = useState({ fullName: "", email: "", password: "", role: "JuniorLawyer" });
  const isFormValid = form.fullName && form.email && form.password;

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Add User"
      description="Invite a new member to the chamber."
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!isFormValid} onClick={() => onSubmit({ ...form })}>Create User</Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Full Name" required>
          <Input value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
        </Field>
        <Field label="Email" required>
          <Input type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
        </Field>
        <Field label="Password" required>
          <Input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
        </Field>
        <Field label="Role">
          <Select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })}>
            <option value="SeniorLawyer">Senior Lawyer</option>
            <option value="JuniorLawyer">Junior Lawyer</option>
            <option value="Assistant">Assistant</option>
            <option value="Accountant">Accountant</option>
          </Select>
        </Field>
      </div>
    </Dialog>
  );
}
