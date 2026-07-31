"use client";

import { useQuery } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { Table } from "@/components/ui/table";
import { Loading, EmptyState } from "@/components/ui/loading";
import { clientPortalService } from "@/lib/services";
import { formatCurrency, formatDate } from "@/lib/utils";
import { Receipt } from "lucide-react";

export default function ClientInvoicesPage() {
  const { data, isLoading } = useQuery({
    queryKey: ["client", "invoices"],
    queryFn: () => clientPortalService.invoices()
  });

  return (
    <div>
      <PageHeader title="Invoices" subtitle="Your billing summary." />

      <Card>
        {isLoading ? (
          <Loading />
        ) : data && data.length > 0 ? (
          <Table>
            <thead>
              <tr>
                <th>Invoice</th>
                <th>Case</th>
                <th>Amount</th>
                <th>Paid</th>
                <th>Balance</th>
                <th>Due Date</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {data.map((inv) => (
                <tr key={inv.id}>
                  <td>
                    <p className="font-medium text-ink">{inv.invoiceNumber}</p>
                    <p className="text-xs text-ink-muted">{formatDate(inv.createdAt)}</p>
                  </td>
                  <td className="max-w-56 truncate text-ink-muted">{inv.caseTitle ?? "—"}</td>
                  <td className="font-medium text-ink">{formatCurrency(inv.amount)}</td>
                  <td className="text-ink-muted">{formatCurrency(inv.paidAmount)}</td>
                  <td className="font-semibold text-ink">{formatCurrency(inv.balance)}</td>
                  <td className="text-ink-muted">{inv.dueDate ? formatDate(inv.dueDate) : "—"}</td>
                  <td><StatusBadge value={inv.status} /></td>
                </tr>
              ))}
            </tbody>
          </Table>
        ) : (
          <EmptyState icon={<Receipt className="h-10 w-10" />} title="No invoices" description="Invoices issued to you will appear here." />
        )}
      </Card>
    </div>
  );
}
