import { apiGet, apiPost, apiPut, apiDelete } from "@/lib/api";
import type { Client, CreateClientInput } from "@/types/models";
import type { PagedResponse } from "@/types/api";

export { caseService } from "./case-service";
export type { CaseQueryParams } from "./case-service";

export interface ClientQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
}

export const clientService = {
  list: (params: ClientQueryParams = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<PagedResponse<Client>>(`/clients?${qs.toString()}`);
  },
  get: (id: string) => apiGet<Client>(`/clients/${id}`),
  search: (q: string) => apiGet<Client[]>(`/clients/search?q=${encodeURIComponent(q)}`),
  create: (input: CreateClientInput) => apiPost<Client>("/clients", input),
  update: (id: string, input: Partial<CreateClientInput>) => apiPut<Client>(`/clients/${id}`, input),
  remove: (id: string) => apiDelete<object>(`/clients/${id}`),
  grantPortalAccess: (clientId: string, data: { fullName: string; email: string; password: string; phone?: string }) =>
    apiPost<object>(`/clients/${clientId}/portal-access`, data),
  revokePortalAccess: (clientId: string) => apiPost<object>(`/clients/${clientId}/revoke-portal`)
};

export const hearingService = {
  list: (params: { page?: number; pageSize?: number } = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<PagedResponse<import("@/types/models").Hearing>>(`/hearings?${qs.toString()}`);
  },
  upcoming: () => apiGet<import("@/types/models").Hearing[]>("/hearings/upcoming"),
  byCase: (caseId: string) => apiGet<import("@/types/models").Hearing[]>(`/hearings/by-case/${caseId}`),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").Hearing>("/hearings", input),
  update: (id: string, input: Record<string, unknown>) =>
    apiPut<import("@/types/models").Hearing>(`/hearings/${id}`, input),
  remove: (id: string) => apiDelete<object>(`/hearings/${id}`)
};

export const taskService = {
  list: (params: { status?: string; priority?: string; assignedTo?: string } = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<import("@/types/models").Task[]>(`/tasks?${qs.toString()}`);
  },
  my: () => apiGet<import("@/types/models").Task[]>("/tasks/my"),
  byCase: (caseId: string) => apiGet<import("@/types/models").Task[]>(`/tasks/by-case/${caseId}`),
  overdue: () => apiGet<import("@/types/models").Task[]>("/tasks/overdue"),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").Task>("/tasks", input),
  update: (id: string, input: Record<string, unknown>) =>
    apiPut<import("@/types/models").Task>(`/tasks/${id}`, input),
  remove: (id: string) => apiDelete<object>(`/tasks/${id}`),
  comments: (id: string) => apiGet<import("@/types/models").TaskComment[]>(`/tasks/${id}/comments`),
  addComment: (id: string, content: string) => apiPost<import("@/types/models").TaskComment>(`/tasks/${id}/comments`, { content })
};

export const documentService = {
  list: (params: { page?: number; pageSize?: number; category?: string; search?: string; caseId?: string } = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<import("@/types/models").Document[]>(`/documents?${qs.toString()}`);
  },
  byCase: (caseId: string) => apiGet<import("@/types/models").Document[]>(`/documents/by-case/${caseId}`),
  recent: (count = 10) => apiGet<import("@/types/models").Document[]>(`/documents/recent?count=${count}`),
  upload: (file: File, caseId: string, category: string, folderPath?: string) => {
    const form = new FormData();
    form.append("file", file);
    return apiPost<import("@/types/models").Document>(
      `/documents/upload?caseId=${caseId}&category=${encodeURIComponent(category)}${folderPath ? `&folderPath=${encodeURIComponent(folderPath)}` : ""}`,
      form
    );
  },
  remove: (id: string) => apiDelete<object>(`/documents/${id}`),
  get: (id: string) => apiGet<import("@/types/models").Document>(`/documents/${id}`)
};

export const invoiceService = {
  list: (status?: string) => {
    const qs = status ? `?status=${encodeURIComponent(status)}` : "";
    return apiGet<import("@/types/models").Invoice[]>(`/invoices${qs}`);
  },
  byClient: (clientId: string) => apiGet<import("@/types/models").Invoice[]>(`/invoices/by-client/${clientId}`),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").Invoice>("/invoices", input),
  markPaid: (id: string) => apiPost<object>(`/invoices/${id}/mark-paid`)
};

export const expenseService = {
  list: (params: { page?: number; pageSize?: number; category?: string } = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<PagedResponse<import("@/types/models").Expense>>(`/expenses?${qs.toString()}`);
  },
  total: () => apiGet<number>("/expenses/total"),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").Expense>("/expenses", input)
};

export const templateService = {
  list: (category?: string) => {
    const qs = category ? `?category=${encodeURIComponent(category)}` : "";
    return apiGet<import("@/types/models").Template[]>(`/templates${qs}`);
  },
  get: (id: string) => apiGet<import("@/types/models").Template>(`/templates/${id}`),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").Template>("/templates", input),
  render: (id: string, variables: Record<string, string>) =>
    apiPost<string>(`/templates/${id}/render`, variables)
};

export const reminderService = {
  list: (params: { status?: string; type?: string } = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<import("@/types/models").Reminder[]>(`/reminders?${qs.toString()}`);
  },
  my: (status?: string) => {
    const qs = status ? `?status=${encodeURIComponent(status)}` : "";
    return apiGet<import("@/types/models").Reminder[]>(`/reminders/my${qs}`);
  },
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").Reminder>("/reminders", input),
  updateStatus: (id: string, status: string) => apiPost<import("@/types/models").Reminder>(`/reminders/${id}/status`, { status }),
  remove: (id: string) => apiDelete<string>(`/reminders/${id}`)
};

export const legalDocumentService = {
  list: (params: { page?: number; pageSize?: number } = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<PagedResponse<import("@/types/models").LegalDocument>>(`/legal-documents?${qs.toString()}`);
  },
  search: (q: string) => apiGet<import("@/types/models").LegalDocument[]>(`/legal-documents/search?q=${encodeURIComponent(q)}`),
  byCategory: (category: string) =>
    apiGet<import("@/types/models").LegalDocument[]>(`/legal-documents/by-category/${encodeURIComponent(category)}`)
};

export const legalSectionService = {
  list: (params: { category?: string; search?: string } = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<import("@/types/models").LegalSection[]>(`/legal-sections?${qs.toString()}`);
  },
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").LegalSection>("/legal-sections", input),
  update: (id: string, input: Record<string, unknown>) =>
    apiPut<import("@/types/models").LegalSection>(`/legal-sections/${id}`, input),
  remove: (id: string) => apiDelete<object>(`/legal-sections/${id}`),
  procedures: (sectionId: string) =>
    apiGet<import("@/types/models").LegalProcedure[]>(`/legal-sections/${sectionId}/procedures`),
  createProcedure: (sectionId: string, input: Record<string, unknown>) =>
    apiPost<import("@/types/models").LegalProcedure>(`/legal-sections/${sectionId}/procedures`, input)
};

export const workflowService = {
  list: () => apiGet<import("@/types/models").WorkflowTemplate[]>("/workflow/templates"),
  get: (id: string) => apiGet<import("@/types/models").WorkflowTemplate>(`/workflow/templates/${id}`),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").WorkflowTemplate>("/workflow/templates", input),
  update: (id: string, input: Record<string, unknown>) =>
    apiPut<import("@/types/models").WorkflowTemplate>(`/workflow/templates/${id}`, input),
  remove: (id: string) => apiDelete<object>(`/workflow/templates/${id}`)
};

export const notificationService = {
  list: (unreadOnly = false) => apiGet<import("@/types/models").Notification[]>(`/notifications?unreadOnly=${unreadOnly}`),
  unreadCount: () => apiGet<number>("/notifications/unread-count"),
  markRead: (id: string) => apiPut<object>(`/notifications/${id}/read`),
  markAllRead: () => apiPut<object>("/notifications/read-all")
};

export const subscriptionService = {
  my: () => apiGet<import("@/types/models").Subscription>("/subscription/my"),
  changePlan: (plan: string) => apiPut<import("@/types/models").Subscription>("/subscription/change-plan", { plan }),
  cancel: () => apiPost<object>("/subscription/cancel")
};

export const searchService = {
  all: (q: string, limit = 10) =>
    apiGet<import("@/types/models").SearchResponse>(`/search?q=${encodeURIComponent(q)}&limit=${limit}`)
};

export const messageService = {
  conversation: (userId: string) => apiGet<import("@/types/models").Message[]>(`/messages/conversation/${userId}`),
  clientConversation: (clientId: string) => apiGet<import("@/types/models").Message[]>(`/messages/client/${clientId}`),
  send: (input: { receiverId: string; content: string; caseId?: string | null }) =>
    apiPost<import("@/types/models").Message>("/messages", input),
  unreadCount: () => apiGet<number>("/messages/unread-count")
};

export const configurationService = {
  getAll: () => apiGet<import("@/types/models").ChamberSettings>("/configuration"),
  getSubsection: (subsection: string) => apiGet<Record<string, unknown>>(`/configuration/${subsection}`),
  update: (settings: Record<string, unknown>) => apiPut<Record<string, unknown>>("/configuration", settings),
  updateSubsection: (subsection: string, data: Record<string, unknown>) =>
    apiPut<Record<string, unknown>>(`/configuration/${subsection}`, data)
};

export const teamService = {
  list: () => apiGet<import("@/types/models").Team[]>("/teams"),
  get: (id: string) => apiGet<import("@/types/models").Team>(`/teams/${id}`),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").Team>("/teams", input),
  update: (id: string, input: Record<string, unknown>) => apiPut<import("@/types/models").Team>(`/teams/${id}`, input),
  remove: (id: string) => apiDelete<object>(`/teams/${id}`),
  addMember: (id: string, input: Record<string, unknown>) => apiPost<import("@/types/models").TeamMember>(`/teams/${id}/members`, input)
};

export const accountingService = {
  dashboard: () => apiGet<Record<string, unknown>>("/accounting/dashboard"),
  profitLoss: (from?: string, to?: string) => {
    const qs = new URLSearchParams();
    if (from) qs.set("from", from);
    if (to) qs.set("to", to);
    return apiGet<Record<string, unknown>>(`/accounting/profit-loss?${qs.toString()}`);
  },
  monthlyReport: (year?: number) => apiGet<Record<string, unknown>>(`/accounting/reports/monthly${year ? `?year=${year}` : ""}`),
  balanceSheet: () => apiGet<Record<string, unknown>>("/accounting/reports/balance-sheet"),
  journals: (params: { page?: number; pageSize?: number } = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<Record<string, unknown>>(`/accounting/journals?${qs.toString()}`);
  },
  createJournal: (input: Record<string, unknown>) => apiPost<import("@/types/models").Journal>("/accounting/journals", input),
  charts: () => apiGet<import("@/types/models").Account[]>("/accounting/charts"),
  createAccount: (input: Record<string, unknown>) => apiPost<import("@/types/models").Account>("/accounting/charts", input)
};

export const payrollService = {
  employees: () => apiGet<import("@/types/models").Employee[]>("/payroll/employees"),
  createEmployee: (input: Record<string, unknown>) => apiPost<import("@/types/models").Employee>("/payroll/employees", input),
  payrolls: (month?: number, year?: number) => {
    const qs = new URLSearchParams();
    if (month) qs.set("month", String(month));
    if (year) qs.set("year", String(year));
    return apiGet<import("@/types/models").Payroll[]>(`/payroll/payrolls?${qs.toString()}`);
  },
  createPayroll: (input: Record<string, unknown>) => apiPost<import("@/types/models").Payroll>("/payroll/payrolls", input),
  approvePayroll: (id: string) => apiPost<import("@/types/models").Payroll>(`/payroll/payrolls/${id}/approve`),
  payPayroll: (id: string) => apiPost<import("@/types/models").Payroll>(`/payroll/payrolls/${id}/pay`),
  attendance: (from: string, to: string) => apiGet<import("@/types/models").Attendance[]>(`/payroll/attendance?from=${from}&to=${to}`)
};

export const bankingService = {
  accounts: () => apiGet<import("@/types/models").BankAccount[]>("/banking/accounts"),
  createAccount: (input: Record<string, unknown>) => apiPost<import("@/types/models").BankAccount>("/banking/accounts", input),
  transactions: (accountId: string, page = 1, pageSize = 20) =>
    apiGet<import("@/types/models").BankTransaction[]>(`/banking/accounts/${accountId}/transactions?page=${page}&pageSize=${pageSize}`),
  createTransaction: (input: Record<string, unknown>) => apiPost<import("@/types/models").BankTransaction>("/banking/transactions", input),
  reconcileTransaction: (id: string) => apiPost<import("@/types/models").BankTransaction>(`/banking/transactions/${id}/reconcile`)
};

export const budgetService = {
  list: (fiscalYear?: number) => apiGet<import("@/types/models").Budget[]>(`/budget${fiscalYear ? `?fiscalYear=${fiscalYear}` : ""}`),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").Budget>("/budget", input),
  approve: (id: string) => apiPost<import("@/types/models").Budget>(`/budget/${id}/approve`)
};

export const fixedAssetService = {
  list: () => apiGet<import("@/types/models").FixedAsset[]>("/fixed-assets"),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").FixedAsset>("/fixed-assets", input),
  dispose: (id: string, input: { disposalDate: string; reason: string }) =>
    apiPost<import("@/types/models").FixedAsset>(`/fixed-assets/${id}/dispose`, input)
};

export const taxService = {
  settings: () => apiGet<import("@/types/models").TaxSetting[]>("/tax/settings"),
  createSetting: (input: Record<string, unknown>) => apiPost<import("@/types/models").TaxSetting>("/tax/settings", input),
  transactions: (year: number) => apiGet<import("@/types/models").TaxTransaction[]>(`/tax/transactions?year=${year}`),
  createTransaction: (input: Record<string, unknown>) => apiPost<import("@/types/models").TaxTransaction>("/tax/transactions", input)
};

export const auditService = {
  logs: (params: { page?: number; pageSize?: number; search?: string; entity?: string; action?: string } = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<Record<string, unknown>>(`/audit/logs?${qs.toString()}`);
  },
  summary: () => apiGet<Record<string, unknown>>("/audit/summary")
};

export const bailService = {
  list: (status?: string) => apiGet<import("@/types/models").Bail[]>(`/bails${status ? `?status=${status}` : ""}`),
  byCase: (caseId: string) => apiGet<import("@/types/models").Bail | null>(`/bails/by-case/${caseId}`),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").Bail>("/bails", input),
  updateStatus: (id: string, input: { status: string; revokedReason?: string }) =>
    apiPost<import("@/types/models").Bail>(`/bails/${id}/status`, input)
};

export const leadService = {
  list: () => apiGet<import("@/types/models").Lead[]>("/leads"),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").Lead>("/leads", input),
  updateStage: (id: string, stage: string, lostReason?: string) =>
    apiPost<import("@/types/models").Lead>(`/leads/${id}/stage`, { stage, lostReason }),
  analytics: () => apiGet<Record<string, unknown>>("/leads/analytics"),
  remove: (id: string) => apiDelete<string>(`/leads/${id}`)
};

export const timeEntryService = {
  list: (params: { status?: string; from?: string; to?: string } = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGet<import("@/types/models").TimeEntry[]>(`/time-entries?${qs.toString()}`);
  },
  running: () => apiGet<import("@/types/models").TimeEntry | null>("/time-entries/running"),
  create: (input: Record<string, unknown>) => apiPost<import("@/types/models").TimeEntry>("/time-entries", input),
  stop: (id: string) => apiPost<import("@/types/models").TimeEntry>(`/time-entries/${id}/stop`),
  update: (id: string, input: Record<string, unknown>) => apiPut<import("@/types/models").TimeEntry>(`/time-entries/${id}`, input)
};

export const adminService = {
  users: (search?: string) => apiGet<import("@/types/models").AdminUser[]>(`/admin/users${search ? `?search=${encodeURIComponent(search)}` : ""}`),
  createUser: (input: Record<string, unknown>) => apiPost<import("@/types/models").AdminUser>("/admin/users", input),
  toggleStatus: (id: string) => apiPost<object>(`/admin/users/${id}/status`),
  systemStats: () => apiGet<Record<string, unknown>>("/admin/system-stats"),
  revenue: (months = 6) => apiGet<unknown[]>(`/admin/revenue?months=${months}`)
};

export const dashboardService = {
  stats: () => apiGet<import("@/types/models").DashboardStats>("/dashboard/stats"),
  caseChart: (months = 12) => apiGet<import("@/types/models").CaseChartPoint[]>(`/dashboard/case-chart?months=${months}`),
  recentActivities: (count = 10) => apiGet<import("@/types/models").RecentActivity[]>(`/dashboard/recent-activities?count=${count}`),
  lawyerProductivity: () => apiGet<import("@/types/models").LawyerProductivity[]>("/dashboard/lawyer-productivity")
};

export const clientPortalService = {
  dashboard: () => apiGet<import("@/types/models").ClientDashboard>("/client-portal/dashboard"),
  profile: () => apiGet<import("@/types/models").ClientProfile>("/client-portal/profile"),
  cases: () => apiGet<import("@/types/models").ClientCaseSummary[]>("/client-portal/cases"),
  caseDetail: (id: string) => apiGet<import("@/types/models").ClientCaseDetail>(`/client-portal/cases/${id}`),
  hearings: () => apiGet<import("@/types/models").ClientHearing[]>("/client-portal/hearings"),
  documents: () => apiGet<import("@/types/models").ClientDocument[]>("/client-portal/documents"),
  invoices: () => apiGet<import("@/types/models").ClientInvoice[]>("/client-portal/invoices"),
  tasks: () => apiGet<import("@/types/models").ClientTask[]>("/client-portal/tasks"),
  messages: () => apiGet<import("@/types/models").Message[]>("/client-portal/messages"),
  sendMessage: (input: { receiverId: string; content: string; caseId?: string | null }) =>
    apiPost<import("@/types/models").Message>("/client-portal/messages", input),
  unreadCount: () => apiGet<number>("/client-portal/messages/unread-count")
};
