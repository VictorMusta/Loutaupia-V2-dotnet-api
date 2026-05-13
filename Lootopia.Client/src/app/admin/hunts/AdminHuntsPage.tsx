import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, useCallback, useEffect, Fragment } from "react";
import { createPortal } from "react-dom";
import { MapContainer, TileLayer, Marker, Popup, useMapEvents, Circle, useMap, Polyline } from "react-leaflet";
import L, { type LatLngLiteral } from "leaflet";
import { huntsApi } from "@/shared/api/hunts";
import { itemsApi } from "@/shared/api/items";
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
import { Plus, MapPin, Play, ChevronDown, ChevronUp, Trash2, Check, Maximize2, GripVertical } from "lucide-react";
import { cn } from "@/shared/lib/utils";

interface HuntStepForm {
  clue: string;
  latitude: number;
  longitude: number;
  radiusMeters: number;
  actionType: string;
}

function MapClickHandler({
  onMapClick,
}: {
  onMapClick: (latlng: LatLngLiteral) => void;
}) {
  useMapEvents({
    click(e) {
      onMapClick(e.latlng);
    },
  });
  return null;
}

export function AdminHuntsPage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [statusFilter, setStatusFilter] = useState<string>("all");

  const { data: hunts, isLoading } = useQuery({
    queryKey: ["admin-hunts"],
    queryFn: () => huntsApi.listAll(),
  });

  const activateMutation = useMutation({
    mutationFn: (id: string) => huntsApi.activate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-hunts"] });
      toast({ title: "Hunt activated", variant: "success" });
    },
    onError: (err: Error) => {
      toast({ title: "Failed to activate", description: err.message, variant: "destructive" });
    },
  });

  const filteredHunts =
    hunts?.filter((h) =>
      statusFilter === "all" ? true : h.status.toLowerCase() === statusFilter.toLowerCase(),
    ) ?? [];

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <h1 className="text-2xl font-bold text-foreground">Hunts</h1>
        <div className="flex gap-2">
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className="rounded-md border border-input bg-background px-3 py-2 text-sm"
          >
            <option value="all">All statuses</option>
            <option value="draft">Draft</option>
            <option value="active">Active</option>
            <option value="archived">Archived</option>
          </select>
          <CreateHuntDialog onSuccess={() => queryClient.invalidateQueries({ queryKey: ["admin-hunts"] })} />
        </div>
      </div>

      <Card className="border-border bg-card">
        <CardHeader>
          <CardTitle className="text-foreground">All hunts</CardTitle>
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
                    <th className="pb-3 pr-4 font-medium">Difficulty</th>
                    <th className="pb-3 pr-4 font-medium">Reward</th>
                    <th className="pb-3 pr-4 font-medium">Steps</th>
                    <th className="pb-3 pr-4 font-medium">Status</th>
                    <th className="pb-3 font-medium">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredHunts.map((hunt) => (
                    <tr key={hunt.id} className="border-b border-border">
                      <td className="py-3 pr-4 font-medium">{hunt.title}</td>
                      <td className="py-3 pr-4">{hunt.difficulty}</td>
                      <td className="py-3 pr-4">{hunt.rewardTokens} LTK</td>
                      <td className="py-3 pr-4">{hunt.stepCount ?? 0}</td>
                      <td className="py-3 pr-4">
                        <Badge
                          variant={
                            hunt.status.toLowerCase() === "active"
                              ? "success"
                              : hunt.status.toLowerCase() === "draft"
                                ? "secondary"
                                : "outline"
                          }
                        >
                          {hunt.status}
                        </Badge>
                      </td>
                      <td className="py-3">
                        {hunt.status.toLowerCase() === "draft" && (
                          <Button
                            size="sm"
                            onClick={() => activateMutation.mutate(hunt.id)}
                            disabled={activateMutation.isPending}
                          >
                            <Play className="h-4 w-4 mr-1" />
                            Activate
                          </Button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              {filteredHunts.length === 0 && (
                <p className="py-8 text-center text-muted-foreground">No hunts found.</p>
              )}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

const createStepIcon = (number: number) =>
  L.divIcon({
    className: "custom-step-icon",
    html: `<div class="flex items-center justify-center w-7 h-7 rounded-full bg-primary text-primary-foreground font-bold text-xs border-2 border-background shadow-md">${number}</div>`,
    iconSize: [28, 28],
    iconAnchor: [14, 14],
  });

const createArrowIcon = (angle: number) =>
  L.divIcon({
    className: "custom-arrow-icon bg-transparent border-0",
    html: `<div class="flex items-center justify-center w-6 h-6 text-primary drop-shadow-md" style="transform: rotate(${angle}deg);">
             <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
           </div>`,
    iconSize: [24, 24],
    iconAnchor: [12, 12],
  });

function MapFitBounds({ steps }: { steps: HuntStepForm[] }) {
  const map = useMap();
  useEffect(() => {
    if (steps.length === 0) return;
    if (steps.length === 1) {
      map.setView([steps[0].latitude, steps[0].longitude], 14);
      return;
    }
    const bounds = L.latLngBounds(steps.map((s) => [s.latitude, s.longitude]));
    map.fitBounds(bounds, { padding: [40, 40] });
  }, [map, steps]);
  return null;
}

function MapFitBoundsInitial({ steps }: { steps: HuntStepForm[] }) {
  const map = useMap();
  useEffect(() => {
    if (steps.length === 0) return;
    if (steps.length === 1) {
      map.setView([steps[0].latitude, steps[0].longitude], 14);
      return;
    }
    const bounds = L.latLngBounds(steps.map((s) => [s.latitude, s.longitude]));
    map.fitBounds(bounds, { padding: [40, 40] });
  }, [map]);
  return null;
}

function CreateHuntDialog({ onSuccess }: { onSuccess: () => void }) {
  const { toast } = useToast();
  const [open, setOpen] = useState(false);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [difficulty, setDifficulty] = useState(1);
  const [reward, setReward] = useState(100);
  const [maxWinners, setMaxWinners] = useState(5);
  const [rewardItemId, setRewardItemId] = useState<string | null>(null);
  const [steps, setSteps] = useState<HuntStepForm[]>([]);
  const [editingStepIndex, setEditingStepIndex] = useState<number | null>(null);
  const [isMapExpanded, setIsMapExpanded] = useState(false);
  const [draggedStepIndex, setDraggedStepIndex] = useState<number | null>(null);
  const center: LatLngLiteral = { lat: 48.8566, lng: 2.3522 };

  const { data: items = [] } = useQuery({
    queryKey: ["items"],
    queryFn: itemsApi.list,
  });

  const createMutation = useMutation({
    mutationFn: (data: Parameters<typeof huntsApi.create>[0]) => huntsApi.create(data),
    onSuccess: () => {
      toast({ title: "Hunt created", variant: "success" });
      setOpen(false);
      resetForm();
      onSuccess();
    },
    onError: (err: Error) => {
      toast({ title: "Failed to create hunt", description: err.message, variant: "destructive" });
    },
  });

  const resetForm = useCallback(() => {
    setTitle("");
    setDescription("");
    setDifficulty(1);
    setReward(100);
    setMaxWinners(5);
    setRewardItemId(null);
    setSteps([]);
    setEditingStepIndex(null);
  }, []);

  const handleMapClick = useCallback((latlng: LatLngLiteral) => {
    if (editingStepIndex !== null) {
      setSteps((prev) => {
        const next = [...prev];
        next[editingStepIndex] = {
          ...next[editingStepIndex],
          latitude: latlng.lat,
          longitude: latlng.lng,
        };
        return next;
      });
      setEditingStepIndex(null);
    } else {
      setSteps((prev) => [
        ...prev,
        {
          clue: "",
          latitude: latlng.lat,
          longitude: latlng.lng,
          radiusMeters: 50,
          actionType: "Reach",
        },
      ]);
    }
  }, [editingStepIndex]);

  const updateStep = useCallback(
    (index: number, field: keyof HuntStepForm, value: string | number) => {
      setSteps((prev) => {
        const next = [...prev];
        next[index] = { ...next[index], [field]: value };
        return next;
      });
    },
    [],
  );

  const removeStep = useCallback((index: number) => {
    setSteps((prev) => prev.filter((_, i) => i !== index));
    setEditingStepIndex(null);
  }, []);

  const moveStep = useCallback((index: number, direction: "up" | "down") => {
    setSteps((prev) => {
      const next = [...prev];
      const target = direction === "up" ? index - 1 : index + 1;
      if (target < 0 || target >= next.length) return prev;
      [next[index], next[target]] = [next[target], next[index]];
      return next;
    });
  }, []);

  const handleSubmit = () => {
    if (!title.trim()) {
      toast({ title: "Title is required", variant: "destructive" });
      return;
    }
    if (steps.length === 0) {
      toast({ title: "Add at least one step", variant: "destructive" });
      return;
    }
    const invalidStep = steps.find((s) => !s.clue.trim());
    if (invalidStep) {
      toast({ title: "All steps need a clue", variant: "destructive" });
      return;
    }
    createMutation.mutate({
      title: title.trim(),
      description: description.trim(),
      difficulty,
      rewardTokens: reward,
      maxWinners,
      rewardItemId,
      steps: steps.map((s) => ({
        clue: s.clue.trim(),
        latitude: s.latitude,
        longitude: s.longitude,
        radiusMeters: s.radiusMeters,
        actionType: s.actionType,
      })),
    });
  };

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button>
          <Plus className="h-4 w-4 mr-2" />
          Create hunt
        </Button>
      </DialogTrigger>
      <DialogContent 
        className="max-w-4xl max-h-[90vh] overflow-y-auto"
        onInteractOutside={(e) => {
          if (isMapExpanded) {
            e.preventDefault();
          }
        }}
      >
        <DialogHeader>
          <DialogTitle>Create hunt</DialogTitle>
          <DialogDescription>
            Define hunt details and place waypoints on the map by clicking.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-6 sm:grid-cols-2">
          <div className="space-y-4">
            <div>
              <label className="mb-1 block text-sm font-medium text-foreground">Title</label>
              <Input
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="Hunt title"
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
            <div className="grid grid-cols-3 gap-3">
              <div>
                <label className="mb-1 block text-xs font-medium text-foreground truncate">Difficulty (1-5)</label>
                <Input
                  type="number"
                  min={1}
                  max={5}
                  value={difficulty}
                  onChange={(e) => setDifficulty(Number(e.target.value) || 1)}
                  className="bg-background"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-foreground truncate">Reward (LTK)</label>
                <Input
                  type="number"
                  min={0}
                  value={reward}
                  onChange={(e) => setReward(Number(e.target.value) || 0)}
                  className="bg-background"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs font-medium text-foreground truncate">Max Winners</label>
                <Input
                  type="number"
                  min={1}
                  value={maxWinners}
                  onChange={(e) => setMaxWinners(Number(e.target.value) || 1)}
                  className="bg-background"
                />
              </div>
            </div>

            <div>
              <label className="mb-1 block text-xs font-medium text-foreground">Bonus Item Reward (Optional)</label>
              <select
                value={rewardItemId || ""}
                onChange={(e) => setRewardItemId(e.target.value || null)}
                className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-primary"
              >
                <option value="">-- No item reward --</option>
                {items.map((it) => (
                  <option key={it.id} value={it.id}>
                    {it.name} ({it.rarity} {it.type})
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="space-y-2">
            <p className="text-sm font-medium text-foreground">Step editor — click map to add waypoints</p>
            <div className="relative h-64 rounded-lg overflow-hidden border border-border group cursor-pointer">
              <MapContainer
                center={[center.lat, center.lng]}
                zoom={13}
                doubleClickZoom={false}
                className="h-full w-full pointer-events-none"
                style={{ minHeight: 256 }}
              >
                <TileLayer
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />
                <MapFitBounds steps={steps} />
                {steps.length >= 2 && (
                  <Polyline 
                    positions={steps.map((s) => [s.latitude, s.longitude])}
                    pathOptions={{ color: "#10b981", weight: 4, opacity: 0.85 }}
                  />
                )}
                {steps.map((step, i) => {
                  const nextStep = steps[i + 1];
                  let arrowEl = null;
                  if (nextStep) {
                    const midLat = (step.latitude + nextStep.latitude) / 2;
                    const midLng = (step.longitude + nextStep.longitude) / 2;
                    const angle = Math.atan2(step.latitude - nextStep.latitude, nextStep.longitude - step.longitude) * (180 / Math.PI);
                    arrowEl = <Marker position={[midLat, midLng]} icon={createArrowIcon(angle)} />;
                  }
                  return (
                    <Fragment key={i}>
                      {arrowEl}
                      <Circle 
                        center={[step.latitude, step.longitude]} 
                        radius={step.radiusMeters}
                        pathOptions={{ color: "hsl(var(--primary))", fillColor: "hsl(var(--primary))", fillOpacity: 0.2, weight: 2 }} 
                      />
                      <Marker 
                        position={[step.latitude, step.longitude]}
                        icon={createStepIcon(i + 1)}
                      >
                        <Popup>
                          Étape #{i + 1} : {step.clue || "(sans indice)"}
                          <br />
                          Rayon : {step.radiusMeters}m
                        </Popup>
                      </Marker>
                    </Fragment>
                  );
                })}
              </MapContainer>
              <div 
                className="absolute inset-0 bg-background/20 hover:bg-background/40 transition-colors flex items-center justify-center z-[500]"
                onClick={() => setIsMapExpanded(true)}
              >
                <Badge variant="secondary" className="px-3 py-1.5 text-sm gap-1.5 shadow-md border border-border/50 pointer-events-none">
                  <Maximize2 className="h-4 w-4" />
                  Cliquez pour agrandir et placer les points
                </Badge>
              </div>
            </div>

            {isMapExpanded && createPortal(
              <div 
                className="fixed inset-0 z-[100000] bg-background/80 backdrop-blur-md flex items-center justify-center p-4 sm:p-6 animate-in fade-in-50 duration-200"
                style={{ pointerEvents: "auto" }}
                onClick={(e) => {
                  if (e.target === e.currentTarget) setIsMapExpanded(false);
                }}
              >
                <div 
                  className="relative w-full max-w-5xl h-[85vh] rounded-2xl overflow-hidden border-2 border-border shadow-2xl bg-card flex flex-col animate-in zoom-in-95 duration-200"
                  style={{ pointerEvents: "auto" }}
                  onClick={(e) => e.stopPropagation()}
                >
                  <div className="absolute top-4 left-4 z-[1000] bg-background/90 backdrop-blur-sm border border-border px-4 py-2 rounded-xl shadow-md pointer-events-none">
                    <p className="text-sm font-semibold text-foreground">
                      📍 Mode Placement — Cliquez sur la carte pour ajouter/déplacer une étape
                    </p>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      {editingStepIndex !== null ? `Modification de l'étape #${editingStepIndex + 1}` : `Étape en cours d'ajout : #${steps.length + 1}`}
                    </p>
                  </div>

                  <MapContainer
                    center={[center.lat, center.lng]}
                    zoom={14}
                    doubleClickZoom={false}
                    className="h-full w-full flex-1"
                  >
                    <TileLayer
                      attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                      url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                    />
                    <MapFitBoundsInitial steps={steps} />
                    <MapClickHandler onMapClick={handleMapClick} />
                    {steps.length >= 2 && (
                      <Polyline 
                        positions={steps.map((s) => [s.latitude, s.longitude])}
                        pathOptions={{ color: "#10b981", weight: 4, opacity: 0.85 }}
                      />
                    )}
                    {steps.map((step, i) => {
                      const nextStep = steps[i + 1];
                      let arrowEl = null;
                      if (nextStep) {
                        const midLat = (step.latitude + nextStep.latitude) / 2;
                        const midLng = (step.longitude + nextStep.longitude) / 2;
                        const angle = Math.atan2(step.latitude - nextStep.latitude, nextStep.longitude - step.longitude) * (180 / Math.PI);
                        arrowEl = <Marker position={[midLat, midLng]} icon={createArrowIcon(angle)} />;
                      }
                      return (
                        <Fragment key={i}>
                          {arrowEl}
                          <Circle 
                            center={[step.latitude, step.longitude]} 
                            radius={step.radiusMeters}
                            pathOptions={{ color: "hsl(var(--primary))", fillColor: "hsl(var(--primary))", fillOpacity: 0.2, weight: 2 }} 
                          />
                          <Marker 
                            position={[step.latitude, step.longitude]}
                            icon={createStepIcon(i + 1)}
                            draggable={true}
                            eventHandlers={{
                              dragend: (e) => {
                                const marker = e.target;
                                const pos = marker.getLatLng();
                                updateStep(i, "latitude", pos.lat);
                                updateStep(i, "longitude", pos.lng);
                              }
                            }}
                          >
                            <Popup>
                              Étape #{i + 1} : {step.clue || "(sans indice)"}
                              <br />
                              Rayon : {step.radiusMeters}m
                            </Popup>
                          </Marker>
                        </Fragment>
                      );
                    })}
                  </MapContainer>

                  <Button
                    size="icon"
                    className="absolute bottom-6 right-6 z-[1000] h-14 w-14 rounded-full bg-emerald-600 hover:bg-emerald-700 text-white shadow-xl border-2 border-white/20 transition-all hover:scale-110"
                    onClick={() => setIsMapExpanded(false)}
                    title="Valider et fermer la carte"
                  >
                    <Check className="h-7 w-7 stroke-[2.5]" />
                  </Button>
                </div>
              </div>,
              document.body
            )}
          </div>
        </div>

        <div className="space-y-3">
          <p className="text-sm font-medium text-foreground">Steps ({steps.length})</p>
          <div className="max-h-48 space-y-2 overflow-y-auto">
            {steps.map((step, i) => (
              <div
                key={i}
                draggable={true}
                onDragStart={() => setDraggedStepIndex(i)}
                onDragOver={(e) => e.preventDefault()}
                onDrop={() => {
                  if (draggedStepIndex !== null && draggedStepIndex !== i) {
                    setSteps((prev) => {
                      const next = [...prev];
                      const item = next.splice(draggedStepIndex, 1)[0];
                      next.splice(i, 0, item);
                      return next;
                    });
                  }
                  setDraggedStepIndex(null);
                }}
                className={cn(
                  "flex flex-wrap items-center gap-2 rounded-lg border p-3 transition-all cursor-move",
                  editingStepIndex === i ? "border-primary bg-primary/5" : "border-border hover:border-primary/50",
                  draggedStepIndex === i ? "opacity-40 border-dashed" : ""
                )}
              >
                <GripVertical className="h-4 w-4 text-muted-foreground shrink-0" />
                <span className="text-muted-foreground font-medium shrink-0">#{i + 1}</span>
                <Input
                  value={step.clue}
                  onChange={(e) => updateStep(i, "clue", e.target.value)}
                  placeholder="Clue"
                  className="flex-1 min-w-[120px] bg-background"
                />
                <Input
                  type="number"
                  value={step.radiusMeters}
                  onChange={(e) => updateStep(i, "radiusMeters", Number(e.target.value) || 50)}
                  placeholder="Radius (m)"
                  className="w-20 bg-background"
                  title="Rayon en mètres"
                />
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => {
                    const nextEditing = editingStepIndex === i ? null : i;
                    setEditingStepIndex(nextEditing);
                    if (nextEditing !== null) setIsMapExpanded(true);
                  }}
                  title="Modifier les coordonnées sur la carte"
                >
                  <MapPin className="h-4 w-4" />
                </Button>
                <Button size="sm" variant="ghost" onClick={() => moveStep(i, "up")} disabled={i === 0}>
                  <ChevronUp className="h-4 w-4" />
                </Button>
                <Button
                  size="sm"
                  variant="ghost"
                  onClick={() => moveStep(i, "down")}
                  disabled={i === steps.length - 1}
                >
                  <ChevronDown className="h-4 w-4" />
                </Button>
                <Button size="sm" variant="destructive" onClick={() => removeStep(i)}>
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            ))}
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={createMutation.isPending || !title.trim() || steps.length === 0}
          >
            {createMutation.isPending ? "Creating…" : "Create hunt"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
