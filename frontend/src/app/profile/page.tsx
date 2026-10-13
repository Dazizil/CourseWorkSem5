"use client";

import { useState } from "react";
import { PostList } from "@/components/PostList";
import { RequireAuth } from "@/components/RequireAuth";
import { api } from "@/lib/api";
import { useAuth } from "@/lib/auth-context";
import type { User } from "@/lib/types";

function ProfileEditor({ user }: { user: User }) {
  const { reload } = useAuth();
  const [form, setForm] = useState({
    displayName: user.displayName ?? "",
    bio: user.bio ?? "",
    avatarUrl: user.avatarUrl ?? "",
  });
  const [message, setMessage] = useState<string | null>(null);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    try {
      await api("/users/me", {
        method: "PUT",
        json: {
          displayName: form.displayName || null,
          bio: form.bio || null,
          avatarUrl: form.avatarUrl || null,
        },
      });
      await reload();
      setMessage("Сохранено");
    } catch (err) {
      setMessage((err as Error).message);
    }
  }

  return (
    <form onSubmit={submit} className="space-y-2 rounded border bg-white p-4">
      <p className="text-sm text-zinc-500">
        @{user.userName} · {user.email} · роли: {user.roles.join(", ")}
      </p>
      <input className="w-full rounded border p-2" placeholder="Отображаемое имя"
        value={form.displayName} onChange={(e) => setForm({ ...form, displayName: e.target.value })} />
      <textarea className="w-full rounded border p-2" placeholder="О себе" rows={3}
        value={form.bio} onChange={(e) => setForm({ ...form, bio: e.target.value })} />
      <input className="w-full rounded border p-2" placeholder="URL аватара"
        value={form.avatarUrl} onChange={(e) => setForm({ ...form, avatarUrl: e.target.value })} />
      <div className="flex items-center gap-3">
        <button className="rounded bg-zinc-900 px-3 py-1.5 text-white">Сохранить</button>
        {message && <span className="text-sm text-zinc-500">{message}</span>}
      </div>
    </form>
  );
}

export default function ProfilePage() {
  const { user } = useAuth();
  return (
    <RequireAuth>
      {user && (
        <div className="space-y-6">
          <h1 className="text-xl font-semibold">Мой профиль</h1>
          <ProfileEditor user={user} />
          <h2 className="text-lg font-semibold">Мои посты</h2>
          <PostList authorId={user.id} canCreate />
        </div>
      )}
    </RequireAuth>
  );
}
