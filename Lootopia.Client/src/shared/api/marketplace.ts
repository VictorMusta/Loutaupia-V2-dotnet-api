import { apiFetch } from "./client";

export interface Listing {
  id: string;
  itemId: string;
  itemName: string;
  itemRarity: string;
  itemType: string;
  price: number;
  stock: number;
  sellerId: string;
  sellerName: string;
  status: string;
  createdAt: string;
}

interface ListingsResponse {
  listings: Listing[];
  totalCount: number;
  page: number;
  size: number;
}

export interface ListingsParams {
  type?: string;
  rarity?: string;
  name?: string;
  minPrice?: number;
  maxPrice?: number;
  sort?: string;
}

export const marketplaceApi = {
  listings: async (params?: ListingsParams): Promise<Listing[]> => {
    const qs = new URLSearchParams();
    if (params?.type) qs.set("type", params.type);
    if (params?.rarity) qs.set("rarity", params.rarity);
    if (params?.name) qs.set("name", params.name);
    if (params?.minPrice !== undefined) qs.set("minPrice", String(params.minPrice));
    if (params?.maxPrice !== undefined) qs.set("maxPrice", String(params.maxPrice));
    if (params?.sort) qs.set("sort", params.sort);
    qs.set("size", "100");
    const q = qs.toString();
    const res = await apiFetch<ListingsResponse>(`/marketplace/listings?${q}`);
    return res.listings;
  },

  purchase: (listingId: string) =>
    apiFetch<void>(`/marketplace/listings/${listingId}/purchase`, {
      method: "POST",
      body: JSON.stringify({ quantity: 1 }),
    }),

  create: (itemId: string, price: number, stock: number) =>
    apiFetch<Listing>("/marketplace/listings", {
      method: "POST",
      body: JSON.stringify({ itemId, price, stock }),
    }),

  cancel: (listingId: string) =>
    apiFetch<void>(`/marketplace/listings/${listingId}/cancel`, {
      method: "POST",
    }),

  myListings: async (): Promise<Listing[]> => {
    const res = await apiFetch<ListingsResponse>("/marketplace/listings/mine");
    return res.listings;
  },
};
