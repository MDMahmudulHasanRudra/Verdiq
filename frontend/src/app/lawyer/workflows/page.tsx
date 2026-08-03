"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Field, Textarea } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { EmptyState, Loading } from "@/components/ui/loading";
import { caseWorkflows } from "@/lib/services";
import { getErrorMessage } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useLanguage } from "@/lib/i18n";
import { Workflow, Plus, Pencil, Trash2, Power, GripVertical, Clock } from "lucide-react";
import type { Workflow as WorkflowModel, CreateWorkflowInput, CreateWorkflowStepInput } from "@/types/models";

export default function WorkflowsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const { t } = useLanguage();
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<WorkflowModel | null>(null);
  const [deleting, setDeleting] = useState<WorkflowModel | null>(null);

  const invalidate = () => qc.invalidateQueries({ queryKey: ["workflows"] });

  const { data: workflows, isLoading } = useQuery({
    queryKey: ["workflows"],
    queryFn: () => caseWorkflows.list()
  });

  const createMutation = useMutation({
    mutationFn: (input: CreateWorkflowInput) => caseWorkflows.create(input),
    onSuccess: () => {
      invalidate();
      setCreateOpen(false);
      toast.success("Workflow created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, input }: { id: string; input: CreateWorkflowInput }) => caseWorkflows.update(id, input),
    onSuccess: () => {
      invalidate();
      setEditing(null);
      toast.success("Workflow updated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const activeMutation = useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) => caseWorkflows.setActive(id, isActive),
    onSuccess: (_data, vars) => {
      invalidate();
      toast.success(vars.isActive ? "Workflow activated" : "Workflow deactivated");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => caseWorkflows.remove(id),
    onSuccess: () => {
      invalidate();
      setDeleting(null);
      toast.success("Workflow deleted");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title={t("workflows.title")}
        subtitle={t("workflows.subtitle")}
        actions={
          <Button onClick={() => setCreateOpen(true)}>
            <Plus className="h-4 w-4" /> {t("workflows.addWorkflow")}
          </Button>
        }
      />

      {isLoading ? (
        <Loading />
      ) : workflows && workflows.length > 0 ? (
        <div className="space-y-4">
          {workflows.map((w) => (
            <Card key={w.id}>
              <CardHeader
                title={
                  <span className="flex items-center gap-2">
                    <Workflow className="h-4 w-4 text-primary-700" />
                    {w.name}
                    {!w.isActive ? <Badge tone="slate">{t("workflows.inactive")}</Badge> : null}
                  </span>
                }
                description={
                  w.description ??
                  (w.steps.length > 0
                    ? `${w.steps.length} step${w.steps.length > 1 ? "s" : ""} · Created by ${w.createdByName ?? "Unknown"}`
                    : "")
                }
                action={
                  <div className="flex shrink-0 items-center gap-1">
                    <button
                      onClick={() => activeMutation.mutate({ id: w.id, isActive: !w.isActive })}
                      className="cursor-pointer rounded-lg p-2 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                      title={w.isActive ? "Deactivate" : "Activate"}
                    >
                      <Power className={`h-4 w-4 ${w.isActive ? "text-emerald-500" : ""}`} />
                    </button>
                    <button
                      onClick={() => setEditing(w)}
                      className="cursor-pointer rounded-lg p-2 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
                      title="Edit workflow"
                    >
                      <Pencil className="h-4 w-4" />
                    </button>
                    <button
                      onClick={() => setDeleting(w)}
                      className="cursor-pointer rounded-lg p-2 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
                      title="Delete workflow"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                }
              />
              <CardContent>
                <ol className="space-y-2">
                  {w.steps.map((s, i) => (
                    <li key={s.id} className="flex items-start gap-3 rounded-lg border border-line p-3">
                      <span className="mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary-50 text-xs font-semibold text-primary-800">
                        {i + 1}
                      </span>
                      <div className="min-w-0 flex-1">
                        <p className="text-sm font-medium text-ink">{s.title}</p>
                        {s.description ? <p className="mt-0.5 text-xs text-ink-muted">{s.description}</p> : null}
                      </div>
                      {s.dueInDays ? (
                        <span className="inline-flex shrink-0 items-center gap-1 text-xs text-ink-muted">
                          <Clock className="h-3.5 w-3.5" /> Due in {s.dueInDays}d
                        </span>
                      ) : null}
                    </li>
                  ))}
                </ol>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <Card>
          <EmptyState
            icon={<Workflow className="h-10 w-10" />}
            title={t("workflows.noWorkflows")}
            description={t("workflows.noWorkflowsDesc")}
            action={<Button onClick={() => setCreateOpen(true)}><Plus className="h-4 w-4" /> {t("workflows.addWorkflow")}</Button>}
          />
        </Card>
      )}

      <WorkflowDialog
        open={createOpen}
        title={t("workflows.addWorkflow")}
        onClose={() => setCreateOpen(false)}
        onSubmit={(input) => createMutation.mutate(input)}
      />

      {editing ? (
        <WorkflowDialog
          open
          title="Edit Workflow"
          initial={editing}
          onClose={() => setEditing(null)}
          onSubmit={(input) => updateMutation.mutate({ id: editing.id, input })}
        />
      ) : null}

      <Dialog
        open={!!deleting}
        onClose={() => setDeleting(null)}
        title="Delete workflow"
        description={deleting ? `"${deleting.name}" will be removed. Cases it is running on keep their current progress.` : ""}
        footer={
          <>
            <Button variant="ghost" onClick={() => setDeleting(null)}>Cancel</Button>
            <Button variant="danger" disabled={deleteMutation.isPending} onClick={() => deleting && deleteMutation.mutate(deleting.id)}>
              <Trash2 className="h-4 w-4" /> Delete Workflow
            </Button>
          </>
        }
      >
        <p className="text-sm text-ink-muted">This action cannot be undone.</p>
      </Dialog>
    </div>
  );
}

interface DraftStep {
  key: number;
  title: string;
  description: string;
  dueInDays: string;
}

function WorkflowDialog({
  open,
  title,
  onClose,
  onSubmit,
  initial
}: {
  open: boolean;
  title: string;
  onClose: () => void;
  onSubmit: (input: CreateWorkflowInput) => void;
  initial?: WorkflowModel;
}) {
  const { t } = useLanguage();
  const [name, setName] = useState(initial?.name ?? "");
  const [description, setDescription] = useState(initial?.description ?? "");
  const [steps, setSteps] = useState<DraftStep[]>(
    initial && initial.steps.length > 0
      ? initial.steps.map((s, i) => ({
          key: i,
          title: s.title,
          description: s.description ?? "",
          dueInDays: s.dueInDays != null ? String(s.dueInDays) : ""
        }))
      : [{ key: 0, title: "", description: "", dueInDays: "" }]
  );

  const updateStep = (key: number, patch: Partial<DraftStep>) =>
    setSteps((prev) => prev.map((s) => (s.key === key ? { ...s, ...patch } : s)));

  const addStep = () =>
    setSteps((prev) => [...prev, { key: Math.max(0, ...prev.map((s) => s.key)) + 1, title: "", description: "", dueInDays: "" }]);

  const removeStep = (key: number) => setSteps((prev) => (prev.length > 1 ? prev.filter((s) => s.key !== key) : prev));

  const moveStep = (key: number, dir: -1 | 1) =>
    setSteps((prev) => {
      const idx = prev.findIndex((s) => s.key === key);
      const target = idx + dir;
      if (idx < 0 || target < 0 || target >= prev.length) return prev;
      const next = [...prev];
      [next[idx], next[target]] = [next[target], next[idx]];
      return next;
    });

  const buildInput = (): CreateWorkflowInput => ({
    name,
    description: description || null,
    steps: steps.map((s, i): CreateWorkflowStepInput => ({
      title: s.title,
      description: s.description || null,
      orderIndex: i,
      dueInDays: s.dueInDays ? Number(s.dueInDays) : null
    }))
  });

  const valid = name.trim().length > 0 && steps.length > 0 && steps.every((s) => s.title.trim().length > 0);

  return (
    <Dialog
      open={open}
      onClose={onClose}
      title={title}
      description="Define the ordered steps of the process. When linked to a case, a step unlocks only after the previous one is completed."
      size="lg"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button disabled={!valid} onClick={() => onSubmit(buildInput())}>
            <Workflow className="h-4 w-4" /> Save Workflow
          </Button>
        </>
      }
    >
      <div className="space-y-5">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Field label={t("workflows.stepName")} required>
            <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="e.g. Bail hearing process" />
          </Field>
          <Field label={t("workflows.stepType")}>
            <Input value={description} onChange={(e) => setDescription(e.target.value)} placeholder="What does this process cover?" />
          </Field>
        </div>

        <div>
          <div className="mb-2 flex items-center justify-between">
            <p className="text-sm font-medium text-ink">
              {t("workflows.steps")} <span className="ml-1 text-xs font-normal text-ink-muted">({steps.length})</span>
            </p>
            <Button size="sm" variant="outline" onClick={addStep}>
              <Plus className="h-3.5 w-3.5" /> Add step
            </Button>
          </div>

          <div className="space-y-3">
            {steps.map((s, i) => (
              <div key={s.key} className="rounded-lg border border-line p-3">
                <div className="flex items-start gap-3">
                  <div className="mt-2 flex flex-col items-center gap-0.5">
                    <GripVertical className="h-4 w-4 text-ink-soft" />
                    <span className="flex h-6 w-6 items-center justify-center rounded-full bg-primary-50 text-xs font-semibold text-primary-800">
                      {i + 1}
                    </span>
                  </div>
                  <div className="grid flex-1 grid-cols-1 gap-3 sm:grid-cols-3">
                    <Field label={t("workflows.stepName")} required className="sm:col-span-2">
                      <Input value={s.title} onChange={(e) => updateStep(s.key, { title: e.target.value })} placeholder="e.g. File bail petition" />
                    </Field>
                    <Field label="Due in (days)">
                      <Input
                        type="number"
                        min={0}
                        value={s.dueInDays}
                        onChange={(e) => updateStep(s.key, { dueInDays: e.target.value })}
                        placeholder="e.g. 7"
                      />
                    </Field>
                    <Field label={t("workflows.stepType")} className="sm:col-span-3">
                      <Textarea rows={2} value={s.description} onChange={(e) => updateStep(s.key, { description: e.target.value })} />
                    </Field>
                  </div>
                  <div className="flex shrink-0 flex-col items-center gap-1">
                    <button
                      onClick={() => moveStep(s.key, -1)}
                      disabled={i === 0}
                      className="cursor-pointer rounded p-1 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink disabled:cursor-not-allowed disabled:opacity-30"
                      title="Move up"
                    >
                      <ChevronUp className="h-4 w-4" />
                    </button>
                    <button
                      onClick={() => moveStep(s.key, 1)}
                      disabled={i === steps.length - 1}
                      className="cursor-pointer rounded p-1 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink disabled:cursor-not-allowed disabled:opacity-30"
                      title="Move down"
                    >
                      <ChevronDown className="h-4 w-4" />
                    </button>
                    <button
                      onClick={() => removeStep(s.key)}
                      className="cursor-pointer rounded p-1 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
                      title="Remove step"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
          {steps.length === 0 ? <p className="text-xs text-ink-muted">Add at least one step.</p> : null}
        </div>
      </div>
    </Dialog>
  );
}

function ChevronUp({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="m18 15-6-6-6 6" />
    </svg>
  );
}

function ChevronDown({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
      <path d="m6 9 6 6 6-6" />
    </svg>
  );
}
