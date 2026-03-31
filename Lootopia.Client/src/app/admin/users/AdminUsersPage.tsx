import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useState, useEffect } from "react";
import { adminApi } from "@/shared/api/admin";
import { Button } from "@/shared/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card";
import { Input } from "@/shared/components/ui/input";
import { Badge } from "@/shared/components/ui/badge";
import { Skeleton } from "@/shared/components/ui/skeleton";
import { useToast } from "@/shared/components/ui/toast";
import { Search, Snowflake, Sun } from "lucide-react";

const PAGE_SIZE = 20;

export function AdminUsersPage() {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");

  useEffect(() => {
    const t = setTimeout(() => setDebouncedSearch(search), 300);
    return () => clearTimeout(t);
  }, [search]);

  const { data, isLoading } = useQuery({
    queryKey: ["admin-users", page, debouncedSearch],
    queryFn: () => adminApi.users(page, PAGE_SIZE, debouncedSearch || undefined),
  });

  const freezeMutation = useMutation({
    mutationFn: (userId: string) => adminApi.freezeUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-users"] });
      toast({ title: "User frozen", variant: "success" });
    },
    onError: (err: Error) => {
      toast({ title: "Failed to freeze user", description: err.message, variant: "destructive" });
    },
  });

  const unfreezeMutation = useMutation({
    mutationFn: (userId: string) => adminApi.unfreezeUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-users"] });
      toast({ title: "User unfrozen", variant: "success" });
    },
    onError: (err: Error) => {
      toast({ title: "Failed to unfreeze user", description: err.message, variant: "destructive" });
    },
  });

  const totalPages = data ? Math.ceil(data.total / PAGE_SIZE) : 0;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-foreground">Users</h1>

      <Card className="border-border bg-card">
        <CardHeader className="flex flex-row items-center justify-between gap-4">
          <CardTitle className="text-foreground">User list</CardTitle>
          <div className="relative w-64">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={search}
              onChange={(e) => {
                setSearch(e.target.value);
                setPage(1);
              }}
              placeholder="Search by email or name..."
              className="pl-9 bg-background"
            />
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <Skeleton className="h-64 w-full" />
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border text-left text-muted-foreground">
                      <th className="pb-3 pr-4 font-medium">Email</th>
                      <th className="pb-3 pr-4 font-medium">Display name</th>
                      <th className="pb-3 pr-4 font-medium">Role</th>
                      <th className="pb-3 pr-4 font-medium">Status</th>
                      <th className="pb-3 font-medium">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data?.items.map((user) => (
                      <tr key={user.id} className="border-b border-border">
                        <td className="py-3 pr-4">{user.email}</td>
                        <td className="py-3 pr-4">{user.displayName}</td>
                        <td className="py-3 pr-4">
                          <Badge variant="outline">{user.role}</Badge>
                        </td>
                        <td className="py-3 pr-4">
                          <Badge variant={user.isFrozen ? "destructive" : "success"}>
                            {user.isFrozen ? "Frozen" : "Active"}
                          </Badge>
                        </td>
                        <td className="py-3">
                          {user.isFrozen ? (
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() => unfreezeMutation.mutate(user.id)}
                              disabled={unfreezeMutation.isPending}
                            >
                              <Sun className="h-4 w-4 mr-1" />
                              Unfreeze
                            </Button>
                          ) : (
                            <Button
                              size="sm"
                              variant="destructive"
                              onClick={() => freezeMutation.mutate(user.id)}
                              disabled={freezeMutation.isPending}
                            >
                              <Snowflake className="h-4 w-4 mr-1" />
                              Freeze
                            </Button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                {!data?.items?.length && (
                  <p className="py-8 text-center text-muted-foreground">No users found.</p>
                )}
              </div>
              {totalPages > 1 && (
                <div className="mt-4 flex items-center justify-between">
                  <p className="text-sm text-muted-foreground">
                    Page {page} of {totalPages} ({data?.total ?? 0} total)
                  </p>
                  <div className="flex gap-2">
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                      disabled={page <= 1}
                    >
                      Previous
                    </Button>
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                      disabled={page >= totalPages}
                    >
                      Next
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
