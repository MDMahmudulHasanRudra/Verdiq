"use client";

import { useState } from "react";
import { useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field, Textarea } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { useTasks } from "@/lib/hooks";
import { taskService } from "@/lib/services";
import { getErrorMessage, formatDate, formatDateTime } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { ListTodo, Plus, CheckCircle2, Clock } from "lucide-react";
import type { Task } from "@/types/models";

const columns = [
  { key: "Pending", label: "To Do", tone: "gold" as const },
  { key: "InProgress", label: "In Progress", tone: "amber" as const },
  { key: "Completed", label: "Done", tone: "green" as const },
  { key: "Cancelled", label: "Cancelled", tone: "slate" as const }
];

export default function TasksPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [createOpen, setCreateOpen] = useState(false);
  const { data: tasks, isLoading } = useTasks();

  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: string }) =>
      taskService.update(id, { status }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["tasks"] });
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const createMutation = useMutation({
    mutationFn: (input: Record<string, unknown>) => taskService.create(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["tasks"] });
      setCreateOpen(false);
      toast.success("Task created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title="Tasks"
        subtitle="Assign and track work across the firm."
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> New Task
          </Button>
        }
      />

      {isLoading ? (
        <Loading />
      ) : tasks && tasks.length > 0 ? (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
          {columns.map((col) => {
            const items = tasks.filter((t) => t.status === col.key);
            return (
              <div key={col.key} className="rounded-xl bg-slate-100/70 p-3">
                <div className="mb-3 flex items-center justify-between px-1">
                  <h3 className="flex items-center gap-2 text-sm font-semibold text-ink">
                    <span className="h-2 w-2 rounded-full bg-current" />
                    {col.label}
                  </h3>
                  <span className="rounded-full bg-card px-2 py-0.5 text-xs font-medium text-ink-muted">{items.length}</span>
                </div>
                <div className="space-y-2">
                  {items.map((t) => (
                    <TaskCard
                      key={t.id}
                      task={t}
                      onStatusChange={(status) => statusMutation.mutate({ id: t.id, status })}
                    />
                  ))}
                  {items.length === 0 ? (
                    <p className="px-1 py-3 text-center text-xs text-ink-muted">No tasks</p>
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
            title="No tasks"
            description="Create tasks for cases, hearings and firm work."
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> New Task</Button>}
          />
        </Card>
      )}

      <CreateTaskDialog open={createOpen} onClose={() => setCreateOpen(false)} onSubmit={(v) => createMutation.mutate(v)} />
    </div>
  );
}

function TaskCard({ task, onStatusChange }: { task: Task; onStatusChange: (status: string) => void }) {
  const isOverdue = task.dueDate && task.status !== "Completed" && new Date(task.dueDate) < new Date();
  return (
    <Card className="p-3">
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-medium text-ink">{task.title}</p>
        {isOverdue ? <Clock className="h-4 w-4 shrink-0 text-red-500" /> : null}
      </div>
      {task.caseTitle ? <p className="mt-1 text-xs text-primary-700">{task.caseTitle}</p> : null}
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
            title="Mark done"
          >
            <CheckCircle2 className="h-4 w-4" />
          </button>
        ) : null}
      </div>
      {task.assignedToName ? (
        <p className="mt-2 border-t border-line-soft pt-2 text-xs text-ink-muted">
          Assigned to <span className="font-medium text-ink">{task.assignedToName}</span>
        </p>
      ) : null}
    </Card>
  );
}

function CreateTaskDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({
    title: "",
    description: "",
    dueDate: "",
    priority: "Medium",
    caseId: "",
    assignedTo: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="New Task"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
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
                status: "Pending"
              })
            }
          >
            Create Task
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Title" required className="sm:col-span-2">
          <Input value={form.title} onChange={(e) => setForm({ ...form, title: e.target.value })} />
        </Field>
        <Field label="Description" className="sm:col-span-2">
          <Textarea rows={3} value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        </Field>
        <Field label="Due Date">
          <Input type="date" value={form.dueDate} onChange={(e) => setForm({ ...form, dueDate: e.target.value })} />
        </Field>
        <Field label="Priority">
          <Select value={form.priority} onChange={(e) => setForm({ ...form, priority: e.target.value })}>
            {["Low", "Medium", "High", "Urgent"].map((p) => (
              <option key={p} value={p}>{p}</option>
            ))}
          </Select>
        </Field>
        <Field label="Case ID">
          <Input value={form.caseId} onChange={(e) => setForm({ ...form, caseId: e.target.value })} placeholder="Optional GUID" />
        </Field>
        <Field label="Assign To (User ID)">
          <Input value={form.assignedTo} onChange={(e) => setForm({ ...form, assignedTo: e.target.value })} placeholder="Optional GUID" />
        </Field>
      </div>
    </Dialog>
  );
}
