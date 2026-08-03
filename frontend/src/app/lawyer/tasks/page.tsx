"use client";

import { useState } from "react";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useTasks } from "@/lib/hooks";
import { taskService, teamService, caseService } from "@/lib/services";
import { getErrorMessage, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { ListTodo, Plus, CheckCircle2, Clock, Trash2, Pencil, MapPin } from "lucide-react";
import type { Task } from "@/types/models";

const columns = [
  { key: "Pending", i18nKey: "tasks.pending", tone: "gold" as const },
  { key: "InProgress", i18nKey: "tasks.inProgress", tone: "amber" as const },
  { key: "Completed", i18nKey: "tasks.completed", tone: "green" as const },
  { key: "Cancelled", i18nKey: "common.no", tone: "slate" as const }
];

const priorities = ["Low", "Medium", "High", "Urgent"];

export default function TasksPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [createOpen, setCreateOpen] = useState(false);
  const [editTask, setEditTask] = useState<Task | null>(null);
  const [deleteTask, setDeleteTask] = useState<Task | null>(null);
  const { data: tasks, isLoading } = useTasks();

  const invalidate = () => qc.invalidateQueries({ queryKey: ["tasks"] });

  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: string }) =>
      taskService.update(id, { status }),
    onSuccess: () => invalidate(),
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => taskService.create(input),
    onSuccess: () => {
      invalidate();
      setCreateOpen(false);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: Record<string, unknown> }) =>
      taskService.update(id, input),
    onSuccess: () => {
      invalidate();
      setEditTask(null);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => taskService.remove(id),
    onSuccess: () => {
      invalidate();
      setDeleteTask(null);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title={t("tasks.title")}
        subtitle={t("tasks.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("tasks.newTask")}
          </Button>
        }
      />

      {isLoading ? (
        <Loading />
      ) : tasks && tasks.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
          {columns.map((col) => {
            const items = tasks.filter((tt) => tt.status === col.key);
            return (
              <div key={col.key} className="rounded-xl bg-slate-100/70 p-3">
                <div className="mb-3 flex items-center justify-between px-1">
                  <h3 className="flex items-center gap-2 text-sm font-semibold text-ink">
                    <span className="h-2 w-2 rounded-full bg-current" />
                    {t(col.i18nKey)}
                  </h3>
                  <span className="rounded-full bg-card px-2 py-0.5 text-xs font-medium text-ink-muted">{items.length}</span>
                </div>
                <div className="space-y-2">
                  {items.map((tt) => (
                    <TaskCard
                      key={tt.id}
                      task={tt}
                      onStatusChange={(status) => statusMutation.mutate({ id: tt.id, status })}
                      onEdit={() => setEditTask(tt)}
                      onDelete={() => setDeleteTask(tt)}
                    />
                  ))}
                  {items.length === 0 ? (
                    <p className="px-1 py-3 text-center text-xs text-ink-muted">{t("tasks.noTasks")}</p>
                  ) : null}
                </div>
              </div>
            );
          })}
        </div>
      ) : (
        <Card>
          <EmptyState
            icon={<ListTodo className="h-10 w-10" />}
            title={t("tasks.noTasks")}
            description={t("tasks.noTasksDesc")}
            action={
              <Button onClick={() => setCreateOpen(true)}>
                <Plus className="h-4 w-4" /> {t("tasks.newTask")}
              </Button>
            }
          />
        </Card>
      )}

      <TaskFormDialog
        open={createOpen}
        title={t("tasks.newTask")}
        submitLabel={t("common.create")}
        onClose={() => setCreateOpen(false)}
        onSubmit={(v) => createMutation.mutate({ ...v, status: "Pending" })}
      />

      <TaskFormDialog
        open={!!editTask}
        editing={editTask}
        title={t("common.edit")}
        submitLabel={t("common.save")}
        onClose={() => setEditTask(null)}
        onSubmit={(v) => editTask && updateMutation.mutate({ id: editTask.id, input: v })}
      />

      <Dialog
        open={!!deleteTask}
        onClose={() => setDeleteTask(null)}
        title={t("common.delete")}
        size="sm"
        footer={
          <>
            <Button variant="ghost" onClick={() => setDeleteTask(null)}>{t("common.cancel")}</Button>
            <Button variant="danger" onClick={() => deleteTask && deleteMutation.mutate(deleteTask.id)}>
              {t("common.delete")}
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">
          {t("documents.deleteConfirm")} <span className="font-medium text-ink">{deleteTask?.title}</span>
        </p>
      </Dialog>
    </div>
  );
}

function TaskCard({
  task,
  onStatusChange,
  onEdit,
  onDelete
}: {
  task: Task;
  onStatusChange: (status: string) => void;
  onEdit: () => void;
  onDelete: () => void;
}) {
  const { t } = useLanguage();
  const isOverdue = task.dueDate && task.status !== "Completed" && new Date(task.dueDate) < new Date();
  return (
    <Card className="group p-3">
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-medium text-ink">{task.title}</p>
        {isOverdue ? <Clock className="h-4 w-4 shrink-0 text-red-500" /> : null}
      </div>
      {task.caseTitle ? (
        <p className="mt-1 flex items-center gap-1 text-xs text-primary-700">
          <MapPin className="h-3 w-3" /> {task.caseTitle}
        </p>
      ) : null}
      {task.description ? <p className="mt-1 line-clamp-2 text-xs text-ink-muted">{task.description}</p> : null}
      <div className="mt-3 flex items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          {task.priority ? <StatusBadge value={task.priority} /> : null}
          <span className="text-xs text-ink-muted">
            {task.dueDate ? formatDate(task.dueDate) : ""}
          </span>
        </div>
        {task.status !== "Completed" ? (
          <button
            onClick={() => onStatusChange("Completed")}
            className="cursor-pointer rounded-lg p-1 text-ink-soft transition-colors hover:bg-emerald-50 hover:text-emerald-600"
            title={t("tasks.completed")}
          >
            <CheckCircle2 className="h-4 w-4" />
          </button>
        ) : null}
      </div>
      {task.assignedToName ? (
        <p className="mt-2 border-t border-line-soft pt-2 text-xs text-ink-muted">
          {t("tasks.assignedTo")} <span className="font-medium text-ink">{task.assignedToName}</span>
        </p>
      ) : null}
      <div className="mt-2 flex items-center justify-end gap-1 opacity-0 transition-opacity group-hover:opacity-100">
        <button onClick={onEdit} className="cursor-pointer rounded-lg p-1 text-ink-muted hover:bg-slate-100 hover:text-ink" aria-label={t("common.edit")}>
          <Pencil className="h-4 w-4" />
        </button>
        <button onClick={onDelete} className="cursor-pointer rounded-lg p-1 text-ink-muted hover:bg-red-50 hover:text-red-600" aria-label={t("common.delete")}>
          <Trash2 className="h-4 w-4" />
        </button>
      </div>
    </Card>
  );
}

function TaskFormDialog({
  open,
  onClose,
  onSubmit,
  editing,
  title,
  submitLabel
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
  editing?: Task | null;
  title: string;
  submitLabel: string;
}) {
  const { t } = useLanguage();
  const [form, setForm] = useState(() => ({
    title: editing?.title ?? "",
    description: editing?.description ?? "",
    dueDate: editing?.dueDate ? editing.dueDate.slice(0, 10) : "",
    priority: editing?.priority ?? "Medium",
    caseId: editing?.caseId ?? "",
    assignedTo: editing?.assignedTo ?? ""
  }));

  const { data: teamsData } = useQuery({
    queryKey: ["teams"],
    queryFn: () => teamService.list(),
    enabled: open
  });
  const members = teamsData?.flatMap((team) => team.members) ?? [];

  const { data: casesData } = useQuery({
    queryKey: ["cases"],
    queryFn: () => caseService.list({ pageSize: 100 }),
    enabled: open
  });
  const cases = casesData?.data ?? [];

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={title}
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>{t("common.cancel")}</Button>
          <Button
            disabled={!form.title}
            onClick={() =>
              onSubmit({
                title: form.title,
                description: form.description || null,
                dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
                priority: form.priority || null,
                caseId: form.caseId || null,
                assignedTo: form.assignedTo || null
              })
            }
          >
            {submitLabel}
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label={t("tasks.titleLabel")} required className="sm:col-span-2">
          <Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
        </Field>
        <Field label={t("tasks.description")} className="sm:col-span-2">
          <Textarea rows={3} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
        <Field label={t("tasks.dueDate")}>
          <Input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} />
        </Field>
        <Field label={t("tasks.priority")}>
          <Select value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })}>
            {priorities.map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </Select>
        </Field>
        <Field label={t("invoices.case")} className="sm:col-span-2">
          <Select value={form.caseId} onChange={(e) => setForm({ ...form, caseId: e.target.value })}>
            <option value="">{t("common.optional")}</option>
            {cases.map((c) => (
              <option key={c.id} value={c.id}>{c.caseNumber} — {c.title}</option>
            ))}
          </Select>
        </Field>
        <Field label={t("tasks.assignedTo")} className="sm:col-span-2">
          <Select value={form.assignedTo} onChange={(e) => setForm({ ...form, assignedTo: e.target.value })}>
            <option value="">{t("common.optional")}</option>
            {members.map((m) => (
              <option key={m.userId} value={m.userId}>{m.userName}</option>
            ))}
          </Select>
        </Field>
      </div>
    </Dialog>
  );
}