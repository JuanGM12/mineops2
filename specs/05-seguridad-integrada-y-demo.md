# SPEC 05 — Seguridad integrada, Swagger y demostración

> **Estado:** Borrador
> **Depende de:** SPEC 02, SPEC 03, SPEC 04
> **Fecha:** 2026-11-20
> **Objetivo:** Integrar JWT Bearer y autorización por roles en Planning y Maintenance, demostrando de forma automatizada la diferencia entre 401 y 403.

## Por qué existe esta spec

La rúbrica prioriza demostrar que autenticación y autorización son controles distintos. La validación debe ocurrir mediante el middleware estándar de ASP.NET Core y no mediante condicionales en controladores.

## Alcance

**Incluye:**

- Validación local de JWT en Planning y Maintenance.
- Validación de firma, issuer, audience, lifetime y signing key.
- Autorización estándar por los cuatro roles definidos.
- Matriz completa de protección de endpoints.
- Swagger/OpenAPI en los tres servicios con esquema Bearer.
- Puertos locales diferenciados.
- Pruebas de integración de 200, 201, 401 y 403.
- Pruebas de token alterado y token expirado.
- `MineOps.Demo.http` en el orden del video.
- Ejecución simultánea local de las tres API.

**Fuera de alcance (para specs posteriores):**

- Permisos, scopes, ABAC o autorización dinámica.
- Introspección remota de tokens.
- Refresh tokens y revocación.
- API Gateway.
- HTTPS y gestión de certificados de producción.
- Docker, mensajería y observabilidad distribuida.

## Configuración y flujo

```text
Identity:    http://localhost:5001
Planning:    http://localhost:5002
Maintenance: http://localhost:5003
```

Los tres servicios compartirán estos nombres y valores de configuración de Development:

```text
Jwt:Issuer = MineOps.Identity
Jwt:Audience = MineOps.Api
Jwt:SigningKey = secreto simétrico académico de longitud segura
Jwt:ExpirationMinutes = 60
```

Planning y Maintenance configurarán `AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)` con:

```text
ValidateIssuer = true
ValidateAudience = true
ValidateLifetime = true
ValidateIssuerSigningKey = true
ClockSkew = 0 para pruebas deterministas de expiración
```

No consultarán Identity para validar cada request.

## Matriz de autorización

| Servicio | Operación | Roles |
| --- | --- | --- |
| Planning | Todos los GET | Cualquier usuario autenticado |
| Planning | POST actividad | `Administrator,Planner` |
| Planning | PUT actividad | `Administrator,Planner` |
| Planning | PATCH cancel | `Administrator,Planner` |
| Planning | PATCH start | `Administrator,Planner,Operator` |
| Planning | PATCH complete | `Administrator,Planner,Operator` |
| Maintenance | Todos los GET | Cualquier usuario autenticado |
| Maintenance | POST orden | `Administrator,MaintenanceSupervisor` |
| Maintenance | PATCH start | `Administrator,MaintenanceSupervisor` |
| Maintenance | PATCH complete | `Administrator,MaintenanceSupervisor` |
| Maintenance | PATCH cancel | `Administrator,MaintenanceSupervisor` |
| Todos | `GET /health` | Anónimo |
| Identity | `POST /api/auth/login` | Anónimo |

Resultados obligatorios:

```text
Sin token                         -> 401
Token alterado                    -> 401
Token expirado                    -> 401
Token válido + rol correcto       -> acceso permitido
Token válido + rol incorrecto     -> 403
```

## Archivos principales

```text
src/Services/Planning/MineOps.Planning.Api/Security/JwtAuthenticationExtensions.cs
src/Services/Maintenance/MineOps.Maintenance.Api/Security/JwtAuthenticationExtensions.cs
src/Services/*/*.Api/OpenApi/OpenApiExtensions.cs
src/Tests/MineOps.Planning.IntegrationTests/Security/
src/Tests/MineOps.Identity.IntegrationTests/Security/
src/Tests/MineOps.Maintenance.IntegrationTests/Security/
MineOps.Demo.http
```

Las pruebas de integración usarán SQL Server local con autenticación integrada, crearán bases con nombres únicos por ejecución y las eliminarán al finalizar. No usarán Docker ni Testcontainers.

## Plan de implementación

1. Centralizar en cada API el enlace de `JwtOptions` y fallar al arrancar ante configuración incompleta.
2. Configurar JwtBearer en Planning con las cuatro validaciones obligatorias.
3. Probar Planning sin token, con token válido, alterado y expirado.
4. Aplicar la matriz de roles a Planning mediante `[Authorize]` y `[Authorize(Roles = ...)]`.
5. Probar 201 con Planner y 403 con MaintenanceSupervisor en operaciones administrativas de Planning.
6. Configurar JwtBearer en Maintenance con las mismas validaciones.
7. Probar Maintenance sin token, con token válido, alterado y expirado.
8. Aplicar la matriz de roles a Maintenance.
9. Probar 201 con MaintenanceSupervisor y 403 con Planner al crear una orden.
10. Verificar que 401 y 403 provienen del middleware de autenticación/autorización.
11. Configurar Swagger en Identity para documentar login.
12. Configurar Swagger Bearer en Planning y Maintenance y documentar roles por endpoint.
13. Fijar los tres puertos locales y verificar que no colisionan.
14. Crear `MineOps.Demo.http` con variables para tokens y solicitudes en el orden del video.
15. Ejecutar las tres API simultáneamente y recorrer manualmente el archivo de demostración.
16. Ejecutar build y todas las suites; detener la fase ante cualquier fallo.

## Secuencia de `MineOps.Demo.http`

1. Health Identity.
2. Health Planning.
3. Health Maintenance.
4. Login Admin.
5. Login Planner.
6. Login MaintenanceSupervisor.
7. Planning sin token → 401.
8. Planning con token Planner → permitido.
9. Crear actividad con Planner → 201.
10. Maintenance con el mismo token Planner → 403.
11. Maintenance con MaintenanceSupervisor → permitido.
12. Crear orden con MaintenanceSupervisor → 201.
13. JWT alterado contra Planning → 401.
14. JWT emitido con expiración de prueba y ya vencido → 401.

## Criterios de aceptación

- [ ] Las tres API pueden ejecutarse simultáneamente en 5001, 5002 y 5003.
- [ ] Los tres health checks responden 200 cuando sus bases están disponibles.
- [ ] Planning y Maintenance validan firma, issuer, audience, lifetime y signing key.
- [ ] Ningún controlador decodifica o valida manualmente JWT.
- [ ] Ningún controlador implementa condicionales manuales de rol.
- [ ] Todos los endpoints coinciden con la matriz de autorización.
- [ ] Sin token, Planning y Maintenance devuelven 401.
- [ ] Un token alterado devuelve 401.
- [ ] Un token expirado devuelve 401 por JwtBearer Middleware.
- [ ] Planner crea una actividad y recibe 201.
- [ ] Ese mismo token Planner intenta crear una orden y recibe 403.
- [ ] MaintenanceSupervisor crea una orden y recibe 201.
- [ ] Operator puede consultar y ejecutar start/complete en Planning, pero no administrar Maintenance.
- [ ] Swagger expone el botón Authorize en Planning y Maintenance.
- [ ] `MineOps.Demo.http` permite repetir el guion sin editar URLs ni cuerpos estructurales.
- [ ] Todas las pruebas de seguridad terminan en verde sobre SQL Server local.

## Decisiones

- **Sí:** autenticación y autorización estándar de ASP.NET Core.
- **Sí:** firma simétrica y validación local para esta entrega académica.
- **Sí:** un issuer y una audience compartidos para simplificar la demostración.
- **Sí:** health endpoints anónimos para comprobar procesos antes de autenticar.
- **No:** introspección contra Identity en cada request.
- **No:** permisos granulares, scopes, ABAC o policies complejas.
- **No:** lógica artificial de expiración en controladores.
- **No:** Testcontainers, porque la ejecución debe usar el SQL Server local del equipo.

## Riesgos

| Riesgo | Mitigación |
| --- | --- |
| Un `ClockSkew` predeterminado hace que el token parezca válido después de expirar | Fijar `ClockSkew = TimeSpan.Zero` y controlar el reloj en pruebas. |
| La clave o parámetros difieren entre servicios | Probar tokens emitidos por Identity contra ambas API en la misma suite de seguridad. |
| Las bases temporales quedan abandonadas tras una prueba interrumpida | Usar prefijo identificable, limpieza en `finally` y documentar un comando seguro de mantenimiento. |
| Los puertos están ocupados durante el video | Verificar los tres puertos en el checklist previo y documentar cómo cambiarlos conjuntamente. |

## Qué **no** incluye esta spec

- Refresh tokens, revocación, MFA o autorización granular.
- API Gateway, Docker o mensajería.
- Seguridad de infraestructura de producción.

