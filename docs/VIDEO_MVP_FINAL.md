# Guion de demostración del MVP MineOps

> Guion para cuatro integrantes. Ejecutar las tres API y `frontend/mineops-web` antes de grabar. Usar registros reales creados durante la sesión. No mostrar valores de JWT ni contraseñas en pantalla.

## Preparación

1. Confirmar SQL Server, migraciones y `GET /health` en puertos 5001, 5002 y 5003.
2. Iniciar Identity, Planning, Maintenance y Vite. Abrir `http://localhost:5173`.
3. Tener a mano los usuarios de demostración del README y un código nuevo para actividad y orden.
4. En el navegador, abrir DevTools > Network con filtro Fetch/XHR. En las API, habilitar logs de requests HTTP si se va a mostrar tráfico.

## Integrante 1 · Introducción, arquitectura y login

- Presentar MineOps como MVP de planeación y mantenimiento con identidad centralizada.
- Mostrar brevemente la arquitectura: React/Vite → Identity (5001), Planning (5002), Maintenance (5003); cada API tiene su propia base SQL Server.
- En la pantalla de login, entrar con `admin@mineops.local`. Mostrar en Network `POST /identity/api/auth/login` y su código 200 sin revelar el token.
- Explicar que el JWT queda solo en memoria del navegador; al recargar se requiere iniciar sesión otra vez.

## Integrante 2 · Dashboard y planeación

- Mostrar los indicadores y gráficos calculados a partir de las respuestas reales. Si aún no hay actividades, mostrar el estado vacío y crear una.
- En Actividades, buscar y filtrar. Crear una actividad con código único, título, área, ubicación, prioridad y fechas. Mostrar su fila y detalle.
- Editar un campo, iniciar y completar la actividad. Verificar que las métricas del dashboard cambien al volver.
- Mencionar que el responsable se representa por UUID porque el backend no expone un directorio de usuarios.

## Integrante 3 · Equipos y mantenimiento

- En Equipos, mostrar los registros creados por el seeder y sus estados. Buscar y abrir el detalle.
- En Órdenes, crear una orden preventiva para un equipo activo y fecha programada. Crear otra correctiva si el tiempo alcanza.
- Iniciar y completar una orden. Mostrar los filtros por estado y tipo y regresar al dashboard para ver los cambios.

## Integrante 4 · Integración, seguridad y cierre

- **Estado actual:** el repositorio no contiene comunicación HTTP Planning → Maintenance. El navegador consulta cada servicio por separado mediante el proxy de Vite. Eso no cumple el requisito de integración síncrona entre microservicios. No presentar esa secuencia como si ya existiera.
- Si se aprueba e implementa el cambio descrito en el README, crear desde la interfaz una actividad asociada a un equipo y mostrar en logs correlacionados la llamada de Planning a Maintenance (`GET /api/equipment/{id}`), el 200 de Maintenance y la respuesta final de Planning. Network solo muestra la solicitud navegador → Planning; la llamada interna debe verificarse en logs de servidor.
- Mostrar seguridad: cerrar sesión e intentar una ruta protegida para ver el retorno al login. Entrar como Planner e intentar crear una orden mediante DevTools/Swagger: el backend responde 403. Una solicitud sin token a Planning responde 401. Evitar mostrar tokens completos en el video.
- Cerrar con las funcionalidades entregadas y las limitaciones actuales.

## Verificación de evidencia

- Una secuencia `React → Planning → Maintenance → Planning → React` solo podrá grabarse una vez exista el endpoint de consulta por ID en Maintenance y la llamada HTTP desde Planning. Registrar hora, ruta, código de estado y un `traceId` compartido en ambos logs.
- Los filtros, conteos y gráficas del frontend se calculan sobre páginas reales de las API. Se pueden contrastar con Swagger.
