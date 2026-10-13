import type { Metadata } from "next";
import { Navbar } from "@/components/Navbar";
import { AuthProvider } from "@/lib/auth-context";
import "./globals.css";

export const metadata: Metadata = {
  title: "SocialNetwork",
  description: "Автоматизированная информационная система социальной сети",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="ru" className="h-full antialiased">
      <body className="flex min-h-full flex-col bg-zinc-50 text-zinc-900">
        <AuthProvider>
          <Navbar />
          <main className="mx-auto w-full max-w-3xl flex-1 px-4 py-6">{children}</main>
        </AuthProvider>
      </body>
    </html>
  );
}
