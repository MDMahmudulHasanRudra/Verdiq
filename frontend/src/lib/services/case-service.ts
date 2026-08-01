import { apiGet, apiGetFull, apiPost, apiPut, apiDelete } from "@/lib/api";
import type { Case, CreateCaseInput, UpdateCaseInput, CaseActivity, ConfirmCaseDeleteInput } from "@/types/models";
import type { PagedResponse } from "@/types/api";

export interface CaseQueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  priority?: string;
  type?: string;
  courtName?: string;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  assignedLawyerId?: string;
  dateFrom?: string;
  dateTo?: string;
}

export const caseService = {
  list: (params: CaseQueryParams = {}) => {
    const qs = new URLSearchParams();
    Object.entries(params).forEach(([k, v]) => {
      if (v !== undefined && v !== null && String(v) !== "") qs.set(k, String(v));
    });
    return apiGetFull<PagedResponse<Case>>(`/cases?${qs.toString()}`);
  },
  get: (id: string) => apiGet<Case>(`/cases/${id}`),
  search: (q: string) => apiGet<Case[]>(`/cases/search?q=${encodeURIComponent(q)}`),
  create: (input: CreateCaseInput) => apiPost<Case>("/cases", input),
  update: (id: string, input: UpdateCaseInput) => apiPut<Case>(`/cases/${id}`, input),
  remove: (id: string, confirm: ConfirmCaseDeleteInput) => apiDelete<object>(`/cases/${id}`, confirm),
  activities: (id: string) => apiGet<CaseActivity[]>(`/cases/${id}/activities`),
  procedures: (id: string) => apiGet<unknown[]>(`/cases/${id}/procedures`),
  generateProcedures: (caseId: string, legalSectionId: string) =>
    apiPost<object>(`/cases/${caseId}/procedures/generate/${legalSectionId}`),
  completeProcedure: (caseId: string, procedureId: string) =>
    apiPost<object>(`/cases/${caseId}/procedures/${procedureId}/complete`)
};
