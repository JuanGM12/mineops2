import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Boxes, Search } from "lucide-react";
import {
  Empty,
  ErrorBox,
  LoadingRows,
  Modal,
  PageTitle,
  Pagination,
} from "../../components/UI";
import { apiError, maintenanceApi } from "../../services/api";
import type { Equipment } from "../../types";
export function EquipmentPage() {
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("all");
  const [page, setPage] = useState(1);
  const [selected, setSelected] = useState<Equipment | null>(null);
  const query = useQuery({
    queryKey: ["equipment"],
    queryFn: maintenanceApi.equipment,
  });
  const data = useMemo(
    () =>
      query.data?.filter(
        (item) =>
          (status === "all" ||
            (status === "active" && item.isActive) ||
            (status === "inactive" && !item.isActive)) &&
          `${item.code} ${item.name}`
            .toLowerCase()
            .includes(search.toLowerCase()),
      ) ?? [],
    [query.data, search, status],
  );
  const perPage = 10;
  return (
    <>
      <PageTitle
        eyebrow="Activos mineros"
        title="Equipos"
        description="Consulta el inventario de equipos registrados en Maintenance."
      />
      <div className="mb-5 grid gap-4 sm:grid-cols-3">
        <div className="card p-5">
          <div className="mb-3 rounded-xl bg-[#eaf5f2] p-2.5 text-[#117b70] w-fit">
            <Boxes size={18} />
          </div>
          <p className="m-0 text-xs text-[#84939d]">Total registrados</p>
          <p className="m-0 mt-1 text-2xl font-extrabold">
            {query.data?.length ?? "—"}
          </p>
        </div>
        <div className="card p-5">
          <div className="mb-3 h-[38px] w-[38px] rounded-xl bg-[#e9f7ed] p-2.5 text-[#2b986b]">
            ●
          </div>
          <p className="m-0 text-xs text-[#84939d]">Activos</p>
          <p className="m-0 mt-1 text-2xl font-extrabold">
            {query.data?.filter((x) => x.isActive).length ?? "—"}
          </p>
        </div>
        <div className="card p-5">
          <div className="mb-3 h-[38px] w-[38px] rounded-xl bg-[#fff4ea] p-2.5 text-[#bc7d3a]">
            ●
          </div>
          <p className="m-0 text-xs text-[#84939d]">Inactivos</p>
          <p className="m-0 mt-1 text-2xl font-extrabold">
            {query.data?.filter((x) => !x.isActive).length ?? "—"}
          </p>
        </div>
      </div>
      <div className="card overflow-hidden">
        <div className="flex flex-col gap-3 p-5 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h2 className="m-0 text-base font-extrabold">
              Inventario de equipos
            </h2>
            <p className="mt-1 text-xs text-[#84939d]">
              Datos devueltos por Maintenance Service
            </p>
          </div>
          <div className="flex gap-2">
            <div className="relative flex-1">
              <Search
                size={16}
                className="absolute left-3 top-3 text-[#9bacb4]"
              />
              <input
                className="field w-full pl-9 sm:w-52"
                placeholder="Buscar equipo"
                value={search}
                onChange={(event) => {
                  setSearch(event.target.value);
                  setPage(1);
                }}
              />
            </div>
            <select
              className="field !w-auto"
              value={status}
              onChange={(event) => {
                setStatus(event.target.value);
                setPage(1);
              }}
            >
              <option value="all">Todos</option>
              <option value="active">Activos</option>
              <option value="inactive">Inactivos</option>
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
        ) : data.length ? (
          <>
            <div className="table-scroll">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Código</th>
                    <th>Equipo</th>
                    <th>Estado</th>
                    <th></th>
                  </tr>
                </thead>
                <tbody>
                  {data
                    .slice((page - 1) * perPage, page * perPage)
                    .map((item) => (
                      <tr key={item.id}>
                        <td className="font-bold text-[#117b70]">
                          {item.code}
                        </td>
                        <td className="font-semibold">{item.name}</td>
                        <td>
                          <span
                            className={`rounded-full px-2.5 py-1 text-[11px] font-bold ${item.isActive ? "bg-[#e4f6eb] text-[#25805a]" : "bg-[#f2f4f5] text-[#7d8d97]"}`}
                          >
                            {item.isActive ? "Activo" : "Inactivo"}
                          </span>
                        </td>
                        <td>
                          <button
                            className="text-xs font-bold text-[#117b70]"
                            onClick={() => setSelected(item)}
                          >
                            Ver detalle
                          </button>
                        </td>
                      </tr>
                    ))}
                </tbody>
              </table>
            </div>
            <Pagination
              page={page}
              pages={Math.ceil(data.length / perPage)}
              total={data.length}
              onChange={setPage}
            />
          </>
        ) : (
          <Empty />
        )}
      </div>
      {selected && (
        <Modal title="Detalle del equipo" onClose={() => setSelected(null)}>
          <div className="grid gap-5 p-6 sm:grid-cols-2">
            <div>
              <p className="label">Código</p>
              <p className="font-semibold">{selected.code}</p>
            </div>
            <div>
              <p className="label">Nombre</p>
              <p className="font-semibold">{selected.name}</p>
            </div>
            <div>
              <p className="label">Estado</p>
              <p className="font-semibold">
                {selected.isActive ? "Activo" : "Inactivo"}
              </p>
            </div>
            <div>
              <p className="label">Identificador</p>
              <p className="break-all text-xs">{selected.id}</p>
            </div>
          </div>
        </Modal>
      )}
    </>
  );
}
