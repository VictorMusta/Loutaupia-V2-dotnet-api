import { apiFetch } from "./client";

export interface LeaderboardEntry {
  rank: number;
  userId: string;
  displayName: string;
  score: number;
}

export interface MyRank {
  rank: number;
  score: number;
  total: number;
}

export const leaderboardsApi = {
  get: (params?: { scope?: string; period?: string; metric?: string }) => {
    const qs = new URLSearchParams();
    if (params?.scope) qs.set("scope", params.scope);
    if (params?.period) qs.set("period", params.period);
    if (params?.metric) qs.set("metric", params.metric);
    const q = qs.toString();
    return apiFetch<LeaderboardEntry[]>(
      `/leaderboards${q ? `?${q}` : ""}`,
    );
  },

  myRank: (params?: { scope?: string; period?: string; metric?: string }) => {
    const qs = new URLSearchParams();
    if (params?.scope) qs.set("scope", params.scope);
    if (params?.period) qs.set("period", params.period);
    if (params?.metric) qs.set("metric", params.metric);
    const q = qs.toString();
    return apiFetch<MyRank>(`/leaderboards/me${q ? `?${q}` : ""}`);
  },
};
