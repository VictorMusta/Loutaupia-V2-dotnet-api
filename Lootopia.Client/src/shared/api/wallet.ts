import { apiFetch } from "./client";

export interface Wallet {
  id: string;
  userId: string;
  balance: number;
  currency: string;
}

export interface Transaction {
  id: string;
  walletId: string;
  amount: number;
  type: string;
  description: string;
  createdAt: string;
}

export interface PaginatedTransactions {
  items: Transaction[];
  total: number;
  page: number;
  size: number;
}

export const walletApi = {
  get: () => apiFetch<Wallet>("/wallet"),

  transactions: (page = 1, size = 20) =>
    apiFetch<PaginatedTransactions>(
      `/wallet/transactions?page=${page}&size=${size}`,
    ),

  credit: (userId: string, amount: number, description: string) =>
    apiFetch<void>("/wallet/credit", {
      method: "POST",
      body: JSON.stringify({ userId, amount, description }),
    }),
};
