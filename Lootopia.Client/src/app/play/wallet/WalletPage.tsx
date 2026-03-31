import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Wallet, ArrowDownLeft, ArrowUpRight } from "lucide-react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/shared/components/ui/tabs";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { walletApi } from "@/shared/api/wallet";
import { formatCurrency, formatDateTime } from "@/shared/lib/utils";

const PAGE_SIZE = 20;

export function WalletPage() {
  const [page, setPage] = useState(1);
  const [typeFilter, setTypeFilter] = useState<"all" | "Credit" | "Debit">("all");

  const { data: wallet, isLoading: walletLoading } = useQuery({
    queryKey: ["wallet"],
    queryFn: () => walletApi.get(),
  });

  const { data: txData, isLoading: txLoading } = useQuery({
    queryKey: ["wallet", "transactions", page],
    queryFn: () => walletApi.transactions(page, PAGE_SIZE),
  });

  const filteredTransactions = txData?.items.filter((t) => {
    if (typeFilter === "all") return true;
    if (typeFilter === "Credit") return t.amount > 0;
    return t.amount < 0;
  }) ?? [];

  const totalPages = txData
    ? Math.ceil(txData.total / PAGE_SIZE)
    : 0;

  return (
    <div className="h-full flex flex-col gap-6 p-4 bg-slate-950 overflow-y-auto">
      <h1 className="text-xl font-bold text-foreground">Portefeuille</h1>

      {walletLoading ? (
        <Skeleton className="h-32 w-full rounded-xl" />
      ) : (
        <Card className="border-primary/30 bg-gradient-to-br from-primary/10 to-transparent">
          <CardContent className="pt-6">
            <div className="flex items-center gap-3">
              <div className="rounded-full bg-primary/20 p-3">
                <Wallet className="h-8 w-8 text-primary" />
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Solde</p>
                <p className="text-3xl font-bold text-foreground">
                  {formatCurrency(wallet?.balance ?? 0)}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      <Tabs value={typeFilter} onValueChange={(v) => setTypeFilter(v as typeof typeFilter)}>
        <TabsList className="grid w-full grid-cols-3 bg-muted">
          <TabsTrigger value="all">Tous</TabsTrigger>
          <TabsTrigger value="Credit">Crédits</TabsTrigger>
          <TabsTrigger value="Debit">Débits</TabsTrigger>
        </TabsList>
        <TabsContent value={typeFilter} className="mt-4">
          {txLoading ? (
            <div className="space-y-3">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-16 w-full rounded-lg" />
              ))}
            </div>
          ) : (
            <div className="space-y-3">
              {filteredTransactions.length === 0 ? (
                <div className="py-12 text-center text-muted-foreground">
                  Aucune transaction
                </div>
              ) : (
                filteredTransactions.map((tx) => (
                  <Card
                    key={tx.id}
                    className="border-border bg-card/80 transition-colors hover:bg-card"
                  >
                    <CardContent className="flex items-center gap-4 py-4">
                      <div
                        className={`rounded-full p-2 ${
                          tx.amount >= 0 ? "bg-success/20" : "bg-destructive/20"
                        }`}
                      >
                        {tx.amount >= 0 ? (
                          <ArrowDownLeft className="h-5 w-5 text-success" />
                        ) : (
                          <ArrowUpRight className="h-5 w-5 text-destructive" />
                        )}
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="font-medium text-foreground truncate">
                          {tx.description}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          {formatDateTime(tx.createdAt)}
                        </p>
                      </div>
                      <span
                        className={`font-semibold shrink-0 ${
                          tx.amount >= 0 ? "text-success" : "text-destructive"
                        }`}
                      >
                        {tx.amount >= 0 ? "+" : ""}
                        {tx.amount.toLocaleString("fr-FR")} LTK
                      </span>
                    </CardContent>
                  </Card>
                ))
              )}
              {totalPages > 1 && (
                <div className="flex justify-center gap-2 pt-4">
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page <= 1}
                    onClick={() => setPage((p) => p - 1)}
                  >
                    Précédent
                  </Button>
                  <span className="flex items-center px-4 text-sm text-muted-foreground">
                    {page} / {totalPages}
                  </span>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={page >= totalPages}
                    onClick={() => setPage((p) => p + 1)}
                  >
                    Suivant
                  </Button>
                </div>
              )}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
