import { useState } from "react";
import { Link, useNavigate } from "react-router";
import { useAuth } from "@/shared/providers/AuthProvider";
import { useToast } from "@/shared/components/ui/toast";
import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/shared/components/ui/card";
import { useEffect } from "react";

function homeForRole(role?: string): string {
  switch (role) {
    case "Admin": return "/admin/dashboard";
    case "Partner": return "/partner/dashboard";
    default: return "/play/hunts";
  }
}

export function LoginPage() {
  const { login, loginAsGuest, isAuthenticated, isLoading, user } = useAuth();
  const { toast } = useToast();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isGuestLoading, setIsGuestLoading] = useState(false);

  useEffect(() => {
    if (!isLoading && isAuthenticated) {
      navigate(homeForRole(user?.role), { replace: true });
    }
  }, [isAuthenticated, isLoading, navigate, user]);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!email.trim() || !password) {
      toast({
        title: "Erreur",
        description: "Veuillez remplir l'email et le mot de passe.",
        variant: "destructive",
      });
      return;
    }
    setIsSubmitting(true);
    try {
      const u = await login(email.trim(), password);
      navigate(homeForRole(u.role), { replace: true });
    } catch (err) {
      toast({
        title: "Erreur de connexion",
        description: err instanceof Error ? err.message : "Identifiants incorrects.",
        variant: "destructive",
      });
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleGuestLogin() {
    setIsGuestLoading(true);
    try {
      const u = await loginAsGuest();
      navigate(homeForRole(u.role), { replace: true });
    } catch (err) {
      toast({
        title: "Erreur",
        description: err instanceof Error ? err.message : "Impossible de se connecter en invité.",
        variant: "destructive",
      });
    } finally {
      setIsGuestLoading(false);
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <div className="animate-spin h-10 w-10 border-4 border-primary border-t-transparent rounded-full" />
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-background px-4 py-12">
      <div className="w-full max-w-md space-y-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
        <div className="text-center">
          <h1 className="text-4xl font-bold bg-gradient-to-r from-violet-500 via-purple-600 to-violet-700 bg-clip-text text-transparent tracking-tight">
            Lootopia
          </h1>
          <p className="mt-2 text-sm text-muted-foreground">
            Chasses au trésor géolocalisées en réalité augmentée
          </p>
        </div>

        <Card className="border-border bg-card shadow-xl transition-all duration-300">
          <CardHeader className="space-y-1">
            <CardTitle className="text-xl">Connexion</CardTitle>
            <CardDescription>Entrez vos identifiants pour accéder à votre compte</CardDescription>
          </CardHeader>
          <form onSubmit={handleSubmit}>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <label htmlFor="email" className="text-sm font-medium text-foreground">
                  Email
                </label>
                <Input
                  id="email"
                  type="email"
                  placeholder="vous@exemple.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="bg-background/50 border-border focus-visible:ring-primary transition-colors"
                  autoComplete="email"
                  disabled={isSubmitting}
                />
              </div>
              <div className="space-y-2">
                <label htmlFor="password" className="text-sm font-medium text-foreground">
                  Mot de passe
                </label>
                <Input
                  id="password"
                  type="password"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="bg-background/50 border-border focus-visible:ring-primary transition-colors"
                  autoComplete="current-password"
                  disabled={isSubmitting}
                />
              </div>
            </CardContent>
            <CardFooter className="flex flex-col gap-3">
              <Button
                type="submit"
                className="w-full bg-primary hover:bg-primary/90 text-primary-foreground transition-all duration-200 hover:scale-[1.02] active:scale-[0.98]"
                disabled={isSubmitting}
              >
                {isSubmitting ? (
                  <span className="flex items-center gap-2">
                    <span className="h-4 w-4 animate-spin rounded-full border-2 border-primary-foreground border-t-transparent" />
                    Connexion...
                  </span>
                ) : (
                  "Se connecter"
                )}
              </Button>
              <div className="relative w-full">
                <div className="absolute inset-0 flex items-center">
                  <span className="w-full border-t border-border" />
                </div>
                <div className="relative flex justify-center text-xs uppercase">
                  <span className="bg-card px-2 text-muted-foreground">ou</span>
                </div>
              </div>
              <Button
                type="button"
                variant="outline"
                className="w-full border-border hover:bg-secondary/50 hover:border-primary/50 transition-all duration-200"
                onClick={handleGuestLogin}
                disabled={isGuestLoading}
              >
                {isGuestLoading ? (
                  <span className="flex items-center gap-2">
                    <span className="h-4 w-4 animate-spin rounded-full border-2 border-primary border-t-transparent" />
                    Connexion...
                  </span>
                ) : (
                  "Jouer en invité"
                )}
              </Button>
              <p className="text-center text-sm text-muted-foreground">
                Pas encore de compte ?{" "}
                <Link
                  to="/register"
                  className="font-medium text-primary hover:text-primary/90 underline-offset-4 hover:underline transition-colors"
                >
                  S&apos;inscrire
                </Link>
              </p>
            </CardFooter>
          </form>
        </Card>
      </div>
    </div>
  );
}
