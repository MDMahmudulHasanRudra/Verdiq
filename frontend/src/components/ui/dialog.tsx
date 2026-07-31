"use client";

import { useEffect, useRef, type ReactNode } from "react";
import { X } from "lucide-react";
import { cn } from "@/lib/utils";

export function Dialog({
  open,
  onClose,
  title,
  description,
  children,
  size = "md",
  footer
}: {
  open: boolean;
  onClose: () => void;
  title?: ReactNode;
  description?: ReactNode;
  children: ReactNode;
  size?: "sm" | "md" | "lg" | "xl";
  footer?: ReactNode;
}) {
  const ref = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    if (open && !dialog.open) {
      dialog.showModal();
    } else if (!open && dialog.open) {
      dialog.close();
    }
  }, [open]);

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    const onCloseEvent = () => {
      if (dialog.open) onClose();
    };
    dialog.addEventListener("close", onCloseEvent);
    return () => dialog.removeEventListener("close", onCloseEvent);
  }, [onClose]);

  const sizes = { sm: "max-w-md", md: "max-w-2xl", lg: "max-w-4xl", xl: "max-w-6xl" };

  return (
    <dialog
      ref={ref}
      className={cn(
        "m-auto w-[calc(100vw-2rem)] rounded-xl border border-line bg-card p-0 shadow-pop backdrop:bg-slate-900/40 backdrop:backdrop-blur-sm",
        sizes[size]
      )}
    >
      <div className="flex items-start justify-between gap-4 border-b border-line-soft px-6 py-4">
        <div>
          {title ? <h2 className="font-display text-lg font-semibold text-ink">{title}</h2> : null}
          {description ? <p className="mt-0.5 text-sm text-ink-muted">{description}</p> : null}
        </div>
        <button
          onClick={onClose}
          className="shrink-0 cursor-pointer rounded-lg p-1 text-ink-muted transition-colors hover:bg-slate-100 hover:text-ink"
          aria-label="Close"
        >
          <X className="h-5 w-5" />
        </button>
      </div>
      <div className="max-h-[70vh] overflow-y-auto px-6 py-5">{children}</div>
      {footer ? <div className="flex items-center justify-end gap-2 border-t border-line-soft px-6 py-4">{footer}</div> : null}
    </dialog>
  );
}
