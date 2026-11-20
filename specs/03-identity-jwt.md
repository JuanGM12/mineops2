# SPEC 03 — Identity Service y emisión de JWT

> **Estado:** Borrador
> **Depende de:** SPEC 01
> **Fecha:** 2026-11-20
> **Objetivo:** Implementar Identity Service con ASP.NET Core Identity, usuarios y roles idempotentes, login y emisión segura de JWT para los servicios de MineOps.

## Alcance

**Incluye:**

- `ApplicationUser : IdentityUser<Guid>`.
- `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>` sobre `MineOpsIdentity`.
- Migraciones propias de Identity para SQL Server.
- Seeder idempotente de cuatro roles y cuatro usuarios.
- Login mediante `UserManager` y ASP.NET Core Identity.
- `ITokenService` y `JwtTokenService`.
- JWT simétrico con issuer y audience compartidos.
- Claims de identificador, nombre, email y roles.
- Endpoint `POST /api/auth/login` y `GET /health`.
- Pruebas de seeder, credenciales y contenido del token.

**Fuera de alcance (para specs posteriores):**

- CRUD de usuarios o roles.
- Registro público.
- Recuperación o cambio de contraseña.
- Refresh tokens y revocación.
- MFA, proveedores externos y OpenID Connect.
- Permisos granulares, scopes, ABAC y policies complejas.
- Configuración JwtBearer en Planning o Maintenance.

## Modelo de datos y configuración

```csharp
ApplicationUser : IdentityUser<Guid>
- string Name
```

Roles exactos:

```text
Administrator
Planner
MaintenanceSupervisor
Operator
```

Usuarios de Development y pruebas:

```text
admin@mineops.local       -> Administrator
planner@mineops.local     -> Planner
maintenance@mineops.local -> MaintenanceSupervisor
operator@mineops.local    -> Operator
```

La contraseña académica común será `MineOps2026!`. Se suministrará desde configuración de Development o desde la configuración aislada de pruebas, se documentará como no apta para producción y no se reutilizará como secreto real.

Configuración:

```text
Jwt:Issuer = MineOps.Identity
Jwt:Audience = MineOps.Api
Jwt:SigningKey = valor seguro externo a producción
Jwt:ExpirationMinutes = 60
Seed:DefaultPassword = MineOps2026! únicamente en Development/pruebas
```

Claims mínimos:

```text
sub      = identificador Guid del usuario
name     = nombre del usuario
email    = correo normalizado
role     = una entrada por cada rol
```

## Contratos y archivos principales

```text
MineOps.Identity.Application/Auth/LoginRequest.cs
MineOps.Identity.Application/Auth/LoginResponse.cs
MineOps.Identity.Application/Auth/ITokenService.cs
MineOps.Identity.Infrastructure/Identity/ApplicationUser.cs
MineOps.Identity.Infrastructure/Identity/MineOpsIdentityDbContext.cs
MineOps.Identity.Infrastructure/Identity/IdentitySeeder.cs
MineOps.Identity.Infrastructure/Auth/JwtOptions.cs
MineOps.Identity.Infrastructure/Auth/JwtTokenService.cs
MineOps.Identity.Infrastructure/Migrations/
MineOps.Identity.Api/Controllers/AuthController.cs
MineOps.Identity.Api/Errors/GlobalExceptionHandler.cs
```

Request y respuesta:

```json
{
  "email": "planner@mineops.local",
  "password": "MineOps2026!"
}
```

```json
{
  "accessToken": "...",
  "expiresAt": "...",
  "tokenType": "Bearer"
}
```

Credenciales inválidas devolverán `401` sin revelar si falló el correo o la contraseña.

## Plan de implementación

1. Crear `ApplicationUser` y configurar Identity con claves `Guid` y su política estándar de contraseñas.
2. Crear el DbContext de Identity y registrar el proveedor oficial de EF Core SQL Server.
3. Crear la migración inicial y aplicarla explícitamente a `MineOpsIdentity`.
4. Implementar `IdentitySeeder` para crear primero los cuatro roles sin duplicados.
5. Extender el seeder para crear los cuatro usuarios mediante `UserManager.CreateAsync`.
6. Asignar cada usuario a su rol y hacer idempotente cada operación.
7. Cargar la contraseña de Development/pruebas desde `Seed:DefaultPassword` y fallar con un mensaje claro si falta.
8. Definir y validar `JwtOptions`, incluyendo una signing key de longitud segura.
9. Implementar `ITokenService` y `JwtTokenService` fuera del controlador.
10. Implementar login con respuesta genérica 401 para credenciales inválidas.
11. Añadir `GET /health` con comprobación de SQL Server.
12. Probar el seeder contra bases SQL Server temporales con nombres únicos.
13. Probar login válido, login inválido, roles, firma, expiración y claims.
14. Ejecutar build y toda la suite; detener la fase ante cualquier fallo.

## Criterios de aceptación

- [ ] Identity utiliza ASP.NET Core Identity y EF Core SQL Server.
- [ ] Las contraseñas se crean con `UserManager` y solo se almacenan como hashes de Identity.
- [ ] La migración de Identity pertenece exclusivamente a Identity Service.
- [ ] Ejecutar el seeder varias veces no duplica roles, usuarios ni relaciones.
- [ ] Los cuatro usuarios y roles existen con la asignación definida.
- [ ] `POST /api/auth/login` devuelve 200 y un JWT ante credenciales válidas.
- [ ] Credenciales inválidas devuelven 401 con un mensaje no enumerativo.
- [ ] El token está firmado y contiene `sub`, `name`, `email` y roles.
- [ ] `expiresAt` coincide con la expiración del JWT.
- [ ] Issuer, audience, signing key y expiración provienen de configuración validada.
- [ ] La signing key de producción no está escrita en código ni versionada.
- [ ] `GET /health` informa correctamente el estado del servicio y la base.
- [ ] Las pruebas se ejecutan sobre SQL Server local y terminan en verde.

## Decisiones

- **Sí:** Identity se concentra en Infrastructure y API; no se crea una capa Domain decorativa.
- **Sí:** usuarios `Guid` para alinear `sub` con `ResponsibleId`.
- **Sí:** contraseña de demostración `MineOps2026!`, válida para la política estándar.
- **Sí:** firma simétrica para la entrega académica.
- **Sí:** un issuer `MineOps.Identity` y una audience `MineOps.Api` compartida por las APIs consumidoras.
- **No:** contraseña literal `password`, porque incumple la política segura predeterminada.
- **No:** CRUD de identidades, refresh tokens o permisos granulares.
- **No:** consultas a Identity desde Planning o Maintenance en cada request.

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| La contraseña académica se confunde con una credencial de producción | Restringirla a Development/pruebas y advertirlo en README. |
| Configuraciones JWT diferentes entre servicios | Definir nombres y valores de configuración idénticos y verificarlos en SPEC 05. |
| Una signing key corta debilita la firma | Validar longitud mínima al arrancar y usar User Secrets o variables de entorno fuera de Development. |

## Qué **no** incluye esta spec

- Registro, recuperación de contraseña, MFA o refresh tokens.
- CRUD de usuarios y roles.
- Autorización de Planning y Maintenance.

