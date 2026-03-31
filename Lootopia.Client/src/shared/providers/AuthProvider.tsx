import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { authApi, type AuthResponse } from "../api/auth";

interface User {
  id: string;
  email: string;
  displayName: string;
  role: string;
}

interface AuthContextValue {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<User>;
  register: (email: string, password: string, displayName: string) => Promise<User>;
  loginAsGuest: () => Promise<User>;
  upgrade: (email: string, password: string, displayName: string) => Promise<User>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function parseJwt(token: string): Record<string, string> {
  const base64Url = token.split(".")[1];
  const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
  const json = decodeURIComponent(
    atob(base64)
      .split("")
      .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
      .join(""),
  );
  return JSON.parse(json);
}

const CLAIM_ROLE = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
const CLAIM_NAMEID = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
const CLAIM_NAME = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";

function userFromToken(token: string): User {
  const claims = parseJwt(token);
  return {
    id: claims["sub"] ?? claims[CLAIM_NAMEID] ?? claims["nameid"] ?? "",
    email: claims["email"] ?? "",
    displayName: claims[CLAIM_NAME] ?? claims["unique_name"] ?? claims["name"] ?? claims["email"] ?? "",
    role: claims[CLAIM_ROLE] ?? claims["role"] ?? "Player",
  };
}

function handleAuthResponse(data: AuthResponse) {
  localStorage.setItem("token", data.accessToken);
  localStorage.setItem("refreshToken", data.refreshToken);
  return userFromToken(data.accessToken);
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem("token");
    if (stored) {
      try {
        const u = userFromToken(stored);
        setUser(u);
        setToken(stored);
      } catch {
        localStorage.removeItem("token");
        localStorage.removeItem("refreshToken");
      }
    }
    setIsLoading(false);
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const data = await authApi.login(email, password);
    const u = handleAuthResponse(data);
    setUser(u);
    setToken(data.accessToken);
    return u;
  }, []);

  const register = useCallback(
    async (email: string, password: string, displayName: string) => {
      const data = await authApi.register(email, password, displayName);
      const u = handleAuthResponse(data);
      setUser(u);
      setToken(data.accessToken);
      return u;
    },
    [],
  );

  const loginAsGuest = useCallback(async () => {
    const deviceId =
      localStorage.getItem("deviceId") ?? crypto.randomUUID();
    localStorage.setItem("deviceId", deviceId);
    const data = await authApi.guest(deviceId);
    const u = handleAuthResponse(data);
    setUser(u);
    setToken(data.accessToken);
    return u;
  }, []);

  const upgrade = useCallback(
    async (email: string, password: string, displayName: string) => {
      const data = await authApi.upgrade(email, password, displayName);
      const u = handleAuthResponse(data);
      setUser(u);
      setToken(data.accessToken);
      return u;
    },
    [],
  );

  const logout = useCallback(() => {
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    setUser(null);
    setToken(null);
  }, []);

  const value = useMemo(
    () => ({
      user,
      token,
      isAuthenticated: !!user,
      isLoading,
      login,
      register,
      loginAsGuest,
      upgrade,
      logout,
    }),
    [user, token, isLoading, login, register, loginAsGuest, upgrade, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
