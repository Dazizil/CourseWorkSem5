"use client";

import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { PostList } from "@/components/PostList";
import { RequireAuth } from "@/components/RequireAuth";
import { api } from "@/lib/api";
import type { User } from "@/lib/types";

export function UserProfile() {
  const { id } = useParams<{ id: string }>();
  const [user, setUser] = useState<User | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api<User>(`/users/${id}`).then(setUser).catch((e: Error) => setError(e.message));
  }, [id]);

  return (
    <RequireAuth>
      {error && <p className="text-red-600">{error}</p>}
      {user && (
        <div className="space-y-6">
          <div className="rounded border bg-white p-4">
            <h1 className="text-xl font-semibold">{user.displayName ?? user.userName}</h1>
            <p className="text-sm text-zinc-500">
              @{user.userName} · {user.roles.join(", ")} {user.isBlocked && "· заблокирован"}
            </p>
            {user.bio && <p className="mt-2 whitespace-pre-wrap">{user.bio}</p>}
          </div>
          <PostList authorId={user.id} />
        </div>
      )}
    </RequireAuth>
  );
}
