import { apiFetch } from "./client";

export interface PartnerReport {
  partnerId: string;
  partnerName: string;
  totalBudget: number;
  totalSpent: number;
  activeCampaigns: number;
  couponsDistributed: number;
  period: { from: string; to: string };
}

export const partnersApi = {
  report: (from?: string, to?: string) => {
    const qs = new URLSearchParams();
    if (from) qs.set("from", from);
    if (to) qs.set("to", to);
    const q = qs.toString();
    return apiFetch<PartnerReport>(
      `/partners/me/report${q ? `?${q}` : ""}`,
    );
  },
};
