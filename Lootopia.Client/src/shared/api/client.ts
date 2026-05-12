const API_BASE = "/api";

export class ApiError extends Error {
  status: number;
  body: unknown;
  code?: string;

  constructor(status: number, body: unknown) {
    let message = `Erreur serveur (${status})`;
    let code: string | undefined;

    if (body && typeof body === "object") {
      const b = body as Record<string, unknown>;
      // Extraire la description ou le message d'erreur de la réponse
      const desc =
        typeof b.description === "string" ? b.description :
        typeof b.Description === "string" ? b.Description :
        typeof b.message === "string" ? b.message :
        typeof b.detail === "string" ? b.detail :
        typeof b.error === "string" ? b.error : undefined;

      code = typeof b.code === "string" ? b.code : typeof b.Code === "string" ? b.Code : undefined;

      if (desc) {
        message = desc;
      }

      // Gérer les erreurs de validation (ex: ProblemDetails avec champ errors)
      if (b.errors && typeof b.errors === "object") {
        const errs = Object.values(b.errors).flat().join(", ");
        if (errs) {
          message = errs;
        }
      }
    }

    // Traduction de certains messages génériques backend vers un français plus naturel
    if (message.includes("The requested resource was not found")) {
      message = "Ressource introuvable.";
    } else if (message.includes("Access denied")) {
      message = "Accès non autorisé.";
    } else if (message.includes("Authentication required")) {
      message = "Authentification requise.";
    } else if (message.includes("A validation error occurred")) {
      message = "Erreur de validation des données.";
    }

    // Ajout de conseils / guidage pour aider l'utilisateur à résoudre le problème
    let guidance = "";
    if (status === 401) {
      guidance = " Votre session a expiré ou est invalide. Veuillez vous reconnecter.";
    } else if (status === 402) {
      guidance = " Solde de jetons (LTK) insuffisant pour effectuer cette opération.";
    } else if (status === 403) {
      guidance = " Vous n'avez pas les permissions requises pour cette action.";
    } else if (status === 404 && message === `Erreur serveur (404)`) {
      guidance = " L'élément demandé n'existe pas ou a été supprimé.";
    } else if (status === 429) {
      guidance = " Trop de requêtes envoyées simultanément. Veuillez patienter un instant.";
    } else if (status >= 500) {
      guidance = " Un problème technique est survenu côté serveur. Veuillez réessayer plus tard.";
    }

    super(guidance ? `${message} — ${guidance.trim()}` : message);
    this.status = status;
    this.body = body;
    this.code = code;
  }
}

async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) return null;

  const res = await fetch(`${API_BASE}/auth/refresh`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ refreshToken }),
  });

  if (!res.ok) {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    return null;
  }

  const data = await res.json();
  localStorage.setItem("token", data.accessToken);
  localStorage.setItem("refreshToken", data.refreshToken);
  return data.accessToken as string;
}

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  let token = localStorage.getItem("token");

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...(options.headers as Record<string, string>),
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  let res = await fetch(`${API_BASE}${path}`, { ...options, headers });

  if (res.status === 401 && token) {
    const newToken = await refreshAccessToken();
    if (newToken) {
      headers["Authorization"] = `Bearer ${newToken}`;
      res = await fetch(`${API_BASE}${path}`, { ...options, headers });
    }
  }

  if (!res.ok) {
    const body = await res.json().catch(() => null);
    throw new ApiError(res.status, body);
  }

  if (res.status === 204) return undefined as T;
  return res.json() as Promise<T>;
}
