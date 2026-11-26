import axios from "axios";
import type {
  Activity,
  ActivityInput,
  Equipment,
  LoginResponse,
  Page,
  WorkOrder,
  WorkOrderInput,
} from "../types";

let accessToken: string | null = null;
export const setAccessToken = (token: string | null) => {
  accessToken = token;
};
export const http = axios.create({ timeout: 15000 });
http.interceptors.request.use((config) => {
  if (accessToken) config.headers.Authorization = `Bearer ${accessToken}`;
  return config;
});
http.interceptors.response.use(
  (response) => response,
  (error) => {
    if (
      axios.isAxiosError(error) &&
      error.response?.status === 401 &&
      !error.config?.url?.includes("/auth/login")
    )
      window.dispatchEvent(new Event("mineops:unauthorized"));
    return Promise.reject(error);
  },
);
export function apiError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    if (error.response?.status === 403)
      return "No tienes permiso para esta operación.";
    if (error.response?.status === 401)
      return "La sesión venció. Inicia sesión nuevamente.";
    const body = error.response?.data as
      { message?: string; errors?: string[] } | undefined;
    if (body?.errors?.length) return body.errors.join(" ");
    if (body?.message) return body.message;
    if (!error.response)
      return "No se pudo conectar con el servicio. Revisa que esté activo.";
  }
  return "Ocurrió un error inesperado.";
}
export const authApi = {
  login: async (email: string, password: string) =>
    (
      await http.post<LoginResponse>("/identity/api/auth/login", {
        email,
        password,
      })
    ).data,
};
export const planningApi = {
  page: async (page = 1, pageSize = 20) =>
    (
      await http.get<Page<Activity>>("/planning/api/activities", {
        params: { page, pageSize },
      })
    ).data,
  get: async (id: string) =>
    (await http.get<Activity>(`/planning/api/activities/${id}`)).data,
  create: async (value: ActivityInput) =>
    (await http.post<Activity>("/planning/api/activities", value)).data,
  update: async (id: string, value: Omit<ActivityInput, "code">) =>
    (await http.put<Activity>(`/planning/api/activities/${id}`, value)).data,
  transition: async (id: string, action: "start" | "complete" | "cancel") =>
    (await http.patch<Activity>(`/planning/api/activities/${id}/${action}`))
      .data,
};
export const maintenanceApi = {
  equipment: async () =>
    (await http.get<Equipment[]>("/maintenance/api/equipment")).data,
  page: async (page = 1, pageSize = 20) =>
    (
      await http.get<Page<WorkOrder>>("/maintenance/api/work-orders", {
        params: { page, pageSize },
      })
    ).data,
  get: async (id: string) =>
    (await http.get<WorkOrder>(`/maintenance/api/work-orders/${id}`)).data,
  create: async (value: WorkOrderInput) =>
    (await http.post<WorkOrder>("/maintenance/api/work-orders", value)).data,
  transition: async (id: string, action: "start" | "complete" | "cancel") =>
    (
      await http.patch<WorkOrder>(
        `/maintenance/api/work-orders/${id}/${action}`,
      )
    ).data,
};
export async function allPages<T>(
  fetchPage: (page: number, pageSize: number) => Promise<Page<T>>,
): Promise<T[]> {
  const first = await fetchPage(1, 100);
  const pages = Math.ceil(first.totalCount / 100);
  if (pages <= 1) return first.items;
  const rest = await Promise.all(
    Array.from({ length: pages - 1 }, (_, index) => fetchPage(index + 2, 100)),
  );
  return [...first.items, ...rest.flatMap((page) => page.items)];
}
