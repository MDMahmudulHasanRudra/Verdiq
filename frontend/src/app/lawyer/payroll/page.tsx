"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input, Select, Field } from "@/components/ui/field";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Dialog } from "@/components/ui/dialog";
import { Loading, EmptyState } from "@/components/ui/loading";
import { payrollService } from "@/lib/services";
import { getErrorMessage, formatCurrency, formatDate } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { ArrowLeftRight, Plus, UserPlus } from "lucide-react";
import type { Employee } from "@/types/models";

export default function PayrollPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [month, setMonth] = useState(new Date().getMonth() + 1);
  const [year, setYear] = useState(new Date().getFullYear());
  const [employeeOpen, setEmployeeOpen] = useState(false);
  const [payrollOpen, setPayrollOpen] = useState(false);

  const { data: employees, isLoading } = useQuery({
    queryKey: ["payroll", "employees"],
    queryFn: () => payrollService.employees()
  });
  const { data: payrolls } = useQuery({
    queryKey: ["payroll", "payrolls", month, year],
    queryFn: () => payrollService.payrolls(month, year)
  });

  const addEmployee = useMutation({
    mutationFn: (input: Record<string, unknown>) => payrollService.createEmployee(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["payroll"] });
      setEmployeeOpen(false);
      toast.success("Employee added");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const createPayroll = useMutation({
    mutationFn: (input: Record<string, unknown>) => payrollService.createPayroll(input),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["payroll"] });
      setPayrollOpen(false);
      toast.success("Payroll run created");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const approve = useMutation({
    mutationFn: (id: string) => payrollService.approvePayroll(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["payroll"] });
      toast.success("Payroll approved");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const totalPayroll = (payrolls ?? []).reduce((s, p) => s + p.netSalary, 0);

  return (
    <div>
      <PageHeader
        title="Payroll"
        subtitle="Manage employees, salaries and monthly runs."
        actions={
          <>
            <Button variant="outline" onClick={() => setEmployeeOpen(true)}>
              <UserPlus className="h-4 w-4" /> Employee
            </Button>
            <Button onClick={() => setPayrollOpen(true)}>
              <Plus className="h-4 w-4" /> Run Payroll
            </Button>
          </>
        }
      />

      <div className="mb-6 grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Employees</p>
          <p className="mt-1 text-2xl font-bold text-ink">{employees?.length ?? 0}</p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-ink-muted">{monthName(month)} {year} Net Payroll</p>
          <p className="mt-1 text-2xl font-bold text-ink">{formatCurrency(totalPayroll)}</p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-ink-muted">Payroll Runs</p>
          <p className="mt-1 text-2xl font-bold text-ink">{payrolls?.length ?? 0}</p>
        </Card>
      </div>

      <div className="mb-4 flex gap-3">
        <Select className="w-36" value={month} onChange={(e) => setMonth(Number(e.target.value))}>
          {Array.from({ length: 12 }, (_, i) => i + 1).map((m) => (
            <option key={m} value={m}>{monthName(m)}</option>
          ))}
        </Select>
        <Select className="w-32" value={year} onChange={(e) => setYear(Number(e.target.value))}>
          {[2024, 2025, 2026, 2027].map((y) => (
            <option key={y} value={y}>{y}</option>
          ))}
        </Select>
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
        <Card>
          <CardHeader title="Employees" />
          {isLoading ? (
            <Loading />
          ) : employees && employees.length > 0 ? (
            <CardContent className="space-y-3">
              {employees.map((e) => (
                <div key={e.id} className="flex items-center justify-between rounded-lg border border-line px-3 py-2">
                  <div>
                    <p className="text-sm font-medium text-ink">{e.fullName}</p>
                    <p className="text-xs text-ink-muted">{e.designation} · {e.department}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <span className="text-sm font-semibold text-ink">{formatCurrency(e.baseSalary)}</span>
                    <StatusBadge value={e.status} />
                  </div>
                </div>
              ))}
            </CardContent>
          ) : (
            <EmptyState
              icon={<UserPlus className="h-10 w-10" />}
              title="No employees"
              action={<Button size="sm" onClick={() => setEmployeeOpen(true)}><Plus className="h-4 w-4" /> Add Employee</Button>}
            />
          )}
        </Card>

        <Card>
          <CardHeader title={`Payroll Runs · ${monthName(month)} ${year}`} />
          {isLoading ? (
            <Loading />
          ) : payrolls && payrolls.length > 0 ? (
            <CardContent className="space-y-3">
              {payrolls.map((p) => (
                <div key={p.id} className="flex items-center justify-between rounded-lg border border-line px-3 py-2">
                  <div>
                    <p className="text-sm font-medium text-ink">{p.employeeName ?? "Payroll"}</p>
                    <p className="text-xs text-ink-muted">
                      {p.payrollNumber} · Gross {formatCurrency(p.grossSalary)} · Net {formatCurrency(p.netSalary)}
                    </p>
                  </div>
                  <div className="flex items-center gap-3">
                    <StatusBadge value={p.status} />
                    {p.status === "Pending" || p.status === "Draft" ? (
                      <Button size="sm" variant="subtle" onClick={() => approve.mutate(p.id)}>Approve</Button>
                    ) : null}
                  </div>
                </div>
              ))}
            </CardContent>
          ) : (
            <EmptyState
              icon={<ArrowLeftRight className="h-10 w-10" />}
              title="No payroll runs"
              description={`Run payroll for ${monthName(month)} ${year}.`}
              action={<Button size="sm" onClick={() => setPayrollOpen(true)}><Plus className="h-4 w-4" /> Run Payroll</Button>}
            />
          )}
        </Card>
      </div>

      <AddEmployeeDialog open={employeeOpen} onClose={() => setEmployeeOpen(false)} onSubmit={(v) => addEmployee.mutate(v)} />
      <RunPayrollDialog open={payrollOpen} onClose={() => setPayrollOpen(false)} employees={employees ?? []} month={month} year={year} onSubmit={(v) => createPayroll.mutate(v)} />
    </div>
  );
}

function monthName(m: number) {
  return ["", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"][m] ?? "";
}

function AddEmployeeDialog({
  open,
  onClose,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({
    name: "",
    designation: "",
    department: "",
    baseSalary: ""
  });
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Add Employee"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.name || !form.baseSalary}
            onClick={() =>
              onSubmit({
                name: form.name,
                designation: form.designation || null,
                department: form.department || null,
                baseSalary: Number(form.baseSalary),
                status: "Active"
              })
            }
          >
            Add Employee
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Name" required className="sm:col-span-2">
          <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
        </Field>
        <Field label="Designation">
          <Input value={form.designation} onChange={(e) => setForm({ ...form, designation: e.target.value })} />
        </Field>
        <Field label="Department">
          <Input value={form.department} onChange={(e) => setForm({ ...form, department: e.target.value })} />
        </Field>
        <Field label="Base Salary (BDT)" required className="sm:col-span-2">
          <Input type="number" value={form.baseSalary} onChange={(e) => setForm({ ...form, baseSalary: e.target.value })} />
        </Field>
      </div>
    </Dialog>
  );
}

function RunPayrollDialog({
  open,
  onClose,
  employees,
  month,
  year,
  onSubmit
}: {
  open: boolean;
  onClose: () => void;
  employees: Employee[];
  month: number;
  year: number;
  onSubmit: (input: Record<string, unknown>) => void;
}) {
  const [form, setForm] = useState({ employeeId: "", grossSalary: "", deductions: "0", bonuses: "0" });
  const selected = employees.find((e) => e.id === form.employeeId);
  return (
    <Dialog
      open={open}
      onClose={onClose}
      title="Run Payroll"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>Cancel</Button>
          <Button
            disabled={!form.employeeId || !form.grossSalary}
            onClick={() =>
              onSubmit({
                employeeId: form.employeeId,
                month,
                year,
                grossSalary: Number(form.grossSalary),
                deductions: Number(form.deductions || 0),
                bonuses: Number(form.bonuses || 0)
              })
            }
          >
            Create Payroll
          </Button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Field label="Employee" required className="sm:col-span-2">
          <Select
            value={form.employeeId}
            onChange={(e) => {
              const emp = employees.find((x) => x.id === e.target.value);
              setForm({
                employeeId: e.target.value,
                grossSalary: emp ? String(emp.baseSalary) : "",
                deductions: "0",
                bonuses: "0"
              });
            }}
          >
            <option value="">Select employee</option>
            {employees.map((e) => (
              <option key={e.id} value={e.id}>{e.fullName}</option>
            ))}
          </Select>
        </Field>
        <Field label="Gross Salary (BDT)" required>
          <Input type="number" value={form.grossSalary} onChange={(e) => setForm({ ...form, grossSalary: e.target.value })} />
        </Field>
        <Field label="Bonuses (BDT)">
          <Input type="number" value={form.bonuses} onChange={(e) => setForm({ ...form, bonuses: e.target.value })} />
        </Field>
        <Field label="Deductions (BDT)" className="sm:col-span-2">
          <Input type="number" value={form.deductions} onChange={(e) => setForm({ ...form, deductions: e.target.value })} />
        </Field>
        {selected ? (
          <p className="text-xs text-ink-muted">
            Net pay: {formatCurrency(Number(form.grossSalary || 0) + Number(form.bonuses || 0) - Number(form.deductions || 0))}
          </p>
        ) : null}
      </div>
    </Dialog>
  );
}
