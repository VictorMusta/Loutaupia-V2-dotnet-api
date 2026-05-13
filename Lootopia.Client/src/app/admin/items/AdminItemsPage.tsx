import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, useCallback } from "react";
import { itemsApi } from "@/shared/api/items";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent } from "@/shared/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { useToast } from "@/shared/components/ui/toast";
import { Plus, Package, Sparkles } from "lucide-react";

export function AdminItemsPage() {
  const { data: items = [], isLoading } = useQuery({
    queryKey: ["items"],
    queryFn: itemsApi.list,
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">Items Catalog</h1>
          <p className="text-muted-foreground">Manage digital items, rewards, and exclusive artifacts.</p>
        </div>
        <CreateItemDialog />
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <Skeleton className="h-32 w-full rounded-xl" />
          <Skeleton className="h-32 w-full rounded-xl" />
          <Skeleton className="h-32 w-full rounded-xl" />
        </div>
      ) : items.length === 0 ? (
        <Card className="border-2 border-dashed border-border p-8 text-center bg-card">
          <Package className="mx-auto h-12 w-12 text-muted-foreground mb-3" />
          <p className="text-base font-semibold text-foreground">No items available</p>
          <p className="text-xs text-muted-foreground mt-1">Create the first digital asset for hunts or store listings.</p>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {items.map((item) => (
            <Card key={item.id} className="border-2 border-border bg-card shadow-sm hover:shadow-md transition-all relative overflow-hidden flex flex-col group">
              <div className="relative h-40 w-full bg-muted/40 border-b border-border overflow-hidden flex items-center justify-center">
                {item.imageUrl ? (
                  <img 
                    src={item.imageUrl} 
                    alt={item.name} 
                    className="h-full w-full object-cover object-center group-hover:scale-105 transition-transform duration-300"
                    onError={(e) => {
                      (e.currentTarget as HTMLElement).style.display = 'none';
                    }}
                  />
                ) : (
                  <Package className="h-12 w-12 text-muted-foreground/30 stroke-[1.5]" />
                )}
                <span className="absolute bottom-2 left-2 font-semibold uppercase px-2 py-0.5 bg-background/90 backdrop-blur-sm rounded border border-border text-[10px] text-foreground shadow-sm">
                  {item.type}
                </span>
              </div>

              <CardContent className="p-4 flex-1 flex flex-col justify-between">
                <div>
                  <div className="flex justify-between items-start mb-1.5 gap-2">
                    <h3 className="font-bold text-foreground text-base truncate flex-1">{item.name}</h3>
                    <Badge
                      variant="outline"
                      className={`text-[10px] font-bold uppercase tracking-wider shrink-0 ${
                        item.rarity === "Legendary"
                          ? "bg-amber-100 text-amber-800 border-amber-300"
                          : item.rarity === "Epic"
                          ? "bg-purple-100 text-purple-800 border-purple-300"
                          : item.rarity === "Rare"
                          ? "bg-blue-100 text-blue-800 border-blue-300"
                          : "bg-gray-100 text-gray-800 border-gray-300"
                      }`}
                    >
                      {item.rarity}
                    </Badge>
                  </div>

                  <p className="text-xs text-muted-foreground line-clamp-2 mb-4 h-8">{item.description}</p>
                </div>

                <div className="flex items-center justify-between pt-2 border-t border-border/50 text-[10px] text-muted-foreground">
                  <span>{item.isTradeable ? "🔄 Tradeable" : "🔒 Account Bound"}</span>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}

function CreateItemDialog() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [rarity, setRarity] = useState("Common");
  const [type, setType] = useState("Artifact");
  const [imageUrl, setImageUrl] = useState("");
  const [isTradeable, setIsTradeable] = useState(true);

  const createMutation = useMutation({
    mutationFn: itemsApi.create,
    onSuccess: () => {
      toast({ title: "Item asset created", variant: "success" });
      queryClient.invalidateQueries({ queryKey: ["items"] });
      setOpen(false);
      resetForm();
    },
    onError: (err: Error) => {
      toast({ title: "Failed to create item", description: err.message, variant: "destructive" });
    },
  });

  const resetForm = useCallback(() => {
    setName("");
    setDescription("");
    setRarity("Common");
    setType("Artifact");
    setImageUrl("");
    setIsTradeable(true);
  }, []);

  const handleSubmit = () => {
    if (!name.trim()) {
      toast({ title: "Item name is required", variant: "destructive" });
      return;
    }
    createMutation.mutate({
      name: name.trim(),
      description: description.trim(),
      rarity,
      type,
      imageUrl: imageUrl.trim() || undefined,
      isTradeable,
    });
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="h-4 w-4 mr-2" />
          Create item asset
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Sparkles className="h-5 w-5 text-primary" />
            New Premium Item
          </DialogTitle>
          <DialogDescription>Define an exclusive token, asset, or prize in the persistent ledger.</DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-2">
          <div>
            <label className="mb-1 block text-xs font-medium text-foreground">Name</label>
            <Input
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Ex: Excalibur Sword, Golden Voucher..."
              className="bg-background"
            />
          </div>

          <div>
            <label className="mb-1 block text-xs font-medium text-foreground">Description</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Artifact background narrative or capability..."
              rows={3}
              className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-primary resize-none"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="mb-1 block text-xs font-medium text-foreground">Rarity</label>
              <select
                value={rarity}
                onChange={(e) => setRarity(e.target.value)}
                className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-primary"
              >
                <option value="Common">Common</option>
                <option value="Rare">Rare</option>
                <option value="Epic">Epic</option>
                <option value="Legendary">Legendary</option>
              </select>
            </div>

            <div>
              <label className="mb-1 block text-xs font-medium text-foreground">Type</label>
              <select
                value={type}
                onChange={(e) => setType(e.target.value)}
                className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-primary"
              >
                <option value="Artifact">Artifact</option>
                <option value="Coupon">Coupon</option>
                <option value="Token">Token</option>
              </select>
            </div>
          </div>

          <div>
            <label className="mb-1 block text-xs font-medium text-foreground">Image URL (Optional)</label>
            <Input
              value={imageUrl}
              onChange={(e) => setImageUrl(e.target.value)}
              placeholder="https://..."
              className="bg-background text-xs"
            />
          </div>

          <div className="flex items-center gap-2 pt-1">
            <input
              type="checkbox"
              id="tradeable"
              checked={isTradeable}
              onChange={(e) => setIsTradeable(e.target.checked)}
              className="rounded border-border text-primary focus:ring-primary h-4 w-4"
            />
            <label htmlFor="tradeable" className="text-xs font-medium text-foreground cursor-pointer">
              Allow item trading on public marketplace
            </label>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={createMutation.isPending}>
            {createMutation.isPending ? "Persisting..." : "Create Asset"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
