import { useRef, useCallback, useEffect } from "react";
import { useGeolocation } from "@/shared/providers/GeolocationProvider";

const SIZE = 120;
const KNOB = 40;
const MAX_R = (SIZE - KNOB) / 2;
const MOVE_STEP = 0.00012;

export function VirtualJoystick() {
  const { debugMode, moveDebug } = useGeolocation();
  const containerRef = useRef<HTMLDivElement>(null);
  const knobRef = useRef<HTMLDivElement>(null);
  const rafRef = useRef<number | null>(null);
  const dirRef = useRef({ dx: 0, dy: 0 });
  const activeRef = useRef(false);

  const updateKnob = useCallback((dx: number, dy: number) => {
    if (knobRef.current) {
      knobRef.current.style.transform = `translate(${dx}px, ${dy}px)`;
    }
  }, []);

  const tick = useCallback(() => {
    if (!activeRef.current) return;
    const { dx, dy } = dirRef.current;
    if (dx !== 0 || dy !== 0) {
      moveDebug(-dy * MOVE_STEP, dx * MOVE_STEP);
    }
    rafRef.current = requestAnimationFrame(tick);
  }, [moveDebug]);

  const handleTouch = useCallback(
    (e: TouchEvent) => {
      e.preventDefault();
      if (!containerRef.current) return;
      const rect = containerRef.current.getBoundingClientRect();
      const cx = rect.left + rect.width / 2;
      const cy = rect.top + rect.height / 2;
      const touch = e.touches[0];
      let dx = touch.clientX - cx;
      let dy = touch.clientY - cy;
      const dist = Math.sqrt(dx * dx + dy * dy);
      if (dist > MAX_R) {
        dx = (dx / dist) * MAX_R;
        dy = (dy / dist) * MAX_R;
      }
      const norm = dist > 0 ? Math.min(dist / MAX_R, 1) : 0;
      dirRef.current = {
        dx: dist > 0 ? (dx / dist) * norm : 0,
        dy: dist > 0 ? (dy / dist) * norm : 0,
      };
      updateKnob(dx, dy);
    },
    [updateKnob],
  );

  const handleEnd = useCallback(() => {
    activeRef.current = false;
    dirRef.current = { dx: 0, dy: 0 };
    updateKnob(0, 0);
    if (rafRef.current !== null) {
      cancelAnimationFrame(rafRef.current);
      rafRef.current = null;
    }
  }, [updateKnob]);

  const handleStart = useCallback(
    (e: TouchEvent) => {
      activeRef.current = true;
      handleTouch(e);
      if (rafRef.current === null) {
        rafRef.current = requestAnimationFrame(tick);
      }
    },
    [handleTouch, tick],
  );

  useEffect(() => {
    const el = containerRef.current;
    if (!el || !debugMode) return;
    el.addEventListener("touchstart", handleStart, { passive: false });
    el.addEventListener("touchmove", handleTouch, { passive: false });
    el.addEventListener("touchend", handleEnd);
    el.addEventListener("touchcancel", handleEnd);
    return () => {
      el.removeEventListener("touchstart", handleStart);
      el.removeEventListener("touchmove", handleTouch);
      el.removeEventListener("touchend", handleEnd);
      el.removeEventListener("touchcancel", handleEnd);
      if (rafRef.current !== null) cancelAnimationFrame(rafRef.current);
    };
  }, [debugMode, handleStart, handleTouch, handleEnd]);

  if (!debugMode) return null;

  return (
    <div
      ref={containerRef}
      className="fixed bottom-24 right-4 z-[9999] rounded-full"
      style={{
        width: SIZE,
        height: SIZE,
        background: "rgba(139, 92, 246, 0.15)",
        border: "2px solid rgba(139, 92, 246, 0.4)",
        touchAction: "none",
      }}
    >
      <div
        ref={knobRef}
        className="absolute rounded-full"
        style={{
          width: KNOB,
          height: KNOB,
          top: (SIZE - KNOB) / 2,
          left: (SIZE - KNOB) / 2,
          background: "rgba(139, 92, 246, 0.7)",
          border: "2px solid rgba(255,255,255,0.6)",
          transition: "transform 0.05s ease-out",
        }}
      />
    </div>
  );
}
