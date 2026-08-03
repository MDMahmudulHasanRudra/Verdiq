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
import { useLanguage } from "@/lib/i18n";

export default function ClientInvoicesPage() {
  const { t } = useLanguage();
  const { data, isLoading } = useQuery({
    queryKey: ["client", "invoices"],
    queryFn: () => clientPortalService.invoices()
  });

  return (
    <div>
      <PageHeader title={t("clientInvoices.title")} subtitle={t("clientInvoices.subtitle")} />

      <Card>
        {isLoading ? (
          <Loading />
        ) : data && data.length > 0 ? (
          <Table>
            <thead>
              <tr>
                <th>{t("clientInvoices.invoice")}</th>
                <th>{t("clientInvoices.case")}</th>
                <th>{t("clientInvoices.amount")}</th>
                <th>{t("clientInvoices.paid")}</th>
                <th>{t("clientInvoices.balance")}</th>
                <th>{t("clientInvoices.dueDate")}</th>
                <th>{t("common.status")}</th>
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
          <EmptyState icon={<Receipt className="h-10 w-10" />} title={t("clientInvoices.noInvoices")} description={t("clientInvoices.noInvoicesDesc")} />
        )}
      </Card>
    </div>
  );
}
