import { useQuery } from "@tanstack/react-query";
import {
  Activity,
  Boxes,
  ClipboardList,
  Clock3,
  ArrowUpRight,
} from "lucide-react";
import {
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { Link } from "react-router-dom";
import {
  Badge,
  Empty,
  ErrorBox,
  LoadingRows,
  PageTitle,
  dateText,
} from "../../components/UI";
import {
  allPages,
  apiError,
  maintenanceApi,
  planningApi,
} from "../../services/api";
import type { Status } from "../../types";

const statusColors: Record<Status, string> = {
  Pending: "#e2ac53",
  InProgress: "#4a94c6",
  Completed: "#36a77b",
  Cancelled: "#d96b69",
};
const statusNames: Record<Status, string> = {
  Pending: "Pendientes",
  InProgress: "En ejecución",
  Completed: "Completadas",
  Cancelled: "Canceladas",
};
function Metric({
  label,
  value,
  icon: Icon,
  accent,
}: {
  label: string;
  value: number | undefined;
  icon: typeof Activity;
  accent: string;
}) {
  return (
    <div className="card p-5 sm:p-6">
      <div className="mb-5 flex items-center justify-between">
        <span className="text-[12px] font-semibold text-[#778996]">
          {label}
        </span>
        <span
          className="rounded-xl p-2.5"
          style={{ backgroundColor: `${accent}17`, color: accent }}
        >
          <Icon size={18} />
        </span>
      </div>
      {value === undefined ? (
        <div className="skeleton h-9 w-16" />
      ) : (
        <p className="m-0 text-[29px] font-extrabold tracking-tight text-[#12303c]">
          {value}
        </p>
      )}
    </div>
  );
}
export function Dashboard() {
  const activities = useQuery({
    queryKey: ["activities", "all"],
    queryFn: () => allPages(planningApi.page),
  });
  const orders = useQuery({
    queryKey: ["orders", "all"],
    queryFn: () => allPages(maintenanceApi.page),
  });
  const equipment = useQuery({
    queryKey: ["equipment"],
    queryFn: maintenanceApi.equipment,
  });
  const a = activities.data ?? [];
  const o = orders.data ?? [];
  const e = equipment.data ?? [];
  const activitySeries = (
    ["Pending", "InProgress", "Completed", "Cancelled"] as Status[]
  ).map((status) => ({
    name: statusNames[status],
    value: a.filter((item) => item.status === status).length,
    color: statusColors[status],
  }));
  const orderSeries = (
    ["Pending", "InProgress", "Completed", "Cancelled"] as Status[]
  ).map((status) => ({
    name: statusNames[status],
    value: o.filter((item) => item.status === status).length,
    color: statusColors[status],
  }));
  const typeSeries = [
    {
      name: "Preventivo",
      value: o.filter((item) => item.maintenanceType === "Preventive").length,
    },
    {
      name: "Correctivo",
      value: o.filter((item) => item.maintenanceType === "Corrective").length,
    },
  ];
  return (
    <>
      <PageTitle
        eyebrow="Vista general"
        title="Dashboard operacional"
        description="Indicadores en tiempo real a partir de Planning y Maintenance."
      />
      <div className="mb-7 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <Metric
          label="Actividades"
          value={activities.isSuccess ? a.length : undefined}
          icon={Activity}
          accent="#117b70"
        />
        <Metric
          label="Pendientes de iniciar"
          value={
            activities.isSuccess
              ? a.filter((x) => x.status === "Pending").length
              : undefined
          }
          icon={Clock3}
          accent="#c58d31"
        />
        <Metric
          label="Equipos registrados"
          value={equipment.isSuccess ? e.length : undefined}
          icon={Boxes}
          accent="#3984bb"
        />
        <Metric
          label="Órdenes de trabajo"
          value={orders.isSuccess ? o.length : undefined}
          icon={ClipboardList}
          accent="#5f6fad"
        />
      </div>
      {(activities.isError || orders.isError || equipment.isError) && (
        <div className="mb-6 space-y-2">
          {activities.isError && (
            <ErrorBox
              message={`Planning: ${apiError(activities.error)}`}
              retry={() => activities.refetch()}
            />
          )}
          {orders.isError && (
            <ErrorBox
              message={`Órdenes: ${apiError(orders.error)}`}
              retry={() => orders.refetch()}
            />
          )}
          {equipment.isError && (
            <ErrorBox
              message={`Equipos: ${apiError(equipment.error)}`}
              retry={() => equipment.refetch()}
            />
          )}
        </div>
      )}
      <div className="grid gap-5 xl:grid-cols-[1.35fr_1fr]">
        <div className="card p-6">
          <div className="mb-6">
            <h2 className="m-0 text-base font-extrabold">
              Actividades por estado
            </h2>
            <p className="mt-1 text-xs text-[#84939d]">
              Distribución de todas las actividades registradas
            </p>
          </div>
          {activities.isLoading ? (
            <div className="skeleton h-64" />
          ) : a.length ? (
            <div className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart
                  data={activitySeries}
                  margin={{ top: 10, right: 5, bottom: 5, left: -20 }}
                >
                  <CartesianGrid vertical={false} stroke="#eef2f3" />
                  <XAxis
                    dataKey="name"
                    tick={{ fontSize: 11, fill: "#82939c" }}
                    axisLine={false}
                    tickLine={false}
                  />
                  <YAxis
                    allowDecimals={false}
                    tick={{ fontSize: 11, fill: "#82939c" }}
                    axisLine={false}
                    tickLine={false}
                  />
                  <Tooltip cursor={{ fill: "#f5faf9" }} />
                  <Bar dataKey="value" radius={[6, 6, 0, 0]} maxBarSize={54}>
                    {activitySeries.map((item) => (
                      <Cell key={item.name} fill={item.color} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </div>
          ) : (
            <Empty text="Crea una actividad para ver la distribución." />
          )}
        </div>
        <div className="card p-6">
          <h2 className="m-0 text-base font-extrabold">
            Mantenimiento por tipo
          </h2>
          <p className="mt-1 text-xs text-[#84939d]">
            Órdenes preventivas y correctivas
          </p>
          {orders.isLoading ? (
            <div className="skeleton mt-6 h-64" />
          ) : o.length ? (
            <div className="h-64">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={typeSeries}
                    dataKey="value"
                    nameKey="name"
                    innerRadius={65}
                    outerRadius={95}
                    paddingAngle={4}
                  >
                    {typeSeries.map((item, index) => (
                      <Cell
                        key={item.name}
                        fill={index === 0 ? "#117b70" : "#e2ac53"}
                      />
                    ))}
                  </Pie>
                  <Tooltip />
                </PieChart>
              </ResponsiveContainer>
              <div className="-mt-2 flex justify-center gap-5 text-xs">
                <span>🟢 Preventivo {typeSeries[0].value}</span>
                <span>🟠 Correctivo {typeSeries[1].value}</span>
              </div>
            </div>
          ) : (
            <Empty text="Crea una orden para ver el desglose." />
          )}
        </div>
      </div>
      <div className="mt-5 grid gap-5 xl:grid-cols-2">
        <div className="card overflow-hidden">
          <div className="flex items-center justify-between p-6">
            <div>
              <h2 className="m-0 text-base font-extrabold">
                Actividades recientes
              </h2>
              <p className="mt-1 text-xs text-[#84939d]">
                Últimos movimientos de planeación
              </p>
            </div>
            <Link
              to="/activities"
              className="flex items-center gap-1 text-xs font-bold text-[#117b70] no-underline"
            >
              Ver todas <ArrowUpRight size={15} />
            </Link>
          </div>
          {activities.isLoading ? (
            <LoadingRows />
          ) : a.length ? (
            <div className="divide-y divide-[#eef2f3]">
              {a.slice(0, 4).map((item) => (
                <div
                  key={item.id}
                  className="flex items-center justify-between gap-3 px-6 py-4"
                >
                  <div className="min-w-0">
                    <p className="m-0 truncate text-sm font-bold">
                      {item.title}
                    </p>
                    <p className="m-0 mt-1 text-xs text-[#84939d]">
                      {item.code} · {dateText(item.scheduledDate)}
                    </p>
                  </div>
                  <Badge status={item.status} />
                </div>
              ))}
            </div>
          ) : (
            <Empty />
          )}
        </div>
        <div className="card overflow-hidden">
          <div className="flex items-center justify-between p-6">
            <div>
              <h2 className="m-0 text-base font-extrabold">
                Órdenes recientes
              </h2>
              <p className="mt-1 text-xs text-[#84939d]">
                Seguimiento del mantenimiento
              </p>
            </div>
            <Link
              to="/work-orders"
              className="flex items-center gap-1 text-xs font-bold text-[#117b70] no-underline"
            >
              Ver todas <ArrowUpRight size={15} />
            </Link>
          </div>
          {orders.isLoading ? (
            <LoadingRows />
          ) : o.length ? (
            <div className="divide-y divide-[#eef2f3]">
              {o.slice(0, 4).map((item) => (
                <div
                  key={item.id}
                  className="flex items-center justify-between gap-3 px-6 py-4"
                >
                  <div className="min-w-0">
                    <p className="m-0 truncate text-sm font-bold">
                      {item.title}
                    </p>
                    <p className="m-0 mt-1 text-xs text-[#84939d]">
                      {item.code} · {dateText(item.scheduledDate)}
                    </p>
                  </div>
                  <Badge status={item.status} />
                </div>
              ))}
            </div>
          ) : (
            <Empty />
          )}
        </div>
      </div>
      <div className="card mt-5 p-6">
        <h2 className="m-0 text-base font-extrabold">Órdenes por estado</h2>
        <p className="mt-1 text-xs text-[#84939d]">
          Conteos calculados con las órdenes disponibles
        </p>
        {orders.isLoading ? (
          <div className="skeleton mt-5 h-24" />
        ) : o.length ? (
          <div className="mt-6 grid grid-cols-2 gap-3 sm:grid-cols-4">
            {orderSeries.map((item) => (
              <div key={item.name} className="rounded-xl bg-[#f8fafb] p-4">
                <span className="text-xs text-[#7d8e99]">{item.name}</span>
                <p
                  className="mb-0 mt-2 text-2xl font-extrabold"
                  style={{ color: item.color }}
                >
                  {item.value}
                </p>
              </div>
            ))}
          </div>
        ) : (
          <Empty />
        )}
      </div>
    </>
  );
}
