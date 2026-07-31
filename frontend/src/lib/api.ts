import axios, {
  AxiosError,
  type AxiosInstance,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig
} from "axios";
import { API_URL } from "@/lib/utils";
import type { AuthResponse } from "@/types/api";

const ACCESS_KEY = "verdiq_access_token";
const REFRESH_KEY = "verdiq_refresh_token";
const USER_KEY = "verdiq_user";
const SA_ACCESS_KEY = "verdiq_sa_access_token";
const SA_REFRESH_KEY = "verdiq_sa_refresh_token";
const SA_USER_KEY = "verdiq_sa_user";

export const tokenStore = {
  get: () => localStorage.getItem(ACCESS_KEY),
  set: (t: string) => localStorage.setItem(ACCESS_KEY, t),
  getRefresh: () => localStorage.getItem(REFRESH_KEY),
  setRefresh: (t: string) => localStorage.setItem(REFRESH_KEY, t),
  getUser: <T>() => {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as T) : null;
  },
  setUser: (u: unknown) => localStorage.setItem(USER_KEY, JSON.stringify(u)),
  clear: () => {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(USER_KEY);
  },
  saGet: () => localStorage.getItem(SA_ACCESS_KEY),
  saSet: (t: string) => localStorage.setItem(SA_ACCESS_KEY, t),
  saGetRefresh: () => localStorage.getItem(SA_REFRESH_KEY),
  saSetRefresh: (t: string) => localStorage.setItem(SA_REFRESH_KEY, t),
  saGetUser: <T>() => {
    const raw = localStorage.getItem(SA_USER_KEY);
    return raw ? (JSON.parse(raw) as T) : null;
  },
  saSetUser: (u: unknown) => localStorage.setItem(SA_USER_KEY, JSON.stringify(u)),
  saClear: () => {
    localStorage.removeItem(SA_ACCESS_KEY);
    localStorage.removeItem(SA_REFRESH_KEY);
    localStorage.removeItem(SA_USER_KEY);
  }
};

// Cookie helpers for middleware route protection
export const cookieStore = {
  set: (name: string, value: string, maxAge = 28800) => {
    document.cookie = `${name}=${value};path=/;max-age=${maxAge};SameSite=Lax`;
  },
  clear: (name: string) => {
    document.cookie = `${name}=;path=/;max-age=0;SameSite=Lax`;
  },
  access: () => cookieStore.set("access_token", tokenStore.get() || "", 28800),
  clearAccess: () => cookieStore.clear("access_token"),
  saAccess: () => cookieStore.set("sa_access_token", tokenStore.saGet() || "", 28800),
  clearSaAccess: () => cookieStore.clear("sa_access_token")
};

let refreshPromise: Promise<string | null> | null = null;

export function getApiClient(): AxiosInstance {
  return api;
}

export function getSuperAdminClient(): AxiosInstance {
  return saApi;
}

async function doRefresh(isSA: boolean): Promise<string | null> {
  const access = isSA ? tokenStore.saGet() : tokenStore.get();
  const refresh = isSA ? tokenStore.saGetRefresh() : tokenStore.getRefresh();
  if (!access || !refresh) return null;
  try {
    const client = isSA ? saApi : api;
    const { data } = await client.post<AuthResponse>("/auth/refresh", {
      accessToken: access,
      refreshToken: refresh
    });
    if (data.accessToken && data.refreshToken) {
      if (isSA) {
        tokenStore.saSet(data.accessToken);
        tokenStore.saSetRefresh(data.refreshToken);
        cookieStore.saAccess();
      } else {
        tokenStore.set(data.accessToken);
        tokenStore.setRefresh(data.refreshToken);
        cookieStore.access();
      }
      return data.accessToken;
    }
    return null;
  } catch {
    return null;
  }
}

function attachInterceptor(instance: AxiosInstance, isSA: boolean) {
  instance.interceptors.request.use((config: InternalAxiosRequestConfig) => {
    const token = isSA ? tokenStore.saGet() : tokenStore.get();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  instance.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
      const original = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;
      const status = error.response?.status;
      const url = original?.url || "";

      // Never attempt refresh on the auth endpoints themselves
      if (status === 401 && original && !original._retry && !url.startsWith("/auth/")) {
        original._retry = true;
        const isSARequest = isSA;
        const base = isSARequest ? saApi : api;
        refreshPromise = refreshPromise || doRefresh(isSARequest);
        try {
          const newToken = await refreshPromise;
          if (newToken) {
            original.headers.Authorization = `Bearer ${newToken}`;
            return base(original);
          }
        } finally {
          refreshPromise = null;
        }
        // Refresh failed — sign out
        if (isSARequest) {
          tokenStore.saClear();
          cookieStore.clearSaAccess();
        } else {
          tokenStore.clear();
          cookieStore.clearAccess();
        }
        if (typeof window !== "undefined") {
          window.location.href = isSARequest ? "/super-admin/login" : "/login";
        }
      }
      return Promise.reject(error);
    }
  );
}

export const api = axios.create({
  baseURL: API_URL,
  timeout: 30000,
  headers: { "Content-Type": "application/json" }
});

export const saApi = axios.create({
  baseURL: API_URL,
  timeout: 30000,
  headers: { "Content-Type": "application/json" }
});

attachInterceptor(api, false);
attachInterceptor(saApi, true);

export async function apiGet<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
  const { data } = await api.get<{ data: T }>(url, config);
  return data.data;
}

export async function apiPost<T>(url: string, body?: unknown): Promise<T> {
  const { data } = await api.post<{ data: T }>(url, body);
  return data.data;
}

export async function apiPut<T>(url: string, body?: unknown): Promise<T> {
  const { data } = await api.put<{ data: T }>(url, body);
  return data.data;
}

export async function apiPatch<T>(url: string, body?: unknown): Promise<T> {
  const { data } = await api.patch<{ data: T }>(url, body);
  return data.data;
}

export async function apiDelete<T>(url: string): Promise<T> {
  const { data } = await api.delete<{ data: T }>(url);
  return data.data;
}

export async function saGet<T>(url: string): Promise<T> {
  const { data } = await saApi.get<{ data: T }>(url);
  return data.data;
}

export async function saPost<T>(url: string, body?: unknown): Promise<T> {
  const { data } = await saApi.post<{ data: T }>(url, body);
  return data.data;
}

export async function saPut<T>(url: string, body?: unknown): Promise<T> {
  const { data } = await saApi.put<{ data: T }>(url, body);
  return data.data;
}

export async function saDelete<T>(url: string): Promise<T> {
  const { data } = await saApi.delete<{ data: T }>(url);
  return data.data;
}
