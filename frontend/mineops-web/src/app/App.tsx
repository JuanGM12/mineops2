import {
  createContext,
  lazy,
  Suspense,
  useContext,
  useState,
  type ReactNode,
} from "react";
import {
  Activity as ActivityIcon,
  Bell,
  Boxes,
  ChevronDown,
  LayoutDashboard,
  LogOut,
  Menu,
  Mountain,
  Wrench,
  X,
} from "lucide-react";
import {
  Link,
  Navigate,
  NavLink,
  Route,
  Routes,
  useLocation,
} from "react-router-dom";
import { useAuth } from "../features/auth/AuthProvider";
const Dashboard = lazy(() =>
  import("../features/dashboard/Dashboard").then((module) => ({
    default: module.Dashboard,
  })),
);
const Activities = lazy(() =>
  import("../features/planning/Activities").then((module) => ({
    default: module.Activities,
  })),
);
const EquipmentPage = lazy(() =>
  import("../features/maintenance/EquipmentPage").then((module) => ({
    default: module.EquipmentPage,
  })),
);
const WorkOrders = lazy(() =>
  import("../features/maintenance/WorkOrders").then((module) => ({
    default: module.WorkOrders,
  })),
);
import { apiError } from "../services/api";

type Toast = { text: string; kind: "success" | "error" };
const ToastContext = createContext<
  (text: string, kind?: Toast["kind"]) => void
>(() => {});
export const useToast = () => useContext(ToastContext);
function Login() {
  const { user, login } = useAuth();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [show, setShow] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  if (user) return <Navigate to="/" replace />;
  const submit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError("");
    setLoading(true);
    try {
      await login(email.trim(), password);
    } catch (err) {
      setError(apiError(err));
    } finally {
      setLoading(false);
    }
  };
  return (
    <div className="min-h-screen bg-[#0b2230] lg:grid lg:grid-cols-[1fr_1fr]">
      <div className="relative hidden overflow-hidden bg-[#0d3040] p-14 text-white lg:flex lg:flex-col lg:justify-between">
        <div
          className="absolute inset-0 opacity-30"
          style={{
            backgroundImage:
              "radial-gradient(circle at 80% 15%,#247f78 0,transparent 34%),linear-gradient(140deg,transparent 60%,#07212e 60.3%)",
          }}
        />
        <div className="relative flex items-center gap-3 text-xl font-extrabold">
          <span className="rounded-xl bg-[#117b70] p-2">
            <Mountain size={24} />
          </span>
          MineOps
        </div>
        <div className="relative max-w-xl">
          <div className="mb-8 h-1 w-14 rounded bg-[#28b5a4]" />
          <p className="mb-3 text-xs font-bold uppercase tracking-[.25em] text-[#77c8bf]">
            Centro de operaciones mineras
          </p>
          <h1 className="text-5xl font-extrabold leading-[1.13]">
            Visibilidad para cada operación. Control en cada decisión.
          </h1>
          <p className="mt-6 max-w-md text-base leading-7 text-[#a9c1ca]">
            Planeación, equipos y mantenimiento conectados en un solo espacio de
            trabajo.
          </p>
        </div>
        <p className="relative text-xs text-[#83a5ae]">
          MineOps · Plataforma académica de gestión operacional
        </p>
      </div>
      <div className="flex min-h-screen items-center justify-center bg-[#f7f9f9] px-5 py-10">
        <div className="w-full max-w-[440px]">
          <div className="mb-10 flex items-center gap-3 text-xl font-extrabold text-[#102935] lg:hidden">
            <span className="rounded-xl bg-[#117b70] p-2 text-white">
              <Mountain size={22} />
            </span>
            MineOps
          </div>
          <div className="mb-8">
            <p className="mb-2 text-xs font-extrabold uppercase tracking-[.2em] text-[#117b70]">
              Bienvenido
            </p>
            <h2 className="m-0 text-[32px] font-extrabold text-[#102935]">
              Inicia sesión
            </h2>
            <p className="mt-2 text-sm text-[#758793]">
              Accede a tu centro de operaciones.
            </p>
          </div>
          <form onSubmit={submit} className="card space-y-5 p-7 sm:p-9">
            <label className="block">
              <span className="label">Correo electrónico</span>
              <input
                className="field"
                type="email"
                autoComplete="username"
                placeholder="usuario@mineops.local"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </label>
            <label className="block">
              <span className="label">Contraseña</span>
              <div className="relative">
                <input
                  className="field pr-20"
                  type={show ? "text" : "password"}
                  autoComplete="current-password"
                  placeholder="Ingresa tu contraseña"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
                <button
                  type="button"
                  className="absolute right-3 top-2.5 text-xs font-bold text-[#117b70]"
                  onClick={() => setShow(!show)}
                >
                  {show ? "Ocultar" : "Mostrar"}
                </button>
              </div>
            </label>
            {error && (
              <div
                role="alert"
                className="rounded-lg bg-[#fff0ee] p-3 text-sm text-[#a94440]"
              >
                {error}
              </div>
            )}
            <button disabled={loading} className="btn btn-primary w-full !py-3">
              {loading ? "Ingresando…" : "Ingresar al sistema"}
            </button>
          </form>
          <p className="mt-7 text-center text-xs text-[#9daab2]">
            Acceso protegido mediante Identity Service y JWT
          </p>
        </div>
      </div>
    </div>
  );
}
const nav = [
  { to: "/", label: "Dashboard", icon: LayoutDashboard },
  { to: "/activities", label: "Actividades", icon: ActivityIcon },
  { to: "/equipment", label: "Equipos", icon: Boxes },
  { to: "/work-orders", label: "Órdenes de mantenimiento", icon: Wrench },
];
function Shell({ children }: { children: ReactNode }) {
  const { user, logout } = useAuth();
  const [open, setOpen] = useState(false);
  const location = useLocation();
  const title =
    nav.find((item) => item.to === location.pathname)?.label ?? "MineOps";
  return (
    <div className="min-h-screen lg:flex">
      <aside
        className={`${open ? "translate-x-0" : "-translate-x-full"} fixed inset-y-0 left-0 z-40 flex w-[260px] flex-col bg-[#0c2635] text-white transition-transform lg:sticky lg:top-0 lg:h-screen lg:translate-x-0`}
      >
        <div className="flex h-20 items-center justify-between border-b border-white/10 px-6">
          <Link
            to="/"
            className="flex items-center gap-3 text-xl font-extrabold text-white no-underline"
            onClick={() => setOpen(false)}
          >
            <span className="rounded-xl bg-[#117b70] p-2">
              <Mountain size={22} />
            </span>
            MineOps
          </Link>
          <button
            className="lg:hidden"
            aria-label="Cerrar menú"
            onClick={() => setOpen(false)}
          >
            <X size={21} />
          </button>
        </div>
        <div className="px-5 pt-8">
          <p className="px-3 text-[10px] font-extrabold uppercase tracking-[.2em] text-[#6e929e]">
            Espacio de trabajo
          </p>
          <nav className="mt-4 space-y-1">
            {nav.map(({ to, label, icon: Icon }) => (
              <NavLink
                key={to}
                end={to === "/"}
                to={to}
                onClick={() => setOpen(false)}
                className={({ isActive }) =>
                  `flex items-center gap-3 rounded-xl px-3 py-3 text-[13px] font-semibold no-underline transition ${isActive ? "bg-[#145265] text-white" : "text-[#a7bdc5] hover:bg-white/5 hover:text-white"}`
                }
              >
                <Icon size={18} />
                {label}
              </NavLink>
            ))}
          </nav>
        </div>
        <div className="mt-auto border-t border-white/10 p-5">
          <div className="mb-4 flex items-center gap-3 px-1">
            <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-[#1d6370] text-xs font-bold">
              {user?.name?.slice(0, 1) || "U"}
            </div>
            <div className="min-w-0">
              <p className="m-0 truncate text-xs font-bold">{user?.name}</p>
              <p className="m-0 truncate text-[11px] text-[#93b1bc]">
                {user?.roles.join(", ")}
              </p>
            </div>
          </div>
          <button
            onClick={logout}
            className="flex w-full items-center gap-3 rounded-xl px-3 py-2 text-left text-xs font-semibold text-[#b3c9d0] hover:bg-white/5"
          >
            <LogOut size={16} /> Cerrar sesión
          </button>
        </div>
      </aside>
      {open && (
        <button
          className="fixed inset-0 z-30 bg-[#0819238c] lg:hidden"
          aria-label="Cerrar menú"
          onClick={() => setOpen(false)}
        />
      )}
      <div className="min-w-0 flex-1">
        <header className="sticky top-0 z-20 flex h-20 items-center justify-between border-b border-[#e6edef] bg-white/95 px-5 backdrop-blur sm:px-8">
          <div className="flex items-center gap-4">
            <button
              className="lg:hidden"
              aria-label="Abrir menú"
              onClick={() => setOpen(true)}
            >
              <Menu size={22} />
            </button>
            <div>
              <p className="m-0 text-[11px] font-semibold uppercase tracking-[.12em] text-[#9aa9b0]">
                Operaciones / {title}
              </p>
              <p className="m-0 mt-1 text-sm font-extrabold text-[#17313e]">
                Centro de operaciones
              </p>
            </div>
          </div>
          <div className="flex items-center gap-4">
            <div className="hidden items-center gap-2 rounded-full bg-[#e7f5f0] px-3 py-1.5 text-[11px] font-bold text-[#267859] sm:flex">
              <span className="h-2 w-2 rounded-full bg-[#35aa72]" /> Sesión
              activa
            </div>
            <Bell size={18} className="text-[#8093a0]" />
            <span className="hidden text-xs font-bold text-[#344a57] sm:inline">
              {user?.name}
              <ChevronDown size={13} className="ml-1 inline" />
            </span>
          </div>
        </header>
        <main className="mx-auto max-w-[1540px] p-5 pb-12 sm:p-8">
          {children}
        </main>
      </div>
    </div>
  );
}
function Protected({ children }: { children: ReactNode }) {
  const { user } = useAuth();
  return user ? <Shell>{children}</Shell> : <Navigate to="/login" replace />;
}
export function App() {
  const [toast, setToast] = useState<Toast | null>(null);
  const notify = (text: string, kind: Toast["kind"] = "success") => {
    setToast({ text, kind });
    window.setTimeout(() => setToast(null), 4500);
  };
  return (
    <ToastContext.Provider value={notify}>
      <Suspense
        fallback={
          <div className="mx-auto max-w-xl p-8 text-sm text-[#748692]">
            Cargando módulo…
          </div>
        }
      >
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route
            path="/"
            element={
              <Protected>
                <Dashboard />
              </Protected>
            }
          />
          <Route
            path="/activities"
            element={
              <Protected>
                <Activities />
              </Protected>
            }
          />
          <Route
            path="/equipment"
            element={
              <Protected>
                <EquipmentPage />
              </Protected>
            }
          />
          <Route
            path="/work-orders"
            element={
              <Protected>
                <WorkOrders />
              </Protected>
            }
          />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Suspense>
      {toast && (
        <div
          role="status"
          className={`fixed bottom-5 right-5 z-[60] max-w-sm rounded-xl px-5 py-4 text-sm font-semibold text-white shadow-xl ${toast.kind === "success" ? "bg-[#117b70]" : "bg-[#b34d4c]"}`}
        >
          {toast.text}
        </div>
      )}
    </ToastContext.Provider>
  );
}
