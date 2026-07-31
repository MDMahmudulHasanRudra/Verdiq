import { saGet, saPost, saPut, saDelete, saApi } from "@/lib/api";
import type {
  SuperAdminChamber,
  SuperAdminUser,
  SuperAdminSubscription,
  SuperAdminDashboard,
  Permission,
  RolePermissions,
  AuditLog,
  SystemConfig,
  SystemHealth,
  AdminCase
} from "@/types/super-admin";

export const superAdminService = {
  login: async (userId: string, password: string) => {
    const { data } = await saApi.post("/super-admin/login", { userId, password });
    return data as {
      success: boolean;
      message: string;
      accessToken: string | null;
      refreshToken: string | null;
      admin: { id: string; name: string; userId: string; role: string } | null;
    };
  },
  dashboard: () => saGet<SuperAdminDashboard>("/super-admin/dashboard"),
  chambers: () => saGet<SuperAdminChamber[]>("/super-admin/chambers"),
  chamber: (id: string) => saGet<SuperAdminChamber>(`/super-admin/chambers/${id}`),
  createChamber: (input: { name: string; address?: string; phone?: string; plan?: string }) =>
    saPost<object>("/super-admin/chambers", input),
  updateChamberPlan: (id: string, plan: string) => saPut<object>(`/super-admin/chambers/${id}/plan`, { plan }),
  impersonate: (id: string, userId?: string | null) =>
    saPost<{ impersonationToken: string; message: string }>(`/super-admin/chambers/${id}/impersonate`, { userId }),
  clearChamber: (id: string) => saDelete<{ success: boolean; message: string }>(`/super-admin/chambers/${id}/clear`),
  deleteChamber: (id: string) => saDelete<object>(`/super-admin/chambers/${id}`),
  users: (chamberId?: string) => saGet<SuperAdminUser[]>(`/super-admin/users${chamberId ? `?chamberId=${chamberId}` : ""}`),
  createUser: (input: Record<string, unknown>) => saPost<object>("/super-admin/users", input),
  resetPassword: (id: string, newPassword: string) => saPost<object>(`/super-admin/users/${id}/reset-password`, { newPassword }),
  toggleUserStatus: (id: string) => saPost<object>(`/super-admin/users/${id}/toggle-status`),
  updateUserSubscription: (id: string, input: Record<string, unknown>) =>
    saPut<object>(`/super-admin/users/${id}/subscription`, input),
  subscriptions: () => saGet<SuperAdminSubscription[]>("/super-admin/subscriptions"),
  permissions: () => saGet<Permission[]>("/super-admin/permissions"),
  rolePermissions: () => saGet<RolePermissions[]>("/super-admin/role-permissions"),
  assignRolePermissions: (role: string, permissionIds: string[]) =>
    saPut<object>("/super-admin/role-permissions", { role, permissionIds }),
  auditLogs: (page = 1, pageSize = 50) =>
    saGet<AuditLog[]>(`/super-admin/audit-logs?page=${page}&pageSize=${pageSize}`),
  billing: () => saGet<Record<string, unknown>>("/super-admin/billing"),
  broadcast: (input: { title: string; message: string; type?: string; targetChamberId?: string }) =>
    saPost<object>("/super-admin/broadcast", input),
  config: () => saGet<SystemConfig>("/super-admin/config"),
  updateConfig: (input: Partial<SystemConfig>) => saPut<object>("/super-admin/config", input),
  health: () => saGet<SystemHealth>("/super-admin/health"),
  cases: () => saGet<AdminCase[]>("/super-admin/cases"),
  revenueChart: (months = 12) => saGet<unknown[]>(`/super-admin/revenue-chart?months=${months}`),
  chamberGrowth: (months = 12) => saGet<unknown[]>(`/super-admin/chamber-growth?months=${months}`)
};
