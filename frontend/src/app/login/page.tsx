"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useAuth } from "@/lib/auth-context";

export default function LoginPage() {
  const { login } = useAuth();
  const router = useRouter();
  const [form, setForm] = useState({ login: "", password: "" });
  const [error, setError] = useState<string | null>(null);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await login(form.login, form.password);
      router.push("/");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  return (
    <form onSubmit={submit} className="mx-auto max-w-sm space-y-3">
      <h1 className="text-xl font-semibold">Вход</h1>
      <input className="w-full rounded border p-2" placeholder="Email или имя пользователя"
        value={form.login} onChange={(e) => setForm({ ...form, login: e.target.value })} />
      <input className="w-full rounded border p-2" type="password" placeholder="Пароль"
        value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
      {error && <p className="text-red-600">{error}</p>}
      <button className="w-full rounded bg-zinc-900 py-2 text-white">Войти</button>
      <p className="text-sm text-zinc-500">
        Нет аккаунта? <Link href="/register" className="underline">Зарегистрироваться</Link>
      </p>
    </form>
  );
}
