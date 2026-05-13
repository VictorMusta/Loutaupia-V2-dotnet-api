import { BrowserRouter, Routes, Route, Navigate } from "react-router";
import { AuthProvider, useAuth } from "@/shared/providers/AuthProvider";
import { QueryProvider } from "@/shared/providers/QueryProvider";
import { GeolocationProvider } from "@/shared/providers/GeolocationProvider";
import { ToastProvider } from "@/shared/components/ui/toast";
import { ProtectedRoute } from "@/shared/components/ProtectedRoute";
import { MobileLayout } from "@/shared/components/Layout/MobileLayout";
import { DesktopLayout } from "@/shared/components/Layout/DesktopLayout";
import { LoginPage } from "@/app/auth/LoginPage";
import { RegisterPage } from "@/app/auth/RegisterPage";
import { HuntMapPage } from "@/app/play/hunt-map/HuntMapPage";
import { HuntPlayPage } from "@/app/play/hunt-play/HuntPlayPage";
import { WalletPage } from "@/app/play/wallet/WalletPage";
import { InventoryPage } from "@/app/play/inventory/InventoryPage";
import { MarketplacePage } from "@/app/play/marketplace/MarketplacePage";
import { TradingPage } from "@/app/play/marketplace/TradingPage";
import { AuctionsPage } from "@/app/play/marketplace/AuctionsPage";
import { LeaderboardsPage } from "@/app/play/leaderboards/LeaderboardsPage";
import { ProfilePage } from "@/app/play/profile/ProfilePage";
import { NotificationsPage } from "@/app/play/notifications/NotificationsPage";
import { AdminDashboardPage } from "@/app/admin/dashboard/AdminDashboardPage";
import { AdminHuntsPage } from "@/app/admin/hunts/AdminHuntsPage";
import { AdminUsersPage } from "@/app/admin/users/AdminUsersPage";
import { AdminFraudPage } from "@/app/admin/fraud/AdminFraudPage";
import { AdminCampaignsPage } from "@/app/admin/campaigns/AdminCampaignsPage";
import { AdminItemsPage } from "@/app/admin/items/AdminItemsPage";
import { AdminPartnersPage } from "@/app/admin/partners/AdminPartnersPage";
import { PartnerDashboardPage } from "@/app/partner/dashboard/PartnerDashboardPage";
import { PartnerCampaignsPage } from "@/app/partner/campaigns/PartnerCampaignsPage";

function RoleRedirect() {
  const { user } = useAuth();
  switch (user?.role) {
    case "Admin": return <Navigate to="/admin/dashboard" replace />;
    case "Partner": return <Navigate to="/partner/dashboard" replace />;
    default: return <Navigate to="/play/hunts" replace />;
  }
}

export default function App() {
  return (
    <QueryProvider>
      <AuthProvider>
        <GeolocationProvider>
          <ToastProvider>
            <BrowserRouter>
              <Routes>
                {/* Public routes */}
                <Route path="/login" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />

                {/* Player mobile routes */}
                <Route
                  path="/play"
                  element={
                    <ProtectedRoute>
                      <MobileLayout />
                    </ProtectedRoute>
                  }
                >
                  <Route index element={<Navigate to="hunts" replace />} />
                  <Route path="hunts" element={<HuntMapPage />} />
                  <Route path="hunts/:id/play" element={<HuntPlayPage />} />
                  <Route path="wallet" element={<WalletPage />} />
                  <Route path="inventory" element={<InventoryPage />} />
                  <Route path="marketplace" element={<MarketplacePage />} />
                  <Route path="marketplace/trading" element={<TradingPage />} />
                  <Route path="marketplace/auctions" element={<AuctionsPage />} />
                  <Route path="leaderboards" element={<LeaderboardsPage />} />
                  <Route path="profile" element={<ProfilePage />} />
                  <Route path="notifications" element={<NotificationsPage />} />
                </Route>

                {/* Admin desktop routes */}
                <Route
                  path="/admin"
                  element={
                    <ProtectedRoute roles={["Admin"]}>
                      <DesktopLayout variant="admin" />
                    </ProtectedRoute>
                  }
                >
                  <Route index element={<Navigate to="dashboard" replace />} />
                  <Route path="dashboard" element={<AdminDashboardPage />} />
                  <Route path="hunts" element={<AdminHuntsPage />} />
                  <Route path="users" element={<AdminUsersPage />} />
                  <Route path="fraud" element={<AdminFraudPage />} />
                  <Route path="campaigns" element={<AdminCampaignsPage />} />
                  <Route path="items" element={<AdminItemsPage />} />
                  <Route path="partners" element={<AdminPartnersPage />} />
                </Route>

                {/* Partner desktop routes */}
                <Route
                  path="/partner"
                  element={
                    <ProtectedRoute roles={["Partner"]}>
                      <DesktopLayout variant="partner" />
                    </ProtectedRoute>
                  }
                >
                  <Route index element={<Navigate to="dashboard" replace />} />
                  <Route path="dashboard" element={<PartnerDashboardPage />} />
                  <Route path="campaigns" element={<PartnerCampaignsPage />} />
                </Route>

                {/* Default redirect based on role */}
                <Route path="*" element={<RoleRedirect />} />
              </Routes>
            </BrowserRouter>
          </ToastProvider>
        </GeolocationProvider>
      </AuthProvider>
    </QueryProvider>
  );
}
