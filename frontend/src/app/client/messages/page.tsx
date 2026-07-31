"use client";

import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/field";
import { Loading, EmptyState } from "@/components/ui/loading";
import { Avatar } from "@/components/ui/avatar";
import { clientPortalService } from "@/lib/services";
import { getErrorMessage, formatDateTime, timeAgo } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { MessageSquare, Send } from "lucide-react";

export default function ClientMessagesPage() {
  const toast = useToast();
  const qc = useQueryClient();
  const [content, setContent] = useState("");

  const { data: messages, isLoading } = useQuery({
    queryKey: ["client", "messages"],
    queryFn: () => clientPortalService.messages(),
    refetchInterval: 20000
  });

  const { data: profile } = useQuery({
    queryKey: ["client", "profile"],
    queryFn: () => clientPortalService.profile()
  });

  const sendMutation = useMutation({
    mutationFn: () => clientPortalService.sendMessage({ receiverId: profile?.id ?? "", content }),
    onSuccess: () => {
      setContent("");
      qc.invalidateQueries({ queryKey: ["client", "messages"] });
      qc.invalidateQueries({ queryKey: ["client", "unread"] });
      toast.success("Message sent");
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  return (
    <div>
      <PageHeader title="Messages" subtitle="Communicate securely with your law chamber." />

      <Card className="flex h-[60vh] flex-col">
        <div className="flex-1 space-y-4 overflow-y-auto p-5">
          {isLoading ? (
            <Loading />
          ) : messages && messages.length > 0 ? (
            messages.map((m) => {
              const isMine = m.senderName === profile?.name;
              return (
                <div key={m.id} className={`flex ${isMine ? "justify-end" : "justify-start"}`}>
                  <div className={`max-w-[75%] ${isMine ? "items-end" : "items-start"}`}>
                    <div className={`flex items-center gap-2 ${isMine ? "flex-row-reverse" : ""}`}>
                      <Avatar name={m.senderName} src={m.senderAvatar} size="sm" />
                      <p className="text-xs font-medium text-ink-muted">{m.senderName}</p>
                    </div>
                    <div
                      className={`mt-1 rounded-2xl px-4 py-2.5 text-sm ${
                        isMine ? "rounded-tr-sm bg-primary-700 text-white" : "rounded-tl-sm bg-slate-100 text-ink"
                      }`}
                    >
                      <p>{m.content}</p>
                    </div>
                    <p className={`mt-1 text-[11px] text-ink-soft ${isMine ? "text-right" : ""}`}>{timeAgo(m.createdAt)}</p>
                  </div>
                </div>
              );
            })
          ) : (
            <EmptyState
              icon={<MessageSquare className="h-10 w-10" />}
              title="No messages yet"
              description="Send a message to your chamber to start the conversation."
            />
          )}
        </div>

        <form
          className="flex items-end gap-2 border-t border-line p-4"
          onSubmit={(e) => {
            e.preventDefault();
            if (content.trim()) sendMutation.mutate();
          }}
        >
          <Textarea
            rows={2}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            placeholder="Type a message…"
            className="flex-1"
          />
          <Button type="submit" disabled={!content.trim() || !profile?.id} loading={sendMutation.isPending}>
            <Send className="h-4 w-4" /> Send
          </Button>
        </form>
      </Card>

      <p className="mt-3 text-xs text-ink-soft">
        {messages?.length ? `Last message ${formatDateTime(messages[messages.length - 1].createdAt)}` : ""}
      </p>
    </div>
  );
}
