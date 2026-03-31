import { apiFetch } from "./client";

export interface Item {
  itemId: string;
  name: string;
  description: string;
  type: string;
  rarity: string;
  imageUrl: string | null;
  isTradeable: boolean;
  quantity: number;
  acquiredAt: string;
}

export interface InventoryResponse {
  items: Item[];
  totalCount: number;
  page: number;
  size: number;
}

export const inventoryApi = {
  list: (params?: { type?: string; rarity?: string; page?: number; size?: number }) => {
    const qs = new URLSearchParams();
    if (params?.type) qs.set("type", params.type);
    if (params?.rarity) qs.set("rarity", params.rarity);
    if (params?.page) qs.set("page", String(params.page));
    if (params?.size) qs.set("size", String(params.size));
    const q = qs.toString();
    return apiFetch<InventoryResponse>(`/inventory${q ? `?${q}` : ""}`);
  },
};
