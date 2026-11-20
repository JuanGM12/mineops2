# SPEC 04 — Maintenance Service

> **Estado:** Borrador
> **Depende de:** SPEC 01, SPEC 03
> **Fecha:** 2026-11-20
> **Objetivo:** Implementar Maintenance Service con WorkOrder como agregado principal, equipos mínimos sembrados, CQRS y persistencia independiente en SQL Server.

## Alcance

**Incluye:**

- Entidad mínima `Equipment` sin API CRUD.
- Seeder idempotente de dos equipos de demostración.
- Agregado `WorkOrder` con comportamiento y reglas de dominio.
- Commands, queries y handlers mediante MediatR.
- Repository, unit of work y EF Core sobre `MineOpsMaintenance`.
- Migraciones propias de Maintenance.
- API REST de órdenes de trabajo.
- Manejo global de errores y `GET /health`.
- Pruebas unitarias, de persistencia y endpoints.

**Fuera de alcance (para specs posteriores):**

- CRUD o catálogo avanzado de equipos.
- Repuestos, inventario, técnicos, costos y evidencias.
- Programación recurrente de mantenimiento preventivo.
- Configuración JwtBearer y autorización por roles.
- Comunicación directa con Planning o Identity.
- Notificaciones y mensajería.

## Modelo de datos

```csharp
Equipment
- Guid Id
- string Code             // único, requerido
- string Name             // requerido
- bool IsActive
- DateTimeOffset CreatedAt // UTC
```

Registros seed:

```text
EXC-001 | Excavadora 001 | activa
TRK-001 | Camión 001     | activo
```

```csharp
WorkOrder
- Guid Id
- string Code                      // único, requerido
- Guid EquipmentId                 // FK requerida
- string Title                     // requerido
- string? Description
- MaintenanceType MaintenanceType  // Preventive, Corrective
- WorkOrderPriority Priority       // Low, Medium, High, Critical
- WorkOrderStatus Status           // Pending, InProgress, Completed, Cancelled
- DateTimeOffset? ScheduledDate    // UTC
- DateTimeOffset? StartedAt        // UTC
- DateTimeOffset? CompletedAt      // UTC
- DateTimeOffset CreatedAt         // UTC
- DateTimeOffset UpdatedAt         // UTC
```

Comportamientos:

```text
Start(startedAt)
Complete(completedAt)
Cancel()
Reschedule(scheduledDate)
UpdateDetails(...)
```

Reglas:

- `Title` y `Code` son obligatorios.
- La orden debe referenciar un equipo existente y activo.
- El mantenimiento `Preventive` requiere `ScheduledDate`.
- Una orden `Completed` no puede iniciarse de nuevo.
- Una orden `Cancelled` no puede completarse.
- `StartedAt` se fija al iniciar y `CompletedAt` al completar.
- Los códigos de equipo y orden tienen índices únicos.

## Contratos y archivos principales

```text
MineOps.Maintenance.Domain/Equipment/Equipment.cs
MineOps.Maintenance.Domain/WorkOrders/WorkOrder.cs
MineOps.Maintenance.Domain/WorkOrders/MaintenanceType.cs
MineOps.Maintenance.Domain/WorkOrders/WorkOrderStatus.cs
MineOps.Maintenance.Domain/WorkOrders/WorkOrderPriority.cs
MineOps.Maintenance.Application/Abstractions/IWorkOrderRepository.cs
MineOps.Maintenance.Application/Abstractions/IEquipmentRepository.cs
MineOps.Maintenance.Application/Abstractions/IUnitOfWork.cs
MineOps.Maintenance.Application/WorkOrders/Commands/
MineOps.Maintenance.Application/WorkOrders/Queries/
MineOps.Maintenance.Infrastructure/Persistence/MaintenanceDbContext.cs
MineOps.Maintenance.Infrastructure/Persistence/MaintenanceSeeder.cs
MineOps.Maintenance.Infrastructure/Persistence/Repositories/
MineOps.Maintenance.Infrastructure/Migrations/
MineOps.Maintenance.Api/Controllers/WorkOrdersController.cs
MineOps.Maintenance.Api/Errors/GlobalExceptionHandler.cs
```

Commands mínimos:

- `CreateWorkOrderCommand`
- `StartWorkOrderCommand`
- `CompleteWorkOrderCommand`
- `CancelWorkOrderCommand`

Queries mínimas:

- `GetWorkOrderByIdQuery`
- `GetWorkOrdersQuery`

## API

```text
POST   /api/work-orders
GET    /api/work-orders?page=1&pageSize=20
GET    /api/work-orders/{id}
PATCH  /api/work-orders/{id}/start
PATCH  /api/work-orders/{id}/complete
PATCH  /api/work-orders/{id}/cancel
GET    /health
```

La paginación y el contrato de error seguirán los mismos valores de Planning. El mapeo HTTP será `400`, `404`, `409`, `422` y `500` según la clasificación establecida en SPEC 02.

## Plan de implementación

1. Crear `Equipment`, sus invariantes y pruebas unitarias.
2. Crear enums y `WorkOrder` con constructor válido y pruebas.
3. Implementar transiciones de orden y cubrir casos válidos e inválidos.
4. Definir DTOs, contratos de repositorio, unidad de trabajo y paginación.
5. Implementar commands y handlers por separado con pruebas.
6. Implementar queries y handlers por separado con pruebas.
7. Crear `MaintenanceDbContext` y las configuraciones de ambas entidades.
8. Implementar repositorios EF y unidad de trabajo async.
9. Crear la migración inicial y aplicarla explícitamente a `MineOpsMaintenance`.
10. Crear el seeder idempotente de los dos equipos.
11. Añadir pruebas de persistencia sobre una base SQL Server temporal con nombre único.
12. Crear el manejador global de excepciones.
13. Crear `WorkOrdersController` como adaptador de MediatR.
14. Añadir paginación con valores 1 y 20 y máximo 100.
15. Añadir `GET /health` con comprobación de SQL Server.
16. Ejecutar build y toda la suite; detener la fase ante cualquier fallo.

## Criterios de aceptación

- [ ] Domain y Application cumplen las mismas reglas de dependencia de Planning.
- [ ] `WorkOrder` contiene las reglas y transiciones; el controlador no las duplica.
- [ ] No se puede crear una orden con un equipo inexistente o inactivo.
- [ ] El seeder crea exactamente los dos equipos y no los duplica.
- [ ] Los códigos de equipo y orden son únicos en SQL Server.
- [ ] Una orden preventiva sin fecha programada es rechazada.
- [ ] Una orden completada no vuelve a iniciarse.
- [ ] Una orden cancelada no puede completarse.
- [ ] Los siete endpoints responden con los contratos definidos.
- [ ] Los listados respetan la paginación y el máximo de 100.
- [ ] Maintenance tiene DbContext, migraciones y base propios.
- [ ] `GET /health` verifica servicio y base.
- [ ] Las pruebas unitarias e integración terminan en verde.

## Decisiones

- **Sí:** `WorkOrder` es el agregado principal de esta entrega.
- **Sí:** `Equipment` es mínimo y se alimenta mediante seed.
- **Sí:** CQRS, MediatR, repository y unit of work para mantener las mismas fronteras de Planning.
- **Sí:** códigos únicos, claves `Guid` y fechas UTC.
- **No:** CRUD de equipos; ampliaría el alcance sin aportar al escenario principal de la rúbrica.
- **No:** acceso a las bases de Identity o Planning.

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| Una orden referencia un equipo borrado o inactivo | No existe borrado en esta entrega y el handler valida `IsActive` antes de crear. |
| Colisiones de código bajo concurrencia | Índice único y respuesta 409. |
| Duplicación entre Planning y Maintenance | Compartir convenciones, no modelos ni DbContexts; mantener cada dominio independiente. |

## Qué **no** incluye esta spec

- CRUD de equipos.
- Inventario, costos, técnicos o evidencias.
- JWT y autorización, que se integran en SPEC 05.

