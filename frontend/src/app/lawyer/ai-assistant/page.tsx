"use client";

import { useState } from "react";
import { PageHeader } from "@/components/ui/page-header";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/field";
import { Card, CardContent } from "@/components/ui/card";
import { EmptyState } from "@/components/ui/loading";
import { useLanguage } from "@/lib/i18n";
import { Bot, Send, FileText, Scale, Search, Lightbulb } from "lucide-react";

const tabs = ["general", "caseAnalysis", "drafting", "legalResearch"] as const;

const promptsByTab: Record<string, { icon: React.ComponentType<{ className?: string }>; labelKey: string }[]> = {
  general: [
    { icon: Lightbulb, labelKey: "Explain a legal concept" },
    { icon: FileText, labelKey: "Summarize a clause" },
    { icon: Scale, labelKey: "Compare legal positions" }
  ],
  caseAnalysis: [
    { icon: Scale, labelKey: "Analyze case strengths" },
    { icon: FileText, labelKey: "Review evidence" },
    { icon: Lightbulb, labelKey: "Suggest arguments" }
  ],
  drafting: [
    { icon: FileText, labelKey: "Draft a legal notice" },
    { icon: FileText, labelKey: "Write a petition" },
    { icon: FileText, labelKey: "Draft a contract clause" }
  ],
  legalResearch: [
    { icon: Search, labelKey: "Find similar cases" },
    { icon: Scale, labelKey: "Research legal precedents" },
    { icon: Lightbulb, labelKey: "Intersecting statutes" }
  ]
};

export default function AIAssistantPage() {
  const { t } = useLanguage();
  const [activeTab, setActiveTab] = useState<string>("general");
  const [input, setInput] = useState("");
  const [messages, setMessages] = useState<{ role: "user" | "assistant"; text: string }[]>([]);
  const [loading, setLoading] = useState(false);

  const handleSend = () => {
    if (!input.trim() || loading) return;
    setMessages((prev) => [...prev, { role: "user", text: input }]);
    setInput("");
    setLoading(true);
    setTimeout(() => {
      setMessages((prev) => [...prev, { role: "assistant", text: "AI responses are coming soon. This is a preview of the interface." }]);
      setLoading(false);
    }, 1000);
  };

  return (
    <div>
      <PageHeader title={t("aiAssistant.title")} subtitle={t("aiAssistant.subtitle")} />

      <div className="mb-4 flex gap-2">
        {tabs.map((tab) => (
          <Button
            key={tab}
            variant={activeTab === tab ? "primary" : "ghost"}
            size="sm"
            onClick={() => setActiveTab(tab)}
          >
            {t(`aiAssistant.${tab}`)}
          </Button>
        ))}
      </div>

      <Card>
        <CardContent className="p-4">
          <div className="mb-4">
            <Input
              placeholder={t("aiAssistant.caseIdOptional")}
              className="mb-2 max-w-xs"
            />
          </div>

          {messages.length === 0 && !loading ? (
            <EmptyState
              icon={<Bot className="h-10 w-10" />}
              title={t("aiAssistant.howCanIHelp")}
              description={t("aiAssistant.askPlaceholder")}
            />
          ) : (
            <div className="mb-4 max-h-96 space-y-3 overflow-y-auto">
              {messages.map((msg, i) => (
                <div
                  key={i}
                  className={`flex ${msg.role === "user" ? "justify-end" : "justify-start"}`}
                >
                  <div
                    className={`max-w-[80%] rounded-lg px-4 py-2 text-sm ${
                      msg.role === "user"
                        ? "bg-primary-600 text-white"
                        : "bg-slate-100 text-ink"
                    }`}
                  >
                    {msg.text}
                  </div>
                </div>
              ))}
              {loading && (
                <div className="flex justify-start">
                  <div className="rounded-lg bg-slate-100 px-4 py-2 text-sm text-ink-muted">Thinking…</div>
                </div>
              )}
            </div>
          )}

          <div className="flex gap-2">
            <Input
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder={t("aiAssistant.sendPlaceholder")}
              onKeyDown={(e) => e.key === "Enter" && handleSend()}
            />
            <Button onClick={handleSend} disabled={!input.trim() || loading}>
              <Send className="h-4 w-4" />
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
