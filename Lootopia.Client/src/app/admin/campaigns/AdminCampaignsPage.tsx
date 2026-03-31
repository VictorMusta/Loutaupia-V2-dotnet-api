import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { adminApi } from "@/shared/api/admin";
import { campaignsApi, type Campaign } from "@/shared/api/campaigns";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
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
import { formatDate, formatCurrency } from "@/shared/lib/utils";
import { Plus, Play, Wallet, Megaphone } from "lucide-react";

export function AdminCampaignsPage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const { data: campaigns, isLoading } = useQuery({
    queryKey: ["admin-campaigns"],
    queryFn: () => campaignsApi.list(),
  });

  const activateMutation = useMutation({
    mutationFn: (id: string) => campaignsApi.activate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-campaigns"] });
      toast({ title: "Campaign activated", variant: "success" });
    },
    onError: (err: Error) => {
      toast({ title: "Failed to activate", description: err.message, variant: "destructive" });
    },
  });

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold text-foreground">Campaigns</h1>
        <CreateCampaignDialog onSuccess={() => queryClient.invalidateQueries({ queryKey: ["admin-campaigns"] })} />
      </div>

      <Card className="border-border bg-card">
        <CardHeader>
          <CardTitle className="text-foreground">All campaigns</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <Skeleton className="h-64 w-full" />
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border text-left text-muted-foreground">
                    <th className="pb-3 pr-4 font-medium">Title</th>
                    <th className="pb-3 pr-4 font-medium">Partner</th>
                    <th className="pb-3 pr-4 font-medium">Budget</th>
                    <th className="pb-3 pr-4 font-medium">Spent</th>
                    <th className="pb-3 pr-4 font-medium">Period</th>
                    <th className="pb-3 pr-4 font-medium">Status</th>
                    <th className="pb-3 font-medium">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {campaigns?.map((c) => (
                    <tr key={c.id} className="border-b border-border">
                      <td className="py-3 pr-4 font-medium">{c.title}</td>
                      <td className="py-3 pr-4">{c.partnerName}</td>
                      <td className="py-3 pr-4">{formatCurrency(c.budget)}</td>
                      <td className="py-3 pr-4">{formatCurrency(c.spent)}</td>
                      <td className="py-3 pr-4">
                        {formatDate(c.startDate)} → {formatDate(c.endDate)}
                      </td>
                      <td className="py-3 pr-4">
                        <Badge
                          variant={
                            c.status.toLowerCase() === "active"
                              ? "success"
                              : c.status.toLowerCase() === "draft"
                                ? "secondary"
                                : "outline"
                          }
                        >
                          {c.status}
                        </Badge>
                      </td>
                      <td className="py-3">
                        <div className="flex gap-2">
                          {c.status.toLowerCase() === "draft" && (
                            <Button
                              size="sm"
                              onClick={() => activateMutation.mutate(c.id)}
                              disabled={activateMutation.isPending}
                            >
                              <Play className="h-4 w-4 mr-1" />
                              Activate
                            </Button>
                          )}
                          <CreditPartnerDialog campaign={c} onSuccess={() => queryClient.invalidateQueries({ queryKey: ["admin-campaigns"] })} />
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {!campaigns?.length && (
                <p className="py-8 text-center text-muted-foreground">No campaigns.</p>
              )}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

function CreateCampaignDialog({ onSuccess }: { onSuccess: () => void }) {
  const { toast } = useToast();
  const [open, setOpen] = useState(false);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [budget, setBudget] = useState(1000);
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");

  const createMutation = useMutation({
    mutationFn: (data: Parameters<typeof campaignsApi.create>[0]) => campaignsApi.create(data),
    onSuccess: () => {
      toast({ title: "Campaign created", variant: "success" });
      setOpen(false);
      setTitle("");
      setDescription("");
      setBudget(1000);
      setStartDate("");
      setEndDate("");
      onSuccess();
    },
    onError: (err: Error) => {
      toast({ title: "Failed to create campaign", description: err.message, variant: "destructive" });
    },
  });

  const handleSubmit = () => {
    if (!title.trim()) {
      toast({ title: "Title is required", variant: "destructive" });
      return;
    }
    if (!startDate || !endDate) {
      toast({ title: "Start and end dates are required", variant: "destructive" });
      return;
    }
    if (budget < 0) {
      toast({ title: "Budget must be positive", variant: "destructive" });
      return;
    }
    createMutation.mutate({ title: title.trim(), description, budget, startDate, endDate });
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="h-4 w-4 mr-2" />
          Create campaign
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Megaphone className="h-5 w-5 text-primary" />
            Create campaign
          </DialogTitle>
          <DialogDescription>Define a new partner campaign.</DialogDescription>
        </DialogHeader>
        <div className="space-y-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-foreground">Title</label>
            <Input
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="Campaign title"
              className="bg-background"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-foreground">Description</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Description"
              rows={3}
              className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
            />
          </div>
          <div>
            <label className="mb-1 block text-sm font-medium text-foreground">Budget (LTK)</label>
            <Input
              type="number"
              min={0}
              value={budget}
              onChange={(e) => setBudget(Number(e.target.value) || 0)}
              className="bg-background"
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="mb-1 block text-sm font-medium text-foreground">Start date</label>
              <Input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="bg-background"
              />
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium text-foreground">End date</label>
              <Input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="bg-background"
              />
            </div>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={createMutation.isPending || !title.trim() || !startDate || !endDate}
          >
            {createMutation.isPending ? "Creating…" : "Create"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function CreditPartnerDialog({ campaign, onSuccess }: { campaign: Campaign; onSuccess: () => void }) {
  const { toast } = useToast();
  const [open, setOpen] = useState(false);
  const [amount, setAmount] = useState(500);

  const creditMutation = useMutation({
    mutationFn: () => adminApi.creditPartner(campaign.partnerId, amount),
    onSuccess: () => {
      toast({ title: "Partner credited", variant: "success" });
      setOpen(false);
      setAmount(500);
      onSuccess();
    },
    onError: (err: Error) => {
      toast({ title: "Failed to credit partner", description: err.message, variant: "destructive" });
    },
  });

  const handleSubmit = () => {
    if (amount <= 0) {
      toast({ title: "Amount must be positive", variant: "destructive" });
      return;
    }
    creditMutation.mutate();
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button size="sm" variant="outline">
          <Wallet className="h-4 w-4 mr-1" />
          Credit partner
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Wallet className="h-5 w-5 text-primary" />
            Credit partner budget
          </DialogTitle>
          <DialogDescription>
            Add budget for {campaign.partnerName}. Current budget: {formatCurrency(campaign.budget)}, spent: {formatCurrency(campaign.spent)}.
          </DialogDescription>
        </DialogHeader>
        <div>
          <label className="mb-1 block text-sm font-medium text-foreground">Amount (LTK)</label>
          <Input
            type="number"
            min={1}
            value={amount}
            onChange={(e) => setAmount(Number(e.target.value) || 0)}
            className="bg-background"
          />
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={creditMutation.isPending || amount <= 0}>
            {creditMutation.isPending ? "Crediting…" : "Credit"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
