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
} from "../../components/UI";
import { allPages, apiError, maintenanceApi } from "../../services/api";
import type {
  MaintenanceType,
  Priority,
  Status,
  WorkOrderInput,
} from "../../types";
import { useAuth } from "../auth/AuthProvider";
import { useToast } from "../../app/App";
const emptyForm: WorkOrderInput = {
  code: "",
  equipmentId: "",
  title: "",
  description: null,
  maintenanceType: "Preventive",
  priority: "Medium",
  scheduledDate: null,
};
const priorities: Priority[] = ["Low", "Medium", "High", "Critical"];
const statuses: Status[] = ["Pending", "InProgress", "Completed", "Cancelled"];
export function WorkOrders() {
  const { can } = useAuth(),
    notify = useToast(),
    client = useQueryClient();
  const [search, setSearch] = useState(""),
    [status, setStatus] = useState("all"),
    [type, setType] = useState("all"),
    [page, setPage] = useState(1);
  const [form, setForm] = useState<WorkOrderInput | null>(null),
    [detail, setDetail] = useState<string | null>(null),
    [confirm, setConfirm] = useState<{
      id: string;
      action: "start" | "complete" | "cancel";
      name: string;
    } | null>(null),
    [formError, setFormError] = useState("");
  const orders = useQuery({
    queryKey: ["orders", "all"],
    queryFn: () => allPages(maintenanceApi.page),
  });
  const equipment = useQuery({
    queryKey: ["equipment"],
    queryFn: maintenanceApi.equipment,
  });
  const detailQuery = useQuery({
    queryKey: ["order", detail],
    queryFn: () => maintenanceApi.get(detail!),
    enabled: !!detail,
  });
  const create = useMutation({
    mutationFn: (value: WorkOrderInput) =>
      maintenanceApi.create({
        ...value,
        scheduledDate: value.scheduledDate
          ? new Date(value.scheduledDate).toISOString()
          : null,
      }),
    onSuccess: () => {
      client.invalidateQueries({ queryKey: ["orders"] });
      setForm(null);
      notify("Orden de mantenimiento creada");
    },
    onError: (error) => setFormError(apiError(error)),
  });
  const transition = useMutation({
    mutationFn: ({
      id,
      action,
    }: {
      id: string;
      action: "start" | "complete" | "cancel";
    }) => maintenanceApi.transition(id, action),
    onSuccess: () => {
      client.invalidateQueries({ queryKey: ["orders"] });
      client.invalidateQueries({ queryKey: ["order"] });
      setConfirm(null);
      notify("Estado de la orden actualizado");
    },
    onError: (error) => {
      notify(apiError(error), "error");
      setConfirm(null);
    },
  });
  const equipmentMap = useMemo(
    () => new Map(equipment.data?.map((item) => [item.id, item]) ?? []),
    [equipment.data],
  );
  const rows = useMemo(
    () =>
      orders.data?.filter(
        (x) =>
          (status === "all" || x.status === status) &&
          (type === "all" || x.maintenanceType === type) &&
          `${x.code} ${x.title} ${x.description ?? ""} ${equipmentMap.get(x.equipmentId)?.name ?? ""} ${equipmentMap.get(x.equipmentId)?.code ?? ""}`
            .toLowerCase()
            .includes(search.toLowerCase()),
      ) ?? [],
    [orders.data, status, type, search, equipmentMap],
  );
  const perPage = 10;
  const change = <K extends keyof WorkOrderInput>(
    key: K,
    value: WorkOrderInput[K],
  ) => setForm((current) => (current ? { ...current, [key]: value } : current));
  const submit = (event: FormEvent) => {
    event.preventDefault();
    if (!form) return;
    if (form.maintenanceType === "Preventive" && !form.scheduledDate) {
      setFormError("El mantenimiento preventivo requiere fecha programada.");
      return;
    }
    create.mutate(form);
  };
  return (
    <>
      <PageTitle
        eyebrow="Mantenimiento"
        title="Órdenes de trabajo"
        description="Programa y da seguimiento al mantenimiento preventivo y correctivo."
        action={
          can("Administrator", "MaintenanceSupervisor") ? (
            <button
              className="btn btn-primary"
              onClick={() => {
                setForm({ ...emptyForm });
                setFormError("");
              }}
            >
              <Plus size={17} /> Nueva orden
            </button>
          ) : undefined
        }
      />
      <div className="card overflow-hidden">
        <div className="flex flex-col gap-4 p-5 xl:flex-row xl:items-center xl:justify-between">
          <div>
            <h2 className="m-0 text-base font-extrabold">
              Historial de órdenes
            </h2>
            <p className="mt-1 text-xs text-[#84939d]">
              {orders.data?.length ?? "—"} registros en Maintenance Service
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <div className="relative flex-1">
              <Search
                size={16}
                className="absolute left-3 top-3 text-[#9bacb4]"
              />
              <input
                className="field min-w-[170px] pl-9"
                placeholder="Buscar orden"
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
              value={type}
              onChange={(e) => {
                setType(e.target.value);
                setPage(1);
              }}
            >
              <option value="all">Todos los tipos</option>
              <option value="Preventive">Preventivo</option>
              <option value="Corrective">Correctivo</option>
            </select>
          </div>
        </div>
        {orders.isError ? (
          <div className="p-5">
            <ErrorBox
              message={apiError(orders.error)}
              retry={() => orders.refetch()}
            />
          </div>
        ) : orders.isLoading ? (
          <LoadingRows />
        ) : rows.length ? (
          <>
            <div className="table-scroll">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Orden</th>
                    <th>Equipo</th>
                    <th>Tipo</th>
                    <th>Prioridad</th>
                    <th>Programada</th>
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
                            onClick={() => setDetail(item.id)}
                            className="text-left"
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
                          {equipmentMap.get(item.equipmentId)?.name ??
                            item.equipmentId.slice(0, 8)}
                          <span className="block text-xs text-[#92a0a8]">
                            {equipmentMap.get(item.equipmentId)?.code ?? ""}
                          </span>
                        </td>
                        <td>
                          {item.maintenanceType === "Preventive"
                            ? "Preventivo"
                            : "Correctivo"}
                        </td>
                        <td>
                          <PriorityText priority={item.priority} />
                        </td>
                        <td>{dateText(item.scheduledDate)}</td>
                        <td>
                          <Badge status={item.status} />
                        </td>
                        <td>
                          <div className="flex flex-wrap gap-2 text-xs font-bold text-[#117b70]">
                            <button onClick={() => setDetail(item.id)}>
                              Detalle
                            </button>
                            {item.status === "Pending" &&
                              can("Administrator", "MaintenanceSupervisor") && (
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
                              can("Administrator", "MaintenanceSupervisor") && (
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
                              can("Administrator", "MaintenanceSupervisor") && (
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
          title="Nueva orden de mantenimiento"
          onClose={() => setForm(null)}
        >
          <form onSubmit={submit} className="space-y-5 p-6">
            <div className="grid gap-4 sm:grid-cols-2">
              <Field label="Código OT *">
                <input
                  className="field"
                  required
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
              <Field label="Equipo *">
                <select
                  className="field"
                  required
                  value={form.equipmentId}
                  onChange={(e) => change("equipmentId", e.target.value)}
                >
                  <option value="">Selecciona un equipo</option>
                  {equipment.data
                    ?.filter((item) => item.isActive)
                    .map((item) => (
                      <option key={item.id} value={item.id}>
                        {item.code} · {item.name}
                      </option>
                    ))}
                </select>
                {equipment.isError && (
                  <span className="mt-1 block text-xs text-[#b34d4c]">
                    {apiError(equipment.error)}
                  </span>
                )}
              </Field>
              <Field label="Tipo de mantenimiento">
                <select
                  className="field"
                  value={form.maintenanceType}
                  onChange={(e) =>
                    change("maintenanceType", e.target.value as MaintenanceType)
                  }
                >
                  <option value="Preventive">Preventivo</option>
                  <option value="Corrective">Correctivo</option>
                </select>
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
              <Field
                label={
                  form.maintenanceType === "Preventive"
                    ? "Fecha programada *"
                    : "Fecha programada"
                }
              >
                <input
                  className="field"
                  type="datetime-local"
                  required={form.maintenanceType === "Preventive"}
                  value={form.scheduledDate ?? ""}
                  onChange={(e) =>
                    change("scheduledDate", e.target.value || null)
                  }
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
              <button
                className="btn btn-primary"
                disabled={
                  create.isPending ||
                  equipment.isLoading ||
                  !equipment.data?.some((item) => item.isActive)
                }
              >
                {create.isPending ? <Spinner /> : null} Crear orden
              </button>
            </div>
          </form>
        </Modal>
      )}
      {detail && (
        <Modal title="Detalle de orden" onClose={() => setDetail(null)}>
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
                    [
                      "Equipo",
                      equipmentMap.get(detailQuery.data.equipmentId)?.name ??
                        detailQuery.data.equipmentId,
                    ],
                    [
                      "Tipo",
                      detailQuery.data.maintenanceType === "Preventive"
                        ? "Preventivo"
                        : "Correctivo",
                    ],
                    ["Prioridad", detailQuery.data.priority],
                    ["Programada", dateText(detailQuery.data.scheduledDate)],
                    ["Iniciada", dateText(detailQuery.data.startedAt)],
                    ["Completada", dateText(detailQuery.data.completedAt)],
                    ["Descripción", detailQuery.data.description ?? "—"],
                  ] as [string, string][]
                ).map(([label, value]) => (
                  <div key={label}>
                    <p className="label">{label}</p>
                    <p className="m-0 break-words text-sm font-semibold">
                      {value}
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
