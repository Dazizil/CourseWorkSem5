"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useAuth } from "@/lib/auth-context";

export default function RegisterPage() {
  const { register } = useAuth();
  const router = useRouter();
  const [form, setForm] = useState({ email: "", userName: "", displayName: "", password: "" });
  const [error, setError] = useState<string | null>(null);

  async function submit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    try {
      await register({ ...form, displayName: form.displayName || undefined });
      router.push("/");
    } catch (err) {
      setError((err as Error).message);
    }
  }

  const field = (key: keyof typeof form, placeholder: string, type = "text") => (
    <input className="w-full rounded border p-2" type={type} placeholder={placeholder}
      value={form[key]} onChange={(e) => setForm({ ...form, [key]: e.target.value })} />
  );

  return (
    <form onSubmit={submit} className="mx-auto max-w-sm space-y-3">
      <h1 className="text-xl font-semibold">Регистрация</h1>
      {field("email", "Email", "email")}
      {field("userName", "Имя пользователя (латиница, цифры, _ .)")}
      {field("displayName", "Отображаемое имя (необязательно)")}
      {field("password", "Пароль (минимум 8 символов)", "password")}
      {error && <p className="text-red-600">{error}</p>}
      <button className="w-full rounded bg-zinc-900 py-2 text-white">Создать аккаунт</button>
      <p className="text-sm text-zinc-500">
        Уже есть аккаунт? <Link href="/login" className="underline">Войти</Link>
      </p>
    </form>
  );
}
