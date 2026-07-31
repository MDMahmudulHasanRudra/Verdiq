"use client";

import { create } from "zustand";
import type { User } from "@/types/api";
import { cookieStore, tokenStore } from "@/lib/api";

interface AuthState {
  user: User | null;
  loading: boolean;
  setUser: (user: User | null) => void;
  setTokens: (access: string, refresh: string) => void;
  clearAuth: () => void;
  init: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  loading: true,

  setUser: (user) => {
    if (user) tokenStore.setUser(user);
    set({ user });
  },

  setTokens: (access, refresh) => {
    tokenStore.set(access);
    tokenStore.setRefresh(refresh);
    cookieStore.access();
  },

  clearAuth: () => {
    tokenStore.clear();
    cookieStore.clearAccess();
    set({ user: null });
  },

  init: () => {
    const user = tokenStore.getUser<User>();
    set({ user, loading: false });
  }
}));

export function useIsAuthenticated() {
  return useAuthStore((s) => !!s.user && !!tokenStore.get());
}
