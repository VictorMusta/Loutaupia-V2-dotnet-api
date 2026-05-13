import { apiFetch } from "./client";

export interface Campaign {
  id: string;
  partnerId: string;
  partnerName: string;
  title: string;
  description: string;
  budget: number;
  spent: number;
  status: string;
  startDate: string;
  endDate: string;
  createdAt: string;
  startedCount?: number;
  completedCount?: number;
  couponsDistributed?: number;
  maxCoupons?: number;
  averageCompletionMinutes?: number;
  dailyTracking?: { date: string; explorations: number }[];
}

export const campaignsApi = {
  list: () => apiFetch<Campaign[]>("/campaigns"),

  mine: () => apiFetch<Campaign[]>("/campaigns/mine"),

  create: (data: {
    title: string;
    description: string;
    budget: number;
    startDate: string;
    endDate: string;
  }) =>
    apiFetch<Campaign>("/campaigns", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  activate: (id: string) =>
    apiFetch<void>(`/campaigns/${id}/activate`, { method: "POST" }),
};
