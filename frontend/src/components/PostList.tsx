"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { useAuth } from "@/lib/auth-context";
import type { PagedResult, Post } from "@/lib/types";

async function fetchPosts(authorId?: string) {
  const query = authorId ? `?authorId=${authorId}` : "";
  return (await api<PagedResult<Post>>(`/posts${query}`)).items;
}

/** Лента постов с формой публикации. Если задан authorId — только посты этого автора. */
export function PostList({ authorId, canCreate = false }: { authorId?: string; canCreate?: boolean }) {
  const { user, hasRole } = useAuth();
  const [posts, setPosts] = useState<Post[]>([]);
  const [content, setContent] = useState("");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchPosts(authorId)
      .then(setPosts)
      .catch((e: Error) => setError(e.message));
  }, [authorId]);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await api("/posts", { method: "POST", json: { content } });
      setContent("");
      setPosts(await fetchPosts(authorId));
    } catch (err) {
      setError((err as Error).message);
    }
  }

  async function remove(id: string) {
    try {
      await api(`/posts/${id}`, { method: "DELETE" });
      setPosts((p) => p.filter((x) => x.id !== id));
    } catch (err) {
      setError((err as Error).message);
    }
  }

  const canModerate = hasRole("Moderator", "Admin");

  return (
    <div className="space-y-4">
      {canCreate && (
        <form onSubmit={submit} className="space-y-2">
          <textarea
            className="w-full rounded border p-2"
            rows={3}
            placeholder="Что нового?"
            value={content}
            onChange={(e) => setContent(e.target.value)}
          />
          <button className="rounded bg-zinc-900 px-3 py-1.5 text-white disabled:opacity-50" disabled={!content.trim()}>
            Опубликовать
          </button>
        </form>
      )}
      {error && <p className="text-red-600">{error}</p>}
      {posts.length === 0 && <p className="text-zinc-500">Постов пока нет.</p>}
      {posts.map((post) => (
        <article key={post.id} className="rounded border bg-white p-4">
          <div className="mb-2 flex items-center justify-between text-sm text-zinc-500">
            <Link href={`/users/${post.authorId}`} className="font-medium text-zinc-900 hover:underline">
              {post.authorDisplayName ?? post.authorUserName} <span className="text-zinc-500">@{post.authorUserName}</span>
            </Link>
            <time>{new Date(post.createdAt).toLocaleString("ru-RU")}</time>
          </div>
          <p className="whitespace-pre-wrap">{post.content}</p>
          {(post.authorId === user?.id || canModerate) && (
            <button className="mt-2 text-sm text-red-600 hover:underline" onClick={() => remove(post.id)}>
              Удалить
            </button>
          )}
        </article>
      ))}
    </div>
  );
}
