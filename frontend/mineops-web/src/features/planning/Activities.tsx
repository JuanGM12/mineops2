import { useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Search } from "lucide-react";
import {
  Badge,
  Empty,
  ErrorBox,
  Field,
  LoadingRows,
  Modal,
  PageTitle,
  Pagination,
  PriorityText,
  Spinner,
  dateText,
  dateTimeLocal,
} from "../../components/UI";
import { allPages, apiError, planningApi } from "../../services/api";
import type { Activity, ActivityInput, Priority, Status } from "../../types";
import { useAuth } from "../auth/AuthProvider";
import { useToast } from "../../app/App";
const emptyForm: ActivityInput = {
  code: "",
  title: "",
  description: null,
  area: "",
  location: "",
  responsibleId: null,
  priority: "Medium",
  scheduledDate: "",
  dueDate: null,
};
const priorities: Priority[] = ["Low", "Medium", "High", "Critical"];
const statuses: Status[] = ["Pending", "InProgress", "Completed", "Cancelled"];
export function Activities() {
  const { can } = useAuth(),
    notify = useToast(),
    client = useQueryClient();
  const [search, setSearch] = useState(""),
    [status, setStatus] = useState("all"),
    [priority, setPriority] = useState("all"),
    [page, setPage] = useState(1);
  const [form, setForm] = useState<ActivityInput | null>(null),
    [editing, setEditing] = useState<string | null>(null),
    [detail, setDetail] = useState<string | null>(null),
    [confirm, setConfirm] = useState<{
      id: string;
      action: "start" | "complete" | "cancel";
      name: string;
    } | null>(null),
    [formError, setFormError] = useState("");
  const query = useQuery({
    queryKey: ["activities", "all"],
    queryFn: () => allPages(planningApi.page),
  });
  const detailQuery = useQuery({
    queryKey: ["activity", detail],
    queryFn: () => planningApi.get(detail!),
    enabled: !!detail,
  });
  const mutation = useMutation({
    mutationFn: async (value: ActivityInput) =>
      editing
        ? planningApi.update(editing, {
            title: value.title,
            description: value.description,
            area: value.area,
            location: value.location,
            responsibleId: value.responsibleId,
            priority: value.priority,
            scheduledDate: new Date(value.scheduledDate).toISOString(),
            dueDate: value.dueDate
              ? new Date(value.dueDate).toISOString()
              : null,
          })
        : planningApi.create({
            ...value,
            scheduledDate: new Date(value.scheduledDate).toISOString(),
            dueDate: value.dueDate
              ? new Date(value.dueDate).toISOString()
              : null,
          }),
    onSuccess: () => {
      client.invalidateQueries({ queryKey: ["activities"] });
      setForm(null);
      setEditing(null);
      notify(editing ? "Actividad actualizada" : "Actividad creada");
    },
    onError: (error) => setFormError(apiError(error)),
  });
  const transition = useMutation({
    mutationFn: async ({
      id,
      action,
    }: {
      id: string;
      action: "start" | "complete" | "cancel";
    }) => planningApi.transition(id, action),
    onSuccess: () => {
      client.invalidateQueries({ queryKey: ["activities"] });
      client.invalidateQueries({ queryKey: ["activity"] });
      setConfirm(null);
      notify("Estado de la actividad actualizado");
    },
    onError: (error) => {
      notify(apiError(error), "error");
      setConfirm(null);
    },
  });
  const rows = useMemo(
    () =>
      query.data?.filter(
        (x) =>
          (status === "all" || x.status === status) &&
          (priority === "all" || x.priority === priority) &&
          `${x.code} ${x.title} ${x.area} ${x.location} ${x.responsibleId ?? ""}`
            .toLowerCase()
            .includes(search.toLowerCase()),
      ) ?? [],
    [query.data, status, priority, search],
  );
  const perPage = 10;
  const begin = (item?: Activity) => {
    setForm(
      item
        ? {
            code: item.code,
            title: item.title,
            description: item.description,
            area: item.area,
            location: item.location,
            responsibleId: item.responsibleId,
            priority: item.priority,
            scheduledDate: dateTimeLocal(item.scheduledDate),
            dueDate: dateTimeLocal(item.dueDate) || null,
          }
        : { ...emptyForm },
    );
    setEditing(item?.id ?? null);
    setFormError("");
  };
  const submit = (event: FormEvent) => {
    event.preventDefault();
    if (!form) return;
    if (form.priority === "Critical" && !form.dueDate) {
      setFormError("La prioridad crítica requiere fecha límite.");
      return;
    }
    if (form.dueDate && new Date(form.dueDate) < new Date(form.scheduledDate)) {
      setFormError("La fecha límite debe ser posterior a la programada.");
      return;
    }
    mutation.mutate(form);
  };
  const change = <K extends keyof ActivityInput>(
    key: K,
    value: ActivityInput[K],
  ) => setForm((current) => (current ? { ...current, [key]: value } : current));
  return (
    <>
      <PageTitle
        eyebrow="Planeación"
        title="Actividades operativas"
        description="Organiza y sigue las actividades de la operación minera."
        action={
          can("Administrator", "Planner") ? (
            <button className="btn btn-primary" onClick={() => begin()}>
              <Plus size={17} /> Nueva actividad
            </button>
          ) : undefined
        }
      />
      <div className="card overflow-hidden">
        <div className="flex flex-col gap-4 p-5 xl:flex-row xl:items-center xl:justify-between">
          <div>
            <h2 className="m-0 text-base font-extrabold">
              Todas las actividades
            </h2>
            <p className="mt-1 text-xs text-[#84939d]">
              {query.data?.length ?? "—"} registros en Planning Service
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <div className="relative flex-1">
              <Search
                size={16}
                className="absolute left-3 top-3 text-[#9bacb4]"
              />
              <input
                className="field min-w-[180px] pl-9"
                placeholder="Buscar actividad"
                value={search}
                onChange={(e) => {
                  setSearch(e.target.value);
                  setPage(1);
                }}
              />
            </div>
            <select
              className="field !w-auto"
              value={status}
              onChange={(e) => {
                setStatus(e.target.value);
                setPage(1);
              }}
            >
              <option value="all">Todos los estados</option>
              {statuses.map((s) => (
                <option key={s} value={s}>
                  {s === "InProgress"
                    ? "En ejecución"
                    : s === "Pending"
                      ? "Pendiente"
                      : s === "Completed"
                        ? "Completada"
                        : "Cancelada"}
                </option>
              ))}
            </select>
            <select
              className="field !w-auto"
              value={priority}
              onChange={(e) => {
                setPriority(e.target.value);
                setPage(1);
              }}
            >
              <option value="all">Todas las prioridades</option>
              {priorities.map((p) => (
                <option key={p} value={p}>
                  {p === "Low"
                    ? "Baja"
                    : p === "Medium"
                      ? "Media"
                      : p === "High"
                        ? "Alta"
                        : "Crítica"}
                </option>
              ))}
            </select>
          </div>
        </div>
        {query.isError ? (
          <div className="p-5">
            <ErrorBox
              message={apiError(query.error)}
              retry={() => query.refetch()}
            />
          </div>
        ) : query.isLoading ? (
          <LoadingRows />
        ) : rows.length ? (
          <>
            <div className="table-scroll">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Código / Actividad</th>
                    <th>Área / Ubicación</th>
                    <th>Responsable</th>
                    <th>Prioridad</th>
                    <th>Programada</th>
                    <th>Límite</th>
                    <th>Estado</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {rows
                    .slice((page - 1) * perPage, page * perPage)
                    .map((item) => (
                      <tr key={item.id}>
                        <td>
                          <button
                            className="text-left"
                            onClick={() => setDetail(item.id)}
                          >
                            <span className="block text-[11px] font-extrabold text-[#117b70]">
                              {item.code}
                            </span>
                            <span className="font-bold text-[#203947]">
                              {item.title}
                            </span>
                          </button>
                        </td>
                        <td>
                          {item.area || "—"}
                          <span className="block text-xs text-[#92a0a8]">
                            {item.location || "—"}
                          </span>
                        </td>
                        <td title={item.responsibleId ?? undefined}>
                          {item.responsibleId
                            ? `${item.responsibleId.slice(0, 8)}…`
                            : "Sin asignar"}
                        </td>
                        <td>
                          <PriorityText priority={item.priority} />
                        </td>
                        <td>{dateText(item.scheduledDate)}</td>
                        <td>{dateText(item.dueDate)}</td>
                        <td>
                          <Badge status={item.status} />
                        </td>
                        <td>
                          <div className="flex flex-wrap gap-2 text-xs font-bold text-[#117b70]">
                            <button onClick={() => setDetail(item.id)}>
                              Detalle
                            </button>
                            {can("Administrator", "Planner") &&
                              ["Pending", "InProgress"].includes(
                                item.status,
                              ) && (
                                <button onClick={() => begin(item)}>
                                  Editar
                                </button>
                              )}
                            {item.status === "Pending" &&
                              can("Administrator", "Planner", "Operator") && (
                                <button
                                  onClick={() =>
                                    setConfirm({
                                      id: item.id,
                                      action: "start",
                                      name: item.title,
                                    })
                                  }
                                >
                                  Iniciar
                                </button>
                              )}
                            {item.status === "InProgress" &&
                              can("Administrator", "Planner", "Operator") && (
                                <button
                                  onClick={() =>
                                    setConfirm({
                                      id: item.id,
                                      action: "complete",
                                      name: item.title,
                                    })
                                  }
                                >
                                  Completar
                                </button>
                              )}
                            {["Pending", "InProgress"].includes(item.status) &&
                              can("Administrator", "Planner") && (
                                <button
                                  className="text-[#b34d4c]"
                                  onClick={() =>
                                    setConfirm({
                                      id: item.id,
                                      action: "cancel",
                                      name: item.title,
                                    })
                                  }
                                >
                                  Cancelar
                                </button>
                              )}
                          </div>
                        </td>
                      </tr>
                    ))}
                </tbody>
              </table>
            </div>
            <Pagination
              page={page}
              pages={Math.ceil(rows.length / perPage)}
              total={rows.length}
              onChange={setPage}
            />
          </>
        ) : (
          <Empty />
        )}
      </div>
      {form && (
        <Modal
          title={editing ? "Editar actividad" : "Nueva actividad"}
          onClose={() => setForm(null)}
        >
          <form onSubmit={submit} className="space-y-5 p-6">
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Código *">
                <input
                  className="field"
                  required
                  disabled={!!editing}
                  value={form.code}
                  onChange={(e) => change("code", e.target.value)}
                />
              </Field>
              <Field label="Título *">
                <input
                  className="field"
                  required
                  value={form.title}
                  onChange={(e) => change("title", e.target.value)}
                />
              </Field>
              <Field label="Área">
                <input
                  className="field"
                  value={form.area}
                  onChange={(e) => change("area", e.target.value)}
                />
              </Field>
              <Field label="Ubicación">
                <input
                  className="field"
                  value={form.location}
                  onChange={(e) => change("location", e.target.value)}
                />
              </Field>
              <Field label="Responsable (UUID)">
                <input
                  className="field"
                  placeholder="Opcional"
                  pattern="[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"
                  title="Ingresa un UUID válido"
                  value={form.responsibleId ?? ""}
                  onChange={(e) =>
                    change("responsibleId", e.target.value || null)
                  }
                />
              </Field>
              <Field label="Prioridad">
                <select
                  className="field"
                  value={form.priority}
                  onChange={(e) =>
                    change("priority", e.target.value as Priority)
                  }
                >
                  {priorities.map((p) => (
                    <option key={p} value={p}>
                      {p === "Low"
                        ? "Baja"
                        : p === "Medium"
                          ? "Media"
                          : p === "High"
                            ? "Alta"
                            : "Crítica"}
                    </option>
                  ))}
                </select>
              </Field>
              <Field label="Fecha programada *">
                <input
                  className="field"
                  type="datetime-local"
                  required
                  value={form.scheduledDate}
                  onChange={(e) => change("scheduledDate", e.target.value)}
                />
              </Field>
              <Field label="Fecha límite">
                <input
                  className="field"
                  type="datetime-local"
                  value={form.dueDate ?? ""}
                  onChange={(e) => change("dueDate", e.target.value || null)}
                />
              </Field>
            </div>
            <Field label="Descripción">
              <textarea
                className="field min-h-24"
                value={form.description ?? ""}
                onChange={(e) => change("description", e.target.value || null)}
              />
            </Field>
            {formError && (
              <p
                role="alert"
                className="rounded-lg bg-[#fff0ee] p-3 text-sm text-[#ab4c46]"
              >
                {formError}
              </p>
            )}
            <div className="flex justify-end gap-2">
              <button
                type="button"
                className="btn btn-light"
                onClick={() => setForm(null)}
              >
                Volver
              </button>
              <button className="btn btn-primary" disabled={mutation.isPending}>
                {mutation.isPending ? <Spinner /> : null}
                {editing ? "Guardar cambios" : "Crear actividad"}
              </button>
            </div>
          </form>
        </Modal>
      )}
      {detail && (
        <Modal title="Detalle de actividad" onClose={() => setDetail(null)}>
          {detailQuery.isLoading ? (
            <LoadingRows />
          ) : detailQuery.isError ? (
            <div className="p-6">
              <ErrorBox message={apiError(detailQuery.error)} />
            </div>
          ) : (
            detailQuery.data && (
              <div className="grid gap-5 p-6 sm:grid-cols-2">
                {(
                  [
                    ["Código", detailQuery.data.code],
                    ["Título", detailQuery.data.title],
                    ["Área", detailQuery.data.area],
                    ["Ubicación", detailQuery.data.location],
                    [
                      "Responsable",
                      detailQuery.data.responsibleId ?? "Sin asignar",
                    ],
                    ["Prioridad", detailQuery.data.priority],
                    ["Programada", dateText(detailQuery.data.scheduledDate)],
                    ["Límite", dateText(detailQuery.data.dueDate)],
                    ["Descripción", detailQuery.data.description ?? "—"],
                  ] as [string, string][]
                ).map(([label, value]) => (
                  <div key={label}>
                    <p className="label">{label}</p>
                    <p className="m-0 break-words text-sm font-semibold">
                      {value || "—"}
                    </p>
                  </div>
                ))}
                <div>
                  <p className="label">Estado</p>
                  <Badge status={detailQuery.data.status} />
                </div>
              </div>
            )
          )}
        </Modal>
      )}
      {confirm && (
        <Modal
          title="Confirmar cambio de estado"
          onClose={() => setConfirm(null)}
        >
          <div className="p-6">
            <p className="mt-0 text-sm">
              ¿Quieres{" "}
              {confirm.action === "start"
                ? "iniciar"
                : confirm.action === "complete"
                  ? "completar"
                  : "cancelar"}{" "}
              <strong>{confirm.name}</strong>?
            </p>
            <div className="flex justify-end gap-2">
              <button
                className="btn btn-light"
                onClick={() => setConfirm(null)}
              >
                Volver
              </button>
              <button
                className={`btn ${confirm.action === "cancel" ? "btn-danger" : "btn-primary"}`}
                disabled={transition.isPending}
                onClick={() =>
                  transition.mutate({ id: confirm.id, action: confirm.action })
                }
              >
                {transition.isPending ? <Spinner /> : null} Confirmar
              </button>
            </div>
          </div>
        </Modal>
      )}
    </>
  );
}
