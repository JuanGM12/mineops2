export type Status = "Pending" | "InProgress" | "Completed" | "Cancelled";
export type Priority = "Low" | "Medium" | "High" | "Critical";
export type MaintenanceType = "Preventive" | "Corrective";
export type Role =
  "Administrator" | "Planner" | "MaintenanceSupervisor" | "Operator";
export interface Page<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
export interface Activity {
  id: string;
  code: string;
  title: string;
  description: string | null;
  area: string;
  location: string;
  responsibleId: string | null;
  priority: Priority;
  status: Status;
  scheduledDate: string;
  dueDate: string | null;
  createdAt: string;
  updatedAt: string;
}
export type ActivityInput = Pick<
  Activity,
  | "code"
  | "title"
  | "description"
  | "area"
  | "location"
  | "responsibleId"
  | "priority"
  | "scheduledDate"
  | "dueDate"
>;
export interface Equipment {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
}
export interface WorkOrder {
  id: string;
  code: string;
  equipmentId: string;
  title: string;
  description: string | null;
  maintenanceType: MaintenanceType;
  priority: Priority;
  status: Status;
  scheduledDate: string | null;
  startedAt: string | null;
  completedAt: string | null;
  createdAt: string;
  updatedAt: string;
}
export type WorkOrderInput = Pick<
  WorkOrder,
  | "code"
  | "equipmentId"
  | "title"
  | "description"
  | "maintenanceType"
  | "priority"
  | "scheduledDate"
>;
export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  tokenType: string;
}
export interface User {
  id: string;
  name: string;
  email: string;
  roles: Role[];
}
