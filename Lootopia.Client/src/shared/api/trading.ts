import { apiFetch } from "./client";

export interface TradeOffer {
  id: string;
  fromUserId: string;
  fromUserName: string;
  toUserId: string;
  toUserName: string;
  offeredItemIds: string[];
  requestedItemIds: string[];
  status: string;
  createdAt: string;
}

export const tradingApi = {
  list: () => apiFetch<TradeOffer[]>("/trading/offers"),

  create: (toUserId: string, offeredItemIds: string[], requestedItemIds: string[]) =>
    apiFetch<TradeOffer>("/trading/offers", {
      method: "POST",
      body: JSON.stringify({ toUserId, offeredItemIds, requestedItemIds }),
    }),

  respond: (offerId: string, accept: boolean) =>
    apiFetch<void>(`/trading/offers/${offerId}/respond`, {
      method: "POST",
      body: JSON.stringify({ accept }),
    }),
};
