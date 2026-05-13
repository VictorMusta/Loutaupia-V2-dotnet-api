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

export interface AdminPartner {
  id: string;
  userId: string;
  businessName: string;
  address?: string;
  tokenBudget: number;
  email: string;
  displayName: string;
  isActive: boolean;
  createdAt: string;
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

  listAdmin: async (): Promise<AdminPartner[]> => {
    const res = await apiFetch<{ partners: AdminPartner[] }>("/admin/partners");
    return res.partners;
  },

  createAdmin: (data: {
    email: string;
    password?: string;
    displayName: string;
    businessName: string;
    address?: string;
    initialBudget: number;
  }) =>
    apiFetch<AdminPartner>("/admin/partners", {
      method: "POST",
      body: JSON.stringify({ ...data, password: data.password || "Partner123!" }),
    }),

  creditAdmin: (partnerId: string, amount: number) =>
    apiFetch<void>(`/admin/partners/${partnerId}/credit`, {
      method: "POST",
      body: JSON.stringify({ amount }),
    }),
};
