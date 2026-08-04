"use client";

import { useCallback, useRef, useState } from "react";
import { Upload, X, FileText, Image, File, CheckCircle2, AlertCircle } from "lucide-react";
import { cn } from "@/lib/utils";

export interface PendingFile {
  file: File;
  id: string;
  status: "pending" | "uploading" | "done" | "error";
  progress: number;
  error?: string;
}

interface FileUploadZoneProps {
  files: PendingFile[];
  onFilesAdd: (files: File[]) => void;
  onFileRemove: (id: string) => void;
  accept?: string;
  multiple?: boolean;
  disabled?: boolean;
  maxFiles?: number;
  maxSizeMb?: number;
  label?: string;
  hint?: string;
}

function getFileIcon(type: string) {
  if (type.startsWith("image/")) return <Image className="h-4 w-4 text-blue-500" />;
  if (type === "application/pdf") return <FileText className="h-4 w-4 text-red-500" />;
  if (type.includes("word") || type.includes("document")) return <FileText className="h-4 w-4 text-blue-600" />;
  return <File className="h-4 w-4 text-slate-400" />;
}

function formatBytes(bytes: number) {
  if (bytes === 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + " " + sizes[i];
}

export function FileUploadZone({
  files,
  onFilesAdd,
  onFileRemove,
  accept,
  multiple = true,
  disabled = false,
  maxFiles = 20,
  maxSizeMb = 50,
  label = "Drop files here or click to browse",
  hint
}: FileUploadZoneProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [isDragging, setIsDragging] = useState(false);

  const handleFiles = useCallback(
    (fileList: FileList | null) => {
      if (!fileList) return;
      const arr = Array.from(fileList);
      const allowed = arr.slice(0, maxFiles - files.length);
      onFilesAdd(allowed);
    },
    [files.length, maxFiles, onFilesAdd]
  );

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setIsDragging(false);
      if (disabled) return;
      handleFiles(e.dataTransfer.files);
    },
    [disabled, handleFiles]
  );

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    if (!disabled) setIsDragging(true);
  }, [disabled]);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  }, []);

  return (
    <div className="space-y-3">
      <div
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onClick={() => !disabled && inputRef.current?.click()}
        className={cn(
          "flex flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed px-4 py-8 text-center transition-all cursor-pointer",
          isDragging
            ? "border-primary-500 bg-primary-50/50"
            : "border-slate-200 hover:border-primary-300 hover:bg-slate-50/50",
          disabled && "cursor-not-allowed opacity-50"
        )}
      >
        <Upload className={cn("h-8 w-8", isDragging ? "text-primary-600" : "text-slate-300")} />
        <p className="text-sm font-medium text-ink">{label}</p>
        <p className="text-xs text-ink-muted">
          {hint || `Max ${maxFiles} files, ${maxSizeMb} MB each`}
        </p>
        <input
          ref={inputRef}
          type="file"
          accept={accept}
          multiple={multiple}
          className="hidden"
          disabled={disabled}
          onChange={(e) => {
            handleFiles(e.target.files);
            e.target.value = "";
          }}
        />
      </div>

      {files.length > 0 && (
        <div className="space-y-2">
          {files.map((f) => (
            <div
              key={f.id}
              className="flex items-center gap-3 rounded-lg border border-line bg-card px-3 py-2"
            >
              {getFileIcon(f.file.type)}
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-medium text-ink">{f.file.name}</p>
                <p className="text-xs text-ink-muted">{formatBytes(f.file.size)}</p>
              </div>
              {f.status === "uploading" && (
                <div className="w-20">
                  <div className="h-1.5 w-full overflow-hidden rounded-full bg-slate-100">
                    <div
                      className="h-full rounded-full bg-primary-600 transition-all duration-300"
                      style={{ width: `${f.progress}%` }}
                    />
                  </div>
                  <p className="mt-0.5 text-center text-[10px] text-ink-muted">{f.progress}%</p>
                </div>
              )}
              {f.status === "done" && (
                <CheckCircle2 className="h-4 w-4 shrink-0 text-emerald-500" />
              )}
              {f.status === "error" && (
                <div className="flex items-center gap-1">
                  <AlertCircle className="h-4 w-4 shrink-0 text-red-500" />
                  <span className="text-xs text-red-500">{f.error}</span>
                </div>
              )}
              {f.status === "pending" && (
                <button
                  onClick={(e) => {
                    e.stopPropagation();
                    onFileRemove(f.id);
                  }}
                  className="shrink-0 cursor-pointer rounded p-1 text-ink-muted transition-colors hover:bg-red-50 hover:text-red-600"
                >
                  <X className="h-4 w-4" />
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
