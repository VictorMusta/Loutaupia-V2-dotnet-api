import { apiFetch } from "./client";

export interface Achievement {
  id: string;
  name: string;
  description: string;
  rarity: string;
  points: number;
  unlockedAt: string | null;
  isUnlocked: boolean;
}

export const achievementsApi = {
  list: (filter?: "all" | "unlocked" | "locked") => {
    const qs = filter && filter !== "all" ? `?filter=${filter}` : "";
    return apiFetch<Achievement[]>(`/achievements${qs}`);
  },
};
