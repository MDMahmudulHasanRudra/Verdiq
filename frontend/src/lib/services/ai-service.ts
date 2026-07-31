import { apiPost } from "@/lib/api";

export interface AiChatMessage {
  role: "user" | "assistant";
  content: string;
  context?: {
    caseId?: string;
    mode?: "general" | "case" | "drafting" | "legal-research" | "paralegal";
  };
}

export const aiService = {
  chat: (messages: AiChatMessage[], context?: AiChatMessage["context"]) =>
    apiPost<{ reply: string }>("/ai/assist", { messages, context })
};
