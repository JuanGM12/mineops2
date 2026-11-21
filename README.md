# MineOps
# Integrantes
Juan Diego Patiño Osorio
Jose Ricardo Quirós García
Anderson Arley Cano Osorio

## Segunda entrega

MineOps es una solución académica de tres microservicios ASP.NET Core 10 para planeación de actividades mineras, mantenimiento de equipos e identidad centralizada. Esta entrega demuestra con middleware estándar que autenticación y autorización son controles diferentes:

```text
Sin token, token inválido o token expirado      -> 401 Unauthorized
Token válido sin el rol requerido               -> 403 Forbidden
Token válido con el rol requerido               -> acceso permitido
```

La solución usa SQL Server local, no requiere Docker y se reproduce con `dotnet restore`, migraciones de EF Core y tres procesos `dotnet run`.

## Arquitectura

Planning y Maintenance siguen Clean Architecture:

```text
Api -> Infrastructure -> Application -> Domain
Api --------------------> Application
```

- `Domain`: entidades, invariantes y transiciones de estado.
- `Application`: casos de uso CQRS, contratos, repositorios y Unit of Work.
- `Infrastructure`: EF Core, SQL Server, repositorios, migraciones y seeders.
- `Api`: controladores, middleware, JWT, Swagger y health checks.

Identity usa `Api -> Infrastructure -> Application`. No incluye un proyecto Domain artificial porque ASP.NET Core Identity proporciona el modelo de identidad. Cada servicio es propietario de su `DbContext`, sus tablas, su migración y su base de datos. Planning y Maintenance validan localmente el JWT firmado; no consultan Identity en cada request.

## Microservicios

### Identity Service

- URL: `http://localhost:5001`
- Base: `MineOpsIdentity`
- Responsabilidad: usuarios, roles, contraseñas, login y emisión de JWT.
- Persistencia: ASP.NET Core Identity con EF Core SQL Server.
- Seeder: cuatro roles, cuatro usuarios y una relación usuario-rol para cada usuario.

### Planning Service

- URL: `http://localhost:5002`
- Base: `MineOpsPlanning`
- Responsabilidad: crear, consultar, actualizar, iniciar, completar y cancelar actividades.
- Arquitectura: Domain, Application, Infrastructure y API.
- Seguridad: todos los endpoints de actividades requieren JWT; las escrituras usan roles.

### Maintenance Service

- URL: `http://localhost:5003`
- Base: `MineOpsMaintenance`
- Responsabilidad: equipos y órdenes de mantenimiento.
- Arquitectura: Domain, Application, Infrastructure y API.
- Seeder: `EXC-001` y `TRK-001`, idempotente.
- Seguridad: las consultas requieren JWT y las operaciones sobre órdenes requieren `Administrator` o `MaintenanceSupervisor`.

## Tecnologías

- .NET SDK 10 / ASP.NET Core 10.
- C# con nullable reference types.
- Entity Framework Core SQL Server 10.0.15.
- ASP.NET Core Identity 10.0.15.
- JWT Bearer 10.0.15 y `System.IdentityModel.Tokens.Jwt` 8.14.0.
- MediatR 12.5.0.
- Swashbuckle 10.3.1.
- xUnit 2.9.3 y `Microsoft.AspNetCore.Mvc.Testing` 10.0.15.

## Requisitos

- Windows con SQL Server accesible como `localhost`.
- Autenticación integrada habilitada para el usuario que ejecuta los comandos.
- .NET SDK compatible con `global.json`; la auditoría se ejecutó con `10.0.401`.
- Herramienta EF Core 10.0.15.

```powershell
dotnet tool install --global dotnet-ef --version 10.0.15
dotnet restore MineOps.sln
```

Si `dotnet-ef` ya está instalada:

```powershell
dotnet tool update --global dotnet-ef --version 10.0.15
```

## Configuración SQL Server

Development usa tres bases independientes:

```text
Server=localhost;Database=MineOpsIdentity;Integrated Security=true;TrustServerCertificate=true
Server=localhost;Database=MineOpsPlanning;Integrated Security=true;TrustServerCertificate=true
Server=localhost;Database=MineOpsMaintenance;Integrated Security=true;TrustServerCertificate=true
```

Las cadenas académicas están en los `appsettings.Development.json` de cada API. Para otro equipo o producción no versione credenciales reales. Use variables de entorno:

```powershell
$env:ConnectionStrings__Identity = "Server=SERVIDOR;Database=MineOpsIdentity;Integrated Security=true;TrustServerCertificate=true"
$env:ConnectionStrings__Planning = "Server=SERVIDOR;Database=MineOpsPlanning;Integrated Security=true;TrustServerCertificate=true"
$env:ConnectionStrings__Maintenance = "Server=SERVIDOR;Database=MineOpsMaintenance;Integrated Security=true;TrustServerCertificate=true"
```

También puede usar User Secrets desde cada proyecto API. Las tres bases pueden estar en una misma instancia, pero ningún servicio comparte tablas ni `DbContext`.

## Configuración JWT

Los tres servicios comparten en Development:

```json
{
  "Jwt": {
    "Issuer": "MineOps.Identity",
    "Audience": "MineOps.Api",
    "SigningKey": "clave-academica-de-al-menos-32-bytes",
    "ExpirationMinutes": 60
  }
}
```

La clave versionada es exclusivamente académica y local. Nunca debe reutilizarse en producción. Configure una clave aleatoria mediante User Secrets o variables de entorno, con el mismo valor en las tres API:

```powershell
$env:Jwt__Issuer = "MineOps.Identity"
$env:Jwt__Audience = "MineOps.Api"
$env:Jwt__SigningKey = "UNA-CLAVE-ALEATORIA-LARGA-Y-SECRETA-DE-PRODUCCION"
$env:Jwt__ExpirationMinutes = "60"
```

Planning y Maintenance validan firma, issuer, audience, lifetime y signing key, con `ClockSkew = 0`. Una configuración JWT incompleta o una clave de menos de 32 bytes impide el arranque.

## Migraciones

Cada comando usa el proyecto Infrastructure que contiene la migración y su API como startup project.

Identity (`20261120151518_InitialIdentity`):

```powershell
dotnet ef database update --project src/Services/Identity/MineOps.Identity.Infrastructure --startup-project src/Services/Identity/MineOps.Identity.Api
```

Planning (`20261120145630_InitialPlanning`):

```powershell
dotnet ef database update --project src/Services/Planning/MineOps.Planning.Infrastructure --startup-project src/Services/Planning/MineOps.Planning.Api
```

Maintenance (`20261120153836_InitialMaintenance`):

```powershell
dotnet ef database update --project src/Services/Maintenance/MineOps.Maintenance.Infrastructure --startup-project src/Services/Maintenance/MineOps.Maintenance.Api
```

La auditoría aplicó las tres migraciones desde cero sobre bases temporales independientes y luego las eliminó de forma segura.

## Seeders

- Identity ejecuta su seeder al arrancar. Crea roles, usuarios y relaciones con `RoleManager` y `UserManager`; las contraseñas se almacenan como hashes de ASP.NET Core Identity.
- Maintenance crea al arrancar los equipos `EXC-001` y `TRK-001`.
- Planning no requiere datos iniciales.

Ambos seeders son idempotentes: reiniciar las API no duplica datos.

## Usuarios de demostración

Estas credenciales existen solo en Development y pruebas académicas:

| Correo | Rol | Contraseña |
| --- | --- | --- |
| `admin@mineops.local` | `Administrator` | `MineOps2026!` |
| `planner@mineops.local` | `Planner` | `MineOps2026!` |
| `maintenance@mineops.local` | `MaintenanceSupervisor` | `MineOps2026!` |
| `operator@mineops.local` | `Operator` | `MineOps2026!` |

No use estas credenciales ni esta contraseña en producción.

## Ejecutar Identity

Terminal 1:

```powershell
dotnet run --project src/Services/Identity/MineOps.Identity.Api --launch-profile http
```

Health: `GET http://localhost:5001/health`.

## Ejecutar Planning

Terminal 2:

```powershell
dotnet run --project src/Services/Planning/MineOps.Planning.Api --launch-profile http
```

Health: `GET http://localhost:5002/health`.

## Ejecutar Maintenance

Terminal 3:

```powershell
dotnet run --project src/Services/Maintenance/MineOps.Maintenance.Api --launch-profile http
```

Health: `GET http://localhost:5003/health`.

## Swagger

- Identity: `http://localhost:5001/swagger`
- Planning: `http://localhost:5002/swagger`
- Maintenance: `http://localhost:5003/swagger`

Identity documenta el login anónimo. Planning y Maintenance muestran el botón **Authorize**. Pegue únicamente el token JWT; Swagger agrega el prefijo `Bearer`.

### Endpoints y matriz de autorización

| Servicio | Método y ruta | Acceso |
| --- | --- | --- |
| Identity | `GET /health` | Anónimo |
| Identity | `POST /api/auth/login` | Anónimo |
| Planning | `GET /health` | Anónimo |
| Planning | `GET /api/activities` | Autenticado |
| Planning | `GET /api/activities/{id}` | Autenticado |
| Planning | `GET /api/activities/pending` | Autenticado |
| Planning | `GET /api/activities/overdue` | Autenticado |
| Planning | `GET /api/activities/responsible/{responsibleId}` | Autenticado |
| Planning | `POST /api/activities` | `Administrator,Planner` |
| Planning | `PUT /api/activities/{id}` | `Administrator,Planner` |
| Planning | `PATCH /api/activities/{id}/cancel` | `Administrator,Planner` |
| Planning | `PATCH /api/activities/{id}/start` | `Administrator,Planner,Operator` |
| Planning | `PATCH /api/activities/{id}/complete` | `Administrator,Planner,Operator` |
| Maintenance | `GET /health` | Anónimo |
| Maintenance | `GET /api/equipment` | Autenticado |
| Maintenance | `GET /api/work-orders` | Autenticado |
| Maintenance | `GET /api/work-orders/{id}` | Autenticado |
| Maintenance | `POST /api/work-orders` | `Administrator,MaintenanceSupervisor` |
| Maintenance | `PATCH /api/work-orders/{id}/start` | `Administrator,MaintenanceSupervisor` |
| Maintenance | `PATCH /api/work-orders/{id}/complete` | `Administrator,MaintenanceSupervisor` |
| Maintenance | `PATCH /api/work-orders/{id}/cancel` | `Administrator,MaintenanceSupervisor` |

## Pruebas

```powershell
dotnet build MineOps.sln --no-restore
dotnet test MineOps.sln --no-build --no-restore
```

Resultado auditado el 20 de noviembre de 2026:

| Suite | Exitosas | Fallidas | Omitidas |
| --- | ---: | ---: | ---: |
| Planning UnitTests | 28 | 0 | 0 |
| Planning IntegrationTests | 15 | 0 | 0 |
| Identity IntegrationTests | 10 | 0 | 0 |
| Maintenance UnitTests | 11 | 0 | 0 |
| Maintenance IntegrationTests | 10 | 0 | 0 |
| **Total** | **74** | **0** | **0** |

Las pruebas de integración crean bases SQL Server con nombres únicos, ejecutan migraciones y las eliminan al terminar. No usan Docker ni Testcontainers.

## Pruebas de seguridad

La demostración se realiza desde Swagger en el navegador, sin Postman ni archivos `.http`:

1. Abra Identity en `http://localhost:5001/swagger`.
2. Ejecute `POST /api/auth/login` con el usuario que quiera demostrar.
3. Copie únicamente `accessToken`, sin comillas.
4. Abra Planning o Maintenance en sus páginas Swagger.
5. Pulse **Authorize** y pegue solamente el token; no escriba `Bearer` porque Swagger lo agrega.
6. Ejecute los endpoints con **Try it out** y **Execute**.

El paso a paso completo, incluyendo los cuerpos JSON y el JWT expirado académico, está en [docs/video-script.md](docs/video-script.md).

1. Tres health checks devuelven 200.
2. Los logins devuelven JWT.
3. Planning sin token devuelve 401.
4. Planning con token Planner devuelve 200.
5. Planner crea una actividad y obtiene 201.
6. El mismo token Planner intenta crear una orden y obtiene 403.
7. MaintenanceSupervisor crea una orden y obtiene 201.
8. Un JWT alterado devuelve 401.
9. Un JWT firmado pero expirado devuelve 401 por JwtBearer Middleware.

`401` significa que no existe una identidad autenticada: falta el token o el token es inválido, está alterado o expiró. `403` significa que el token es válido y el usuario está autenticado, pero no posee uno de los roles requeridos.

## Git Flow

El flujo objetivo parte de `develop`; no se desarrolla directamente en `main`:

```text
feature/csharp-solution
feature/planning-csharp
feature/identity-service
feature/jwt-authentication
feature/maintenance-service
feature/authorization
feature/security-tests
docs/readme-second-delivery
```

Para cada rama: crear desde `develop`, realizar commits pequeños, publicar, abrir Pull Request hacia `develop`, revisar y hacer merge con build/tests verdes. La entrega finaliza con un Pull Request de `develop` hacia `main`.

## Arquitectura futura

Fuera de esta entrega quedan API Gateway, refresh tokens, revocación, MFA, autorización granular, mensajería, observabilidad distribuida, CI/CD, contenedores y servicios como Sync, Evidence, Inventory, SST o Notifications. Producción debe incorporar HTTPS, gestor de secretos, rotación de claves y bases con mínimo privilegio.

El guion detallado está en [docs/video-script.md](docs/video-script.md) y la evidencia completa en [docs/second-delivery-audit.md](docs/second-delivery-audit.md).
