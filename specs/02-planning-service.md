# SPEC 02 — Planning Service en C#

> **Estado:** Borrador
> **Depende de:** SPEC 01
> **Fecha:** 2026-11-20
> **Objetivo:** Implementar Planning Service en C# con un dominio Activity, CQRS, EF Core y una API persistida en su propia base SQL Server.

## Por qué existe esta spec

Planning fue descrito como un servicio previo en PHP/Laravel, pero el repositorio no contiene esa implementación. Esta spec formaliza su comportamiento desde los requisitos disponibles sin conservar una dependencia tecnológica con PHP.

## Alcance

**Incluye:**

- Agregado `Activity` con comportamiento y reglas de dominio.
- Commands, queries y handlers mediante MediatR.
- Contratos `IActivityRepository` e `IUnitOfWork`.
- Implementaciones EF Core sobre `MineOpsPlanning`.
- Migraciones propias de Planning para SQL Server.
- API REST de actividades con controladores delgados.
- Paginación `page=1&pageSize=20`, limitada a 100 elementos.
- Manejo global y consistente de errores.
- Endpoint `GET /health`.
- Pruebas unitarias, de repositorio y de endpoints.

**Fuera de alcance (para specs posteriores):**

- Configuración JwtBearer y autorización por roles.
- Identity Service y validación cruzada de usuarios.
- Interfaz web o aplicación móvil.
- Sincronización, evidencias, inventario, SST y notificaciones.
- Eliminación física de actividades.
- Mensajería, eventos distribuidos y API Gateway.

## Modelo de datos

```csharp
Activity
- Guid Id
- string Code                  // único, requerido
- string Title                 // requerido
- string? Description
- string Area
- string Location
- Guid? ResponsibleId
- ActivityPriority Priority    // Low, Medium, High, Critical
- ActivityStatus Status        // Pending, InProgress, Completed, Cancelled
- DateTimeOffset ScheduledDate // UTC
- DateTimeOffset? DueDate      // UTC
- DateTimeOffset CreatedAt     // UTC
- DateTimeOffset UpdatedAt     // UTC
```

Comportamientos del agregado:

```text
Start()
Complete()
Cancel()
Reschedule(scheduledDate, dueDate)
AssignResponsible(responsibleId)
UpdateDetails(...)
```

Reglas obligatorias:

- `Title` no puede estar vacío.
- `Code` no puede estar vacío ni repetirse.
- `DueDate` no puede ser anterior a `ScheduledDate`.
- Una actividad `Critical` requiere `DueDate`.
- Una actividad `Completed` no puede iniciarse otra vez.
- Una actividad `Cancelled` no puede completarse.
- `ResponsibleId` puede ser nulo al crear y asignarse posteriormente.

EF Core creará un índice único sobre `Activity.Code`.

## Contratos y archivos principales

```text
MineOps.Planning.Domain/Activities/Activity.cs
MineOps.Planning.Domain/Activities/ActivityStatus.cs
MineOps.Planning.Domain/Activities/ActivityPriority.cs
MineOps.Planning.Application/Abstractions/IActivityRepository.cs
MineOps.Planning.Application/Abstractions/IUnitOfWork.cs
MineOps.Planning.Application/Activities/Commands/
MineOps.Planning.Application/Activities/Queries/
MineOps.Planning.Infrastructure/Persistence/PlanningDbContext.cs
MineOps.Planning.Infrastructure/Persistence/Configurations/ActivityConfiguration.cs
MineOps.Planning.Infrastructure/Persistence/Repositories/EfActivityRepository.cs
MineOps.Planning.Infrastructure/Persistence/EfUnitOfWork.cs
MineOps.Planning.Infrastructure/Migrations/
MineOps.Planning.Api/Controllers/ActivitiesController.cs
MineOps.Planning.Api/Errors/GlobalExceptionHandler.cs
```

Commands:

- `CreateActivityCommand`
- `UpdateActivityCommand`
- `StartActivityCommand`
- `CompleteActivityCommand`
- `CancelActivityCommand`

Queries:

- `GetActivityByIdQuery`
- `GetActivitiesQuery`
- `GetPendingActivitiesQuery`
- `GetOverdueActivitiesQuery`
- `GetActivitiesByResponsibleQuery`

Cada command y query tendrá un handler separado.

## API

```text
POST   /api/activities
GET    /api/activities?page=1&pageSize=20
GET    /api/activities/{id}
PUT    /api/activities/{id}
PATCH  /api/activities/{id}/start
PATCH  /api/activities/{id}/complete
PATCH  /api/activities/{id}/cancel
GET    /api/activities/pending?page=1&pageSize=20
GET    /api/activities/overdue?page=1&pageSize=20
GET    /api/activities/responsible/{responsibleId}?page=1&pageSize=20
GET    /health
```

Los listados devolverán metadatos `page`, `pageSize`, `totalCount` y `totalPages` junto con los elementos.

Los errores tendrán esta forma:

```json
{
  "success": false,
  "message": "...",
  "errors": []
}
```

El mapeo será: validación de entrada `400`, recurso inexistente `404`, conflicto de estado o código duplicado `409`, regla de negocio incumplida `422` y error inesperado `500` sin stack trace en producción.

## Plan de implementación

1. Crear enums y excepciones de dominio, y cubrirlos con pruebas unitarias.
2. Crear `Activity` con constructor válido y cubrir todas sus invariantes.
3. Añadir los métodos de transición de estado y sus pruebas positivas y negativas.
4. Definir DTOs, contratos de repositorio, unidad de trabajo y resultado paginado.
5. Implementar `CreateActivityCommand` y su handler con pruebas.
6. Implementar `UpdateActivityCommand` y su handler con pruebas.
7. Implementar los commands de transición y sus handlers con pruebas.
8. Implementar las cinco queries y sus handlers con pruebas.
9. Crear `PlanningDbContext` y la configuración de EF Core sin filtrar dependencias a Domain.
10. Implementar `EfActivityRepository` y `EfUnitOfWork` con operaciones async y cancelación.
11. Crear y aplicar la migración inicial a `MineOpsPlanning` mediante `dotnet ef database update`.
12. Añadir pruebas contra una base SQL Server temporal con nombre único y limpieza al finalizar.
13. Crear el manejador global de excepciones y verificar los códigos HTTP definidos.
14. Crear `ActivitiesController` como adaptador de MediatR sin acceso a DbContext ni repositorios concretos.
15. Añadir paginación y validar límites de `page` y `pageSize`.
16. Añadir `GET /health` con comprobación de SQL Server.
17. Ejecutar build y toda la suite; detener la fase ante cualquier fallo.

## Criterios de aceptación

- [ ] Domain no referencia ASP.NET Core, EF Core, SQL Server, Identity ni Infrastructure.
- [ ] Application no referencia Infrastructure.
- [ ] Los controladores no contienen reglas de negocio ni usan DbContext.
- [ ] Cada command y query declarado tiene un handler.
- [ ] Todas las reglas y transiciones de `Activity` tienen pruebas unitarias.
- [ ] `Code` tiene un índice único en SQL Server.
- [ ] Las fechas se guardan como valores UTC.
- [ ] Los diez endpoints de actividades responden con el contrato documentado.
- [ ] Los listados usan valores predeterminados 1 y 20 y rechazan tamaños mayores a 100.
- [ ] La persistencia usa `IActivityRepository` e `IUnitOfWork`.
- [ ] Las transacciones se ejecutan realmente sobre SQL Server.
- [ ] Planning posee su migración y su base; no comparte DbContext ni tablas.
- [ ] `GET /health` informa correctamente el estado del servicio y la base.
- [ ] Las respuestas 400, 404, 409, 422 y 500 son consistentes.
- [ ] Las pruebas unitarias e integración terminan en verde.

## Decisiones

- **Sí:** agregado con comportamiento para evitar un modelo anémico.
- **Sí:** CQRS con MediatR, un handler por operación.
- **Sí:** repository y unit of work para separar Application de EF Core.
- **Sí:** `Guid? ResponsibleId`, asignable después de crear la actividad.
- **Sí:** códigos únicos y fechas UTC.
- **No:** acceso Controller → DbContext o Controller → repositorio concreto.
- **No:** DELETE físico, porque no está solicitado y `Cancelled` cubre el cierre operativo.
- **No:** verificación síncrona del usuario en Identity; la identidad llegará por JWT en SPEC 05.

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| Dos solicitudes crean el mismo código simultáneamente | Índice único y traducción de la excepción de persistencia a 409. |
| Fechas locales producen falsos vencimientos | Normalizar y persistir `DateTimeOffset` en UTC. |
| Los handlers duplican reglas del agregado | Mantener invariantes y transiciones dentro de `Activity`. |

## Qué **no** incluye esta spec

- Login, JWT y roles.
- Comunicación directa con Identity.
- DELETE de actividades.
- Aplicación Android, API Gateway o mensajería.

