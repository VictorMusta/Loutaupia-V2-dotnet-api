import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, useEffect } from "react";
import { adminApi, type FraudAlert } from "@/shared/api/admin";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Input } from "@/shared/components/ui/input";
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
import { CheckCircle, Snowflake, AlertTriangle, Settings2, ShieldCheck } from "lucide-react";

const PAGE_SIZE = 20;

export function AdminFraudPage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [selectedAlert, setSelectedAlert] = useState<FraudAlert | null>(null);
  
  // Bulk selection
  const [selectedAlertIds, setSelectedAlertIds] = useState<Set<string>>(new Set());

  // Settings state
  const [settingsForm, setSettingsForm] = useState({
    threshold: "5",
    timeWindow: "60",
    radius: "50"
  });

  const { data: settingsData } = useQuery({
    queryKey: ["admin-fraud-settings"],
    queryFn: adminApi.getFraudSettings,
  });

  useEffect(() => {
    if (settingsData) {
      setSettingsForm({
        threshold: settingsData["Fraud_ThresholdCount"] || "5",
        timeWindow: settingsData["Fraud_TimeWindowSeconds"] || "60",
        radius: settingsData["Fraud_RadiusMeters"] || "50",
      });
    }
  }, [settingsData]);

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

  const settingsMutation = useMutation({
    mutationFn: (settings: Record<string, string>) => adminApi.updateFraudSettings(settings),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-fraud-settings"] });
      toast({ title: "Settings updated", variant: "success" });
    },
    onError: (err: Error) => {
      toast({ title: "Failed to update settings", description: err.message, variant: "destructive" });
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

  const handleBulkAcknowledge = async () => {
    const ids = Array.from(selectedAlertIds);
    if (ids.length === 0) return;
    
    // Process sequentially to not overload backend
    for (const id of ids) {
      try {
        await acknowledgeMutation.mutateAsync(id);
      } catch (e) {
        console.error(e);
      }
    }
    setSelectedAlertIds(new Set());
    toast({ title: `${ids.length} alerts acknowledged`, variant: "success" });
  };

  const toggleSelection = (id: string) => {
    const next = new Set(selectedAlertIds);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    setSelectedAlertIds(next);
  };

  const toggleAll = () => {
    if (!data) return;
    if (selectedAlertIds.size === data.items.filter(i => i.status === "New").length) {
      setSelectedAlertIds(new Set());
    } else {
      setSelectedAlertIds(new Set(data.items.filter(i => i.status === "New").map(i => i.id)));
    }
  };

  const saveSettings = () => {
    settingsMutation.mutate({
      "Fraud_ThresholdCount": settingsForm.threshold,
      "Fraud_TimeWindowSeconds": settingsForm.timeWindow,
      "Fraud_RadiusMeters": settingsForm.radius,
    });
  };

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-foreground">Fraud Management</h1>

      <Card className="border-border bg-card">
        <CardHeader>
          <CardTitle className="text-foreground flex items-center gap-2">
            <Settings2 className="h-5 w-5 text-primary" />
            Detection Rules
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground mb-4">
            A player will trigger a fraud alert if they validate more than <strong>{settingsForm.threshold} steps</strong> within <strong>{settingsForm.timeWindow} seconds</strong> inside a radius of <strong>{settingsForm.radius} meters</strong>.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-4">
            <div>
              <label className="text-xs text-muted-foreground block mb-1">Max Validations</label>
              <Input 
                type="number" 
                value={settingsForm.threshold} 
                onChange={e => setSettingsForm(s => ({...s, threshold: e.target.value}))}
              />
            </div>
            <div>
              <label className="text-xs text-muted-foreground block mb-1">Time Window (sec)</label>
              <Input 
                type="number" 
                value={settingsForm.timeWindow} 
                onChange={e => setSettingsForm(s => ({...s, timeWindow: e.target.value}))}
              />
            </div>
            <div>
              <label className="text-xs text-muted-foreground block mb-1">Radius (meters)</label>
              <Input 
                type="number" 
                value={settingsForm.radius} 
                onChange={e => setSettingsForm(s => ({...s, radius: e.target.value}))}
              />
            </div>
          </div>
          <Button 
            onClick={saveSettings} 
            disabled={settingsMutation.isPending}
            className="bg-primary hover:bg-primary/90"
          >
            Save Rules
          </Button>
        </CardContent>
      </Card>

      <Card className="border-border bg-card">
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-foreground">Pending & Recent Alerts</CardTitle>
          {selectedAlertIds.size > 0 && (
            <Button size="sm" onClick={handleBulkAcknowledge}>
              <ShieldCheck className="h-4 w-4 mr-2" />
              Acknowledge Selected ({selectedAlertIds.size})
            </Button>
          )}
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
                      <th className="pb-3 pr-4 font-medium w-8">
                        <input 
                          type="checkbox" 
                          className="rounded border-input bg-background"
                          checked={(data?.items?.filter(i => i.status === "New").length ?? 0) > 0 && selectedAlertIds.size === (data?.items?.filter(i => i.status === "New").length ?? 0)}
                          onChange={toggleAll}
                        />
                      </th>
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
                      <tr key={alert.id} className="border-b border-border hover:bg-muted/50 transition-colors">
                        <td className="py-3 pr-4">
                          {alert.status === "New" && (
                            <input 
                              type="checkbox"
                              className="rounded border-input bg-background"
                              checked={selectedAlertIds.has(alert.id)}
                              onChange={() => toggleSelection(alert.id)}
                            />
                          )}
                        </td>
                        <td className="py-3 pr-4 font-medium">{alert.type}</td>
                        <td className="py-3 pr-4">{alert.userName}</td>
                        <td className="py-3 pr-4">
                          <Badge variant={getSeverityVariant(alert.severity)}>
                            {alert.severity}
                          </Badge>
                        </td>
                        <td className="py-3 pr-4">
                          <Badge variant={alert.status === "New" ? "warning" : "secondary"}>
                            {alert.status}
                          </Badge>
                        </td>
                        <td className="py-3 pr-4 text-muted-foreground">{formatDateTime(alert.createdAt)}</td>
                        <td className="py-3">
                          <div className="flex gap-2">
                            <Button size="sm" variant="outline" onClick={() => setSelectedAlert(alert)}>
                              Details
                            </Button>
                            {alert.status === "New" && (
                              <Button
                                size="sm"
                                variant="outline"
                                className="text-success border-success/50 hover:bg-success/10"
                                onClick={() => acknowledgeMutation.mutate(alert.id)}
                                disabled={acknowledgeMutation.isPending}
                              >
                                <CheckCircle className="h-4 w-4" />
                              </Button>
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
            {selectedAlert?.status === "New" && (
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
