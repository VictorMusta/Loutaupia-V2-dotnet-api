import { useEffect, useMemo } from "react";
import { useNavigate } from "react-router";
import { MapContainer, TileLayer, Marker, Popup, useMap, useMapEvents } from "react-leaflet";
import { useQuery } from "@tanstack/react-query";
import L from "leaflet";
import { Star, MapPin, Bug } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { huntsApi } from "@/shared/api/hunts";
import { useGeolocation } from "@/shared/providers/GeolocationProvider";
import { useToast } from "@/shared/components/ui/toast";
import { formatCurrency } from "@/shared/lib/utils";
import { PlayerSprite } from "@/shared/components/PlayerSprite";
import { VirtualJoystick } from "@/shared/components/VirtualJoystick";

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

  // Arrondir les coordonnées pour la clé de cache afin d'éviter de re-fetcher à chaque pixel de déplacement
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

  if (isLoading && !hunts) {
    return (
      <div className="h-full flex flex-col items-center justify-center gap-4 p-6 overflow-y-auto">
        <Skeleton className="h-12 w-48 rounded-lg" />
        <Skeleton className="h-[60vh] w-full rounded-xl" />
      </div>
    );
  }

  return (
    <div className="relative h-full min-h-[400px]">
      <MapContainer
        center={center}
        zoom={15}
        className="h-full w-full rounded-none"
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
                  className="w-full bg-primary hover:bg-primary/90"
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

      {/* Debug banner */}
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
  );
}
