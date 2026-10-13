"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { RequireAuth } from "@/components/RequireAuth";
import { api } from "@/lib/api";
import type { PagedResult, User } from "@/lib/types";

export default function UsersPage() {
  const [query, setQuery] = useState("");
  const [users, setUsers] = useState<User[]>([]);

  useEffect(() => {
    const timer = setTimeout(() => {
      api<PagedResult<User>>(`/users?query=${encodeURIComponent(query)}`)
        .then((r) => setUsers(r.items))
        .catch(() => setUsers([]));
    }, 300);
    return () => clearTimeout(timer);
  }, [query]);

  return (
    <RequireAuth>
      <h1 className="mb-4 text-xl font-semibold">Люди</h1>
      <input className="mb-4 w-full rounded border p-2" placeholder="Поиск по имени"
        value={query} onChange={(e) => setQuery(e.target.value)} />
      <ul className="divide-y rounded border bg-white">
        {users.map((u) => (
          <li key={u.id} className="p-3">
            <Link href={`/users/${u.id}`} className="hover:underline">
              {u.displayName ?? u.userName} <span className="text-zinc-500">@{u.userName}</span>
            </Link>
          </li>
        ))}
      </ul>
    </RequireAuth>
  );
}
