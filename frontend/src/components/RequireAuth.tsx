"use client";

import { useRouter } from "next/navigation";
import { useEffect, type ReactNode } from "react";
import { useAuth } from "@/lib/auth-context";
import type { Role } from "@/lib/types";

/** Клиентская защита страницы. Настоящая проверка прав всегда выполняется на бэкенде. */
export function RequireAuth({ roles, children }: { roles?: Role[]; children: ReactNode }) {
  const { user, loading, hasRole } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!loading && !user) router.replace("/login");
  }, [loading, user, router]);

  if (loading || !user) return <p className="text-zinc-500">Загрузка…</p>;
  if (roles && !hasRole(...roles)) return <p className="text-red-600">Недостаточно прав.</p>;
  return <>{children}</>;
}
