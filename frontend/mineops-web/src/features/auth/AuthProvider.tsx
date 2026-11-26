import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { useQueryClient } from "@tanstack/react-query";
import { authApi, setAccessToken } from "../../services/api";
import type { Role, User } from "../../types";

type AuthContextType = {
  user: User | null;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
  can: (...roles: Role[]) => boolean;
};
const AuthContext = createContext<AuthContextType | null>(null);
function parseToken(token: string): User {
  const payload = token.split(".")[1];
  if (!payload) throw new Error("Token inválido");
  const decoded = JSON.parse(
    atob(payload.replace(/-/g, "+").replace(/_/g, "/")),
  ) as Record<string, unknown>;
  const rawRoles = decoded.role;
  return {
    id: String(decoded.sub ?? ""),
    name: String(decoded.name ?? ""),
    email: String(decoded.email ?? ""),
    roles: (Array.isArray(rawRoles)
      ? rawRoles
      : rawRoles
        ? [rawRoles]
        : []) as Role[],
  };
}
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const queryClient = useQueryClient();
  const timeout = useRef<number | null>(null);
  const logout = useCallback(() => {
    if (timeout.current) clearTimeout(timeout.current);
    setAccessToken(null);
    setUser(null);
    queryClient.clear();
  }, [queryClient]);
  useEffect(() => {
    window.addEventListener("mineops:unauthorized", logout);
    return () => window.removeEventListener("mineops:unauthorized", logout);
  }, [logout]);
  const login = useCallback(
    async (email: string, password: string) => {
      const response = await authApi.login(email, password);
      if (new Date(response.expiresAt).getTime() <= Date.now())
        throw new Error("El token recibido ya expiró.");
      const nextUser = parseToken(response.accessToken);
      if (timeout.current) clearTimeout(timeout.current);
      setAccessToken(response.accessToken);
      setUser(nextUser);
      timeout.current = window.setTimeout(
        logout,
        Math.max(0, new Date(response.expiresAt).getTime() - Date.now()),
      );
    },
    [logout],
  );
  const value = useMemo(
    () => ({
      user,
      login,
      logout,
      can: (...roles: Role[]) =>
        !!user && roles.some((role) => user.roles.includes(role)),
    }),
    [user, login, logout],
  );
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error("AuthProvider missing");
  return value;
}
