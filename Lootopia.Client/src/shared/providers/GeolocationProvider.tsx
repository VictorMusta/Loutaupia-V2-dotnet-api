import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
  type ReactNode,
} from "react";

export interface GeoPosition {
  lat: number;
  lng: number;
  accuracy: number;
  timestamp: number;
}

interface GeoContextValue {
  position: GeoPosition | null;
  error: string | null;
  isTracking: boolean;
  debugMode: boolean;
  heading: number;
  startTracking: () => void;
  stopTracking: () => void;
  toggleDebugMode: () => void;
  moveDebug: (dlat: number, dlng: number) => void;
}

const GeoContext = createContext<GeoContextValue | null>(null);

const DEBUG_STEP = 0.0001; // ~11 meters
const DEFAULT_POS: GeoPosition = {
  lat: 48.8566,
  lng: 2.3522,
  accuracy: 1,
  timestamp: Date.now(),
};

export function GeolocationProvider({ children }: { children: ReactNode }) {
  const [realPosition, setRealPosition] = useState<GeoPosition | null>(null);
  const [debugPosition, setDebugPosition] = useState<GeoPosition | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isTracking, setIsTracking] = useState(false);
  const [debugMode, setDebugMode] = useState(false);
  const [heading, setHeading] = useState(0);
  const watchIdRef = useRef<number | null>(null);

  const position = debugMode ? debugPosition : realPosition;

  const moveDebug = useCallback((dlat: number, dlng: number) => {
    if (dlat !== 0 || dlng !== 0) {
      setHeading(Math.atan2(dlng, dlat) * (180 / Math.PI));
    }
    setDebugPosition((prev) => {
      const base = prev ?? realPosition ?? DEFAULT_POS;
      return {
        lat: base.lat + dlat,
        lng: base.lng + dlng,
        accuracy: 1,
        timestamp: Date.now(),
      };
    });
  }, [realPosition]);

  const toggleDebugMode = useCallback(() => {
    setDebugMode((prev) => {
      if (!prev) {
        setDebugPosition(realPosition ?? DEFAULT_POS);
      }
      return !prev;
    });
  }, [realPosition]);

  const stopTracking = useCallback(() => {
    if (watchIdRef.current !== null) {
      navigator.geolocation.clearWatch(watchIdRef.current);
      watchIdRef.current = null;
      setIsTracking(false);
    }
  }, []);

  const startTracking = useCallback(() => {
    if (!navigator.geolocation) {
      setError("La géolocalisation n'est pas supportée");
      return;
    }
    if (watchIdRef.current !== null) return;

    const id = navigator.geolocation.watchPosition(
      (pos) => {
        setRealPosition({
          lat: pos.coords.latitude,
          lng: pos.coords.longitude,
          accuracy: pos.coords.accuracy,
          timestamp: pos.timestamp,
        });
        setError(null);
      },
      (err) => {
        setError(err.message);
      },
      { enableHighAccuracy: true, maximumAge: 1000, timeout: 10000 },
    );
    watchIdRef.current = id;
    setIsTracking(true);
  }, []);

  // Ctrl+Shift+D to toggle debug
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.ctrlKey && e.shiftKey && e.key === "D") {
        e.preventDefault();
        toggleDebugMode();
      }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, [toggleDebugMode]);

  // ZQSD / Arrow keys movement
  useEffect(() => {
    if (!debugMode) return;

    const pressed = new Set<string>();
    let raf: number | null = null;

    const tick = () => {
      let dlat = 0;
      let dlng = 0;
      if (pressed.has("z") || pressed.has("arrowup")) dlat += DEBUG_STEP;
      if (pressed.has("s") || pressed.has("arrowdown")) dlat -= DEBUG_STEP;
      if (pressed.has("q") || pressed.has("arrowleft")) dlng -= DEBUG_STEP;
      if (pressed.has("d") || pressed.has("arrowright")) dlng += DEBUG_STEP;
      if (dlat !== 0 || dlng !== 0) {
        moveDebug(dlat, dlng);
      }
      raf = requestAnimationFrame(tick);
    };

    const onDown = (e: KeyboardEvent) => {
      const key = e.key.toLowerCase();
      if (["z", "q", "s", "d", "arrowup", "arrowdown", "arrowleft", "arrowright"].includes(key)) {
        e.preventDefault();
        if (!pressed.has(key)) {
          pressed.add(key);
          if (raf === null) raf = requestAnimationFrame(tick);
        }
      }
    };

    const onUp = (e: KeyboardEvent) => {
      pressed.delete(e.key.toLowerCase());
      if (pressed.size === 0 && raf !== null) {
        cancelAnimationFrame(raf);
        raf = null;
      }
    };

    window.addEventListener("keydown", onDown);
    window.addEventListener("keyup", onUp);
    return () => {
      window.removeEventListener("keydown", onDown);
      window.removeEventListener("keyup", onUp);
      if (raf !== null) cancelAnimationFrame(raf);
    };
  }, [debugMode, moveDebug]);

  useEffect(() => {
    return () => {
      if (watchIdRef.current !== null) {
        navigator.geolocation.clearWatch(watchIdRef.current);
      }
    };
  }, []);

  const value: GeoContextValue = {
    position,
    error,
    isTracking,
    debugMode,
    heading,
    startTracking,
    stopTracking,
    toggleDebugMode,
    moveDebug,
  };

  return <GeoContext.Provider value={value}>{children}</GeoContext.Provider>;
}

export function useGeolocation() {
  const ctx = useContext(GeoContext);
  if (!ctx) throw new Error("useGeolocation must be used within GeolocationProvider");
  return ctx;
}
