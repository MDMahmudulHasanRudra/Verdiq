"use client";

import { api, tokenStore, cookieStore } from "@/lib/api";
import type { AuthResponse } from "@/types/api";
import { useAuthStore } from "@/lib/store/auth-store";
import { useRouter } from "next/navigation";

export async function performLogin(email: string, password: string): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>("/auth/login", { email, password });
  return data;
}

export async function performRegister(payload: Record<string, unknown>): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>("/auth/register", payload);
  return data;
}

export function applyAuthSession(data: AuthResponse) {
  const store = useAuthStore.getState();
  if (data.accessToken && data.refreshToken) {
    store.setTokens(data.accessToken, data.refreshToken);
    cookieStore.access();
  }
  if (data.user) {
    store.setUser(data.user);
  }
}

export function redirectAfterLogin(userRole: string | undefined, router: ReturnType<typeof useRouter>, next?: string) {
  const role = (userRole || "").toLowerCase();
  if (next && next.startsWith("/")) {
    router.replace(next);
    return;
  }
  if (role === "client") {
    router.replace("/client");
  } else {
    router.replace("/lawyer");
  }
}

export function performLogout() {
  const store = useAuthStore.getState();
  store.clearAuth();
  tokenStore.clear();
  cookieStore.clearAccess();
}
