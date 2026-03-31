import { apiFetch } from "./client";

export interface Auction {
  id: string;
  itemId: string;
  itemName: string;
  itemRarity: string;
  sellerId: string;
  sellerName: string;
  startingPrice: number;
  currentPrice: number;
  bidCount: number;
  endsAt: string;
  status: string;
  createdAt: string;
}

export interface Bid {
  id: string;
  auctionId: string;
  bidderId: string;
  bidderName: string;
  amount: number;
  createdAt: string;
}

export const auctionsApi = {
  list: () => apiFetch<Auction[]>("/auctions"),

  get: (id: string) => apiFetch<Auction & { bids: Bid[] }>(`/auctions/${id}`),

  create: (itemId: string, startingPrice: number, durationHours: number) =>
    apiFetch<Auction>("/auctions", {
      method: "POST",
      body: JSON.stringify({ itemId, startingPrice, durationHours }),
    }),

  bid: (auctionId: string, amount: number) =>
    apiFetch<void>(`/auctions/${auctionId}/bid`, {
      method: "POST",
      body: JSON.stringify({ amount }),
    }),

  close: (auctionId: string) =>
    apiFetch<void>(`/auctions/${auctionId}/close`, { method: "POST" }),
};
