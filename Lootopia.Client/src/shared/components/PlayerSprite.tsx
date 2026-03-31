import L from "leaflet";
import { Marker } from "react-leaflet";

interface PlayerSpriteProps {
  lat: number;
  lng: number;
  heading?: number;
}

function buildSvg(heading: number): string {
  return `
<svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48">
  <style>
    @keyframes pulse { 0%,100% { transform: scale(1); } 50% { transform: scale(1.08); } }
    .body { animation: pulse 1.6s ease-in-out infinite; transform-origin: 24px 24px; }
  </style>
  <g class="body" transform="rotate(${heading}, 24, 24)">
    <!-- shadow -->
    <ellipse cx="24" cy="42" rx="10" ry="3" fill="rgba(0,0,0,0.15)" />
    <!-- feet -->
    <rect x="17" y="36" width="5" height="4" rx="1" fill="#5B3E31" />
    <rect x="26" y="36" width="5" height="4" rx="1" fill="#5B3E31" />
    <!-- body -->
    <rect x="14" y="22" width="20" height="16" rx="3" fill="#7C3AED" />
    <!-- belt -->
    <rect x="14" y="32" width="20" height="3" fill="#F59E0B" />
    <rect x="22" y="31" width="4" height="5" rx="1" fill="#D97706" />
    <!-- arms -->
    <rect x="8" y="24" width="6" height="10" rx="3" fill="#A78BFA" />
    <rect x="34" y="24" width="6" height="10" rx="3" fill="#A78BFA" />
    <!-- head -->
    <circle cx="24" cy="16" r="10" fill="#FBBF24" />
    <!-- eyes -->
    <circle cx="20" cy="14" r="2" fill="#1E1B4B" />
    <circle cx="28" cy="14" r="2" fill="#1E1B4B" />
    <!-- eye shine -->
    <circle cx="20.7" cy="13.3" r="0.7" fill="white" />
    <circle cx="28.7" cy="13.3" r="0.7" fill="white" />
    <!-- mouth -->
    <path d="M20 19 Q24 23 28 19" fill="none" stroke="#1E1B4B" stroke-width="1.2" stroke-linecap="round" />
    <!-- hat -->
    <path d="M14 12 Q24 2 34 12" fill="#7C3AED" />
    <rect x="12" y="11" width="24" height="3" rx="1" fill="#6D28D9" />
    <!-- direction arrow -->
    <polygon points="24,0 20,6 28,6" fill="#EF4444" />
  </g>
</svg>`;
}

function createIcon(heading: number) {
  return L.divIcon({
    html: buildSvg(heading),
    className: "",
    iconSize: [48, 48],
    iconAnchor: [24, 42],
  });
}

export function PlayerSprite({ lat, lng, heading = 0 }: PlayerSpriteProps) {
  return (
    <Marker
      position={[lat, lng]}
      icon={createIcon(heading)}
      interactive={false}
    />
  );
}
