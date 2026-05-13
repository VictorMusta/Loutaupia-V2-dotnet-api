import { apiFetch } from "./client";

export interface Item {
  id: string;
  name: string;
  description: string;
  rarity: string;
  type: string;
  imageUrl?: string | null;
  isTradeable: boolean;
}

export const itemsApi = {
  list: async (): Promise<Item[]> => {
    const res = await apiFetch<{ items: Item[] }>("/items");
    return res.items;
  },

  create: (data: {
    name: string;
    description: string;
    rarity: string;
    type: string;
    imageUrl?: string;
    isTradeable: boolean;
  }) =>
    apiFetch<{ itemId: string }>("/items", {
      method: "POST",
      body: JSON.stringify(data),
    }),
};
