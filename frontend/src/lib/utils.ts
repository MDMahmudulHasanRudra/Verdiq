import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";
import dayjs from "dayjs";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

const DEFAULT_API_URL = "http://localhost:5000/api";

// The API base URL is used by the browser (axios + download links). When no
// explicit NEXT_PUBLIC_API_URL is configured (the localhost build default),
// derive it at runtime from the page's own hostname so a VPS deploy works
// with zero extra configuration: the API is served on port 5000 of the same
// host that serves the frontend.
function resolveApiUrl(): string {
  const configured = process.env.NEXT_PUBLIC_API_URL;
  if (configured && configured !== DEFAULT_API_URL) return configured;
  if (typeof window !== "undefined") {
    return `${window.location.protocol}//${window.location.hostname}:5000/api`;
  }
  return DEFAULT_API_URL;
}

export const API_URL = resolveApiUrl();

export function formatCurrency(amount: number | null | undefined, currency = "BDT") {
  if (amount === null || amount === undefined || Number.isNaN(amount)) return "—";
  const n = Number(amount);
  const formatted = n.toLocaleString("en-BD", {
    minimumFractionDigits: n % 1 === 0 ? 0 : 2,
    maximumFractionDigits: 2
  });
  return `${currency === "BDT" ? "৳" : currency === "USD" ? "$" : currency + " "}${formatted}`;
}

export function formatDate(value: string | null | undefined, format = "DD MMM YYYY") {
  if (!value) return "—";
  const d = dayjs(value);
  if (!d.isValid()) return "—";
  return d.format(format);
}

export function formatDateTime(value: string | null | undefined) {
  if (!value) return "—";
  const d = dayjs(value);
  if (!d.isValid()) return "—";
  return d.format("DD MMM YYYY, hh:mm A");
}

export function timeAgo(value: string | null | undefined) {
  if (!value) return "—";
  const d = dayjs(value);
  if (!d.isValid()) return "—";
  const diff = dayjs().diff(d, "minute");
  if (diff < 1) return "just now";
  if (diff < 60) return `${diff}m ago`;
  const hours = Math.floor(diff / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `${days}d ago`;
  const months = Math.floor(days / 30);
  if (months < 12) return `${months}mo ago`;
  return `${Math.floor(months / 12)}y ago`;
}

export function initials(name: string | null | undefined) {
  if (!name) return "?";
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0]!.toUpperCase())
    .join("");
}

export function firstError(errors?: string[] | null, fallback = "Something went wrong") {
  if (errors && errors.length > 0) return errors[0];
  return fallback;
}

export function getErrorMessage(err: unknown): string {
  if (typeof err === "object" && err !== null) {
    const e = err as { response?: { data?: { message?: string; errors?: string[] } }; message?: string };
    if (e.response?.data?.message) return firstError(e.response.data.errors, e.response.data.message);
    if (e.message) return e.message;
  }
  return "Something went wrong";
}

export async function apiDownload(url: string): Promise<Blob> {
  const response = await fetch(url, { credentials: "include" });
  if (!response.ok) throw new Error("Download failed");
  return response.blob();
}

export function downloadBlob(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
