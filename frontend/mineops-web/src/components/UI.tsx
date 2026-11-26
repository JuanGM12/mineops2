import {
  AlertCircle,
  ChevronLeft,
  ChevronRight,
  Inbox,
  LoaderCircle,
  X,
} from "lucide-react";
import { useEffect, type ReactNode } from "react";
import type { Priority, Status } from "../types";
export const statusLabel: Record<Status, string> = {
  Pending: "Pendiente",
  InProgress: "En ejecución",
  Completed: "Completada",
  Cancelled: "Cancelada",
};
export const priorityLabel: Record<Priority, string> = {
  Low: "Baja",
  Medium: "Media",
  High: "Alta",
  Critical: "Crítica",
};
export function Badge({ status }: { status: Status }) {
  return (
    <span
      className={`status-${status} inline-flex items-center rounded-full px-2.5 py-1 text-[11px] font-bold`}
    >
      {statusLabel[status]}
    </span>
  );
}
export function PriorityText({ priority }: { priority: Priority }) {
  return (
    <span className={`priority-${priority} font-semibold`}>
      {priorityLabel[priority]}
    </span>
  );
}
export function PageTitle({
  eyebrow,
  title,
  description,
  action,
}: {
  eyebrow: string;
  title: string;
  description: string;
  action?: ReactNode;
}) {
  return (
    <div className="mb-7 flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
      <div>
        <p className="mb-2 text-[11px] font-extrabold uppercase tracking-[.2em] text-[#117b70]">
          {eyebrow}
        </p>
        <h1 className="m-0 text-2xl font-extrabold tracking-tight text-[#102935] sm:text-[30px]">
          {title}
        </h1>
        <p className="mb-0 mt-2 text-sm text-[#748692]">{description}</p>
      </div>
      {action}
    </div>
  );
}
export function ErrorBox({
  message,
  retry,
}: {
  message: string;
  retry?: () => void;
}) {
  return (
    <div className="card flex items-center gap-3 p-5 text-sm text-[#ab4c46]">
      <AlertCircle size={18} />
      <span className="flex-1">{message}</span>
      {retry && (
        <button className="btn btn-light" onClick={retry}>
          Reintentar
        </button>
      )}
    </div>
  );
}
export function Empty({
  title = "Sin resultados",
  text = "No hay registros que coincidan con esta vista.",
}: {
  title?: string;
  text?: string;
}) {
  return (
    <div className="flex flex-col items-center justify-center px-6 py-16 text-center">
      <div className="mb-4 rounded-2xl bg-[#eef5f4] p-4 text-[#117b70]">
        <Inbox size={28} />
      </div>
      <h3 className="m-0 text-base font-bold">{title}</h3>
      <p className="mt-2 text-sm text-[#82919b]">{text}</p>
    </div>
  );
}
export function LoadingRows() {
  return (
    <div className="space-y-3 p-6">
      {[1, 2, 3, 4, 5].map((i) => (
        <div key={i} className="skeleton h-12 w-full" />
      ))}
    </div>
  );
}
export function Pagination({
  page,
  pages,
  total,
  onChange,
}: {
  page: number;
  pages: number;
  total: number;
  onChange: (page: number) => void;
}) {
  return (
    <div className="flex items-center justify-between border-t border-[#edf1f2] px-5 py-4 text-xs text-[#7c8b96]">
      <span>
        {total} registros · Página {page} de {Math.max(pages, 1)}
      </span>
      <div className="flex gap-2">
        <button
          className="btn btn-light !p-2"
          aria-label="Página anterior"
          disabled={page <= 1}
          onClick={() => onChange(page - 1)}
        >
          <ChevronLeft size={16} />
        </button>
        <button
          className="btn btn-light !p-2"
          aria-label="Página siguiente"
          disabled={page >= pages}
          onClick={() => onChange(page + 1)}
        >
          <ChevronRight size={16} />
        </button>
      </div>
    </div>
  );
}
export function Modal({
  title,
  children,
  onClose,
}: {
  title: string;
  children: ReactNode;
  onClose: () => void;
}) {
  useEffect(() => {
    const listener = (event: KeyboardEvent) => {
      if (event.key === "Escape") onClose();
    };
    window.addEventListener("keydown", listener);
    return () => window.removeEventListener("keydown", listener);
  }, [onClose]);
  return (
    <div
      className="modal-backdrop"
      role="presentation"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onClose();
      }}
    >
      <section
        className="modal"
        role="dialog"
        aria-modal="true"
        aria-label={title}
      >
        <div className="flex items-center justify-between border-b border-[#edf1f2] px-6 py-5">
          <h2 className="m-0 text-lg font-extrabold text-[#102935]">{title}</h2>
          <button
            aria-label="Cerrar"
            className="rounded-lg p-1 text-[#7b8f9a] hover:bg-[#f3f6f7]"
            onClick={onClose}
          >
            <X size={20} />
          </button>
        </div>
        {children}
      </section>
    </div>
  );
}
export function Spinner() {
  return <LoaderCircle size={16} className="animate-spin" />;
}
export function Field({
  label,
  children,
}: {
  label: string;
  children: ReactNode;
}) {
  return (
    <label className="block">
      <span className="label">{label}</span>
      {children}
    </label>
  );
}
export const dateText = (date?: string | null) =>
  date
    ? new Intl.DateTimeFormat("es-CO", {
        day: "2-digit",
        month: "short",
        year: "numeric",
      }).format(new Date(date))
    : "—";
export const dateTimeLocal = (date?: string | null) =>
  date
    ? new Date(
        new Date(date).getTime() - new Date(date).getTimezoneOffset() * 60000,
      )
        .toISOString()
        .slice(0, 16)
    : "";
