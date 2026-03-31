import { useEffect, useMemo, useState } from "react";
import { useParams, useNavigate } from "react-router";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { MapContainer, TileLayer, Circle, useMap } from "react-leaflet";
import { MapPin, CheckCircle2, Trophy, Navigation, AlertCircle, Bug } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { huntsApi } from "@/shared/api/hunts";
import { useGeolocation } from "@/shared/providers/GeolocationProvider";
import { useToast } from "@/shared/components/ui/toast";
import { ApiError } from "@/shared/api/client";
import { formatCurrency } from "@/shared/lib/utils";
import { PlayerSprite } from "@/shared/components/PlayerSprite";
import { VirtualJoystick } from "@/shared/components/VirtualJoystick";

function haversineDistance(
  lat1: number, lng1: number,
  lat2: number, lng2: number,
): number {
  const R = 6371000;
  const toRad = (d: number) => (d * Math.PI) / 180;
  const dLat = toRad(lat2 - lat1);
  const dLng = toRad(lng2 - lng1);
  const a =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(lat1)) * Math.cos(toRad(lat2)) * Math.sin(dLng / 2) ** 2;
  return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}

function formatDistance(meters: number): string {
  if (meters < 1000) return `${Math.round(meters)} m`;
  return `${(meters / 1000).toFixed(1)} km`;
}

function distanceColor(meters: number, radius: number): string {
  if (meters <= radius) return "text-green-400";
  if (meters <= radius * 3) return "text-amber-400";
  return "text-red-400";
}

function MapAutoCenter({ lat, lng }: { lat: number; lng: number }) {
  const map = useMap();
  useEffect(() => {
    map.setView([lat, lng], map.getZoom(), { animate: true });
  }, [lat, lng, map]);
  return null;
}


export function HuntPlayPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const { position, startTracking, stopTracking, debugMode, heading, toggleDebugMode } = useGeolocation();

  const [stepValidated, setStepValidated] = useState(false);
  const [showCompletion, setShowCompletion] = useState(false);

  useEffect(() => {
    startTracking();
    return () => stopTracking();
  }, [startTracking, stopTracking]);

  const { data: myHunts, isLoading } = useQuery({
    queryKey: ["hunts", "my"],
    queryFn: () => huntsApi.myHunts(),
  });

  const playerHunt = myHunts?.find((h) => h.huntId === id);
  const currentStep = playerHunt?.steps.find(
    (s) => s.order === playerHunt.currentStep,
  );
  const isCompleted = playerHunt?.status === "Completed";

  const distance = useMemo(() => {
    if (!position || !currentStep) return null;
    return haversineDistance(
      position.lat, position.lng,
      currentStep.latitude, currentStep.longitude,
    );
  }, [position, currentStep]);

  const validateMutation = useMutation({
    mutationFn: ({
      stepOrder, lat, lng,
    }: { stepOrder: number; lat: number; lng: number }) =>
      huntsApi.validateStep(id!, stepOrder, lat, lng),
    onSuccess: (result) => {
      setStepValidated(true);
      queryClient.invalidateQueries({ queryKey: ["hunts", "my"] });
      if (result.huntCompleted) {
        setShowCompletion(true);
        toast({
          title: "Chasse terminée !",
          description: `Récompense: ${formatCurrency(result.reward ?? 0)}`,
          variant: "success",
        });
      } else {
        toast({ title: "Étape validée", description: result.message, variant: "success" });
      }
    },
    onError: (err) => {
      const body = err instanceof ApiError ? err.body : null;
      const desc =
        body && typeof body === "object" && "description" in (body as Record<string, unknown>)
          ? String((body as Record<string, string>).description)
          : err instanceof Error ? err.message : "Erreur de validation.";
      toast({ title: "Validation impossible", description: desc, variant: "destructive" });
    },
  });

  useEffect(() => {
    setStepValidated(false);
  }, [playerHunt?.currentStep]);

  if (!id) {
    navigate("/play/hunts", { replace: true });
    return null;
  }

  if (isLoading && !playerHunt) {
    return (
      <div className="h-full flex flex-col gap-4 p-4 bg-slate-950">
        <Skeleton className="h-12 w-full rounded-lg" />
        <Skeleton className="h-48 w-full rounded-xl" />
        <Skeleton className="h-24 w-full rounded-lg" />
      </div>
    );
  }

  if (!playerHunt) {
    return (
      <div className="h-full flex flex-col items-center justify-center gap-4 p-6 bg-slate-950">
        <AlertCircle className="h-12 w-12 text-muted-foreground" />
        <p className="text-muted-foreground text-center">
          Chasse introuvable. Elle n'a peut-être pas encore démarré.
        </p>
        <Button variant="outline" onClick={() => navigate("/play/hunts")}>
          Retour à la carte
        </Button>
      </div>
    );
  }

  if (showCompletion || isCompleted) {
    return (
      <div className="h-full flex flex-col items-center justify-center gap-6 p-6 bg-slate-950 relative overflow-hidden">
        <div className="absolute inset-0 pointer-events-none overflow-hidden">
          {[...Array(30)].map((_, i) => (
            <div
              key={i}
              className="absolute w-2 h-2 rounded-full bg-primary animate-[confetti_1.5s_ease-out_forwards]"
              style={{
                left: `${Math.random() * 100}%`,
                top: "50%",
                animationDelay: `${Math.random() * 0.5}s`,
              }}
            />
          ))}
        </div>
        <div className="relative z-10 flex flex-col items-center gap-4 animate-in zoom-in-50 duration-500">
          <div className="rounded-full bg-primary/20 p-6">
            <Trophy className="h-16 w-16 text-primary" />
          </div>
          <h2 className="text-2xl font-bold text-foreground">Chasse terminée !</h2>
          <p className="text-muted-foreground text-center">{playerHunt.huntTitle}</p>
          <Button className="bg-primary hover:bg-primary/90" onClick={() => navigate("/play/hunts")}>
            Retour à la carte
          </Button>
        </div>
      </div>
    );
  }

  const mapCenter = currentStep
    ? [currentStep.latitude, currentStep.longitude] as [number, number]
    : [48.8566, 2.3522] as [number, number];

  return (
    <div className="h-full flex flex-col bg-slate-950 pb-safe">
      {/* Header */}
      <div className="flex items-center justify-between p-4 pb-2">
        <h1 className="text-lg font-semibold text-foreground truncate">
          {playerHunt.huntTitle}
        </h1>
        <span className="text-sm text-muted-foreground shrink-0 ml-2">
          Étape {playerHunt.currentStep} / {playerHunt.steps.length}
        </span>
      </div>

      {/* Progress bar */}
      <div className="px-4 pb-3">
        <div className="flex gap-1">
          {playerHunt.steps.map((s) => (
            <div
              key={s.order}
              className={`h-1.5 flex-1 rounded-full transition-colors ${
                s.validated
                  ? "bg-green-500"
                  : s.order === playerHunt.currentStep
                    ? "bg-primary"
                    : "bg-muted"
              }`}
            />
          ))}
        </div>
      </div>

      {/* Map */}
      {currentStep && (
        <div className="mx-4 h-48 rounded-xl overflow-hidden border border-border">
          <MapContainer
            center={mapCenter}
            zoom={16}
            className="h-full w-full"
            style={{ minHeight: 192 }}
            zoomControl={false}
          >
            <TileLayer
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a>'
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
            />
            <MapAutoCenter lat={currentStep.latitude} lng={currentStep.longitude} />
            <Circle
              center={[currentStep.latitude, currentStep.longitude]}
              radius={currentStep.radiusMeters}
              pathOptions={{ color: "#8b5cf6", fillColor: "#8b5cf6", fillOpacity: 0.15, weight: 2 }}
            />
            {position && (
              <PlayerSprite lat={position.lat} lng={position.lng} heading={heading} />
            )}
          </MapContainer>
        </div>
      )}

      {/* Distance indicator */}
      {currentStep && (
        <div className="mx-4 mt-3">
          <Card className="border-border bg-card/80">
            <CardContent className="py-3 flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Navigation className="h-5 w-5 text-primary" />
                <span className="text-sm text-muted-foreground">Distance</span>
              </div>
              {distance !== null ? (
                <span className={`text-lg font-bold ${distanceColor(distance, currentStep.radiusMeters)}`}>
                  {formatDistance(distance)}
                </span>
              ) : (
                <span className="text-sm text-muted-foreground">Localisation...</span>
              )}
            </CardContent>
          </Card>
        </div>
      )}

      {/* Clue */}
      <div className="flex-1 mx-4 mt-3 min-h-0">
        <Card className="border-border bg-card/80 h-full flex flex-col">
          <CardContent className="py-4 flex-1 flex flex-col gap-3">
            <div className="flex items-center gap-2">
              <MapPin className="h-5 w-5 text-primary shrink-0" />
              <span className="font-semibold text-foreground">Indice</span>
            </div>
            {currentStep && (
              <p className="text-foreground/90 leading-relaxed text-sm">
                {currentStep.clue}
              </p>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Validate button */}
      <div className="p-4 pt-3">
        <Button
          className="w-full h-12 bg-primary hover:bg-primary/90 text-base"
          disabled={!position || validateMutation.isPending || stepValidated}
          onClick={() => {
            if (position && currentStep) {
              setStepValidated(false);
              validateMutation.mutate({
                stepOrder: currentStep.order,
                lat: position.lat,
                lng: position.lng,
              });
            }
          }}
        >
          {validateMutation.isPending ? (
            <span className="flex items-center gap-2">
              <span className="h-5 w-5 animate-spin rounded-full border-2 border-primary-foreground border-t-transparent" />
              Validation...
            </span>
          ) : stepValidated ? (
            <>
              <CheckCircle2 className="h-5 w-5 mr-2" />
              Étape validée !
            </>
          ) : (
            <>
              <MapPin className="h-5 w-5 mr-2" />
              Valider l'étape
            </>
          )}
        </Button>
      </div>

      {/* Debug banner */}
      {debugMode && (
        <div className="fixed top-0 left-0 right-0 z-[9999] bg-amber-500 text-black text-center text-sm font-bold py-1 flex items-center justify-center gap-2">
          <Bug className="h-4 w-4" />
          MODE DEBUG — ZQSD / flèches pour se déplacer
          <button
            onClick={toggleDebugMode}
            className="ml-3 px-2 py-0.5 text-xs bg-black/20 rounded hover:bg-black/30"
          >
            Désactiver
          </button>
        </div>
      )}

      <VirtualJoystick />
    </div>
  );
}
