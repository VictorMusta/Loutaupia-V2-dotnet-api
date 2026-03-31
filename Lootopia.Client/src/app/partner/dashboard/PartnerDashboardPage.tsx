import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { partnersApi } from "@/shared/api/partners";
import { useAuth } from "@/shared/providers/AuthProvider";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { Wallet, Gift, Megaphone, TrendingUp } from "lucide-react";
import { formatCurrency } from "@/shared/lib/utils";

export function PartnerDashboardPage() {
  const { user } = useAuth();
  const [dateFrom, setDateFrom] = useState<string>(() => {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return d.toISOString().slice(0, 10);
  });
  const [dateTo, setDateTo] = useState<string>(() =>
    new Date().toISOString().slice(0, 10),
  );

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["partner-report", dateFrom, dateTo],
    queryFn: () => partnersApi.report(dateFrom, dateTo),
  });

  useEffect(() => {
    refetch();
  }, [dateFrom, dateTo, refetch]);

  if (error) {
    return (
      <div className="rounded-lg border border-destructive/50 bg-destructive/10 p-4 text-destructive">
        Failed to load activity report. Please try again.
      </div>
    );
  }

  const budgetRemaining = data ? data.totalBudget - data.totalSpent : 0;

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Partner dashboard</h1>
          <p className="text-sm text-muted-foreground">Welcome, {user?.displayName}</p>
        </div>
        <div className="flex gap-2">
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            className="rounded-md border border-input bg-background px-3 py-2 text-sm"
          />
          <input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            className="rounded-md border border-input bg-background px-3 py-2 text-sm"
          />
        </div>
      </div>

      {/* Stats cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card className="border-border bg-card">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Budget remaining
            </CardTitle>
            <Wallet className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <span className="text-2xl font-bold text-foreground">
                {formatCurrency(budgetRemaining)}
              </span>
            )}
          </CardContent>
        </Card>

        <Card className="border-border bg-card">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Coupons distributed
            </CardTitle>
            <Gift className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <span className="text-2xl font-bold text-foreground">
                {data?.couponsDistributed?.toLocaleString() ?? 0}
              </span>
            )}
          </CardContent>
        </Card>

        <Card className="border-border bg-card">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Active campaigns
            </CardTitle>
            <Megaphone className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <span className="text-2xl font-bold text-foreground">
                {data?.activeCampaigns ?? 0}
              </span>
            )}
          </CardContent>
        </Card>

        <Card className="border-border bg-card">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total spent
            </CardTitle>
            <TrendingUp className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <span className="text-2xl font-bold text-foreground">
                {formatCurrency(data?.totalSpent ?? 0)}
              </span>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Activity report summary */}
      <Card className="border-border bg-card">
        <CardHeader>
          <CardTitle className="text-foreground">Activity report</CardTitle>
          <p className="text-sm text-muted-foreground">
            Period: {dateFrom} → {dateTo}
          </p>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <Skeleton className="h-32 w-full" />
          ) : data ? (
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <div>
                <p className="text-sm text-muted-foreground">Partner</p>
                <p className="font-medium">{data.partnerName}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total budget</p>
                <p className="font-medium">{formatCurrency(data.totalBudget)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total spent</p>
                <p className="font-medium">{formatCurrency(data.totalSpent)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Active campaigns</p>
                <p className="font-medium">{data.activeCampaigns}</p>
              </div>
            </div>
          ) : (
            <p className="text-muted-foreground">No data for this period.</p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
