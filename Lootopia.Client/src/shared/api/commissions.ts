import { apiFetch } from "./client";

export interface CommissionSchema {
  id: string;
  name: string;
  rate: number;
  type: string;
}

export interface Payout {
  id: string;
  partnerId: string;
  amount: number;
  status: string;
  createdAt: string;
}

export const commissionsApi = {
  schemas: () => apiFetch<CommissionSchema[]>("/commissions/schemas"),
  payouts: () => apiFetch<Payout[]>("/commissions/payouts"),
};
