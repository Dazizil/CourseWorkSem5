import type { AuthResponse, ProblemDetails } from "./types";

const ACCESS_KEY = "sn.accessToken";
const REFRESH_KEY = "sn.refreshToken";

export class ApiError extends Error {
  constructor(public status: number, message: string, public details?: ProblemDetails) {
    super(message);
  }
}

export const tokenStorage = {
  get access() {
    return typeof window === "undefined" ? null : localStorage.getItem(ACCESS_KEY);
  },
  get refresh() {
    return typeof window === "undefined" ? null : localStorage.getItem(REFRESH_KEY);
  },
  save(auth: AuthResponse) {
    localStorage.setItem(ACCESS_KEY, auth.accessToken);
    localStorage.setItem(REFRESH_KEY, auth.refreshToken);
  },
  clear() {
    localStorage.removeItem(ACCESS_KEY);
    localStorage.removeItem(REFRESH_KEY);
  },
};

let refreshPromise: Promise<boolean> | null = null;

/** Обновляет пару токенов. Параллельные вызовы ждут один и тот же запрос. */
async function refreshTokens(): Promise<boolean> {
  const refreshToken = tokenStorage.refresh;
  if (!refreshToken) return false;

  refreshPromise ??= (async () => {
    try {
      const res = await fetch("/api/auth/refresh", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken }),
      });
      if (!res.ok) {
        tokenStorage.clear();
        return false;
      }
      tokenStorage.save((await res.json()) as AuthResponse);
      return true;
    } finally {
      refreshPromise = null;
    }
  })();
  return refreshPromise;
}

async function toError(res: Response): Promise<ApiError> {
  let details: ProblemDetails | undefined;
  try {
    details = (await res.json()) as ProblemDetails;
  } catch {
    /* тело не JSON */
  }
  const validation = details?.errors ? Object.values(details.errors).flat().join(" ") : "";
  return new ApiError(res.status, validation || details?.title || `Ошибка ${res.status}`, details);
}

export async function api<T = void>(path: string, init: RequestInit & { json?: unknown } = {}, retry = true): Promise<T> {
  const { json, ...rest } = init;
  const headers = new Headers(rest.headers);
  if (json !== undefined) headers.set("Content-Type", "application/json");
  const token = tokenStorage.access;
  if (token) headers.set("Authorization", `Bearer ${token}`);

  const res = await fetch(`/api${path}`, {
    ...rest,
    headers,
    body: json !== undefined ? JSON.stringify(json) : rest.body,
  });

  if (res.status === 401 && retry && (await refreshTokens())) {
    return api<T>(path, init, false);
  }
  if (!res.ok) throw await toError(res);
  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}
