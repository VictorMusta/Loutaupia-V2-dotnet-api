import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Bell, Check } from "lucide-react";
import { Card, CardContent } from "@/shared/components/ui/card";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { notificationsApi } from "@/shared/api/notifications";
import { formatDateTime } from "@/shared/lib/utils";

export function NotificationsPage() {
  const queryClient = useQueryClient();

  const { data: notifications, isLoading } = useQuery({
    queryKey: ["notifications"],
    queryFn: () => notificationsApi.list(),
  });

  const markReadMutation = useMutation({
    mutationFn: (id: string) => notificationsApi.markAsRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
  });

  const unreadCount = notifications?.filter((n) => !n.isRead).length ?? 0;

  return (
    <div className="h-full flex flex-col gap-4 p-4 overflow-y-auto">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-bold text-foreground">Notifications</h1>
        {unreadCount > 0 && (
          <span className="rounded-full bg-primary px-2.5 py-0.5 text-xs font-semibold text-primary-foreground">
            {unreadCount}
          </span>
        )}
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {[...Array(6)].map((_, i) => (
            <Skeleton key={i} className="h-20 rounded-xl" />
          ))}
        </div>
      ) : (
        <div className="space-y-3">
          {notifications?.length === 0 ? (
            <div className="py-12 text-center text-muted-foreground">
              <Bell className="h-12 w-12 mx-auto mb-3 opacity-50" />
              <p>Aucune notification</p>
            </div>
          ) : (
            notifications?.map((notif) => (
              <Card
                key={notif.id}
                className={`border-border cursor-pointer transition-all hover:border-primary/30 ${
                  !notif.isRead ? "bg-primary/5 border-primary/20" : "bg-card"
                }`}
                onClick={() => {
                  if (!notif.isRead) {
                    markReadMutation.mutate(notif.id);
                  }
                }}
              >
                <CardContent className="p-4 flex gap-3">
                  <div
                    className={`shrink-0 w-10 h-10 rounded-full flex items-center justify-center ${
                      notif.isRead ? "bg-muted" : "bg-primary/20"
                    }`}
                  >
                    {notif.isRead ? (
                      <Check className="h-5 w-5 text-muted-foreground" />
                    ) : (
                      <Bell className="h-5 w-5 text-primary" />
                    )}
                  </div>
                  <div className="flex-1 min-w-0">
                    <p
                      className={`font-medium ${
                        notif.isRead ? "text-muted-foreground" : "text-foreground"
                      }`}
                    >
                      {notif.title}
                    </p>
                    <p className="text-sm text-muted-foreground mt-0.5 line-clamp-2">
                      {notif.message}
                    </p>
                    <p className="text-xs text-muted-foreground mt-1">
                      {formatDateTime(notif.createdAt)}
                    </p>
                  </div>
                </CardContent>
              </Card>
            ))
          )}
        </div>
      )}
    </div>
  );
}
