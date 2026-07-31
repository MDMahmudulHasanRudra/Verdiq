"use client";

import { useEffect, useRef, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { PageHeader } from "@/components/ui/page-header";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/field";
import { EmptyState } from "@/components/ui/loading";
import { aiService, type AiChatMessage } from "@/lib/services/ai-service";
import { getErrorMessage, initials } from "@/lib/utils";
import { useToast } from "@/components/ui/toast";
import { useAuthStore } from "@/lib/store/auth-store";
import { Sparkles, Send, Scale, FileText, BookOpen, Briefcase } from "lucide-react";

const modes = [
  { id: "general", label: "General", icon: Sparkles, hint: "Firm questions and guidance" },
  { id: "case", label: "Case Analysis", icon: Briefcase, hint: "Analyze a case" },
  { id: "drafting", label: "Drafting", icon: FileText, hint: "Draft documents" },
  { id: "legal-research", label: "Legal Research", icon: BookOpen, hint: "Research a point of law" }
] as const;

type Mode = (typeof modes)[number]["id"];

export default function AiAssistantPage() {
  const toast = useToast();
  const user = useAuthStore((s) => s.user);
  const [messages, setMessages] = useState<AiChatMessage[]>([]);
  const [input, setInput] = useState("");
  const [mode, setMode] = useState<Mode>("general");
  const [caseId, setCaseId] = useState("");
  const endRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    endRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const chat = useMutation({
    mutationFn: (msgs: AiChatMessage[]) => aiService.chat(msgs, {
      mode,
      caseId: caseId || undefined
    }),
    onSuccess: (data) => {
      setMessages((m) => [...m, { role: "assistant", content: data.reply }]);
    },
    onError: (e) => toast.error(getErrorMessage(e))
  });

  const send = () => {
    const content = input.trim();
    if (!content) return;
    const next = [...messages, { role: "user" as const, content }];
    setMessages(next);
    setInput("");
    chat.mutate(next);
  };

  return (
    <div className="flex h-[calc(100vh-8rem)] flex-col">
      <PageHeader
        title={
          <span className="flex items-center gap-2">
            <Sparkles className="h-6 w-6 text-gold-600" /> AI Assistant
          </span>
        }
        subtitle="A legal copilot for drafting, research and case strategy."
      />

      <div className="mb-4 flex flex-wrap items-center gap-2">
        {modes.map((m) => (
          <button
            key={m.id}
            onClick={() => setMode(m.id)}
            className={`flex cursor-pointer items-center gap-2 rounded-full border px-3 py-1.5 text-xs font-medium transition-colors ${
              mode === m.id
                ? "border-primary-600 bg-primary-50 text-primary-800"
                : "border-line bg-card text-ink-muted hover:border-primary-300"
            }`}
            title={m.hint}
          >
            <m.icon className="h-3.5 w-3.5" /> {m.label}
          </button>
        ))}
        {mode === "case" ? (
          <input
            className="input h-8 w-48 text-xs"
            placeholder="Case ID (optional)"
            value={caseId}
            onChange={(e) => setCaseId(e.target.value)}
          />
        ) : null}
      </div>

      <Card className="flex flex-1 flex-col overflow-hidden">
        <div className="flex-1 space-y-4 overflow-y-auto p-5">
          {messages.length === 0 ? (
            <EmptyState
              icon={<Scale className="h-10 w-10" />}
              title="How can I help?"
              description={`I'm Verdiq's legal assistant. Ask me about ${mode === "legal-research" ? "a point of law" : mode === "drafting" ? "drafting a pleading or notice" : mode === "case" ? "a case's strategy or next steps" : "your cases and firm operations"}.`}
            />
          ) : (
            messages.map((m, i) =>
              m.role === "user" ? (
                <div key={i} className="flex items-start justify-end gap-3">
                  <div className="max-w-[75%] rounded-2xl rounded-tr-sm bg-primary-700 px-4 py-2.5 text-sm text-white">
                    {m.content}
                  </div>
                  <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-gold-600 text-xs font-semibold text-white">
                    {initials(user?.fullName ?? "Me")}
                  </div>
                </div>
              ) : (
                <div key={i} className="flex items-start gap-3">
                  <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary-50">
                    <Sparkles className="h-4 w-4 text-primary-700" />
                  </div>
                  <div className="max-w-[80%] whitespace-pre-wrap rounded-2xl rounded-tl-sm border border-line bg-card px-4 py-2.5 text-sm text-ink">
                    {m.content}
                  </div>
                </div>
              )
            )
          )}
          {chat.isPending ? (
            <div className="flex items-start gap-3">
              <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary-50">
                <Sparkles className="h-4 w-4 animate-pulse text-primary-700" />
              </div>
              <div className="rounded-2xl rounded-tl-sm border border-line bg-card px-4 py-3">
                <span className="flex gap-1">
                  {[0, 1, 2].map((i) => (
                    <span key={i} className="h-1.5 w-1.5 animate-bounce rounded-full bg-primary-400" style={{ animationDelay: `${i * 150}ms` }} />
                  ))}
                </span>
              </div>
            </div>
          ) : null}
          <div ref={endRef} />
        </div>
        <div className="flex items-end gap-3 border-t border-line-soft p-4">
          <Textarea
            rows={1}
            className="min-h-10 flex-1 resize-none"
            placeholder={`Ask about ${mode.replace("-", " ")}…`}
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                send();
              }
            }}
          />
          <Button onClick={send} disabled={!input.trim() || chat.isPending}>
            <Send className="h-4 w-4" /> Send
          </Button>
        </div>
      </Card>
    </div>
  );
}
