import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router";
import { MapContainer, TileLayer, Marker, Popup, useMap, useMapEvents } from "react-leaflet";
import { useQuery } from "@tanstack/react-query";
import L from "leaflet";
import { Star, MapPin, Bug, Clock, Compass, List, Map as MapIcon, ChevronRight } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { huntsApi } from "@/shared/api/hunts";
import { useGeolocation } from "@/shared/providers/GeolocationProvider";
import { useToast } from "@/shared/components/ui/toast";
import { formatCurrency, cn } from "@/shared/lib/utils";
import { PlayerSprite } from "@/shared/components/PlayerSprite";
import { VirtualJoystick } from "@/shared/components/VirtualJoystick";
import { Badge } from "@/shared/components/ui/badge";

const HUNT_COLORS = ["#6d28d9", "#22c55e", "#f59e0b", "#ef4444", "#8b5cf6"];

function getMarkerColor(difficulty: number): string {
  return HUNT_COLORS[Math.min(difficulty - 1, HUNT_COLORS.length - 1)] ?? HUNT_COLORS[0];
}

function createMarkerIcon(color: string) {
  return L.divIcon({
    className: "custom-marker",
    html: `<div style="
      background: ${color};
      width: 28px;
      height: 28px;
      border-radius: 50% 50% 50% 0;
      transform: rotate(-45deg);
      border: 2px solid white;
      box-shadow: 0 2px 6px rgba(0,0,0,0.4);
    "></div>`,
    iconSize: [28, 28],
    iconAnchor: [14, 28],
  });
}

function MapCenterController({
  lat,
  lng,
}: {
  lat: number;
  lng: number;
}) {
  const map = useMap();

  useEffect(() => {
    map.setView([lat, lng], map.getZoom(), { animate: true });
  }, [lat, lng, map]);

  return null;
}

function MapFocusController({ location }: { location: { lat: number; lng: number } | null }) {
  const map = useMap();
  useEffect(() => {
    if (location) {
      map.setView([location.lat, location.lng], 16, { animate: true });
    }
  }, [location, map]);
  return null;
}

function MapDebugClickHandler({
  onMapClick,
}: {
  onMapClick: (lat: number, lng: number) => void;
}) {
  useMapEvents({
    click(e) {
      onMapClick(e.latlng.lat, e.latlng.lng);
    },
  });
  return null;
}


export function HuntMapPage() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const { position, startTracking, isTracking, debugMode, heading, toggleDebugMode, setDebugPositionAbsolute } = useGeolocation();

  // Tab d'affichage réactif pour mobile : "map" ou "list"
  const [mobileView, setMobileView] = useState<"map" | "list">("map");
  // Permet de centrer la carte sur une chasse sélectionnée depuis la liste
  const [focusedLocation, setFocusedLocation] = useState<{ lat: number; lng: number } | null>(null);

  useEffect(() => {
    startTracking();
  }, [startTracking]);

  const center = useMemo(
    () =>
      position
        ? [position.lat, position.lng] as [number, number]
        : ([48.8566, 2.3522] as [number, number]),
    [position],
  );

  const queryLat = position ? Number(position.lat.toFixed(3)) : undefined;
  const queryLng = position ? Number(position.lng.toFixed(3)) : undefined;

  const { data: hunts, isLoading } = useQuery({
    queryKey: ["hunts", "list", queryLat, queryLng],
    queryFn: () =>
      huntsApi.list({
        lat: position?.lat ?? 48.8566,
        lng: position?.lng ?? 2.3522,
        radius: 50,
      }),
    enabled: !!position || !isTracking,
    placeholderData: (previousData) => previousData,
  });

  const handleStartHunt = async (huntId: string) => {
    try {
      await huntsApi.start(huntId);
      navigate(`/play/hunts/${huntId}/play`);
    } catch (err) {
      toast({
        title: "Erreur",
        description:
          err instanceof Error ? err.message : "Impossible de démarrer la chasse.",
        variant: "destructive",
      });
    }
  };

  const focusOnHunt = (lat: number, lng: number) => {
    setFocusedLocation({ lat, lng });
    setMobileView("map"); // Bascule immédiatement vers l'onglet carte sur téléphone
    toast({
      title: "Carte centrée",
      description: "La position de la quête a été localisée.",
      variant: "default",
    });
  };

  if (isLoading && !hunts) {
    return (
      <div className="h-full flex flex-col items-center justify-center gap-4 p-6 overflow-y-auto">
        <Skeleton className="h-12 w-48 rounded-lg" />
        <Skeleton className="h-[60vh] w-full rounded-xl" />
      </div>
    );
  }

  // Rendu de la liste des chasses partagé entre le volet PC et l'onglet Mobile
  const renderHuntsList = () => (
    <div className="p-4 space-y-4">
      <div className="border-b border-border pb-3">
        <h2 className="text-lg font-extrabold text-foreground flex items-center gap-2">
          <Compass className="h-5 w-5 text-primary" />
          Quêtes Géolocalisées
        </h2>
        <p className="text-xs text-muted-foreground mt-0.5">
          {hunts?.length || 0} aventure(s) détectée(s) dans votre secteur
        </p>
      </div>

      {hunts?.length === 0 ? (
        <div className="p-8 text-center rounded-lg border border-dashed border-border bg-muted/5">
          <MapPin className="h-8 w-8 mx-auto text-muted-foreground/40 mb-2" />
          <p className="text-xs font-semibold text-foreground">Aucune quête à proximité</p>
          <p className="text-[10px] text-muted-foreground mt-1">Déplacez-vous pour explorer d'autres zones de la carte.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {hunts?.map((hunt) => {
            const estimatedDurationMinutes = hunt.difficulty * 6 + 5;

            return (
              <div
                key={hunt.id}
                className="rounded-xl border border-border bg-card hover:border-primary/40 p-3.5 transition-all flex flex-col gap-3 shadow-sm hover:shadow"
              >
                {/* En-tête de la carte */}
                <div className="flex items-start justify-between gap-2">
                  <div className="min-w-0 flex-1">
                    <h3 className="text-sm font-bold text-foreground truncate">{hunt.title}</h3>
                    <div className="flex items-center gap-1 mt-0.5">
                      {Array.from({ length: hunt.difficulty }).map((_, i) => (
                        <Star key={i} className="h-3 w-3 fill-amber-400 text-amber-400" />
                      ))}
                      <span className="text-[10px] text-muted-foreground ml-1">
                        Niveau {hunt.difficulty}
                      </span>
                    </div>
                  </div>
                  <Badge 
                    variant="outline" 
                    className="shrink-0 text-xs font-extrabold px-2 py-0.5 border-primary/30 bg-primary/10 text-primary"
                  >
                    {formatCurrency(hunt.rewardTokens)}
                  </Badge>
                </div>

                {/* Corps central avec indicateurs d'étapes et de temps */}
                <div className="grid grid-cols-2 gap-2 bg-background/50 p-2.5 rounded-lg border border-border/40 text-xs">
                  <div className="flex items-center gap-1.5 text-muted-foreground">
                    <Compass className="h-3.5 w-3.5 text-primary shrink-0" />
                    <span className="truncate text-[11px]">
                      <strong className="text-foreground">{Math.min(5, hunt.difficulty + 2)} étapes</strong> à valider
                    </span>
                  </div>

                  <div className="flex items-center gap-1.5 text-muted-foreground">
                    <Clock className="h-3.5 w-3.5 text-blue-500 shrink-0" />
                    <span className="truncate text-[11px]">
                      Durée : <strong className="text-foreground">~{estimatedDurationMinutes} min</strong>
                    </span>
                  </div>
                </div>

                {/* Boutons d'action pour le joueur */}
                <div className="flex items-center gap-2 pt-1">
                  <Button
                    variant="secondary"
                    size="sm"
                    className="flex-1 text-xs h-8 bg-secondary hover:bg-secondary/80 text-foreground"
                    onClick={() => focusOnHunt(hunt.latitude, hunt.longitude)}
                  >
                    <MapPin className="h-3.5 w-3.5 mr-1 text-primary" />
                    Localiser
                  </Button>

                  <Button
                    size="sm"
                    className="flex-1 text-xs h-8 bg-primary hover:bg-primary/90 font-bold"
                    onClick={() => handleStartHunt(hunt.id)}
                  >
                    Démarrer
                    <ChevronRight className="h-3.5 w-3.5 ml-0.5" />
                  </Button>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );

  return (
    <div className="flex flex-col md:flex-row h-full w-full overflow-hidden relative">
      {/* Sélecteur d'onglet mobile flottant au-dessus */}
      <div className="md:hidden flex items-center justify-center p-2 bg-card border-b border-border shrink-0 z-10">
        <div className="inline-flex rounded-lg p-0.5 bg-secondary w-full max-w-xs border border-border">
          <button
            onClick={() => setMobileView("map")}
            className={cn(
              "flex-1 flex items-center justify-center gap-1.5 py-1.5 text-xs font-bold rounded-md transition-all",
              mobileView === "map" 
                ? "bg-background text-primary shadow-sm" 
                : "text-muted-foreground hover:text-foreground"
            )}
          >
            <MapIcon className="h-3.5 w-3.5" />
            Carte
          </button>
          <button
            onClick={() => setMobileView("list")}
            className={cn(
              "flex-1 flex items-center justify-center gap-1.5 py-1.5 text-xs font-bold rounded-md transition-all",
              mobileView === "list" 
                ? "bg-background text-primary shadow-sm" 
                : "text-muted-foreground hover:text-foreground"
            )}
          >
            <List className="h-3.5 w-3.5" />
            Liste des quêtes
          </button>
        </div>
      </div>

      {/* Volet Latéral Liste des Chasses (Visible en permanence sur PC, conditionnel sur téléphone) */}
      <div 
        className={cn(
          "w-full md:w-80 lg:w-96 border-r border-border bg-card flex-col overflow-y-auto shrink-0 transition-all",
          mobileView === "list" ? "flex flex-1 md:flex-initial" : "hidden md:flex"
        )}
      >
        {renderHuntsList()}
      </div>

      {/* Zone de la Carte Leaflet (Visible en permanence sur PC, conditionnelle sur téléphone) */}
      <div 
        className={cn(
          "flex-1 relative min-h-[300px] flex flex-col transition-all",
          mobileView === "map" ? "flex" : "hidden md:flex"
        )}
      >
        <MapContainer
          center={center}
          zoom={15}
          className="h-full w-full rounded-none flex-1"
          zoomControl={false}
        >
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          {debugMode && (
            <MapDebugClickHandler onMapClick={setDebugPositionAbsolute} />
          )}
          {position && (
            <>
              <MapCenterController lat={position.lat} lng={position.lng} />
              <PlayerSprite lat={position.lat} lng={position.lng} heading={heading} />
            </>
          )}
          <MapFocusController location={focusedLocation} />
          
          {hunts?.map((hunt) => (
            <Marker
              key={hunt.id}
              position={[hunt.latitude, hunt.longitude]}
              icon={createMarkerIcon(getMarkerColor(hunt.difficulty))}
            >
              <Popup>
                <div className="min-w-[200px] p-2">
                  <h3 className="font-semibold text-foreground mb-1">{hunt.title}</h3>
                  <div className="flex items-center gap-1 mb-2">
                    {Array.from({ length: hunt.difficulty }).map((_, i) => (
                      <Star
                        key={i}
                        className="h-4 w-4 fill-amber-400 text-amber-400"
                      />
                    ))}
                    <span className="text-xs text-muted-foreground ml-1">
                      Difficulté
                    </span>
                  </div>
                  <p className="text-sm text-muted-foreground mb-3">
                    Récompense: {formatCurrency(hunt.rewardTokens)}
                  </p>
                  <Button
                    size="sm"
                    className="w-full bg-primary hover:bg-primary/90 font-bold"
                    onClick={() => handleStartHunt(hunt.id)}
                  >
                    <MapPin className="h-4 w-4 mr-1" />
                    Démarrer
                  </Button>
                </div>
              </Popup>
            </Marker>
          ))}
        </MapContainer>

        {/* Bouton de debug flottant */}
        <div className="absolute top-4 left-4 right-4 z-[1000] flex justify-between items-center gap-2" style={debugMode ? { top: 36 } : undefined}>
          <div className="rounded-lg bg-card border border-border px-3 py-2 shadow-lg">
            <p className="text-sm font-medium text-foreground">
              {hunts?.length ?? 0} chasse(s) à proximité
            </p>
          </div>
          <Button
            variant={debugMode ? "default" : "outline"}
            size="sm"
            className={debugMode ? "bg-amber-500 hover:bg-amber-600 text-black font-bold shadow-lg" : "bg-card shadow-lg"}
            onClick={toggleDebugMode}
          >
            <Bug className="h-4 w-4 mr-1" />
            {debugMode ? "Débug (Actif)" : "Débug"}
          </Button>
        </div>

        {/* Bannière de débuggage */}
        {debugMode && (
          <div className="fixed top-0 left-0 right-0 z-[9999] bg-amber-500 text-black text-center text-xs sm:text-sm font-bold py-1 px-2 flex items-center justify-center gap-2 shadow-md">
            <Bug className="h-4 w-4 shrink-0" />
            <span className="truncate">MODE DEBUG — Cliquez sur la carte ou ZQSD pour déplacer le joueur</span>
            <button
              onClick={toggleDebugMode}
              className="ml-2 px-2 py-0.5 text-xs bg-black/20 rounded hover:bg-black/30 shrink-0"
            >
              Désactiver
            </button>
          </div>
        )}

        <VirtualJoystick />
      </div>
    </div>
  );
}
