import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
import { campaignsApi, type Campaign } from "@/shared/api/campaigns";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent } from "@/shared/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Badge } from "@/shared/components/ui/badge";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { formatDate, formatCurrency } from "@/shared/lib/utils";
import { Megaphone, MapPin, Users, CheckCircle2, Clock, Ticket, TrendingUp, BarChart3, Activity } from "lucide-react";

export function PartnerCampaignsPage() {
  const [selectedCampaign, setSelectedCampaign] = useState<Campaign | null>(null);

  const { data: campaigns = [], isLoading } = useQuery({
    queryKey: ["partner-campaigns"],
    queryFn: () => campaignsApi.mine(),
  });

  // Calculate high-level KPIs across all user campaigns
  const totalHunts = campaigns.length;
  const totalStarted = campaigns.reduce((acc, c) => acc + (c.startedCount || 0), 0);
  const totalCompleted = campaigns.reduce((acc, c) => acc + (c.completedCount || 0), 0);
  const totalCoupons = campaigns.reduce((acc, c) => acc + (c.couponsDistributed || 0), 0);
  const avgCompletionRate = totalStarted > 0 ? Math.round((totalCompleted / totalStarted) * 100) : 0;

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-3xl font-extrabold tracking-tight text-foreground flex items-center gap-2">
          <Activity className="h-7 w-7 text-primary animate-pulse" />
          Mes Chasses Sponsorisées & Analytics
        </h1>
        <p className="text-sm text-muted-foreground mt-1">
          Suivez en temps réel l'engagement géolocalisé de vos chasses, la fréquentation physique de votre secteur et la distribution de vos coupons.
        </p>
      </div>

      {/* Ribbon Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <Card className="border-border bg-card overflow-hidden">
          <div className="p-4 flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-primary/10 text-primary">
              <MapPin className="h-5 w-5" />
            </div>
            <div>
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Chasses Liées</p>
              <p className="text-2xl font-bold text-foreground mt-0.5">{totalHunts}</p>
            </div>
          </div>
        </Card>

        <Card className="border-border bg-card overflow-hidden">
          <div className="p-4 flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-emerald-500/10 text-emerald-500">
              <Users className="h-5 w-5" />
            </div>
            <div>
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Explorations Démarrées</p>
              <p className="text-2xl font-bold text-foreground mt-0.5">{totalStarted}</p>
            </div>
          </div>
        </Card>

        <Card className="border-border bg-card overflow-hidden">
          <div className="p-4 flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-blue-500/10 text-blue-500">
              <CheckCircle2 className="h-5 w-5" />
            </div>
            <div>
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Taux de Complétion</p>
              <p className="text-2xl font-bold text-foreground mt-0.5">{avgCompletionRate}%</p>
            </div>
          </div>
        </Card>

        <Card className="border-border bg-card overflow-hidden">
          <div className="p-4 flex items-center gap-3">
            <div className="p-2.5 rounded-lg bg-amber-500/10 text-amber-500">
              <Ticket className="h-5 w-5" />
            </div>
            <div>
              <p className="text-xs font-medium text-muted-foreground uppercase tracking-wider">Coupons Distribués</p>
              <p className="text-2xl font-bold text-foreground mt-0.5">{totalCoupons}</p>
            </div>
          </div>
        </Card>
      </div>

      {/* Main Campaign Cards Grid */}
      <div className="space-y-6">
        <h2 className="text-xl font-bold text-foreground flex items-center gap-2">
          <BarChart3 className="h-5 w-5 text-primary" />
          Détails & Suivi d'Engagement Quotidien
        </h2>

        {isLoading ? (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <Skeleton className="h-96 w-full rounded-xl" />
            <Skeleton className="h-96 w-full rounded-xl" />
          </div>
        ) : campaigns.length === 0 ? (
          <Card className="p-12 text-center border-dashed bg-card border-border">
            <Megaphone className="h-12 w-12 mx-auto text-muted-foreground/40 mb-3 stroke-[1.5]" />
            <p className="text-base font-semibold text-foreground">Aucune chasse actuellement sponsorisée</p>
            <p className="text-xs text-muted-foreground mt-1">
              Les administrateurs peuvent lier vos établissements lors de la publication de nouvelles chasses géolocalisées.
            </p>
          </Card>
        ) : (
          <div className="grid grid-cols-1 xl:grid-cols-2 gap-6">
            {campaigns.map((camp) => {
              const started = camp.startedCount || 0;
              const completed = camp.completedCount || 0;
              const completionRate = started > 0 ? Math.round((completed / started) * 100) : 0;
              const maxCoupons = camp.maxCoupons || 100;
              const couponsDist = camp.couponsDistributed || 0;
              const couponPercent = Math.min(100, Math.round((couponsDist / maxCoupons) * 100));
              const tracking = camp.dailyTracking || [];
              const maxDayVal = Math.max(10, ...tracking.map(t => t.explorations));

              return (
                <Card key={camp.id} className="border border-border bg-card overflow-hidden flex flex-col justify-between hover:border-primary/40 transition-colors">
                  {/* Card Header */}
                  <div className="p-5 border-b border-border bg-muted/10 flex items-start justify-between gap-4">
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <Badge variant={camp.status.toLowerCase() === "active" ? "success" : "secondary"} className="text-[10px]">
                          {camp.status}
                        </Badge>
                        <span className="text-xs text-muted-foreground truncate">Créée le {formatDate(camp.createdAt)}</span>
                      </div>
                      <h3 className="text-lg font-bold text-foreground mt-2 truncate">{camp.title}</h3>
                      <p className="text-xs text-muted-foreground line-clamp-2 mt-1">{camp.description}</p>
                    </div>
                  </div>

                  {/* Card Content with KPI columns */}
                  <CardContent className="p-5 space-y-6 flex-1">
                    {/* Primary Metrics Row */}
                    <div className="grid grid-cols-3 gap-3 bg-background p-3 rounded-lg border border-border text-center">
                      <div>
                        <p className="text-[10px] text-muted-foreground font-medium uppercase">Démarrées</p>
                        <p className="text-lg font-bold text-foreground mt-0.5">{started}</p>
                      </div>
                      <div className="border-x border-border">
                        <p className="text-[10px] text-muted-foreground font-medium uppercase">Complétées</p>
                        <p className="text-lg font-bold text-foreground mt-0.5 text-emerald-500">{completed}</p>
                      </div>
                      <div>
                        <p className="text-[10px] text-muted-foreground font-medium uppercase">Budget Alloué</p>
                        <p className="text-lg font-bold text-primary mt-0.5">{formatCurrency(camp.budget)}</p>
                      </div>
                    </div>

                    {/* Completion rate progress bar */}
                    <div className="space-y-1.5">
                      <div className="flex justify-between text-xs">
                        <span className="text-muted-foreground flex items-center gap-1">
                          <TrendingUp className="h-3.5 w-3.5 text-primary" />
                          Taux de réussite des joueurs
                        </span>
                        <span className="font-bold text-foreground">{completionRate}%</span>
                      </div>
                      <div className="h-2 w-full bg-secondary rounded-full overflow-hidden">
                        <div 
                          className="h-full bg-gradient-to-r from-primary to-blue-500 rounded-full transition-all duration-500" 
                          style={{ width: `${completionRate}%` }} 
                        />
                      </div>
                    </div>

                    {/* Vouchers Progress bar */}
                    <div className="space-y-1.5">
                      <div className="flex justify-between text-xs">
                        <span className="text-muted-foreground flex items-center gap-1">
                          <Ticket className="h-3.5 w-3.5 text-amber-500" />
                          Coupons de réduction distribués
                        </span>
                        <span className="font-bold text-foreground">{couponsDist} / {maxCoupons}</span>
                      </div>
                      <div className="h-2 w-full bg-secondary rounded-full overflow-hidden">
                        <div 
                          className="h-full bg-amber-500 rounded-full transition-all duration-500" 
                          style={{ width: `${couponPercent}%` }} 
                        />
                      </div>
                    </div>

                    {/* Average Time Indicator */}
                    <div className="flex items-center justify-between p-2.5 rounded-md bg-secondary/30 border border-border/60 text-xs">
                      <span className="text-muted-foreground flex items-center gap-1.5">
                        <Clock className="h-4 w-4 text-blue-500" />
                        Temps de complétion moyen
                      </span>
                      <span className="font-extrabold text-foreground bg-background px-2 py-0.5 rounded border border-border">
                        ~{camp.averageCompletionMinutes ?? 0} min
                      </span>
                    </div>

                    {/* Daily activity chart (7 days) */}
                    {tracking.length > 0 && (
                      <div className="space-y-2 pt-2 border-t border-border">
                        <p className="text-[10px] font-semibold text-muted-foreground uppercase tracking-wider text-center">
                          Explorations sur les 7 derniers jours
                        </p>
                        <div className="h-28 flex items-end justify-between gap-2 pt-4 px-2">
                          {tracking.map((day, i) => {
                            const heightPercent = Math.max(8, Math.round((day.explorations / maxDayVal) * 100));
                            const isToday = i === tracking.length - 1;

                            return (
                              <div key={day.date} className="flex-1 flex flex-col items-center gap-1 h-full justify-end group/bar relative">
                                {/* Tooltip */}
                                <div className="absolute -top-7 scale-0 group-hover/bar:scale-100 transition-all duration-150 bg-foreground text-background text-[10px] font-bold px-2 py-0.5 rounded shadow-lg pointer-events-none z-10 whitespace-nowrap">
                                  {day.explorations} suivis
                                </div>
                                
                                {/* Bar */}
                                <div 
                                  className={`w-full rounded-t transition-all duration-300 group-hover/bar:brightness-125 ${
                                    isToday 
                                      ? "bg-primary" 
                                      : "bg-primary/40 hover:bg-primary/70"
                                  }`}
                                  style={{ height: `${heightPercent}%` }}
                                />
                                {/* Label */}
                                <span className="text-[8px] text-muted-foreground font-medium truncate max-w-full">
                                  {day.date.split("-").slice(1).join("/")}
                                </span>
                              </div>
                            );
                          })}
                        </div>
                      </div>
                    )}
                  </CardContent>

                  {/* Footer view raw metrics action */}
                  <div className="p-3 bg-muted/5 border-t border-border flex justify-end">
                    <Button 
                      size="sm" 
                      variant="ghost" 
                      className="text-xs h-7 text-muted-foreground hover:text-foreground"
                      onClick={() => setSelectedCampaign(camp)}
                    >
                      Détails de facturation
                    </Button>
                  </div>
                </Card>
              );
            })}
          </div>
        )}
      </div>

      {/* Raw Details Dialog */}
      <Dialog open={!!selectedCampaign} onOpenChange={() => setSelectedCampaign(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Informations Financières & Contrat</DialogTitle>
            <DialogDescription>Détails bruts de la répartition budgétaire de la campagne</DialogDescription>
          </DialogHeader>
          {selectedCampaign && (
            <div className="space-y-4 text-sm">
              <div>
                <p className="text-xs text-muted-foreground">Titre officiel</p>
                <p className="font-semibold text-foreground">{selectedCampaign.title}</p>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-xs text-muted-foreground">Budget Total Engagé</p>
                  <p className="font-bold text-primary">{formatCurrency(selectedCampaign.budget)}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Budget Consommé</p>
                  <p className="font-bold text-foreground">{formatCurrency(selectedCampaign.spent)}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Date de démarrage</p>
                  <p className="text-foreground">{formatDate(selectedCampaign.startDate || selectedCampaign.createdAt)}</p>
                </div>
                <div>
                  <p className="text-xs text-muted-foreground">Date de fin prévue</p>
                  <p className="text-foreground">{formatDate(selectedCampaign.endDate || selectedCampaign.createdAt)}</p>
                </div>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setSelectedCampaign(null)}>
              Fermer
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
