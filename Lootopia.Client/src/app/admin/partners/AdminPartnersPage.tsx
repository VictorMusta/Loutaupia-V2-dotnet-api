import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, useCallback } from "react";
import { partnersApi, type AdminPartner } from "@/shared/api/partners";
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
import { Plus, Briefcase, PlusCircle, Building2, MapPin, Mail } from "lucide-react";
import { formatDate, formatCurrency } from "@/shared/lib/utils";

export function AdminPartnersPage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [selectedPartnerForCredit, setSelectedPartnerForCredit] = useState<AdminPartner | null>(null);
  const [creditAmount, setCreditAmount] = useState<number>(1000);

  const { data: partners = [], isLoading } = useQuery({
    queryKey: ["admin-partners"],
    queryFn: partnersApi.listAdmin,
  });

  const creditMutation = useMutation({
    mutationFn: ({ id, amount }: { id: string; amount: number }) => partnersApi.creditAdmin(id, amount),
    onSuccess: () => {
      toast({ title: "Compte partenaire crédité avec succès", variant: "success" });
      queryClient.invalidateQueries({ queryKey: ["admin-partners"] });
      setSelectedPartnerForCredit(null);
      setCreditAmount(1000);
    },
    onError: (err: Error) => {
      toast({ title: "Erreur lors du crédit", description: err.message, variant: "destructive" });
    },
  });

  const handleCreditSubmit = () => {
    if (!selectedPartnerForCredit || creditAmount <= 0) return;
    creditMutation.mutate({ id: selectedPartnerForCredit.id, amount: creditAmount });
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight text-foreground">Comptes Partenaires</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Gérez les établissements affiliés, les accès sponsorisés et allouez des budgets de récompense.
          </p>
        </div>
        <CreatePartnerDialog onSuccess={() => queryClient.invalidateQueries({ queryKey: ["admin-partners"] })} />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {isLoading ? (
          Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-56 w-full rounded-xl" />
          ))
        ) : partners.length === 0 ? (
          <Card className="col-span-full border-2 border-dashed border-border p-12 text-center bg-card">
            <Briefcase className="mx-auto h-12 w-12 text-muted-foreground/40 mb-3 stroke-[1.5]" />
            <p className="text-base font-semibold text-foreground">Aucun compte partenaire</p>
            <p className="text-xs text-muted-foreground mt-1">Créez le premier partenaire pour lier des chasses sponsorisées.</p>
          </Card>
        ) : (
          partners.map((partner) => (
            <Card key={partner.id} className="border-2 border-border bg-card shadow-sm hover:shadow-md transition-all flex flex-col justify-between overflow-hidden relative group">
              <div className="p-5 border-b border-border/60 bg-muted/20">
                <div className="flex items-start justify-between gap-3">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-1.5 text-xs font-semibold text-primary uppercase tracking-wide">
                      <Building2 className="h-3.5 w-3.5 shrink-0" />
                      <span className="truncate">{partner.displayName}</span>
                    </div>
                    <h3 className="text-lg font-bold text-foreground truncate mt-1">{partner.businessName}</h3>
                  </div>
                  <Badge variant={partner.isActive ? "success" : "secondary"} className="shrink-0 text-[10px]">
                    {partner.isActive ? "Actif" : "Inactif"}
                  </Badge>
                </div>

                {partner.address && (
                  <div className="flex items-center gap-1.5 text-xs text-muted-foreground mt-2 truncate">
                    <MapPin className="h-3.5 w-3.5 shrink-0 text-muted-foreground/70" />
                    <span className="truncate">{partner.address}</span>
                  </div>
                )}
                <div className="flex items-center gap-1.5 text-xs text-muted-foreground mt-1 truncate">
                  <Mail className="h-3.5 w-3.5 shrink-0 text-muted-foreground/70" />
                  <span className="truncate">{partner.email}</span>
                </div>
              </div>

              <CardContent className="p-5 flex-1 flex flex-col justify-between gap-4">
                <div className="bg-background rounded-lg p-3 border border-border flex items-center justify-between">
                  <div>
                    <p className="text-[10px] font-medium text-muted-foreground uppercase tracking-wider">Budget Restant</p>
                    <p className="text-xl font-extrabold text-foreground mt-0.5">{formatCurrency(partner.tokenBudget)}</p>
                  </div>
                  <Button 
                    size="sm" 
                    variant="outline" 
                    className="h-8 gap-1 border-primary/30 hover:bg-primary/10 hover:text-primary transition-colors text-xs font-semibold"
                    onClick={() => {
                      setSelectedPartnerForCredit(partner);
                      setCreditAmount(1000);
                    }}
                  >
                    <PlusCircle className="h-3.5 w-3.5 text-primary" />
                    Créditer
                  </Button>
                </div>

                <div className="text-[10px] text-muted-foreground text-right">
                  Créé le {formatDate(partner.createdAt)}
                </div>
              </CardContent>
            </Card>
          ))
        )}
      </div>

      {/* Credit Dialog */}
      <Dialog open={!!selectedPartnerForCredit} onOpenChange={(open) => !open && setSelectedPartnerForCredit(null)}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Allouer du budget d'engagement</DialogTitle>
            <DialogDescription>
              Ajoutez des crédits LTK au compte de <strong>{selectedPartnerForCredit?.businessName}</strong> pour financer leurs récompenses.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-3">
            <div>
              <label className="mb-1 block text-xs font-medium text-foreground">Montant à créditer (LTK)</label>
              <Input
                type="number"
                min={1}
                step={100}
                value={creditAmount}
                onChange={(e) => setCreditAmount(Number(e.target.value) || 0)}
                className="bg-background font-bold text-lg"
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setSelectedPartnerForCredit(null)}>
              Annuler
            </Button>
            <Button onClick={handleCreditSubmit} disabled={creditMutation.isPending || creditAmount <= 0}>
              {creditMutation.isPending ? "Crédit en cours..." : "Valider l'allocation"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}

function CreatePartnerDialog({ onSuccess }: { onSuccess: () => void }) {
  const { toast } = useToast();
  const [open, setOpen] = useState(false);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [displayName, setDisplayName] = useState("");
  const [businessName, setBusinessName] = useState("");
  const [address, setAddress] = useState("");
  const [initialBudget, setInitialBudget] = useState(5000);

  const createMutation = useMutation({
    mutationFn: partnersApi.createAdmin,
    onSuccess: () => {
      toast({ title: "Compte partenaire créé avec succès", variant: "success" });
      setOpen(false);
      resetForm();
      onSuccess();
    },
    onError: (err: Error) => {
      toast({ title: "Échec de création", description: err.message, variant: "destructive" });
    },
  });

  const resetForm = useCallback(() => {
    setEmail("");
    setPassword("");
    setDisplayName("");
    setBusinessName("");
    setAddress("");
    setInitialBudget(5000);
  }, []);

  const handleSubmit = () => {
    if (!email.trim() || !businessName.trim() || !displayName.trim()) {
      toast({ title: "Veuillez remplir tous les champs obligatoires", variant: "destructive" });
      return;
    }
    createMutation.mutate({
      email: email.trim(),
      password: password.trim() || undefined,
      displayName: displayName.trim(),
      businessName: businessName.trim(),
      address: address.trim() || undefined,
      initialBudget: Number(initialBudget) || 0,
    });
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button className="gap-2 font-semibold">
          <Plus className="h-4 w-4" />
          Nouveau Partenaire
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Briefcase className="h-5 w-5 text-primary" />
            Créer un compte Partenaire
          </DialogTitle>
          <DialogDescription>
            Provisionnez un accès professionnel pour parrainer des chasses et des coupons géolocalisés.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-3 py-2">
          <div>
            <label className="mb-1 block text-xs font-medium text-foreground">Nom du contact / Responsable *</label>
            <Input
              value={displayName}
              onChange={(e) => setDisplayName(e.target.value)}
              placeholder="Ex: Jean-Pierre Boulanger"
              className="bg-background text-xs"
            />
          </div>

          <div>
            <label className="mb-1 block text-xs font-medium text-foreground">Nom de l'établissement *</label>
            <Input
              value={businessName}
              onChange={(e) => setBusinessName(e.target.value)}
              placeholder="Ex: Boulangerie Jean-Pierre"
              className="bg-background text-xs"
            />
          </div>

          <div>
            <label className="mb-1 block text-xs font-medium text-foreground">Adresse de connexion (Email) *</label>
            <Input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="contact@boulangerie.io"
              className="bg-background text-xs"
            />
          </div>

          <div>
            <label className="mb-1 block text-xs font-medium text-foreground">Mot de passe (Défaut : Partner123!)</label>
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Laisser vide pour utiliser le mot de passe par défaut"
              className="bg-background text-xs"
            />
          </div>

          <div>
            <label className="mb-1 block text-xs font-medium text-foreground">Adresse postale de l'établissement (Optionnel)</label>
            <Input
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              placeholder="Ex: 12 Rue de la Paix, Lyon"
              className="bg-background text-xs"
            />
          </div>

          <div>
            <label className="mb-1 block text-xs font-medium text-foreground">Budget initial alloué (LTK)</label>
            <Input
              type="number"
              min={0}
              step={500}
              value={initialBudget}
              onChange={(e) => setInitialBudget(Number(e.target.value) || 0)}
              className="bg-background font-bold text-sm"
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Annuler
          </Button>
          <Button onClick={handleSubmit} disabled={createMutation.isPending}>
            {createMutation.isPending ? "Création..." : "Confirmer la création"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
