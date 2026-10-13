"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";

export function Navbar() {
  const { user, logout, hasRole } = useAuth();
  const router = useRouter();

  return (
    <header className="border-b border-zinc-200 bg-white">
      <nav className="mx-auto flex max-w-3xl items-center gap-4 px-4 py-3 text-sm">
        <Link href="/" className="font-semibold">SocialNetwork</Link>
        {user && (
          <>
            <Link href="/users" className="hover:underline">Люди</Link>
            <Link href="/profile" className="hover:underline">Профиль</Link>
            {hasRole("Admin", "Moderator") && <Link href="/admin" className="hover:underline">Администрирование</Link>}
          </>
        )}
        <div className="ml-auto flex items-center gap-3">
          {user ? (
            <>
              <span className="text-zinc-500">@{user.userName}</span>
              <button
                className="rounded border px-2 py-1 hover:bg-zinc-100"
                onClick={async () => {
                  await logout();
                  router.push("/login");
                }}
              >
                Выйти
              </button>
            </>
          ) : (
            <>
              <Link href="/login" className="hover:underline">Вход</Link>
              <Link href="/register" className="hover:underline">Регистрация</Link>
            </>
          )}
        </div>
      </nav>
    </header>
  );
}
