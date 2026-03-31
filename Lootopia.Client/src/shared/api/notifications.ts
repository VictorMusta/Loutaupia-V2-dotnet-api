import { apiFetch } from "./client";

export interface Notification {
  id: string;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface NotificationPreferences {
  huntUpdates: boolean;
  marketplaceAlerts: boolean;
  achievementAlerts: boolean;
  systemMessages: boolean;
}

export const notificationsApi = {
  list: () => apiFetch<Notification[]>("/notifications"),

  markAsRead: (id: string) =>
    apiFetch<void>(`/notifications/${id}/read`, { method: "POST" }),

  preferences: () =>
    apiFetch<NotificationPreferences>("/notifications/preferences"),

  updatePreferences: (prefs: Partial<NotificationPreferences>) =>
    apiFetch<void>("/notifications/preferences", {
      method: "PUT",
      body: JSON.stringify(prefs),
    }),
};
