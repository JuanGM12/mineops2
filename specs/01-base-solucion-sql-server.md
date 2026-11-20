# SPEC 01 — Base de la solución y entorno SQL Server

> **Estado:** Aprobado
> **Depende de:** Ninguna
> **Fecha:** 2026-11-20
> **Objetivo:** Crear una solución .NET 10 compilable con la estructura de los tres microservicios, sus pruebas, Git y la configuración local de SQL Server.

## Por qué existe esta spec

El directorio de trabajo está vacío y no contiene el proyecto PHP/Laravel mencionado en el requerimiento original. No existe código anterior del cual recuperar reglas, endpoints, pruebas o decisiones arquitectónicas.

Esta spec establece una base reproducible antes de incorporar comportamiento de negocio.

## Alcance

**Incluye:**

- Inicialización del repositorio Git con ramas `main` y `develop`.
- Creación de `MineOps.sln` para `net10.0` usando el SDK instalado `10.0.401`.
- Creación de los proyectos bajo `src/Services` y `src/Tests`.
- Referencias entre proyectos compatibles con Clean Architecture.
- Configuración de compilación común, nullable reference types y análisis estático.
- Dependencias base de ASP.NET Core, EF Core SQL Server, MediatR y xUnit.
- Configuración de desarrollo para una instancia local predeterminada de SQL Server.
- Separación de las bases `MineOpsIdentity`, `MineOpsPlanning` y `MineOpsMaintenance`.
- Un primer `dotnet build` y `dotnet test` exitosos.

**Fuera de alcance (para specs posteriores):**

- Entidades, casos de uso y endpoints de Planning.
- ASP.NET Core Identity, login y emisión de JWT.
- Entidades, casos de uso y endpoints de Maintenance.
- Autenticación, autorización, Swagger y pruebas integradas de seguridad.
- Docker y Docker Compose.
- Migraciones de negocio.
- Eliminación o migración de código PHP, porque no existe en este repositorio.

## Modelo de datos y configuración

Esta spec no introduce tablas de negocio.

Las API usarán estas claves de configuración en Development:

```text
ConnectionStrings:Planning = Server=localhost;Database=MineOpsPlanning;Integrated Security=true;TrustServerCertificate=true
ConnectionStrings:Identity = Server=localhost;Database=MineOpsIdentity;Integrated Security=true;TrustServerCertificate=true
ConnectionStrings:Maintenance = Server=localhost;Database=MineOpsMaintenance;Integrated Security=true;TrustServerCertificate=true
```

La configuración versionada no contendrá credenciales de producción.

## Estructura de archivos

```text
MineOps.sln
Directory.Build.props
.gitignore
src/Services/Planning/MineOps.Planning.Domain/
src/Services/Planning/MineOps.Planning.Application/
src/Services/Planning/MineOps.Planning.Infrastructure/
src/Services/Planning/MineOps.Planning.Api/
src/Services/Identity/MineOps.Identity.Application/
src/Services/Identity/MineOps.Identity.Infrastructure/
src/Services/Identity/MineOps.Identity.Api/
src/Services/Maintenance/MineOps.Maintenance.Domain/
src/Services/Maintenance/MineOps.Maintenance.Application/
src/Services/Maintenance/MineOps.Maintenance.Infrastructure/
src/Services/Maintenance/MineOps.Maintenance.Api/
src/Tests/MineOps.Planning.UnitTests/
src/Tests/MineOps.Planning.IntegrationTests/
src/Tests/MineOps.Identity.IntegrationTests/
src/Tests/MineOps.Maintenance.UnitTests/
src/Tests/MineOps.Maintenance.IntegrationTests/
```

Las referencias permitidas serán:

```text
Planning.Application -> Planning.Domain
Planning.Infrastructure -> Planning.Application + Planning.Domain
Planning.Api -> Planning.Application + Planning.Infrastructure
Maintenance.Application -> Maintenance.Domain
Maintenance.Infrastructure -> Maintenance.Application + Maintenance.Domain
Maintenance.Api -> Maintenance.Application + Maintenance.Infrastructure
Identity.Infrastructure -> Identity.Application
Identity.Api -> Identity.Application + Identity.Infrastructure
```

Los proyectos Domain y Application no referenciarán las API ni Infrastructure.

## Plan de implementación

1. Inicializar Git, crear `main`, crear `develop` desde `main` y continuar sobre `develop`.
2. Añadir `.gitignore` para .NET, Visual Studio, Rider, secretos locales, resultados de pruebas y artefactos de compilación.
3. Crear `MineOps.sln` en formato `.sln` y fijar todos los proyectos a `net10.0`.
4. Crear los proyectos Planning y agregarlos a la solución sin lógica de negocio.
5. Crear los proyectos Identity y agregarlos a la solución sin capas artificiales de dominio.
6. Crear los proyectos Maintenance y agregarlos a la solución sin lógica de negocio.
7. Crear los cinco proyectos xUnit y agregarlos a la solución.
8. Configurar las referencias de proyectos de acuerdo con las reglas anteriores.
9. Añadir las dependencias NuGet base compatibles con .NET 10 y evitar versiones mayores incompatibles.
10. Añadir las cadenas de conexión de Development para SQL Server local y mantener vacía la configuración sensible de producción.
11. Ejecutar `dotnet restore` y `dotnet build MineOps.sln`.
12. Añadir una prueba de humo por proyecto de pruebas y ejecutar `dotnet test MineOps.sln`.

Cada paso debe terminar con la solución compilando; no se continúa si falla una compilación o una prueba.

## Criterios de aceptación

- [ ] El repositorio Git tiene `main` y `develop`.
- [ ] `git status` no incluye `bin`, `obj`, resultados de pruebas ni secretos locales.
- [ ] `dotnet --version` usa un SDK 10 compatible con `net10.0`.
- [ ] `MineOps.sln` contiene los once proyectos productivos y los cinco proyectos de pruebas descritos.
- [ ] Planning y Maintenance respetan las referencias de Clean Architecture.
- [ ] Identity no contiene un proyecto Domain artificial.
- [ ] Las tres cadenas de conexión apuntan a bases separadas en `localhost`.
- [ ] Ningún proyecto contiene paquetes de MySQL, PHP o Laravel.
- [ ] `dotnet build MineOps.sln` termina con código de salida cero.
- [ ] `dotnet test MineOps.sln` termina con código de salida cero.

## Decisiones

- **Sí:** .NET 10, porque el equipo tiene instalado el SDK `10.0.401` y el requerimiento prefiere la versión LTS disponible.
- **Sí:** SQL Server local con autenticación integrada de Windows.
- **Sí:** tres bases independientes en la misma instancia local.
- **Sí:** MediatR para los flujos CQRS de Planning y Maintenance.
- **Sí:** `Guid` para entidades y usuarios.
- **No:** MySQL y Pomelo, reemplazados expresamente por SQL Server y el proveedor oficial de EF Core.
- **No:** Docker, Docker Compose y Testcontainers.
- **No:** mezcla o adaptadores PHP-C#, porque la solución final será exclusivamente C#.
- **No:** un Shared Kernel de negocio entre microservicios; cada servicio será propietario de su modelo.

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| La instancia local no permite crear bases con la identidad de Windows actual | Verificar permisos antes de crear la primera migración y documentar el requisito en README. |
| Una dependencia aún no ofrece una versión compatible con .NET 10 | Seleccionar una versión estable compatible y validar con restore, build y test antes de continuar. |
| Las pruebas de integración concurrentes colisionan en SQL Server local | Usar nombres de base únicos por ejecución y desactivar paralelismo cuando una suite comparta infraestructura. |

## Qué **no** incluye esta spec

- Funcionalidad de Planning, Identity o Maintenance.
- Seguridad JWT o autorización por roles.
- Docker.
- Aplicación Android, API Gateway, mensajería o servicios futuros.

