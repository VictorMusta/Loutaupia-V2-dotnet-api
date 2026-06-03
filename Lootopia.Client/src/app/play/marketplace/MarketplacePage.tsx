import { useState, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Package, ShoppingCart, Plus, Search } from "lucide-react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/shared/components/ui/tabs";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/shared/components/ui/dialog";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { marketplaceApi } from "@/shared/api/marketplace";
import { inventoryApi } from "@/shared/api/inventory";
import { useToast } from "@/shared/components/ui/toast";
import { formatCurrency } from "@/shared/lib/utils";

const RARITY_COLORS: Record<string, string> = {
  Common: "bg-slate-500",
  Rare: "bg-blue-600",
  Epic: "bg-purple-600",
  Legendary: "bg-amber-500",
};

const RARITIES = ["Common", "Rare", "Epic", "Legendary"] as const;

const SORT_OPTIONS = [
  { value: "", label: "Plus récents" },
  { value: "price_asc", label: "Prix croissant" },
  { value: "price_desc", label: "Prix décroissant" },
  { value: "name_asc", label: "Nom A-Z" },
  { value: "name_desc", label: "Nom Z-A" },
] as const;

export function MarketplacePage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [nameSearch, setNameSearch] = useState("");
  const [debouncedName, setDebouncedName] = useState("");
  const [rarityFilter, setRarityFilter] = useState<string | undefined>();
  const [minPrice, setMinPrice] = useState("");
  const [maxPrice, setMaxPrice] = useState("");
  const [sort, setSort] = useState("");
  const [confirmListing, setConfirmListing] = useState<{
    id: string;
    itemName: string;
    price: number;
  } | null>(null);
  const [sellItemId, setSellItemId] = useState("");
  const [sellPrice, setSellPrice] = useState("");
  const [sellStock, setSellStock] = useState("1");

  useEffect(() => {
    const t = setTimeout(() => setDebouncedName(nameSearch), 300);
    return () => clearTimeout(t);
  }, [nameSearch]);

  const listingFilters = {
    name: debouncedName || undefined,
    rarity: rarityFilter,
    minPrice: minPrice ? parseFloat(minPrice) : undefined,
    maxPrice: maxPrice ? parseFloat(maxPrice) : undefined,
    sort: sort || undefined,
  };

  const { data: listings, isLoading: listingsLoading } = useQuery({
    queryKey: ["marketplace", "listings", listingFilters],
    queryFn: () => marketplaceApi.listings(listingFilters),
  });

  const { data: myListings, isLoading: myListingsLoading } = useQuery({
    queryKey: ["marketplace", "myListings"],
    queryFn: () => marketplaceApi.myListings(),
  });

  const { data: inventory } = useQuery({
    queryKey: ["inventory"],
    queryFn: () => inventoryApi.list({ size: 100 }),
  });

  const purchaseMutation = useMutation({
    mutationFn: (listingId: string) => marketplaceApi.purchase(listingId),
    onSuccess: () => {
      setConfirmListing(null);
      queryClient.invalidateQueries({ queryKey: ["marketplace"] });
      queryClient.invalidateQueries({ queryKey: ["wallet"] });
      toast({ title: "Achat réussi", variant: "success" });
    },
    onError: (err) => {
      toast({
        title: "Erreur",
        description: err instanceof Error ? err.message : "Achat impossible",
        variant: "destructive",
      });
    },
  });

  const createMutation = useMutation({
    mutationFn: ({ itemId, price, stock }: { itemId: string; price: number; stock: number }) =>
      marketplaceApi.create(itemId, price, stock),
    onSuccess: () => {
      setSellItemId("");
      setSellPrice("");
      setSellStock("1");
      queryClient.invalidateQueries({ queryKey: ["marketplace"] });
      queryClient.invalidateQueries({ queryKey: ["inventory"] });
      toast({ title: "Article mis en vente", variant: "success" });
    },
    onError: (err) => {
      toast({
        title: "Erreur",
        description: err instanceof Error ? err.message : "Impossible de vendre",
        variant: "destructive",
      });
    },
  });

  const handleSell = () => {
    const price = parseFloat(sellPrice);
    const stock = parseInt(sellStock, 10);
    if (!sellItemId || isNaN(price) || price <= 0 || isNaN(stock) || stock <= 0) {
      toast({
        title: "Erreur",
        description: "Sélectionnez un objet, un prix et une quantité valides.",
        variant: "destructive",
      });
      return;
    }
    createMutation.mutate({ itemId: sellItemId, price, stock });
  };

  const tradeableItems = inventory?.items?.filter((i) => i.isTradeable) ?? [];
  const selectedItem = tradeableItems.find((i) => i.itemId === sellItemId);

  return (
    <div className="h-full flex flex-col gap-4 p-4 overflow-y-auto">
      <h1 className="text-xl font-bold text-foreground">Marché</h1>

      <Tabs defaultValue="buy" className="flex-1">
        <TabsList className="grid w-full grid-cols-2 bg-muted">
          <TabsTrigger value="buy">Acheter</TabsTrigger>
          <TabsTrigger value="sell">Mes Ventes</TabsTrigger>
        </TabsList>

        <TabsContent value="buy" className="mt-4 space-y-4">
          <div className="space-y-3">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Rechercher par nom..."
                value={nameSearch}
                onChange={(e) => setNameSearch(e.target.value)}
                className="pl-9"
              />
            </div>

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

            <div className="flex gap-2">
              <Input
                type="number"
                min="0"
                placeholder="Prix min"
                value={minPrice}
                onChange={(e) => setMinPrice(e.target.value)}
              />
              <Input
                type="number"
                min="0"
                placeholder="Prix max"
                value={maxPrice}
                onChange={(e) => setMaxPrice(e.target.value)}
              />
            </div>

            <select
              value={sort}
              onChange={(e) => setSort(e.target.value)}
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
            >
              {SORT_OPTIONS.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>

          {listingsLoading ? (
            <div className="grid grid-cols-1 gap-3">
              {[...Array(4)].map((_, i) => (
                <Skeleton key={i} className="h-24 rounded-xl" />
              ))}
            </div>
          ) : (
            <div className="space-y-3">
              {listings?.length === 0 ? (
                <div className="py-12 text-center text-muted-foreground">
                  Aucune vente en cours
                </div>
              ) : (
                listings?.map((listing) => (
                  <Card
                    key={listing.id}
                    className="border-border bg-card flex flex-row overflow-hidden"
                  >
                    <div className="w-20 h-20 bg-muted shrink-0 flex items-center justify-center">
                      <Package className="h-10 w-10 text-muted-foreground" />
                    </div>
                    <div className="flex-1 p-4 flex flex-col justify-between min-w-0">
                      <div>
                        <p className="font-medium text-foreground truncate">
                          {listing.itemName}
                        </p>
                        <p className="text-xs text-muted-foreground">
                          Par {listing.sellerName}
                        </p>
                      </div>
                      <div className="flex items-center justify-between gap-2">
                        <Badge
                          className={
                            RARITY_COLORS[listing.itemRarity] ?? "bg-muted"
                          }
                          variant="secondary"
                        >
                          {listing.itemRarity}
                        </Badge>
                        <div className="flex items-center gap-2">
                          <span className="font-bold text-primary">
                            {formatCurrency(listing.price)}
                          </span>
                          <Button
                            size="sm"
                            className="bg-primary hover:bg-primary/90"
                            onClick={() =>
                              setConfirmListing({
                                id: listing.id,
                                itemName: listing.itemName,
                                price: listing.price,
                              })
                            }
                          >
                            <ShoppingCart className="h-4 w-4 mr-1" />
                            Acheter
                          </Button>
                        </div>
                      </div>
                    </div>
                  </Card>
                ))
              )}
            </div>
          )}
        </TabsContent>

        <TabsContent value="sell" className="mt-4 space-y-4">
          <Card className="border-border bg-card">
            <CardContent className="p-4 space-y-4">
              <h3 className="font-semibold text-foreground">
                Mettre en vente
              </h3>
              <div>
                <label className="text-sm text-muted-foreground block mb-2">
                  Objet
                </label>
                <select
                  value={sellItemId}
                  onChange={(e) => {
                    setSellItemId(e.target.value);
                    setSellStock("1");
                  }}
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                >
                  <option value="">Sélectionner...</option>
                  {tradeableItems.map((item) => (
                    <option key={item.itemId} value={item.itemId}>
                      {item.name} ({item.rarity})
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="text-sm text-muted-foreground block mb-2">
                  Prix (LTK)
                </label>
                <Input
                  type="number"
                  min="1"
                  placeholder="0"
                  value={sellPrice}
                  onChange={(e) => setSellPrice(e.target.value)}
                />
              </div>
              <div>
                <label className="text-sm text-muted-foreground block mb-2">
                  Quantité {selectedItem ? `(Max: ${selectedItem.quantity})` : ""}
                </label>
                <Input
                  type="number"
                  min="1"
                  max={selectedItem?.quantity ?? 1}
                  value={sellStock}
                  onChange={(e) => setSellStock(e.target.value)}
                />
              </div>
              <Button
                className="w-full bg-primary hover:bg-primary/90"
                onClick={handleSell}
                disabled={createMutation.isPending}
              >
                <Plus className="h-4 w-4 mr-2" />
                Mettre en vente
              </Button>
            </CardContent>
          </Card>

          {myListingsLoading ? (
            <Skeleton className="h-32 rounded-xl" />
          ) : (
            <div className="space-y-3">
              <h3 className="font-semibold text-foreground">Mes annonces</h3>
              {myListings?.length === 0 ? (
                <p className="text-sm text-muted-foreground">
                  Aucune annonce active
                </p>
              ) : (
                myListings?.map((listing) => (
                  <Card
                    key={listing.id}
                    className="border-border bg-card p-4 flex flex-row items-center justify-between"
                  >
                    <div>
                      <p className="font-medium text-foreground">
                        {listing.itemName}
                      </p>
                      <p className="text-sm text-primary font-semibold">
                        {formatCurrency(listing.price)}
                      </p>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={async () => {
                        try {
                          await marketplaceApi.cancel(listing.id);
                          queryClient.invalidateQueries({
                            queryKey: ["marketplace"],
                          });
                          toast({ title: "Annonce annulée", variant: "success" });
                        } catch {
                          toast({
                            title: "Erreur",
                            variant: "destructive",
                          });
                        }
                      }}
                    >
                      Annuler
                    </Button>
                  </Card>
                ))
              )}
            </div>
          )}
        </TabsContent>
      </Tabs>

      <Dialog open={!!confirmListing} onOpenChange={() => setConfirmListing(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirmer l&apos;achat</DialogTitle>
            <DialogDescription>
              {confirmListing && (
                <>
                  Acheter {confirmListing.itemName} pour{" "}
                  {formatCurrency(confirmListing.price)} ?
                </>
              )}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmListing(null)}>
              Annuler
            </Button>
            <Button
              className="bg-primary"
              onClick={() =>
                confirmListing && purchaseMutation.mutate(confirmListing.id)
              }
              disabled={purchaseMutation.isPending}
            >
              Confirmer
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
