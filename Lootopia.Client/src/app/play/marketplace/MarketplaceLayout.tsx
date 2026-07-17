import { NavLink, Outlet } from "react-router";
import { Store, Gavel, ArrowLeftRight } from "lucide-react";
import { cn } from "@/shared/lib/utils";

const sections = [
  { to: "/play/marketplace", label: "Boutique", icon: Store, end: true },
  { to: "/play/marketplace/auctions", label: "Enchères", icon: Gavel },
  { to: "/play/marketplace/trading", label: "Échanges", icon: ArrowLeftRight },
] as const;

export function MarketplaceLayout() {
  return (
    <div className="h-full flex flex-col">
      <div className="px-4 pt-4 pb-2 space-y-3 border-b border-border bg-card/50">
        <h1 className="text-xl font-bold text-foreground">Marché</h1>
        <nav className="grid grid-cols-3 gap-2">
          {sections.map(({ to, label, icon: Icon, ...rest }) => (
            <NavLink
              key={to}
              to={to}
              {...rest}
              className={({ isActive }) =>
                cn(
                  "flex flex-col items-center gap-1 rounded-lg px-2 py-2.5 text-xs font-medium transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground shadow-sm"
                    : "bg-muted text-muted-foreground hover:text-foreground",
                )
              }
            >
              <Icon className="h-4 w-4 shrink-0" />
              <span>{label}</span>
            </NavLink>
          ))}
        </nav>
      </div>
      <div className="flex-1 min-h-0 overflow-hidden">
        <Outlet />
      </div>
    </div>
  );
}
