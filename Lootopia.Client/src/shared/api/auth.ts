import { apiFetch } from "./client";

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
}

export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  role: string;
  createdAt: string;
}

export const authApi = {
  login: (email: string, password: string) =>
    apiFetch<AuthResponse>("/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    }),

  register: (email: string, password: string, displayName: string) =>
    apiFetch<AuthResponse>("/auth/register", {
      method: "POST",
      body: JSON.stringify({ email, password, displayName }),
    }),

  guest: (deviceId: string) =>
    apiFetch<AuthResponse>("/auth/guest", {
      method: "POST",
      body: JSON.stringify({ deviceId }),
    }),

  refresh: (refreshToken: string) =>
    apiFetch<AuthResponse>("/auth/refresh", {
      method: "POST",
      body: JSON.stringify({ refreshToken }),
    }),

  upgrade: (email: string, password: string, displayName: string) =>
    apiFetch<AuthResponse>("/auth/upgrade", {
      method: "POST",
      body: JSON.stringify({ email, password, displayName }),
    }),

  requestMagicLink: (email: string) =>
    apiFetch<{ message: string }>("/auth/magic-link", {
      method: "POST",
      body: JSON.stringify({ email }),
    }),

  verifyMagicLink: (token: string) =>
    apiFetch<AuthResponse>(`/auth/magic-link/verify?token=${token}`),
};
