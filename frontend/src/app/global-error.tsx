"use client";

import { useEffect } from "react";

export default function GlobalError({
  error,
  reset
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error("[Verdiq] global error:", error);
  }, [error]);

  return (
    <html lang="en">
      <body
        style={{
          backgroundColor: "#f8fafc",
          color: "#0f172a",
          fontFamily: "system-ui, -apple-system, sans-serif",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          minHeight: "100vh",
          margin: 0,
          padding: 24
        }}
      >
        <div style={{ maxWidth: 420, textAlign: "center" }}>
          <div
            style={{
              fontSize: 14,
              fontWeight: 600,
              color: "#b91c1c",
              textTransform: "uppercase",
              letterSpacing: "0.08em"
            }}
          >
            Critical error
          </div>
          <h1
            style={{
              fontFamily: "Georgia, serif",
              fontSize: 26,
              fontWeight: 600,
              margin: "12px 0 8px"
            }}
          >
            Verdiq hit an unexpected error
          </h1>
          <p style={{ fontSize: 14, color: "#64748b", lineHeight: 1.6, margin: "0 0 20px" }}>
            Something went wrong at the app level. Try again — if it persists, restart the dev
            server.
          </p>
          <button
            onClick={reset}
            style={{
              cursor: "pointer",
              border: "none",
              borderRadius: 10,
              padding: "10px 20px",
              fontSize: 14,
              fontWeight: 600,
              backgroundColor: "#1e3a8a",
              color: "#fff"
            }}
          >
            Try again
          </button>
        </div>
      </body>
    </html>
  );
}
