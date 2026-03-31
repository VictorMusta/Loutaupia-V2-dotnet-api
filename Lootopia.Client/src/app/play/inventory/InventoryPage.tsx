import { useState } from "react";
import { useInfiniteQuery } from "@tanstack/react-query";
import { Package } from "lucide-react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Badge } from "@/shared/components/ui/badge";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/shared/components/ui/tabs";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { inventoryApi } from "@/shared/api/inventory";

const RARITY_COLORS: Record<string, string> = {
  Common: "bg-slate-500",
  Rare: "bg-blue-600",
  Epic: "bg-purple-600",
  Legendary: "bg-amber-500",
};

const TYPES = ["Coupon", "Artifact", "Token"] as const;
const RARITIES = ["Common", "Rare", "Epic", "Legendary"] as const;

const PAGE_SIZE = 24;

export function InventoryPage() {
  const [typeFilter, setTypeFilter] = useState<string | undefined>();
  const [rarityFilter, setRarityFilter] = useState<string | undefined>();

  const {
    data,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isLoading,
  } = useInfiniteQuery({
    queryKey: ["inventory", typeFilter, rarityFilter],
    queryFn: ({ pageParam = 1 }) =>
      inventoryApi.list({
        type: typeFilter,
        rarity: rarityFilter,
        page: pageParam,
        size: PAGE_SIZE,
      }),
    getNextPageParam: (lastPage, pages) => {
      const loaded = pages.reduce((acc, p) => acc + p.items.length, 0);
      return loaded < lastPage.totalCount ? pages.length + 1 : undefined;
    },
    initialPageParam: 1,
  });

  const items = data?.pages.flatMap((p) => p.items) ?? [];
  const total = data?.pages[0]?.totalCount ?? 0;

  return (
    <div className="h-full flex flex-col gap-4 p-4 bg-slate-950 overflow-y-auto">
      <h1 className="text-xl font-bold text-foreground">Inventaire</h1>

      <Tabs defaultValue="type" className="space-y-4">
        <TabsList className="bg-muted">
          <TabsTrigger value="type">Par type</TabsTrigger>
          <TabsTrigger value="rarity">Par rareté</TabsTrigger>
        </TabsList>
        <TabsContent value="type" className="space-y-2">
          <div className="flex flex-wrap gap-2">
            <Button
              variant={!typeFilter ? "default" : "outline"}
              size="sm"
              onClick={() => setTypeFilter(undefined)}
            >
              Tous
            </Button>
            {TYPES.map((t) => (
              <Button
                key={t}
                variant={typeFilter === t ? "default" : "outline"}
                size="sm"
                onClick={() => setTypeFilter(typeFilter === t ? undefined : t)}
              >
                {t}
              </Button>
            ))}
          </div>
        </TabsContent>
        <TabsContent value="rarity" className="space-y-2">
          <div className="flex flex-wrap gap-2">
            <Button
              variant={!rarityFilter ? "default" : "outline"}
              size="sm"
              onClick={() => setRarityFilter(undefined)}
            >
              Toutes
            </Button>
            {RARITIES.map((r) => (
              <Button
                key={r}
                variant={rarityFilter === r ? "default" : "outline"}
                size="sm"
                onClick={() =>
                  setRarityFilter(rarityFilter === r ? undefined : r)
                }
              >
                {r}
              </Button>
            ))}
          </div>
        </TabsContent>
      </Tabs>

      {isLoading ? (
        <div className="grid grid-cols-2 gap-4">
          {[...Array(8)].map((_, i) => (
            <Skeleton key={i} className="h-40 rounded-xl" />
          ))}
        </div>
      ) : (
        <>
          <p className="text-sm text-muted-foreground">
            {items.length} objet(s) sur {total}
          </p>
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
            {items.map((item) => (
              <Card
                key={item.itemId}
                className="border-border bg-card/80 overflow-hidden transition-all hover:border-primary/50 hover:shadow-lg hover:shadow-primary/10"
              >
                <div className="aspect-square bg-muted flex items-center justify-center">
                  {item.imageUrl ? (
                    <img
                      src={item.imageUrl}
                      alt={item.name}
                      className="w-full h-full object-cover"
                    />
                  ) : (
                    <Package className="h-12 w-12 text-muted-foreground" />
                  )}
                </div>
                <CardContent className="p-3">
                  <p className="font-medium text-foreground truncate">
                    {item.name}
                  </p>
                  <div className="flex items-center gap-2 mt-1 flex-wrap">
                    <Badge
                      className={RARITY_COLORS[item.rarity] ?? "bg-muted"}
                      variant="secondary"
                    >
                      {item.rarity}
                    </Badge>
                    <span className="text-xs text-muted-foreground">
                      {item.type}
                    </span>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
          {hasNextPage && (
            <div className="flex justify-center py-4">
              <Button
                variant="outline"
                onClick={() => fetchNextPage()}
                disabled={isFetchingNextPage}
              >
                {isFetchingNextPage ? "Chargement..." : "Charger plus"}
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
