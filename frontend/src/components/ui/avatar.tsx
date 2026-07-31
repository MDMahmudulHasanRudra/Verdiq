"use client";

import { cn, initials } from "@/lib/utils";

export function Avatar({
  name,
  src,
  size = "md",
  className
}: {
  name: string | null | undefined;
  src?: string | null;
  size?: "sm" | "md" | "lg" | "xl";
  className?: string;
}) {
  const sizes = {
    sm: "h-8 w-8 text-xs",
    md: "h-10 w-10 text-sm",
    lg: "h-14 w-14 text-lg",
    xl: "h-20 w-20 text-2xl"
  };
  if (src) {
    return (
      // eslint-disable-next-line @next/next/no-img-element
      <img
        src={src}
        alt={name || ""}
        className={cn("shrink-0 rounded-full object-cover ring-1 ring-line", sizes[size], className)}
      />
    );
  }
  return (
    <div
      className={cn(
        "flex shrink-0 items-center justify-center rounded-full bg-primary-700 font-semibold text-white",
        sizes[size],
        className
      )}
    >
      {initials(name)}
    </div>
  );
}
