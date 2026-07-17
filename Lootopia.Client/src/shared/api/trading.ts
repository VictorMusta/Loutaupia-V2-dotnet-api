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

interface TradeOfferItemDto {
  itemId?: string | null;
  itemName?: string | null;
  quantity: number;
  tokenAmount: number;
}

interface TradeOfferDto {
  id: string;
  initiatorId: string;
  initiatorName?: string | null;
  receiverId: string;
  receiverName?: string | null;
  status: string;
  expiresAt: string;
  createdAt: string;
  offeredItems: TradeOfferItemDto[];
  requestedItems: TradeOfferItemDto[];
}

interface TradesListResponse {
  offers: TradeOfferDto[];
  totalCount: number;
  page: number;
  size: number;
}

function mapTradeOffer(dto: TradeOfferDto): TradeOffer {
  return {
    id: dto.id,
    fromUserId: dto.initiatorId,
    fromUserName: dto.initiatorName ?? "—",
    toUserId: dto.receiverId,
    toUserName: dto.receiverName ?? "—",
    offeredItemIds: dto.offeredItems
      .map((item) => item.itemId)
      .filter((id): id is string => !!id),
    requestedItemIds: dto.requestedItems
      .map((item) => item.itemId)
      .filter((id): id is string => !!id),
    status: dto.status,
    createdAt: dto.createdAt,
  };
}

export const tradingApi = {
  list: async () => {
    const res = await apiFetch<TradesListResponse>("/trading/offers?size=100");
    return res.offers.map(mapTradeOffer);
  },

  create: (
    receiverId: string,
    offeredItemIds: string[],
    requestedItemIds: string[],
  ) =>
    apiFetch<TradeOffer>("/trading/offers", {
      method: "POST",
      body: JSON.stringify({
        receiverId,
        offeredItems: offeredItemIds.map((itemId) => ({
          itemId,
          quantity: 1,
          tokenAmount: 0,
        })),
        requestedItems: requestedItemIds.map((itemId) => ({
          itemId,
          quantity: 1,
          tokenAmount: 0,
        })),
      }),
    }),

  respond: (offerId: string, accept: boolean) =>
    apiFetch<void>(`/trading/offers/${offerId}/respond`, {
      method: "POST",
      body: JSON.stringify({ action: accept ? "accept" : "refuse" }),
    }),
};
