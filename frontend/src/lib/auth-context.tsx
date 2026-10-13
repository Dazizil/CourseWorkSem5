"use client";

import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import { api, tokenStorage } from "./api";
import type { AuthResponse, Role, User } from "./types";

interface AuthContextValue {
  user: User | null;
  loading: boolean;
  login: (login: string, password: string) => Promise<void>;
  register: (data: { email: string; userName: string; password: string; displayName?: string }) => Promise<void>;
  logout: () => Promise<void>;
  reload: () => Promise<void>;
  hasRole: (...roles: Role[]) => boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

async function fetchCurrentUser(): Promise<User | null> {
  if (!tokenStorage.access) return null;
  try {
    return await api<User>("/users/me");
  } catch {
    tokenStorage.clear();
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  const reload = useCallback(async () => {
    setUser(await fetchCurrentUser());
  }, []);

  useEffect(() => {
    fetchCurrentUser()
      .then(setUser)
      .finally(() => setLoading(false));
  }, []);

  const login = useCallback(async (login: string, password: string) => {
    const auth = await api<AuthResponse>("/auth/login", { method: "POST", json: { login, password } });
    tokenStorage.save(auth);
    setUser(auth.user);
  }, []);

  const register = useCallback<AuthContextValue["register"]>(async (data) => {
    const auth = await api<AuthResponse>("/auth/register", { method: "POST", json: data });
    tokenStorage.save(auth);
    setUser(auth.user);
  }, []);

  const logout = useCallback(async () => {
    const refreshToken = tokenStorage.refresh;
    if (refreshToken) {
      await api("/auth/logout", { method: "POST", json: { refreshToken } }).catch(() => undefined);
    }
    tokenStorage.clear();
    setUser(null);
  }, []);

  const hasRole = useCallback((...roles: Role[]) => !!user && roles.some((r) => user.roles.includes(r)), [user]);

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout, reload, hasRole }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside <AuthProvider>");
  return ctx;
}
