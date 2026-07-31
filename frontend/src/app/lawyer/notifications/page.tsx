"use client";

import { useState } from "react";
import { useQuery, useQueryClient, useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { StatusBadge } from "@/components/ui/badge";
import { EmptyState, Loading } from "@/components/ui/loading";
import { notificationService } from "@/lib/services";
import { useMarkNotificationRead } from "@/lib/hooks";
import { getErrorMessage, timeAgo } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { Bell, CheckCheck, MailOpen } from "lucide-react";

export default function NotificationsPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const markRead = useMarkNotificationRead();
  const [unreadOnly, setUnreadOnly] = useState(false);

  const { data: notifications, isLoading } = useQuery({
    queryKey: ["notifications", unreadOnly],
    queryFn: () => notificationService.list(unreadOnly)
  });

  const markAll = useMutation({
    mutationFn: () => notificationService.markAllRead(),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["notifications"] });
      toast.success("All notifications marked as read");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader
        title="Notifications"
        subtitle="Stay on top of reminders, mentions and system updates."
        actions={
          <Button variant="outline" onClick={() => markAll.mutate()}>
            <CheckCheck className="h-4 w-4" /> Mark all read
          </Button>
        }
      />

      <div className="mb-4 flex gap-2">
        <Button
          size="sm"
          variant={unreadOnly ? "subtle" : "ghost"}
          onClick={() => setUnreadOnly((v) => !v)}
        >
          Unread only
        </Button>
      </div>

      <Card>
        {isLoading ? (
          <Loading />
        ) : notifications && notifications.length > 0 ? (
          <div className="divide-y divide-line-soft">
            {notifications.map((n) => (
              <button
                key={n.id}
                onClick={() => !n.isRead && markRead.mutate(n.id)}
                className={`flex w-full cursor-pointer items-start gap-4 px-5 py-4 text-left transition-colors hover:bg-slate-50 ${
                  n.isRead ? "opacity-70" : ""
                }`}
              >
                <div className={`mt-0.5 rounded-full p-2 ${n.isRead ? "bg-slate-100 text-ink-soft" : "bg-primary-50 text-primary-700"}`}>
                  <MailOpen className="h-4 w-4" />
                </div>
                <div className="flex-1">
                  <div className="flex items-center justify-between gap-3">
                    <p className="font-medium text-ink">{n.title}</p>
                    <span className="shrink-0 text-xs text-ink-soft">{timeAgo(n.createdAt)}</span>
                  </div>
                  <p className="mt-0.5 text-sm text-ink-muted">{n.message}</p>
                </div>
                {!n.isRead ? (
                  <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-primary-600" />
                ) : null}
              </button>
            ))}
          </div>
        ) : (
          <EmptyState icon={<Bell className="h-10 w-10" />} title="No notifications" description="You're all caught up." />
        )}
      </Card>
    </div>
  );
}
