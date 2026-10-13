export type Role = "User" | "Moderator" | "Admin";

export interface User {
  id: string;
  email: string;
  userName: string;
  displayName: string | null;
  bio: string | null;
  avatarUrl: string | null;
  isBlocked: boolean;
  roles: Role[];
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  user: User;
}

export interface Post {
  id: string;
  content: string;
  authorId: string;
  authorUserName: string;
  authorDisplayName: string | null;
  createdAt: string;
}

export interface RoleInfo {
  id: string;
  name: Role;
  description: string | null;
  usersCount: number;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

/** RFC 7807 ProblemDetails, который возвращает API при ошибках. */
export interface ProblemDetails {
  title?: string;
  status?: number;
  errors?: Record<string, string[]>;
}
