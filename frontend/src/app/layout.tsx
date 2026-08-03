import type { Metadata } from "next";
import { EB_Garamond, Hind_Siliguri, Lato } from "next/font/google";
import { Providers } from "@/lib/providers";
import { LanguageProvider } from "@/lib/i18n";
import { ToastProvider } from "@/components/ui/toast";
import "./globals.css";

const garamond = EB_Garamond({
  subsets: ["latin"],
  weight: ["400", "500", "600", "700"],
  variable: "--font-garamond",
  display: "swap"
});

const lato = Lato({
  subsets: ["latin"],
  weight: ["300", "400", "700", "900"],
  variable: "--font-lato",
  display: "swap"
});

const hindSiliguri = Hind_Siliguri({
  subsets: ["latin", "bengali"],
  weight: ["300", "400", "500", "600", "700"],
  variable: "--font-bengali",
  display: "swap"
});

const APP_NAME = process.env.NEXT_PUBLIC_APP_NAME || "Verdiq";

export const metadata: Metadata = {
  title: {
    default: `${APP_NAME} — Law Firm Management`,
    template: `%s · ${APP_NAME}`
  },
  description: "Law firm and chamber management system for the Bangladesh legal market."
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className={`${garamond.variable} ${lato.variable} ${hindSiliguri.variable} antialiased`}>
        <Providers>
          <LanguageProvider>
            <ToastProvider>{children}</ToastProvider>
          </LanguageProvider>
        </Providers>
      </body>
    </html>
  );
}
