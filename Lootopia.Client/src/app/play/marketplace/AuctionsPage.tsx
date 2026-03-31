import { useState, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Gavel, Clock } from "lucide-react";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { auctionsApi } from "@/shared/api/auctions";
import { useToast } from "@/shared/components/ui/toast";
import { formatCurrency, formatDateTime } from "@/shared/lib/utils";

function Countdown({ endsAt }: { endsAt: string }) {
  const [remaining, setRemaining] = useState("");

  useEffect(() => {
    const end = new Date(endsAt).getTime();
    const update = () => {
      const now = Date.now();
      const diff = Math.max(0, end - now);
      if (diff === 0) {
        setRemaining("Terminé");
        return;
      }
      const h = Math.floor(diff / 3600000);
      const m = Math.floor((diff % 3600000) / 60000);
      const s = Math.floor((diff % 60000) / 1000);
      setRemaining(`${h}h ${m}m ${s}s`);
    };
    update();
    const id = setInterval(update, 1000);
    return () => clearInterval(id);
  }, [endsAt]);

  return (
    <span className="flex items-center gap-1 text-amber-500">
      <Clock className="h-4 w-4" />
      {remaining}
    </span>
  );
}

export function AuctionsPage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [bidAmount, setBidAmount] = useState("");

  const { data: auctions, isLoading } = useQuery({
    queryKey: ["auctions", "list"],
    queryFn: () => auctionsApi.list(),
  });

  const { data: auctionDetail, isLoading: detailLoading } = useQuery({
    queryKey: ["auctions", selectedId],
    queryFn: () => auctionsApi.get(selectedId!),
    enabled: !!selectedId,
  });

  const bidMutation = useMutation({
    mutationFn: ({ auctionId, amount }: { auctionId: string; amount: number }) =>
      auctionsApi.bid(auctionId, amount),
    onSuccess: () => {
      setBidAmount("");
      queryClient.invalidateQueries({ queryKey: ["auctions"] });
      queryClient.invalidateQueries({ queryKey: ["wallet"] });
      toast({ title: "Enchère placée", variant: "success" });
    },
    onError: (err) => {
      toast({
        title: "Erreur",
        description: err instanceof Error ? err.message : "Enchère impossible",
        variant: "destructive",
      });
    },
  });

  const activeAuctions = auctions?.filter((a) => a.status === "Active") ?? [];

  const handleBid = () => {
    const amount = parseFloat(bidAmount);
    if (!selectedId || isNaN(amount) || amount <= 0) {
      toast({
        title: "Erreur",
        description: "Montant invalide",
        variant: "destructive",
      });
      return;
    }
    const minBid = (auctionDetail?.currentPrice ?? 0) + 1;
    if (amount < minBid) {
      toast({
        title: "Erreur",
        description: `Montant minimum: ${formatCurrency(minBid)}`,
        variant: "destructive",
      });
      return;
    }
    bidMutation.mutate({ auctionId: selectedId, amount });
  };

  return (
    <div className="h-full flex flex-col gap-4 p-4 bg-slate-950 overflow-y-auto">
      <h1 className="text-xl font-bold text-foreground">Enchères</h1>

      {isLoading ? (
        <div className="space-y-3">
          {[...Array(4)].map((_, i) => (
            <Skeleton key={i} className="h-20 rounded-xl" />
          ))}
        </div>
      ) : (
        <div className="space-y-3">
          {activeAuctions.length === 0 ? (
            <div className="py-12 text-center text-muted-foreground">
              Aucune enchère en cours
            </div>
          ) : (
            activeAuctions.map((auction) => (
              <Card
                key={auction.id}
                className={`border-border bg-card/80 cursor-pointer transition-all hover:border-primary/50 ${
                  selectedId === auction.id ? "ring-2 ring-primary" : ""
                }`}
                onClick={() =>
                  setSelectedId(selectedId === auction.id ? null : auction.id)
                }
              >
                <CardContent className="p-4 flex flex-row items-center justify-between">
                  <div className="min-w-0">
                    <p className="font-medium text-foreground truncate">
                      {auction.itemName}
                    </p>
                    <div className="flex items-center gap-2 mt-1">
                      <Badge variant="secondary">{auction.itemRarity}</Badge>
                      <span className="text-sm text-primary font-semibold">
                        {formatCurrency(auction.currentPrice)}
                      </span>
                      <Countdown endsAt={auction.endsAt} />
                    </div>
                  </div>
                  <Gavel className="h-6 w-6 text-muted-foreground shrink-0" />
                </CardContent>
              </Card>
            ))
          )}
        </div>
      )}

      {selectedId && (
        <Card className="border-primary/30 bg-card/80 mt-4">
          <CardHeader>
            <CardTitle className="text-lg">Détails</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {detailLoading ? (
              <Skeleton className="h-24 w-full" />
            ) : auctionDetail ? (
              <>
                <div>
                  <p className="text-foreground font-medium">
                    {auctionDetail.itemName}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Vendeur: {auctionDetail.sellerName} •{" "}
                    {auctionDetail.bidCount} enchère(s)
                  </p>
                </div>
                <div className="space-y-2">
                  <p className="text-sm text-muted-foreground">
                    Prix actuel:{" "}
                    <span className="font-bold text-primary">
                      {formatCurrency(auctionDetail.currentPrice)}
                    </span>
                  </p>
                  <p className="text-xs text-muted-foreground">
                    Se termine le {formatDateTime(auctionDetail.endsAt)}
                  </p>
                </div>
                {auctionDetail.bids && auctionDetail.bids.length > 0 && (
                  <div>
                    <p className="text-sm font-medium text-foreground mb-2">
                      Historique des enchères
                    </p>
                    <div className="space-y-1 max-h-24 overflow-y-auto">
                      {auctionDetail.bids.map((bid) => (
                        <div
                          key={bid.id}
                          className="flex justify-between text-sm"
                        >
                          <span className="text-muted-foreground">
                            {bid.bidderName}
                          </span>
                          <span className="text-primary">
                            {formatCurrency(bid.amount)}
                          </span>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
                {auctionDetail.status === "Active" && (
                  <div className="flex gap-2">
                    <Input
                      type="number"
                      min={
                        (auctionDetail.currentPrice ?? 0) + 1
                      }
                      placeholder="Montant (LTK)"
                      value={bidAmount}
                      onChange={(e) => setBidAmount(e.target.value)}
                    />
                    <Button
                      className="bg-primary hover:bg-primary/90 shrink-0"
                      onClick={handleBid}
                      disabled={bidMutation.isPending}
                    >
                      Enchérir
                    </Button>
                  </div>
                )}
              </>
            ) : null}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
