import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Trophy, Award } from "lucide-react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Tabs, TabsList, TabsTrigger } from "@/shared/components/ui/tabs";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { leaderboardsApi } from "@/shared/api/leaderboards";
import { useAuth } from "@/shared/providers/AuthProvider";

export function LeaderboardsPage() {
  const { user } = useAuth();
  const [scope, setScope] = useState("global");
  const [period, setPeriod] = useState("all");

  const { data: entries, isLoading } = useQuery({
    queryKey: ["leaderboards", scope, period],
    queryFn: () =>
      leaderboardsApi.get({
        scope: scope === "global" ? undefined : scope,
        period: period === "all" ? undefined : period,
      }),
  });

  const { data: myRank } = useQuery({
    queryKey: ["leaderboards", "me", scope, period],
    queryFn: () =>
      leaderboardsApi.myRank({
        scope: scope === "global" ? undefined : scope,
        period: period === "all" ? undefined : period,
      }),
  });

  return (
    <div className="h-full flex flex-col gap-4 p-4 bg-slate-950 overflow-y-auto">
      <h1 className="text-xl font-bold text-foreground">Classements</h1>

      {myRank && (
        <Card className="border-primary/30 bg-gradient-to-r from-primary/10 to-transparent">
          <CardContent className="p-4 flex items-center gap-4">
            <div className="rounded-full bg-primary/20 p-3">
              <Award className="h-8 w-8 text-primary" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Votre classement</p>
              <p className="text-2xl font-bold text-foreground">
                #{myRank.rank} / {myRank.total}
              </p>
              <p className="text-sm text-primary font-semibold">
                {myRank.score.toLocaleString("fr-FR")} pts
              </p>
            </div>
          </CardContent>
        </Card>
      )}

      <div className="space-y-4">
        <div>
          <p className="text-sm text-muted-foreground mb-2">Périmètre</p>
          <Tabs value={scope} onValueChange={setScope}>
            <TabsList className="grid w-full grid-cols-2 bg-muted">
              <TabsTrigger value="global">Global</TabsTrigger>
              <TabsTrigger value="hunt">Par chasse</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
        <div>
          <p className="text-sm text-muted-foreground mb-2">Période</p>
          <Tabs value={period} onValueChange={setPeriod}>
            <TabsList className="grid w-full grid-cols-3 bg-muted">
              <TabsTrigger value="all">Tout</TabsTrigger>
              <TabsTrigger value="week">Semaine</TabsTrigger>
              <TabsTrigger value="month">Mois</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
      </div>

      <div className="flex-1">
          {isLoading ? (
            <div className="space-y-2">
              {[...Array(10)].map((_, i) => (
                <Skeleton key={i} className="h-12 rounded-lg" />
              ))}
            </div>
          ) : (
            <div className="space-y-2">
              {entries?.length === 0 ? (
                <div className="py-12 text-center text-muted-foreground">
                  Aucun classement
                </div>
              ) : (
                entries?.map((entry, idx) => (
                  <Card
                    key={entry.userId}
                    className={`border-border transition-colors ${
                      entry.userId === user?.id
                        ? "bg-primary/20 border-primary/50 ring-1 ring-primary/30"
                        : "bg-card/80"
                    }`}
                  >
                    <CardContent className="flex items-center gap-4 py-3 px-4">
                      <span
                        className={`w-8 text-center font-bold ${
                          idx < 3 ? "text-amber-500" : "text-muted-foreground"
                        }`}
                      >
                        #{entry.rank}
                      </span>
                      {idx < 3 && (
                        <Trophy
                          className={`h-5 w-5 ${
                            idx === 0
                              ? "text-amber-400"
                              : idx === 1
                                ? "text-slate-400"
                                : "text-amber-700"
                          }`}
                        />
                      )}
                      <span className="flex-1 font-medium text-foreground truncate">
                        {entry.displayName}
                      </span>
                      <span className="font-semibold text-primary">
                        {entry.score.toLocaleString("fr-FR")}
                      </span>
                    </CardContent>
                  </Card>
                ))
              )}
            </div>
          )}
      </div>
    </div>
  );
}
