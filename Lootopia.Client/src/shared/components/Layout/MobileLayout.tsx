import { NavLink, Outlet } from "react-router";
import { Map, Backpack, Store, Trophy, User, Wallet, Bell } from "lucide-react";
import { useAuth } from "@/shared/providers/AuthProvider";
import { cn } from "@/shared/lib/utils";

const navItems = [
  { to: "/play/hunts", icon: Map, label: "Chasses" },
  { to: "/play/inventory", icon: Backpack, label: "Inventaire" },
  { to: "/play/marketplace", icon: Store, label: "Marché" },
  { to: "/play/leaderboards", icon: Trophy, label: "Classement" },
  { to: "/play/profile", icon: User, label: "Profil" },
];

export function MobileLayout() {
  const { user } = useAuth();

  return (
    <div className="flex flex-col h-dvh bg-background">
      <header className="flex items-center justify-between px-4 py-3 border-b border-border bg-card/80 backdrop-blur-sm">
        <h1 className="text-lg font-bold text-primary">Lootopia</h1>
        <div className="flex items-center gap-3">
          <NavLink to="/play/wallet" className="flex items-center gap-1 text-sm text-gold">
            <Wallet className="h-4 w-4" />
          </NavLink>
          <NavLink to="/play/notifications" className="relative">
            <Bell className="h-5 w-5 text-muted-foreground" />
          </NavLink>
          {user && (
            <span className="text-xs text-muted-foreground truncate max-w-[80px]">
              {user.displayName}
            </span>
          )}
        </div>
      </header>

      <main className="flex-1 overflow-y-auto">
        <Outlet />
      </main>

      <nav className="flex items-center justify-around border-t border-border bg-card/80 backdrop-blur-sm pb-safe">
        {navItems.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            className={({ isActive }) =>
              cn(
                "flex flex-col items-center gap-1 py-2 px-3 text-xs transition-colors",
                isActive ? "text-primary" : "text-muted-foreground",
              )
            }
          >
            <Icon className="h-5 w-5" />
            <span>{label}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
