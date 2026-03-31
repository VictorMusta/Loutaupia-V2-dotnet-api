import { apiFetch } from "./client";

export interface Hunt {
  id: string;
  title: string;
  description: string;
  difficulty: number;
  rewardTokens: number;
  status: string;
  stepCount: number;
  distanceKm?: number;
  latitude: number;
  longitude: number;
}

export interface HuntStep {
  order: number;
  clue: string;
  actionType: string;
  radiusMeters: number;
  latitude: number;
  longitude: number;
  validated: boolean;
}

export interface PlayerHunt {
  huntId: string;
  huntTitle: string;
  status: string;
  currentStep: number;
  startedAt: string;
  completedAt: string | null;
  steps: HuntStep[];
}

export interface StepValidationResult {
  success: boolean;
  message: string;
  reward?: number;
  huntCompleted?: boolean;
}

interface ListHuntsResponse {
  hunts: Hunt[];
}

export interface AdminHunt {
  id: string;
  title: string;
  description: string;
  difficulty: number;
  stepCount: number;
  rewardTokens: number;
  status: string;
  startDate: string | null;
  createdBy: string;
}

export const huntsApi = {
  list: async (params?: { lat?: number; lng?: number; radius?: number }): Promise<Hunt[]> => {
    const qs = new URLSearchParams();
    if (params?.lat) qs.set("lat", String(params.lat));
    if (params?.lng) qs.set("lng", String(params.lng));
    if (params?.radius) qs.set("radius", String(params.radius));
    const q = qs.toString();
    const res = await apiFetch<ListHuntsResponse>(`/hunts${q ? `?${q}` : ""}`);
    return res.hunts;
  },

  listAll: async (): Promise<AdminHunt[]> => {
    const res = await apiFetch<{ hunts: AdminHunt[] }>("/hunts/admin/all");
    return res.hunts;
  },

  get: (id: string) => apiFetch<Hunt>(`/hunts/${id}`),

  create: (data: {
    title: string;
    description: string;
    difficulty: number;
    rewardTokens: number;
    steps: { clue: string; latitude: number; longitude: number; radiusMeters: number; actionType: string }[];
  }) =>
    apiFetch<{ huntId: string }>("/hunts", {
      method: "POST",
      body: JSON.stringify(data),
    }),

  activate: (id: string) =>
    apiFetch<void>(`/hunts/${id}/activate`, { method: "POST" }),

  start: (huntId: string) =>
    apiFetch<PlayerHunt>(`/hunts/${huntId}/start`, { method: "POST" }),

  validateStep: (huntId: string, stepOrder: number, lat: number, lng: number) =>
    apiFetch<StepValidationResult>(
      `/hunts/${huntId}/steps/${stepOrder}/validate`,
      {
        method: "POST",
        body: JSON.stringify({ latitude: lat, longitude: lng }),
      },
    ),

  myHunts: async (): Promise<PlayerHunt[]> => {
    const res = await apiFetch<{ hunts: PlayerHunt[] }>("/hunts/my");
    return res.hunts;
  },
};
