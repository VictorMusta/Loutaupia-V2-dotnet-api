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
  minIncrement: number;
  minBid?: number;
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

interface AuctionSummaryDto {
  id: string;
  sellerId: string;
  itemId: string;
  reservePrice: number;
  minIncrement: number;
  endTime: string;
  status: string;
  currentHighestBid?: number | null;
  currentPrice: number;
  bidCount: number;
  itemName?: string | null;
  itemRarity?: string | null;
  itemImageUrl?: string | null;
  sellerName?: string | null;
}

interface AuctionsListResponse {
  items: AuctionSummaryDto[];
  totalCount: number;
}

interface AuctionDetailDto {
  id: string;
  sellerId: string;
  sellerName?: string | null;
  itemId: string;
  reservePrice: number;
  minIncrement: number;
  startTime: string;
  endTime: string;
  status: string;
  currentHighestBid?: number | null;
  currentPrice: number;
  minBid: number;
  bidCount: number;
  itemName?: string | null;
  itemImageUrl?: string | null;
  bids: Array<{
    id: string;
    bidderId: string;
    bidderName?: string | null;
    amount: number;
    createdAt: string;
  }>;
}

function mapAuctionSummary(dto: AuctionSummaryDto): Auction {
  return {
    id: dto.id,
    itemId: dto.itemId,
    itemName: dto.itemName ?? "Objet",
    itemRarity: dto.itemRarity ?? "Common",
    sellerId: dto.sellerId,
    sellerName: dto.sellerName ?? "—",
    startingPrice: dto.reservePrice,
    currentPrice: dto.currentPrice,
    minIncrement: dto.minIncrement,
    bidCount: dto.bidCount,
    endsAt: dto.endTime,
    status: dto.status,
    createdAt: dto.endTime,
  };
}

function mapAuctionDetail(dto: AuctionDetailDto): Auction & { bids: Bid[] } {
  return {
    ...mapAuctionSummary({
      id: dto.id,
      sellerId: dto.sellerId,
      itemId: dto.itemId,
      reservePrice: dto.reservePrice,
      minIncrement: dto.minIncrement,
      endTime: dto.endTime,
      status: dto.status,
      currentHighestBid: dto.currentHighestBid,
      currentPrice: dto.currentPrice,
      bidCount: dto.bidCount,
      itemName: dto.itemName,
      itemRarity: undefined,
      itemImageUrl: dto.itemImageUrl,
      sellerName: dto.sellerName,
    }),
    sellerName: dto.sellerName ?? "—",
    minBid: dto.minBid,
    bids: dto.bids.map((bid) => ({
      id: bid.id,
      auctionId: dto.id,
      bidderId: bid.bidderId,
      bidderName: bid.bidderName ?? "—",
      amount: bid.amount,
      createdAt: bid.createdAt,
    })),
  };
}

export const auctionsApi = {
  list: async () => {
    const res = await apiFetch<AuctionsListResponse>("/auctions?page=1&size=100");
    return res.items.map(mapAuctionSummary);
  },

  get: async (id: string) => {
    const res = await apiFetch<AuctionDetailDto>(`/auctions/${id}`);
    return mapAuctionDetail(res);
  },

  create: (
    itemId: string,
    reservePrice: number,
    minIncrement: number,
    durationMinutes: number,
  ) =>
    apiFetch<Auction>("/auctions", {
      method: "POST",
      body: JSON.stringify({ itemId, reservePrice, minIncrement, durationMinutes }),
    }),

  bid: (auctionId: string, amount: number) =>
    apiFetch<void>(`/auctions/${auctionId}/bid`, {
      method: "POST",
      body: JSON.stringify({ amount }),
    }),

  close: (auctionId: string) =>
    apiFetch<void>(`/auctions/${auctionId}/close`, { method: "POST" }),
};
