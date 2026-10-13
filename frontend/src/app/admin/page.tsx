"use client";

import { useEffect, useState } from "react";
import { RequireAuth } from "@/components/RequireAuth";
import { api } from "@/lib/api";
import { useAuth } from "@/lib/auth-context";
import type { PagedResult, Role, RoleInfo, User } from "@/lib/types";

const ASSIGNABLE_ROLES: Role[] = ["Moderator", "Admin"];

async function fetchAdminData(isAdmin: boolean) {
  const [users, roles] = await Promise.all([
    api<PagedResult<User>>("/users?pageSize=100"),
    isAdmin ? api<RoleInfo[]>("/roles") : Promise.resolve([]),
  ]);
  return { users: users.items, roles };
}

function AdminPanel() {
  const { user: me, hasRole } = useAuth();
  const isAdmin = hasRole("Admin");
  const [users, setUsers] = useState<User[]>([]);
  const [roles, setRoles] = useState<RoleInfo[]>([]);
  const [error, setError] = useState<string | null>(null);

  function apply(data: Awaited<ReturnType<typeof fetchAdminData>>) {
    setUsers(data.users);
    setRoles(data.roles);
  }

  useEffect(() => {
    fetchAdminData(isAdmin)
      .then(apply)
      .catch((e: Error) => setError(e.message));
  }, [isAdmin]);

  async function run(action: () => Promise<unknown>) {
    setError(null);
    try {
      await action();
      apply(await fetchAdminData(isAdmin));
    } catch (e) {
      setError((e as Error).message);
    }
  }

  return (
    <div className="space-y-6">
      <h1 className="text-xl font-semibold">Администрирование</h1>
      {error && <p className="text-red-600">{error}</p>}

      {isAdmin && (
        <section>
          <h2 className="mb-2 font-semibold">Роли</h2>
          <ul className="grid grid-cols-3 gap-2">
            {roles.map((r) => (
              <li key={r.id} className="rounded border bg-white p-3 text-sm">
                <div className="font-medium">{r.name}</div>
                <div className="text-zinc-500">{r.description}</div>
                <div className="text-zinc-500">Пользователей: {r.usersCount}</div>
              </li>
            ))}
          </ul>
        </section>
      )}

      <section>
        <h2 className="mb-2 font-semibold">Пользователи</h2>
        <table className="w-full rounded border bg-white text-sm">
          <thead className="bg-zinc-100 text-left">
            <tr>
              <th className="p-2">Пользователь</th>
              <th className="p-2">Роли</th>
              <th className="p-2">Действия</th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id} className="border-t align-top">
                <td className="p-2">
                  @{u.userName}
                  <div className="text-zinc-500">{u.email}</div>
                  {u.isBlocked && <div className="text-red-600">заблокирован</div>}
                </td>
                <td className="p-2">{u.roles.join(", ")}</td>
                <td className="space-x-2 space-y-1 p-2">
                  {isAdmin &&
                    u.id !== me?.id &&
                    ASSIGNABLE_ROLES.map((role) =>
                      u.roles.includes(role) ? (
                        <button key={role} className="rounded border px-2 py-0.5 hover:bg-zinc-100"
                          onClick={() => run(() => api(`/roles/users/${u.id}/${role}`, { method: "DELETE" }))}>
                          − {role}
                        </button>
                      ) : (
                        <button key={role} className="rounded border px-2 py-0.5 hover:bg-zinc-100"
                          onClick={() => run(() => api(`/roles/users/${u.id}`, { method: "POST", json: { roleName: role } }))}>
                          + {role}
                        </button>
                      ),
                    )}
                  {!u.roles.includes("Admin") && (
                    <button className="rounded border px-2 py-0.5 text-red-600 hover:bg-red-50"
                      onClick={() => run(() => api(`/users/${u.id}/${u.isBlocked ? "unblock" : "block"}`, { method: "POST" }))}>
                      {u.isBlocked ? "Разблокировать" : "Заблокировать"}
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  );
}

export default function AdminPage() {
  return (
    <RequireAuth roles={["Admin", "Moderator"]}>
      <AdminPanel />
    </RequireAuth>
  );
}
