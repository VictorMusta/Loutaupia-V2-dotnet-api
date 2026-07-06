import { apiFetch } from "./client";

export interface FraudAlert {
  id: string;
  userId: string;
  userName: string;
  type: string;
  severity: string;
  description: string;
  status: string;
  createdAt: string;
}

export interface UserSummary {
  id: string;
  email: string;
  displayName: string;
  role: string;
  isFrozen: boolean;
  createdAt: string;
}

export const adminApi = {
  fraudAlerts: (page = 1, size = 20) =>
    apiFetch<{ items: FraudAlert[]; total: number }>(
      `/admin/fraud-alerts?page=${page}&size=${size}`,
    ),

  acknowledgeFraudAlert: (alertId: string) =>
    apiFetch<void>(`/admin/fraud-alerts/${alertId}/acknowledge`, {
      method: "POST",
    }),

  freezeUser: (userId: string) =>
    apiFetch<void>(`/admin/users/${userId}/freeze`, { method: "POST" }),

  unfreezeUser: (userId: string) =>
    apiFetch<void>(`/admin/users/${userId}/unfreeze`, { method: "POST" }),

  users: (page = 1, size = 20, search?: string) => {
    const qs = new URLSearchParams({ page: String(page), size: String(size) });
    if (search) qs.set("search", search);
    return apiFetch<{ items: UserSummary[]; total: number }>(
      `/admin/users?${qs}`,
    );
  },

  creditPartner: (partnerId: string, amount: number) =>
    apiFetch<void>(`/admin/partners/${partnerId}/credit`, {
      method: "POST",
      body: JSON.stringify({ amount }),
    }),

  activityReport: (from?: string, to?: string) => {
    const qs = new URLSearchParams();
    if (from) qs.set("from", from);
    if (to) qs.set("to", to);
    const q = qs.toString();
    return apiFetch<{
      totalUsers: number;
      activeHunts: number;
      pendingAlerts: number;
      registrationsPerDay: { date: string; count: number }[];
      completionsPerWeek: { week: string; count: number }[];
    }>(`/admin/report${q ? `?${q}` : ""}`);
  },

  getFraudSettings: () =>
    apiFetch<Record<string, string>>(`/admin/fraud-settings`),

  updateFraudSettings: (settings: Record<string, string>) =>
    apiFetch<void>(`/admin/fraud-settings`, {
      method: "PUT",
      body: JSON.stringify(settings),
    }),
};
