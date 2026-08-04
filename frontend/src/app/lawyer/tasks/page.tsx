"use client";

import { useState, useRef, useCallback } from "react";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card } from "@/components/ui/card";
import { Badge, StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useTasks } from "@/lib/hooks";
import { taskService, teamService, caseService } from "@/lib/services";
import { getErrorMessage, formatDate, formatDateTime } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import {
  ListTodo, Plus, CheckCircle2, Clock, Trash2, Pencil, MapPin,
  MessageSquare, Paperclip, Eye, Timer, Play, Pause, Send,
  ChevronDown, AlertCircle, Users, MoreHorizontal
} from "lucide-react";
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
  const [detailTask, setDetailTask] = useState<Task | null>(null);
  const [deleteTask, setDeleteTask] = useState<Task | null>(null);
  const [statusFilter, setStatusFilter] = useState("");
  const [priorityFilter, setPriorityFilter] = useState("");
  const [assigneeFilter, setAssigneeFilter] = useState("");
  const { data: tasks, isLoading } = useTasks({
    status: statusFilter || undefined,
    priority: priorityFilter || undefined,
    assignedTo: assigneeFilter || undefined
  });

  const { data: membersData } = useQuery({
    queryKey: ["teams"],
    queryFn: () => teamService.list()
  });
  const members = membersData?.flatMap((t) => t.members) ?? [];

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
      setDetailTask(null);
      toast.success(t("common.success"));
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const reorderMutation = useMutation({
    mutationFn: (items: { id: string; sortOrder: number; status?: string }[]) =>
      taskService.reorder(items),
    onSuccess: () => invalidate(),
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const handleDragStart = (e: React.DragEvent, taskId: string) => {
    e.dataTransfer.setData("text/plain", taskId);
    e.dataTransfer.effectAllowed = "move";
  };

  const handleDrop = (e: React.DragEvent, targetStatus: string) => {
    e.preventDefault();
    const taskId = e.dataTransfer.getData("text/plain");
    if (!taskId) return;
    const task = tasks?.find((tt) => tt.id === taskId);
    if (!task || task.status === targetStatus) return;
    statusMutation.mutate({ id: taskId, status: targetStatus });
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "move";
  };

  const overdueTasks = tasks?.filter((tt) =>
    tt.dueDate && tt.status !== "Completed" && tt.status !== "Cancelled" && new Date(tt.dueDate) < new Date()
  ) ?? [];

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

      {overdueTasks.length > 0 ? (
        <Card className="mb-4 border-amber-200 bg-amber-50/50 p-3">
          <div className="flex items-center gap-2">
            <AlertCircle className="h-4 w-4 text-amber-600" />
            <p className="text-sm font-medium text-amber-800">
              {overdueTasks.length} overdue task{overdueTasks.length > 1 ? "s" : ""}
            </p>
          </div>
        </Card>
      ) : null}

      <Card className="mb-4">
        <div className="flex flex-wrap items-center gap-3 p-4">
          <Select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="h-9 w-36">
            <option value="">All statuses</option>
            {columns.map((c) => (
              <option key={c.key} value={c.key}>{c.key === "InProgress" ? "In Progress" : c.key}</option>
            ))}
          </Select>
          <Select value={priorityFilter} onChange={(e) => setPriorityFilter(e.target.value)} className="h-9 w-36">
            <option value="">All priorities</option>
            {priorities.map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </Select>
          <Select value={assigneeFilter} onChange={(e) => setAssigneeFilter(e.target.value)} className="h-9 w-44">
            <option value="">All assignees</option>
            {members.map((m) => (
              <option key={m.userId} value={m.userId}>{m.userName}</option>
            ))}
          </Select>
          <div className="ml-auto text-xs text-ink-muted">
            {tasks?.length ?? 0} task{(tasks?.length ?? 0) !== 1 ? "s" : ""}
          </div>
        </div>
      </Card>

      {isLoading ? (
        <Loading />
      ) : tasks && tasks.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
          {columns.map((col) => {
            const items = tasks.filter((tt) => tt.status === col.key);
            return (
              <div
                key={col.key}
                className="rounded-xl bg-slate-100/70 p-3"
                onDrop={(e) => handleDrop(e, col.key)}
                onDragOver={handleDragOver}
              >
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
                      onClick={() => setDetailTask(tt)}
                      onDragStart={handleDragStart}
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

      {detailTask && (
        <TaskDetailDialog
          task={detailTask}
          open={!!detailTask}
          onClose={() => setDetailTask(null)}
          onEdit={() => { setEditTask(detailTask); setDetailTask(null); }}
          onDelete={() => { setDeleteTask(detailTask); setDetailTask(null); }}
          onStatusChange={(status) => { statusMutation.mutate({ id: detailTask.id, status }); setDetailTask(null); }}
          members={members}
        />
      )}
    </div>
  );
}

function TaskCard({
  task,
  onStatusChange,
  onEdit,
  onDelete,
  onClick,
  onDragStart
}: {
  task: Task;
  onStatusChange: (status: string) => void;
  onEdit: () => void;
  onDelete: () => void;
  onClick: () => void;
  onDragStart: (e: React.DragEvent, taskId: string) => void;
}) {
  const { t } = useLanguage();
  const isOverdue = task.dueDate && task.status !== "Completed" && task.status !== "Cancelled" && new Date(task.dueDate) < new Date();
  return (
    <Card
      className={`group cursor-pointer p-3 ${isOverdue ? "border-amber-300 bg-amber-50/30" : ""}`}
      draggable
      onDragStart={(e) => onDragStart(e, task.id)}
      onClick={onClick}
    >
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
        <div className="flex items-center gap-1">
          {task.commentCount > 0 ? (
            <span className="flex items-center gap-0.5 text-xs text-ink-muted">
              <MessageSquare className="h-3 w-3" /> {task.commentCount}
            </span>
          ) : null}
          {task.attachmentCount > 0 ? (
            <span className="flex items-center gap-0.5 text-xs text-ink-muted">
              <Paperclip className="h-3 w-3" /> {task.attachmentCount}
            </span>
          ) : null}
          {task.status !== "Completed" ? (
            <button
              onClick={(e) => { e.stopPropagation(); onStatusChange("Completed"); }}
              className="cursor-pointer rounded-lg p-1 text-ink-soft transition-colors hover:bg-emerald-50 hover:text-emerald-600"
              title={t("tasks.completed")}
            >
              <CheckCircle2 className="h-4 w-4" />
            </button>
          ) : null}
        </div>
      </div>
      {task.assignedToName ? (
        <p className="mt-2 border-t border-line-soft pt-2 text-xs text-ink-muted">
          {t("tasks.assignedTo")} <span className="font-medium text-ink">{task.assignedToName}</span>
        </p>
      ) : null}
      <div className="mt-2 flex items-center justify-end gap-1 opacity-0 transition-opacity group-hover:opacity-100">
        <button onClick={(e) => { e.stopPropagation(); onEdit(); }} className="cursor-pointer rounded-lg p-1 text-ink-muted hover:bg-slate-100 hover:text-ink" aria-label={t("common.edit")}>
          <Pencil className="h-4 w-4" />
        </button>
        <button onClick={(e) => { e.stopPropagation(); onDelete(); }} className="cursor-pointer rounded-lg p-1 text-ink-muted hover:bg-red-50 hover:text-red-600" aria-label={t("common.delete")}>
          <Trash2 className="h-4 w-4" />
        </button>
      </div>
    </Card>
  );
}

function TaskDetailDialog({
  task,
  open,
  onClose,
  onEdit,
  onDelete,
  onStatusChange,
  members
}: {
  task: Task;
  open: boolean;
  onClose: () => void;
  onEdit: () => void;
  onDelete: () => void;
  onStatusChange: (status: string) => void;
  members: { userId: string; userName: string }[];
}) {
  const toast = useToast();
  const qc = useQueryClient();
  const [commentText, setCommentText] = useState("");
  const [timerRunning, setTimerRunning] = useState(false);
  const [timerSeconds, setTimerSeconds] = useState(0);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const { data: fullTask, refetch: refetchTask } = useQuery({
    queryKey: ["task", task.id],
    queryFn: () => taskService.get(task.id),
    enabled: open
  });

  const { data: comments } = useQuery({
    queryKey: ["task-comments", task.id],
    queryFn: () => taskService.comments(task.id),
    enabled: open
  });

  const addCommentMutation = useMutation({
    mutationFn: (content: string) => taskService.addComment(task.id, content),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["task-comments", task.id] });
      setCommentText("");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const toggleWatcherMutation = useMutation({
    mutationFn: () => taskService.toggleWatcher(task.id),
    onSuccess: () => {
      refetchTask();
      qc.invalidateQueries({ queryKey: ["tasks"] });
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const startTimerMutation = useMutation({
    mutationFn: () => taskService.startTimer(task.id),
    onSuccess: () => {
      setTimerRunning(true);
      setTimerSeconds(0);
      timerRef.current = setInterval(() => setTimerSeconds((s) => s + 1), 1000);
      refetchTask();
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const stopTimerMutation = useMutation({
    mutationFn: (minutes: number) => taskService.stopTimer(task.id, minutes),
    onSuccess: () => {
      setTimerRunning(false);
      if (timerRef.current) clearInterval(timerRef.current);
      setTimerSeconds(0);
      refetchTask();
      qc.invalidateQueries({ queryKey: ["tasks"] });
      toast.success("Time logged");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const isOverdue = task.dueDate && task.status !== "Completed" && task.status !== "Cancelled" && new Date(task.dueDate) < new Date();
  const isWatching = fullTask?.watcherIds?.includes(task.id) ?? false;

  const formatTimer = (seconds: number) => {
    const h = Math.floor(seconds / 3600);
    const m = Math.floor((seconds % 3600) / 60);
    const s = seconds % 60;
    return `${h.toString().padStart(2, "0")}:${m.toString().padStart(2, "0")}:${s.toString().padStart(2, "0")}`;
  };

  const nextStatuses: Record<string, string[]> = {
    Pending: ["InProgress", "Cancelled"],
    InProgress: ["Completed", "Cancelled"],
    Completed: [],
    Cancelled: ["Pending"]
  };

  return (
    <Dialog open={open} onClose={onClose} title="Task Details" size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Close</Button>
          <Button variant="outline" onClick={onEdit}><Pencil className="h-4 w-4" /> Edit</Button>
          <Button variant="danger" onClick={onDelete}><Trash2 className="h-4 w-4" /> Delete</Button>
        </>
      }
    >
      <div className="space-y-6">
        <div className="flex items-start justify-between">
          <div>
            <h3 className="text-lg font-semibold text-ink">{task.title}</h3>
            <div className="mt-1 flex items-center gap-2">
              <StatusBadge value={task.status} />
              {task.priority ? <StatusBadge value={task.priority} /> : null}
              {isOverdue ? <Badge tone="red">Overdue</Badge> : null}
            </div>
          </div>
          {timerRunning ? (
            <div className="flex items-center gap-2 rounded-lg bg-emerald-50 px-3 py-2">
              <Timer className="h-4 w-4 text-emerald-600" />
              <span className="font-mono text-lg font-bold text-emerald-700">{formatTimer(timerSeconds)}</span>
              <Button size="sm" variant="danger" onClick={() => stopTimerMutation.mutate(Math.round(timerSeconds / 60))}>
                <Pause className="h-3.5 w-3.5" /> Stop
              </Button>
            </div>
          ) : null}
        </div>

        {task.description ? (
          <div>
            <h4 className="mb-1 text-sm font-semibold text-ink">Description</h4>
            <p className="whitespace-pre-wrap rounded-lg bg-slate-50 p-3 text-sm text-ink-muted">{task.description}</p>
          </div>
        ) : null}

        <div className="grid grid-cols-2 gap-4">
          <div>
            <p className="text-xs text-ink-muted">Assigned to</p>
            <p className="text-sm font-medium text-ink">{task.assignedToName ?? "—"}</p>
          </div>
          <div>
            <p className="text-xs text-ink-muted">Due date</p>
            <p className={`text-sm font-medium ${isOverdue ? "text-red-600" : "text-ink"}`}>
              {task.dueDate ? formatDate(task.dueDate) : "—"}
            </p>
          </div>
          <div>
            <p className="text-xs text-ink-muted">Case</p>
            <p className="text-sm font-medium text-primary-700">{task.caseTitle ?? "—"}</p>
          </div>
          <div>
            <p className="text-xs text-ink-muted">Created</p>
            <p className="text-sm font-medium text-ink">{formatDate(task.createdAt)}</p>
          </div>
          {(task.estimatedHours ?? task.actualHours) ? (
            <div>
              <p className="text-xs text-ink-muted">Time tracking</p>
              <p className="text-sm font-medium text-ink">
                {task.actualHours ? `${task.actualHours}h logged` : ""}
                {task.estimatedHours ? ` / ${task.estimatedHours}h estimated` : ""}
              </p>
            </div>
          ) : null}
        </div>

        <div className="flex flex-wrap gap-2">
          {nextStatuses[task.status]?.map((s) => (
            <Button key={s} size="sm" variant="outline" onClick={() => onStatusChange(s)}>
              Move to {s === "InProgress" ? "In Progress" : s}
            </Button>
          ))}
          {!timerRunning && task.status !== "Completed" && task.status !== "Cancelled" ? (
            <Button size="sm" variant="outline" onClick={() => startTimerMutation.mutate()}>
              <Play className="h-3.5 w-3.5" /> Start Timer
            </Button>
          ) : null}
          <Button size="sm" variant={isWatching ? "subtle" : "outline"} onClick={() => toggleWatcherMutation.mutate()}>
            <Eye className="h-3.5 w-3.5" /> {isWatching ? "Unwatch" : "Watch"}
          </Button>
        </div>

        {fullTask?.watcherIds && fullTask.watcherIds.length > 0 ? (
          <div>
            <h4 className="mb-1 flex items-center gap-1.5 text-sm font-semibold text-ink">
              <Users className="h-4 w-4" /> Watchers ({fullTask.watcherIds.length})
            </h4>
            <div className="flex flex-wrap gap-1">
              {fullTask.watcherIds.map((wid) => {
                const member = members.find((m) => m.userId === wid);
                return <Badge key={wid} tone="slate">{member?.userName ?? "Unknown"}</Badge>;
              })}
            </div>
          </div>
        ) : null}

        <div>
          <h4 className="mb-2 flex items-center gap-1.5 text-sm font-semibold text-ink">
            <MessageSquare className="h-4 w-4" /> Comments ({comments?.length ?? 0})
          </h4>
          <div className="space-y-3">
            {comments && comments.length > 0 ? (
              comments.map((c) => (
                <div key={c.id} className="rounded-lg bg-slate-50 p-3">
                  <div className="flex items-center justify-between">
                    <p className="text-sm font-medium text-ink">{c.userName}</p>
                    <p className="text-xs text-ink-muted">{formatDate(c.createdAt)}</p>
                  </div>
                  <p className="mt-1 text-sm text-ink-muted">{c.content}</p>
                </div>
              ))
            ) : (
              <p className="text-xs text-ink-muted">No comments yet.</p>
            )}
          </div>
          <div className="mt-3 flex gap-2">
            <Input
              placeholder="Add a comment..."
              value={commentText}
              onChange={(e) => setCommentText(e.target.value)}
              onKeyDown={(e) => { if (e.key === "Enter" && commentText.trim()) addCommentMutation.mutate(commentText.trim()); }}
            />
            <Button
              size="sm"
              disabled={!commentText.trim()}
              onClick={() => addCommentMutation.mutate(commentText.trim())}
            >
              <Send className="h-4 w-4" />
            </Button>
          </div>
        </div>

        {fullTask?.attachments && fullTask.attachments.length > 0 ? (
          <div>
            <h4 className="mb-2 flex items-center gap-1.5 text-sm font-semibold text-ink">
              <Paperclip className="h-4 w-4" /> Attachments ({fullTask.attachments.length})
            </h4>
            <div className="space-y-2">
              {fullTask.attachments.map((a) => (
                <div key={a.id} className="flex items-center justify-between rounded-lg border border-line p-2">
                  <div className="flex items-center gap-2">
                    <Paperclip className="h-4 w-4 text-ink-muted" />
                    <span className="text-sm text-ink">{a.originalFileName}</span>
                    <span className="text-xs text-ink-muted">
                      {a.fileSize > 1048576 ? `${(a.fileSize / 1048576).toFixed(1)} MB` : `${(a.fileSize / 1024).toFixed(0)} KB`}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        ) : null}
      </div>
    </Dialog>
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
    assignedTo: editing?.assignedTo ?? "",
    estimatedHours: editing?.estimatedHours?.toString() ?? ""
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
                assignedTo: form.assignedTo || null,
                estimatedHours: form.estimatedHours ? Number(form.estimatedHours) : null
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
        <Field label="Estimated Hours">
          <Input type="number" step="0.5" value={form.estimatedHours} onChange={(e) => setForm({ ...form, estimatedHours: e.target.value })} placeholder="e.g. 2.5" />
        </Field>
        <Field label={t("invoices.case")}>
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
