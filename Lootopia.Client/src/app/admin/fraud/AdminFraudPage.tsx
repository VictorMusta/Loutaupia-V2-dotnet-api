import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { adminApi, type FraudAlert } from "@/shared/api/admin";
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
import { useToast } from "@/shared/components/ui/toast";
import { formatDateTime } from "@/shared/lib/utils";
import { CheckCircle, Snowflake, AlertTriangle } from "lucide-react";

const PAGE_SIZE = 20;

export function AdminFraudPage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [selectedAlert, setSelectedAlert] = useState<FraudAlert | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ["admin-fraud-alerts", page],
    queryFn: () => adminApi.fraudAlerts(page, PAGE_SIZE),
  });

  const acknowledgeMutation = useMutation({
    mutationFn: (alertId: string) => adminApi.acknowledgeFraudAlert(alertId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-fraud-alerts"] });
      setSelectedAlert(null);
      toast({ title: "Alert acknowledged", variant: "success" });
    },
    onError: (err: Error) => {
      toast({ title: "Failed to acknowledge", description: err.message, variant: "destructive" });
    },
  });

  const freezeMutation = useMutation({
    mutationFn: (userId: string) => adminApi.freezeUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-fraud-alerts"] });
      toast({ title: "User frozen", variant: "success" });
    },
    onError: (err: Error) => {
      toast({ title: "Failed to freeze user", description: err.message, variant: "destructive" });
    },
  });

  const totalPages = data ? Math.ceil(data.total / PAGE_SIZE) : 0;

  const getSeverityVariant = (severity: string) => {
    const s = severity.toLowerCase();
    if (s === "high" || s === "critical") return "destructive";
    if (s === "medium") return "warning";
    return "secondary";
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-foreground">Fraud alerts</h1>

      <Card className="border-border bg-card">
        <CardHeader>
          <CardTitle className="text-foreground">Alerts</CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <Skeleton className="h-64 w-full" />
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border text-left text-muted-foreground">
                      <th className="pb-3 pr-4 font-medium">Type</th>
                      <th className="pb-3 pr-4 font-medium">User</th>
                      <th className="pb-3 pr-4 font-medium">Severity</th>
                      <th className="pb-3 pr-4 font-medium">Status</th>
                      <th className="pb-3 pr-4 font-medium">Date</th>
                      <th className="pb-3 font-medium">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data?.items.map((alert) => (
                      <tr key={alert.id} className="border-b border-border">
                        <td className="py-3 pr-4">{alert.type}</td>
                        <td className="py-3 pr-4">{alert.userName}</td>
                        <td className="py-3 pr-4">
                          <Badge variant={getSeverityVariant(alert.severity)}>
                            {alert.severity}
                          </Badge>
                        </td>
                        <td className="py-3 pr-4">
                          <Badge variant={alert.status === "Pending" ? "warning" : "secondary"}>
                            {alert.status}
                          </Badge>
                        </td>
                        <td className="py-3 pr-4">{formatDateTime(alert.createdAt)}</td>
                        <td className="py-3">
                          <div className="flex gap-2">
                            <Button size="sm" variant="outline" onClick={() => setSelectedAlert(alert)}>
                              Details
                            </Button>
                            {alert.status === "Pending" && (
                              <>
                                <Button
                                  size="sm"
                                  onClick={() => acknowledgeMutation.mutate(alert.id)}
                                  disabled={acknowledgeMutation.isPending}
                                >
                                  <CheckCircle className="h-4 w-4 mr-1" />
                                  Acknowledge
                                </Button>
                                <Button
                                  size="sm"
                                  variant="destructive"
                                  onClick={() => freezeMutation.mutate(alert.userId)}
                                  disabled={freezeMutation.isPending}
                                >
                                  <Snowflake className="h-4 w-4 mr-1" />
                                  Freeze user
                                </Button>
                              </>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {!data?.items?.length && (
                  <p className="py-8 text-center text-muted-foreground">No fraud alerts.</p>
                )}
              </div>
              {totalPages > 1 && (
                <div className="mt-4 flex items-center justify-between">
                  <p className="text-sm text-muted-foreground">
                    Page {page} of {totalPages} ({data?.total ?? 0} total)
                  </p>
                  <div className="flex gap-2">
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page <= 1}
                    >
                      Previous
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                      disabled={page >= totalPages}
                    >
                      Next
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>

      <Dialog open={!!selectedAlert} onOpenChange={() => setSelectedAlert(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-warning" />
              Alert details
            </DialogTitle>
            <DialogDescription>Full fraud alert information</DialogDescription>
          </DialogHeader>
          {selectedAlert && (
            <div className="space-y-4">
              <div>
                <p className="text-sm font-medium text-muted-foreground">Type</p>
                <p className="text-foreground">{selectedAlert.type}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">User</p>
                <p className="text-foreground">{selectedAlert.userName} ({selectedAlert.userId})</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Severity</p>
                <Badge variant={getSeverityVariant(selectedAlert.severity)}>
                  {selectedAlert.severity}
                </Badge>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Description</p>
                <p className="text-foreground">{selectedAlert.description}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-muted-foreground">Created at</p>
                <p className="text-foreground">{formatDateTime(selectedAlert.createdAt)}</p>
              </div>
            </div>
          )}
          <DialogFooter>
            {selectedAlert?.status === "Pending" && (
              <>
                <Button
                  variant="outline"
                  onClick={() => freezeMutation.mutate(selectedAlert.userId)}
                  disabled={freezeMutation.isPending}
                >
                  <Snowflake className="h-4 w-4 mr-2" />
                  Freeze user
                </Button>
                <Button
                  onClick={() => acknowledgeMutation.mutate(selectedAlert.id)}
                  disabled={acknowledgeMutation.isPending}
                >
                  Acknowledge
                </Button>
              </>
            )}
            <Button variant="outline" onClick={() => setSelectedAlert(null)}>
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
