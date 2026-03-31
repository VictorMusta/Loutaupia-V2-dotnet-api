import { NavLink, Outlet, useLocation } from "react-router";
import {
  LayoutDashboard,
  Map,
  Users,
  ShieldAlert,
  Megaphone,
  LogOut,
  Bell,
  type LucideIcon,
} from "lucide-react";
import { useAuth } from "@/shared/providers/AuthProvider";
import { cn } from "@/shared/lib/utils";

interface SidebarItem {
  to: string;
  icon: LucideIcon;
  label: string;
}

const adminNav: SidebarItem[] = [
  { to: "/admin/dashboard", icon: LayoutDashboard, label: "Dashboard" },
  { to: "/admin/hunts", icon: Map, label: "Chasses" },
  { to: "/admin/users", icon: Users, label: "Utilisateurs" },
  { to: "/admin/fraud", icon: ShieldAlert, label: "Fraude" },
  { to: "/admin/campaigns", icon: Megaphone, label: "Campagnes" },
];

const partnerNav: SidebarItem[] = [
  { to: "/partner/dashboard", icon: LayoutDashboard, label: "Dashboard" },
  { to: "/partner/campaigns", icon: Megaphone, label: "Mes Campagnes" },
];

export function DesktopLayout({ variant }: { variant: "admin" | "partner" }) {
  const { user, logout } = useAuth();
  const location = useLocation();
  const items = variant === "admin" ? adminNav : partnerNav;

  const breadcrumb = location.pathname
    .split("/")
    .filter(Boolean)
    .map((s) => s.charAt(0).toUpperCase() + s.slice(1));

  return (
    <div className="flex h-screen bg-background">
      <aside className="w-64 border-r border-border bg-card flex flex-col">
        <div className="p-6">
          <h1 className="text-xl font-bold text-primary">Lootopia</h1>
          <p className="text-xs text-muted-foreground mt-1 capitalize">{variant}</p>
        </div>

        <nav className="flex-1 px-3 space-y-1">
          {items.map(({ to, icon: Icon, label }) => (
            <NavLink
              key={to}
              to={to}
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors",
                  isActive
                    ? "bg-primary/10 text-primary"
                    : "text-muted-foreground hover:text-foreground hover:bg-secondary",
                )
              }
            >
              <Icon className="h-4 w-4" />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="p-4 border-t border-border">
          <div className="flex items-center gap-3">
            <div className="h-8 w-8 rounded-full bg-primary/20 flex items-center justify-center text-sm font-bold text-primary">
              {user?.displayName?.charAt(0) ?? "?"}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium truncate">{user?.displayName}</p>
              <p className="text-xs text-muted-foreground truncate">{user?.email}</p>
            </div>
            <button onClick={logout} className="text-muted-foreground hover:text-destructive transition-colors">
              <LogOut className="h-4 w-4" />
            </button>
          </div>
        </div>
      </aside>

      <div className="flex-1 flex flex-col overflow-hidden">
        <header className="flex items-center justify-between px-6 py-4 border-b border-border bg-card/50">
          <nav className="text-sm text-muted-foreground">
            {breadcrumb.map((part, i) => (
              <span key={i}>
                {i > 0 && <span className="mx-2">/</span>}
                <span className={i === breadcrumb.length - 1 ? "text-foreground font-medium" : ""}>
                  {part}
                </span>
              </span>
            ))}
          </nav>
          <NavLink to={`/${variant}/notifications`} className="relative">
            <Bell className="h-5 w-5 text-muted-foreground hover:text-foreground transition-colors" />
          </NavLink>
        </header>

        <main className="flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
