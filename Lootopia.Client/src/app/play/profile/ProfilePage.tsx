import { useQuery } from "@tanstack/react-query";
import { User, LogOut, Mail, Shield } from "lucide-react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { useAuth } from "@/shared/providers/AuthProvider";
import { achievementsApi } from "@/shared/api/achievements";

const RARITY_COLORS: Record<string, string> = {
  Common: "bg-slate-500/30 border-slate-500",
  Rare: "bg-blue-600/30 border-blue-600",
  Epic: "bg-purple-600/30 border-purple-600",
  Legendary: "bg-amber-500/30 border-amber-500",
};

export function ProfilePage() {
  const { user, logout } = useAuth();

  const { data: achievements, isLoading } = useQuery({
    queryKey: ["achievements"],
    queryFn: () => achievementsApi.list("all"),
  });

  return (
    <div className="h-full flex flex-col gap-6 p-4 overflow-y-auto">
      <h1 className="text-xl font-bold text-foreground">Profil</h1>

      <Card className="border-border bg-card">
        <CardContent className="p-6">
          <div className="flex items-center gap-4">
            <div className="rounded-full bg-primary/20 p-4">
              <User className="h-12 w-12 text-primary" />
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-xl font-semibold text-foreground truncate">
                {user?.displayName ?? "—"}
              </p>
              <div className="flex items-center gap-2 mt-1 text-sm text-muted-foreground">
                <Mail className="h-4 w-4 shrink-0" />
                <span className="truncate">{user?.email ?? "—"}</span>
              </div>
              <div className="flex items-center gap-2 mt-1">
                <Shield className="h-4 w-4 text-primary" />
                <span className="text-sm text-primary">{user?.role ?? "Player"}</span>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      <div>
        <h2 className="font-semibold text-foreground mb-3">Succès</h2>
        {isLoading ? (
          <div className="grid grid-cols-3 gap-4">
            {[...Array(9)].map((_, i) => (
              <Skeleton key={i} className="h-24 rounded-xl" />
            ))}
          </div>
        ) : (
          <div className="grid grid-cols-3 gap-4">
            {achievements?.map((a) => (
              <Card
                key={a.id}
                className={`border overflow-hidden transition-all ${
                  a.isUnlocked
                    ? RARITY_COLORS[a.rarity] ?? "bg-card border-border"
                    : "bg-muted/50 border-border opacity-60"
                }`}
              >
                <CardContent className="p-3 text-center">
                  <div
                    className={`w-12 h-12 mx-auto rounded-lg mb-2 flex items-center justify-center ${
                      a.isUnlocked ? "bg-primary/20" : "bg-muted"
                    }`}
                  >
                    <span className="text-2xl">
                      {a.isUnlocked ? "🏆" : "🔒"}
                    </span>
                  </div>
                  <p
                    className={`text-xs font-medium truncate ${
                      a.isUnlocked ? "text-foreground" : "text-muted-foreground"
                    }`}
                  >
                    {a.name}
                  </p>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>

      <div className="mt-auto pt-4">
        <Button
          variant="outline"
          className="w-full border-destructive/50 text-destructive hover:bg-destructive/10"
          onClick={logout}
        >
          <LogOut className="h-5 w-5 mr-2" />
          Se déconnecter
        </Button>
      </div>
    </div>
  );
}
