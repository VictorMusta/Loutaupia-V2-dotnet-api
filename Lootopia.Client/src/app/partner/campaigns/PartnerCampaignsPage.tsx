import { useQuery } from "@tanstack/react-query";
import { useState } from "react";
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
} from "@/shared/components/ui/dialog";
import { Badge } from "@/shared/components/ui/badge";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { formatDate, formatCurrency } from "@/shared/lib/utils";
import { Megaphone, ChevronRight } from "lucide-react";

export function PartnerCampaignsPage() {
  const [selectedCampaign, setSelectedCampaign] = useState<Campaign | null>(null);

  const { data: campaigns, isLoading } = useQuery({
    queryKey: ["partner-campaigns"],
    queryFn: () => campaignsApi.mine(),
  });

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-foreground">My campaigns</h1>

      <Card className="border-border bg-card">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Megaphone className="h-5 w-5 text-primary" />
            Campaigns
          </CardTitle>
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
                    <th className="pb-3 pr-4 font-medium">Budget</th>
                    <th className="pb-3 pr-4 font-medium">Spent</th>
                    <th className="pb-3 pr-4 font-medium">Period</th>
                    <th className="pb-3 pr-4 font-medium">Status</th>
                    <th className="pb-3 font-medium">Details</th>
                  </tr>
                </thead>
                <tbody>
                  {campaigns?.map((c) => (
                    <tr key={c.id} className="border-b border-border hover:bg-secondary/30">
                      <td className="py-3 pr-4 font-medium">{c.title}</td>
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
                        <Button
                          size="sm"
                          variant="ghost"
                          onClick={() => setSelectedCampaign(c)}
                        >
                          <ChevronRight className="h-4 w-4 mr-1" />
                          View
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {!campaigns?.length && (
                <p className="py-8 text-center text-muted-foreground">No campaigns yet.</p>
              )}
            </div>
          )}
        </CardContent>
      </Card>

      <Dialog open={!!selectedCampaign} onOpenChange={() => setSelectedCampaign(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Megaphone className="h-5 w-5 text-primary" />
              Campaign details
            </DialogTitle>
            <DialogDescription>Full campaign information</DialogDescription>
          </DialogHeader>
          {selectedCampaign && (
            <div className="space-y-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Title</p>
                <p className="text-lg font-semibold text-foreground">{selectedCampaign.title}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Description</p>
                <p className="text-foreground">{selectedCampaign.description || "—"}</p>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Budget</p>
                  <p className="font-medium">{formatCurrency(selectedCampaign.budget)}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Spent</p>
                  <p className="font-medium">{formatCurrency(selectedCampaign.spent)}</p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Remaining</p>
                  <p className="font-medium">
                    {formatCurrency(selectedCampaign.budget - selectedCampaign.spent)}
                  </p>
                </div>
                <div>
                  <p className="text-sm font-medium text-muted-foreground">Status</p>
                  <Badge
                    variant={
                      selectedCampaign.status.toLowerCase() === "active"
                        ? "success"
                        : selectedCampaign.status.toLowerCase() === "draft"
                          ? "secondary"
                          : "outline"
                    }
                  >
                    {selectedCampaign.status}
                  </Badge>
                </div>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Period</p>
                <p className="text-foreground">
                  {formatDate(selectedCampaign.startDate)} → {formatDate(selectedCampaign.endDate)}
                </p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Created</p>
                <p className="text-foreground">{formatDate(selectedCampaign.createdAt)}</p>
              </div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setSelectedCampaign(null)}>
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
