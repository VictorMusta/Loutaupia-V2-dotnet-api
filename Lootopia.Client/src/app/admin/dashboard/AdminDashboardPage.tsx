import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  BarChart,
  Bar,
} from "recharts";
import { adminApi } from "@/shared/api/admin";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { Users, Map, ShieldAlert, TrendingUp } from "lucide-react";

export function AdminDashboardPage() {
  const [dateFrom, setDateFrom] = useState<string>(() => {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return d.toISOString().slice(0, 10);
  });
  const [dateTo, setDateTo] = useState<string>(() =>
    new Date().toISOString().slice(0, 10),
  );

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["admin-activity-report", dateFrom, dateTo],
    queryFn: () => adminApi.activityReport(dateFrom, dateTo),
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

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold text-foreground">Dashboard</h1>
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
              Total Users
            </CardTitle>
            <Users className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <span className="text-2xl font-bold text-foreground">
                {data?.totalUsers?.toLocaleString() ?? 0}
              </span>
            )}
          </CardContent>
        </Card>

        <Card className="border-border bg-card">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Active Hunts
            </CardTitle>
            <Map className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <span className="text-2xl font-bold text-foreground">
                {data?.activeHunts ?? 0}
              </span>
            )}
          </CardContent>
        </Card>

        <Card className="border-border bg-card">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Pending Fraud Alerts
            </CardTitle>
            <ShieldAlert className="h-4 w-4 text-destructive" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-24" />
            ) : (
              <span className="text-2xl font-bold text-foreground">
                {data?.pendingAlerts ?? 0}
              </span>
            )}
          </CardContent>
        </Card>

        <Card className="border-border bg-card">
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Period
            </CardTitle>
            <TrendingUp className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            <span className="text-sm text-muted-foreground">
              {dateFrom} → {dateTo}
            </span>
          </CardContent>
        </Card>
      </div>

      {/* Charts */}
      <div className="grid gap-6 lg:grid-cols-2">
        <Card className="border-border bg-card">
          <CardHeader>
            <CardTitle className="text-foreground">Registrations per day</CardTitle>
          </CardHeader>
          <CardContent className="pt-0">
              {isLoading ? (
                <Skeleton className="h-64 w-full" />
              ) : (
                <div className="h-64">
                  <ResponsiveContainer width="100%" height="100%">
                    <LineChart
                      data={data?.registrationsPerDay ?? []}
                      margin={{ top: 5, right: 5, left: 0, bottom: 0 }}
                    >
                      <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                      <XAxis
                        dataKey="date"
                        stroke="hsl(var(--color-muted-foreground))"
                        fontSize={12}
                      />
                      <YAxis stroke="hsl(var(--color-muted-foreground))" fontSize={12} />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: "hsl(var(--color-card))",
                          border: "1px solid hsl(var(--color-border))",
                          borderRadius: "0.5rem",
                        }}
                        labelStyle={{ color: "hsl(var(--color-foreground))" }}
                      />
                      <Line
                        type="monotone"
                        dataKey="count"
                        stroke="#6d28d9"
                        strokeWidth={2}
                        dot={{ fill: "#6d28d9" }}
                      />
                    </LineChart>
                  </ResponsiveContainer>
                </div>
              )}
          </CardContent>
        </Card>

        <Card className="border-border bg-card">
          <CardHeader>
            <CardTitle className="text-foreground">Hunt completions per week</CardTitle>
          </CardHeader>
          <CardContent className="pt-0">
              {isLoading ? (
                <Skeleton className="h-64 w-full" />
              ) : (
                <div className="h-64">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart
                      data={data?.completionsPerWeek ?? []}
                      margin={{ top: 5, right: 5, left: 0, bottom: 0 }}
                    >
                      <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
                      <XAxis
                        dataKey="week"
                        stroke="hsl(var(--color-muted-foreground))"
                        fontSize={12}
                      />
                      <YAxis stroke="hsl(var(--color-muted-foreground))" fontSize={12} />
                      <Tooltip
                        contentStyle={{
                          backgroundColor: "hsl(var(--color-card))",
                          border: "1px solid hsl(var(--color-border))",
                          borderRadius: "0.5rem",
                        }}
                      />
                      <Bar dataKey="count" fill="#6d28d9" radius={[4, 4, 0, 0]} />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
              )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
