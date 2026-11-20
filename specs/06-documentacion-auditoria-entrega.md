# SPEC 06 — Documentación, guion y auditoría de la segunda entrega

> **Estado:** Borrador
> **Depende de:** SPEC 05
> **Fecha:** 2026-11-20
> **Objetivo:** Documentar la ejecución reproducible de MineOps y auditar con evidencia que la segunda entrega cumple cada criterio de la rúbrica antes de grabar el video.

## Alcance

**Incluye:**

- Reescritura completa de `README.md` para la segunda entrega.
- Comandos de instalación, migración, seed, ejecución y pruebas.
- Documentación de credenciales académicas y configuración segura.
- Guion técnico ejecutable del video.
- Auditoría final con estado y evidencia por criterio.
- Inventario final de solución, proyectos, archivos, paquetes, migraciones, bases, endpoints y pruebas.
- Recomendación de commit y descripción de Pull Request.
- Veredicto explícito `SÍ` o `NO` sobre el cumplimiento completo.

**Fuera de alcance:**

- Cambios funcionales no exigidos por hallazgos de la auditoría.
- Nuevos microservicios o nuevas capacidades de dominio.
- Despliegue productivo, nube, CI/CD o Docker.
- Grabación o edición del video.

## Modelo de evidencia

Esta spec no introduce datos persistentes de aplicación.

La auditoría se guardará en `docs/second-delivery-audit.md` con esta tabla:

```text
| Criterio | Estado | Evidencia |
| --- | --- | --- |
| 3 microservicios | | |
| Planning funcional | | |
| Maintenance funcional | | |
| ASP.NET Core Identity | | |
| Seeders | | |
| Usuarios | | |
| Roles | | |
| Login | | |
| JWT firmado | | |
| Expiración JWT | | |
| Role claims | | |
| Validación de firma | | |
| Validación issuer | | |
| Validación audience | | |
| Validación lifetime | | |
| Planning protegido | | |
| Maintenance protegido | | |
| 401 sin token | | |
| 401 token inválido | | |
| 401 token expirado | | |
| 403 rol incorrecto | | |
| SQL Server | | |
| Swagger | | |
| Tests | | |
| README | | |
```

Cada estado será `Cumple`, `No cumple` o `No verificable`. Cada evidencia citará una ruta, comando reproducible, resultado de prueba o respuesta HTTP concreta.

Los hallazgos se clasificarán como `CRÍTICO`, `IMPORTANTE` o `RECOMENDADO`.

## Archivos principales

```text
README.md
docs/video-script.md
docs/second-delivery-audit.md
MineOps.Demo.http
```

## Contenido obligatorio de README

```text
# MineOps
## Segunda entrega
## Arquitectura
## Microservicios
### Identity Service
### Planning Service
### Maintenance Service
## Tecnologías
## Requisitos
## Configuración SQL Server
## Configuración JWT
## Migraciones
## Seeders
## Usuarios de demostración
## Ejecutar Identity
## Ejecutar Planning
## Ejecutar Maintenance
## Swagger
## Pruebas
## Pruebas de seguridad
## Git Flow
## Arquitectura futura
```

README explicará expresamente:

```text
401 = usuario no autenticado o token ausente, inválido o expirado.
403 = usuario autenticado con token válido, pero sin el rol requerido.
```

El Git Flow documentará estas ramas de trabajo desde `develop`:

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

Cada rama tendrá commits pequeños y Pull Request hacia `develop`. La entrega finalizará con un Pull Request de `develop` hacia `main`.

## Plan de implementación

1. Ejecutar `dotnet build MineOps.sln` y guardar el resultado verificable.
2. Ejecutar `dotnet test MineOps.sln` y registrar el total de pruebas exitosas, fallidas y omitidas.
3. Aplicar desde cero las tres migraciones sobre bases limpias y comprobar el seed idempotente.
4. Ejecutar simultáneamente las tres API y verificar los health checks.
5. Recorrer `MineOps.Demo.http` completo y registrar los códigos HTTP observados.
6. Escribir `docs/video-script.md` con comandos, terminales, solicitudes y explicación oral de 401 frente a 403.
7. Reescribir `README.md` con todas las secciones obligatorias y comandos copiables.
8. Inventariar proyectos, archivos clave, paquetes NuGet, bases, migraciones, usuarios, roles y endpoints.
9. Completar la tabla de auditoría únicamente con evidencia obtenida.
10. Clasificar hallazgos y corregir solo defectos que impidan cumplir una spec aprobada.
11. Repetir build, test, migraciones y demostración después de cualquier corrección.
12. Emitir el veredicto final `SÍ` solo si todos los criterios están en `Cumple`; en caso contrario emitir `NO` y la lista exacta de faltantes.
13. Añadir al reporte un mensaje de commit recomendado y una descripción de Pull Request de `develop` hacia `main`.

## Guion mínimo del video

1. Mostrar Identity, Planning y Maintenance en tres terminales.
2. Mostrar los roles, usuarios y relaciones creados por el seeder.
3. Realizar login válido y mostrar el JWT sin revelar la signing key.
4. Consultar Planning sin Authorization y mostrar 401.
5. Consultar Planning con JWT válido y mostrar 200.
6. Iniciar sesión como Planner y crear una actividad con 201.
7. Reutilizar ese mismo JWT en Maintenance y mostrar 403.
8. Explicar que el usuario está autenticado, pero no posee el rol requerido.
9. Iniciar sesión como MaintenanceSupervisor y crear una orden con 201.
10. Alterar manualmente un JWT y mostrar 401.
11. Usar un JWT expirado y mostrar 401 emitido por middleware.

## Criterios de aceptación

- [ ] README contiene todas las secciones obligatorias y comandos reproducibles.
- [ ] README usa SQL Server y no conserva instrucciones de MySQL o Docker.
- [ ] Las credenciales académicas están claramente separadas de producción.
- [ ] Se documenta cómo configurar signing key y conexiones sin versionar secretos reales.
- [ ] Se documentan las tres migraciones y sus proyectos de inicio.
- [ ] Se documentan los cuatro usuarios y roles de Development.
- [ ] Se documentan todos los endpoints y la matriz de autorización.
- [ ] Se explica correctamente la diferencia entre 401 y 403.
- [ ] El guion reproduce todos los escenarios prioritarios del video.
- [ ] La auditoría contiene un estado y evidencia para cada fila.
- [ ] No hay pruebas fallidas antes de emitir el veredicto.
- [ ] El total exacto de pruebas exitosas aparece en el reporte.
- [ ] El reporte enumera los 21 puntos solicitados en el requerimiento original.
- [ ] El veredicto final es `SÍ` solamente si toda la rúbrica está demostrada.
- [ ] Si el veredicto es `NO`, se enumeran exactamente los faltantes antes de grabar.

## Decisiones

- **Sí:** evidencia reproducible antes que afirmaciones narrativas.
- **Sí:** una auditoría persistida en el repositorio y un guion separado para el video.
- **Sí:** Git Flow con trabajo desde `develop`, ramas de implementación y PR final `develop` → `main`.
- **No:** marcar un criterio como cumplido sin prueba, archivo o respuesta HTTP verificable.
- **No:** añadir funcionalidades durante la auditoría; solo se corrigen incumplimientos de specs aprobadas.
- **No:** documentar MySQL, Docker o servicios futuros como parte de esta entrega.

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| El proyecto funciona en el equipo actual pero README omite un prerrequisito | Validar la guía desde bases limpias y registrar versión de SDK e instancia SQL Server. |
| El video falla por datos previamente existentes | Documentar una preparación idempotente y usar códigos de demostración conocidos. |
| La auditoría afirma resultados desactualizados | Ejecutarla después del último cambio y registrar comandos y conteos finales. |

## Qué **no** incluye esta spec

- Funcionalidades nuevas.
- Docker, nube, CI/CD o despliegue productivo.
- Grabación del video.
- Servicios futuros como Sync, Evidence, Inventory, SST o Notifications.

