"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useToast } from "@/components/ui/toast";
import { getErrorMessage } from "@/lib/utils";
import {
  caseService,
  clientService,
  dashboardService,
  documentService,
  hearingService,
  invoiceService,
  leadService,
  notificationService,
  taskService,
  timeEntryService
} from "@/lib/services";

export function useDashboardStats() {
  return useQuery({ queryKey: ["dashboard", "stats"], queryFn: () => dashboardService.stats() });
}

export function useCaseChart(months = 12) {
  return useQuery({ queryKey: ["dashboard", "caseChart", months], queryFn: () => dashboardService.caseChart(months) });
}

export function useRecentActivities(count = 10) {
  return useQuery({
    queryKey: ["dashboard", "activities", count],
    queryFn: () => dashboardService.recentActivities(count)
  });
}

export function useLawyerProductivity() {
  return useQuery({ queryKey: ["dashboard", "productivity"], queryFn: () => dashboardService.lawyerProductivity() });
}

export function useCases(
  params: {
    page?: number;
    pageSize?: number;
    status?: string;
    priority?: string;
    search?: string;
    assignedToMe?: boolean;
    type?: string;
    courtName?: string;
    sortBy?: string;
    sortOrder?: "asc" | "desc";
    dateFrom?: string;
    dateTo?: string;
  } = {}
) {
  return useQuery({
    queryKey: ["cases", params],
    queryFn: () =>
      caseService.list({
        page: params.page,
        pageSize: params.pageSize,
        status: params.status,
        priority: params.priority,
        search: params.search,
        assignedLawyerId: params.assignedToMe ? "me" : undefined,
        type: params.type,
        courtName: params.courtName,
        sortBy: params.sortBy,
        sortOrder: params.sortOrder,
        dateFrom: params.dateFrom,
        dateTo: params.dateTo
      })
  });
}

export function useCase(id: string) {
  return useQuery({ queryKey: ["case", id], queryFn: () => caseService.get(id), enabled: !!id });
}

export function useCaseActivities(id: string) {
  return useQuery({
    queryKey: ["case", id, "activities"],
    queryFn: () => caseService.activities(id),
    enabled: !!id
  });
}

export function useCaseProcedures(id: string) {
  return useQuery({
    queryKey: ["case", id, "procedures"],
    queryFn: () => caseService.procedures(id),
    enabled: !!id
  });
}

export function useClients(params: { page?: number; pageSize?: number; search?: string; status?: string; clientType?: string } = {}) {
  return useQuery({
    queryKey: ["clients", params],
    queryFn: () => clientService.list(params)
  });
}

export function useHearings() {
  return useQuery({ queryKey: ["hearings"], queryFn: () => hearingService.list() });
}

export function useUpcomingHearings() {
  return useQuery({ queryKey: ["hearings", "upcoming"], queryFn: () => hearingService.upcoming() });
}

export function useDocuments(params: { page?: number; pageSize?: number; category?: string; search?: string } = {}) {
  return useQuery({
    queryKey: ["documents", params],
    queryFn: () => documentService.list(params)
  });
}

export function useTasks(params: { status?: string; priority?: string; assignedTo?: string } = {}) {
  return useQuery({ queryKey: ["tasks", params], queryFn: () => taskService.list(params) });
}

export function useMyTasks() {
  return useQuery({ queryKey: ["tasks", "mine"], queryFn: () => taskService.my() });
}

export function useInvoices(status?: string) {
  return useQuery({ queryKey: ["invoices", status], queryFn: () => invoiceService.list(status) });
}

export function useLeads() {
  return useQuery({ queryKey: ["leads"], queryFn: () => leadService.list() });
}

export function useRunningTimer() {
  return useQuery({ queryKey: ["time-entries", "running"], queryFn: () => timeEntryService.running() });
}

export function useUnreadNotifications() {
  return useQuery({
    queryKey: ["notifications", "unread-count"],
    queryFn: () => notificationService.unreadCount(),
    refetchInterval: 60000
  });
}

export function useStartTimer() {
  const qc = useQueryClient();
  const toast = useToast();
  return useMutation({
    mutationFn: (input: { caseId?: string | null; description: string; billable: boolean; hourlyRate?: number }) =>
      timeEntryService.create(input),
    onSuccess: (data) => {
      qc.invalidateQueries({ queryKey: ["time-entries"] });
      toast.success("Timer started");
      return data;
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });
}

export function useStopTimer() {
  const qc = useQueryClient();
  const toast = useToast();
  return useMutation({
    mutationFn: (id: string) => timeEntryService.stop(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["time-entries"] });
      toast.success("Timer stopped");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });
}

export function useMarkNotificationRead() {
  const qc = useQueryClient();
  const toast = useToast();
  return useMutation({
    mutationFn: (id: string) => notificationService.markRead(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["notifications"] });
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });
}
