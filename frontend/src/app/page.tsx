"use client";

import { PostList } from "@/components/PostList";
import { RequireAuth } from "@/components/RequireAuth";

export default function FeedPage() {
  return (
    <RequireAuth>
      <h1 className="mb-4 text-xl font-semibold">Лента</h1>
      <PostList canCreate />
    </RequireAuth>
  );
}
