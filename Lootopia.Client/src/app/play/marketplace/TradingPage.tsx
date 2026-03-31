import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { ArrowLeftRight, Send } from "lucide-react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { tradingApi } from "@/shared/api/trading";
import { inventoryApi } from "@/shared/api/inventory";
import { useAuth } from "@/shared/providers/AuthProvider";
import { useToast } from "@/shared/components/ui/toast";
import { formatDateTime } from "@/shared/lib/utils";

export function TradingPage() {
  const { user } = useAuth();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [toUserId, setToUserId] = useState("");
  const [offeredIds, setOfferedIds] = useState<string[]>([]);
  const [requestedIdsInput, setRequestedIdsInput] = useState("");
  const [showCreateForm, setShowCreateForm] = useState(false);

  const { data: offers, isLoading } = useQuery({
    queryKey: ["trading", "list"],
    queryFn: () => tradingApi.list(),
  });

  const { data: inventory } = useQuery({
    queryKey: ["inventory"],
    queryFn: () => inventoryApi.list({ size: 100 }),
  });

  const respondMutation = useMutation({
    mutationFn: ({ offerId, accept }: { offerId: string; accept: boolean }) =>
      tradingApi.respond(offerId, accept),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["trading"] });
      queryClient.invalidateQueries({ queryKey: ["inventory"] });
      toast({ title: "Réponse envoyée", variant: "success" });
    },
    onError: (err) => {
      toast({
        title: "Erreur",
        description: err instanceof Error ? err.message : "Action impossible",
        variant: "destructive",
      });
    },
  });

  const requestedIds = requestedIdsInput
    .split(/[\s,]+/)
    .map((s) => s.trim())
    .filter(Boolean);

  const createMutation = useMutation({
    mutationFn: () =>
      tradingApi.create(toUserId, offeredIds, requestedIds),
    onSuccess: () => {
      setToUserId("");
      setOfferedIds([]);
      setRequestedIdsInput("");
      setShowCreateForm(false);
      queryClient.invalidateQueries({ queryKey: ["trading"] });
      toast({ title: "Offre envoyée", variant: "success" });
    },
    onError: (err) => {
      toast({
        title: "Erreur",
        description: err instanceof Error ? err.message : "Impossible d'envoyer",
        variant: "destructive",
      });
    },
  });

  const received = offers?.filter((o) => o.toUserId === user?.id) ?? [];
  const sent = offers?.filter((o) => o.fromUserId === user?.id) ?? [];

  const toggleOffered = (id: string) => {
    setOfferedIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id],
    );
  };

  const handleCreate = () => {
    if (!toUserId.trim()) {
      toast({
        title: "Erreur",
        description: "Indiquez l'ID ou le nom du destinataire.",
        variant: "destructive",
      });
      return;
    }
    if (offeredIds.length === 0 && requestedIds.length === 0) {
      toast({
        title: "Erreur",
        description: "Proposez ou demandez au moins un objet.",
        variant: "destructive",
      });
      return;
    }
    createMutation.mutate();
  };

  return (
    <div className="h-full flex flex-col gap-4 p-4 bg-slate-950 overflow-y-auto">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-foreground">Échanges</h1>
        <Button
          size="sm"
          className="bg-primary hover:bg-primary/90"
          onClick={() => setShowCreateForm(!showCreateForm)}
        >
          <ArrowLeftRight className="h-4 w-4 mr-2" />
          Proposer un échange
        </Button>
      </div>

      {showCreateForm && (
        <Card className="border-border bg-card/80">
          <CardContent className="p-4 space-y-4">
            <div>
              <label className="text-sm text-muted-foreground block mb-2">
                Destinataire (ID utilisateur)
              </label>
              <Input
                placeholder="ID ou nom"
                value={toUserId}
                onChange={(e) => setToUserId(e.target.value)}
              />
            </div>
            <div>
              <label className="text-sm text-muted-foreground block mb-2">
                Objets que je propose
              </label>
              <div className="flex flex-wrap gap-2">
                {inventory?.items?.filter((i) => i.isTradeable).map((item) => (
                  <Button
                    key={item.itemId}
                    variant={offeredIds.includes(item.itemId) ? "default" : "outline"}
                    size="sm"
                    onClick={() => toggleOffered(item.itemId)}
                  >
                    {item.name}
                  </Button>
                ))}
              </div>
            </div>
            <div>
              <label className="text-sm text-muted-foreground block mb-2">
                IDs des objets demandés (séparés par des virgules)
              </label>
              <Input
                placeholder="id1, id2, id3..."
                value={requestedIdsInput}
                onChange={(e) => setRequestedIdsInput(e.target.value)}
              />
            </div>
            <Button
              className="bg-primary w-full"
              onClick={handleCreate}
              disabled={createMutation.isPending}
            >
              <Send className="h-4 w-4 mr-2" />
              Envoyer l&apos;offre
            </Button>
          </CardContent>
        </Card>
      )}

      {isLoading ? (
        <div className="space-y-3">
          {[...Array(4)].map((_, i) => (
            <Skeleton key={i} className="h-24 rounded-xl" />
          ))}
        </div>
      ) : (
        <div className="space-y-4">
          <section>
            <h2 className="font-semibold text-foreground mb-2">
              Reçues
            </h2>
            {received.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                Aucune offre reçue
              </p>
            ) : (
              received.map((offer) => (
                <Card
                  key={offer.id}
                  className="border-border bg-card/80 mb-3"
                >
                  <CardContent className="p-4">
                    <p className="text-sm text-muted-foreground">
                      De {offer.fromUserName} •{" "}
                      {formatDateTime(offer.createdAt)}
                    </p>
                    <p className="mt-1 text-foreground">
                      Propose {offer.offeredItemIds.length} objet(s), demande{" "}
                      {offer.requestedItemIds.length} objet(s)
                    </p>
                    {offer.status === "Pending" && (
                      <div className="flex gap-2 mt-3">
                        <Button
                          size="sm"
                          variant="outline"
                          onClick={() =>
                            respondMutation.mutate({
                              offerId: offer.id,
                              accept: false,
                            })
                          }
                          disabled={respondMutation.isPending}
                        >
                          Refuser
                        </Button>
                        <Button
                          size="sm"
                          className="bg-success hover:bg-success/90"
                          onClick={() =>
                            respondMutation.mutate({
                              offerId: offer.id,
                              accept: true,
                            })
                          }
                          disabled={respondMutation.isPending}
                        >
                          Accepter
                        </Button>
                      </div>
                    )}
                  </CardContent>
                </Card>
              ))
            )}
          </section>
          <section>
            <h2 className="font-semibold text-foreground mb-2">Envoyées</h2>
            {sent.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                Aucune offre envoyée
              </p>
            ) : (
              sent.map((offer) => (
                <Card
                  key={offer.id}
                  className="border-border bg-card/80 mb-3"
                >
                  <CardContent className="p-4">
                    <p className="text-sm text-muted-foreground">
                      À {offer.toUserName} • {formatDateTime(offer.createdAt)}
                    </p>
                    <p className="mt-1 text-foreground">
                      Statut: {offer.status}
                    </p>
                  </CardContent>
                </Card>
              ))
            )}
          </section>
        </div>
      )}
    </div>
  );
}
