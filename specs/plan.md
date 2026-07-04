## PLAN — Chronos: Sistema de Gestión Académica Personal para Estudiantes Universitarios de la UNSCH basado en Spec-Driven Development (SDD), Ayacucho 2026

---

## Plan Técnico de Implementación

**Versión:** 1.0

**Fecha:** Julio 2026

**Basado en:** SPEC.md v1.0

---

## 1. Arquitectura del sistema

Chronos sigue una **arquitectura N-Tier** con 5 proyectos independientes:

```
Chronos/
├── Chronos.Domain/              # Entidades del dominio (sin dependencias externas)
│   └── Entities/
│       ├── Usuario.cs
│       ├── Tarea.cs
│       └── Examen.cs
├── Chronos.Infrastructure/      # Persistencia de datos
│   ├── ChronosDbContext.cs
│   └── Migrations/
├── Chronos.Web/                 # Capa de presentación MVC
│   ├── Controllers/
│   ├── Views/
│   └── Models/ (ViewModels)
├── Chronos.Tests/               # Pruebas unitarias MSTest
└── Chronos.IntegrationTests/    # Pruebas de integración xUnit
```
### Diagrama de dependencias

```
Chronos.Web ──────────────→ Chronos.Domain
│                            ↑
└──→ Chronos.Infrastructure ─┘
│
↓
SQL Server
Chronos.Tests ──────────────→ Chronos.Domain
Chronos.IntegrationTests ───→ Chronos.Infrastructure
───→ Chronos.Web (via WebApplicationFactory)
```
---

## 2. Stack tecnológico

| Categoría | Tecnología | Versión | Justificación |
|---|---|---|---|
| Framework web | ASP.NET Core MVC | .NET 10 | Arquitectura MVC robusta, soporte LTS |
| ORM | Entity Framework Core | 10.0.8 | Migraciones automáticas, LINQ |
| Base de datos | SQL Server | 2022 | Relacional, soporte CASCADE DELETE |
| Frontend | Razor Views + Bootstrap | 5.3.0 | Diseño responsivo sin framework JS |
| Gráficas | Chart.js | 4.4.0 | Gráfica de barras del dashboard |
| Hashing | SHA-256 (System.Security) | Built-in | Seguridad de contraseñas |
| Pruebas unitarias | MSTest + EF InMemory | 3.x / 10.0.8 | Aislamiento, velocidad |
| Pruebas integración | xUnit + Testcontainers | 2.x / 4.12.0 | SQL Server real en Docker |
| IA utilizada | Claude Sonnet (Anthropic) | 4.6 | Diseño, desarrollo, documentación |
| IA utilizada | GitHub Copilot (Microsoft) | Haiku 4.5 | Generación de pruebas unitarias |

---

## 3. Modelo de datos

### Entidad: Usuario

```
Usuarios
├── Id: int (PK, identity)
├── Nombre: nvarchar(100) NOT NULL
├── Apellido: nvarchar(100) NOT NULL
├── Carrera: nvarchar(150) NOT NULL
├── Email: nvarchar(200) NOT NULL UNIQUE
├── PasswordHash: nvarchar(max) NOT NULL
├── FechaNacimiento: datetime2 NOT NULL
├── RecordatorioHoras: int DEFAULT 24
├── PreguntaSecreta: nvarchar(max) NULL
└── RespuestaSecretaHash: nvarchar(max) NULL
```
### Entidad: Tarea

```
Tareas
├── Id: int (PK, identity)
├── Titulo: nvarchar(200) NOT NULL
├── Curso: nvarchar(100) NOT NULL
├── FechaEntrega: datetime2 NOT NULL
├── Prioridad: nvarchar(20) NOT NULL  → "Alta" | "Media" | "Baja"
├── Estado: nvarchar(20) NOT NULL     → "Pendiente" | "Entregada"
├── CreadoEn: datetime2 NOT NULL
└── UsuarioId: int NOT NULL (FK → Usuarios, CASCADE DELETE)
```
### Entidad: Examen

```
Examenes
├── Id: int (PK, identity)
├── Curso: nvarchar(100) NOT NULL
├── Tema: nvarchar(200) NOT NULL
├── FechaExamen: datetime2 NOT NULL
├── Lugar: nvarchar(100) NOT NULL
├── Prioridad: nvarchar(20) NOT NULL  → "Alta" | "Media" | "Baja"
├── Estado: nvarchar(20) NOT NULL     → "Pendiente" | "Rendido"
├── CreadoEn: datetime2 NOT NULL
└── UsuarioId: int NOT NULL (FK → Usuarios, CASCADE DELETE)
```
---

## 4. Decisiones técnicas clave

### DT-01: SHA-256 con Convert.ToHexString() — NO Base64

**Decisión:** Usar `Convert.ToHexString()` para el hash de contraseñas.

**Razón:** Produce exactamente 64 caracteres hexadecimales, predecible y
consistente. Base64 produce caracteres variables y puede incluir `+`, `/`, `=`
que complican comparaciones.

**Impacto:** Toda verificación de contraseña y respuesta secreta usa este formato.

### DT-02: Autenticación por sesión (sin ASP.NET Identity)

**Decisión:** Usar `HttpContext.Session` con claves `UsuarioId` y `UsuarioNombre`.

**Razón:** Simplicidad para el contexto académico. ASP.NET Identity agrega
complejidad innecesaria (roles, claims, tokens JWT) que está fuera de alcance.

**Impacto:** Las pruebas de integración simulan sesión mediante cookies.

### DT-03: EF Core InMemory para pruebas unitarias

**Decisión:** Usar base de datos en memoria con `Guid.NewGuid().ToString()`
como nombre de BD por prueba.

**Razón:** Garantiza aislamiento total entre pruebas. Cada prueba tiene su
propia BD efímera, eliminando interferencias.

**Impacto:** 81 pruebas unitarias completamente independientes.

### DT-04: Testcontainers para pruebas de integración

**Decisión:** Usar `MsSqlContainer` de Testcontainers con `WebApplicationFactory`.

**Razón:** Las pruebas de integración deben ejecutarse contra SQL Server real
para validar constraints (UNIQUE, CASCADE DELETE) que InMemory no soporta.

**Impacto:** Requiere Docker Desktop. 12 pruebas de integración con BD real.

### DT-05: Arquitectura N-Tier sin repositorios

**Decisión:** Los controladores acceden directamente a `ChronosDbContext`.

**Razón:** Para el alcance del sistema (académico, un solo desarrollador),
agregar repositorios e interfaces sería over-engineering sin beneficio real.

**Impacto:** Código más simple y directo, fácil de mantener.

---

## 5. Seguridad

| Mecanismo | Implementación |
|---|---|
| Hashing de contraseñas | SHA-256 + Convert.ToHexString() |
| Hashing de respuesta secreta | SHA-256 + Convert.ToHexString() |
| Protección de rutas | Verificación de sesión en cada controlador |
| Token temporal | SessionKey para operaciones sensibles |
| Aislamiento de datos | Filtro por UsuarioId en todas las consultas |

---

## 6. Planificación del desarrollo

### Fase 1 — Análisis (Semana 1)

| Tarea | Estado |
|---|---|
| Identificar problema y usuarios objetivo | ✅ Completado |
| Definir 12 requerimientos funcionales | ✅ Completado |
| Definir 8 requerimientos no funcionales | ✅ Completado |
| Elaborar 8 casos de uso | ✅ Completado |
| Crear diagrama de casos de uso | ✅ Completado |

### Fase 2 — Diseño (Semana 2)

| Tarea | Estado |
|---|---|
| Definir arquitectura N-Tier | ✅ Completado |
| Diseñar modelo de datos (3 entidades) | ✅ Completado |
| Crear diagrama de clases | ✅ Completado |
| Crear diagrama entidad-relación | ✅ Completado |
| Definir flujos principales (autenticación, tareas, perfil) | ✅ Completado |
| Elaborar SPEC.md como fuente de verdad | ✅ Completado |

### Fase 3 — Implementación (Semanas 3-4)

| Tarea | Estado |
|---|---|
| Crear solución con 4 proyectos en Visual Studio | ✅ Completado |
| Implementar entidades del dominio | ✅ Completado |
| Configurar ChronosDbContext y migraciones | ✅ Completado |
| Implementar AccountController (registro, login, recuperación) | ✅ Completado |
| Implementar TareasController (CRUD completo) | ✅ Completado |
| Implementar ExamenesController (CRUD completo) | ✅ Completado |
| Implementar DashboardController (estadísticas y urgentes) | ✅ Completado |
| Implementar PerfilController (edición, contraseña, pregunta) | ✅ Completado |
| Diseñar vistas Razor con Bootstrap 5 | ✅ Completado |
| Implementar alertas de color por vencimiento | ✅ Completado |
| Integrar Chart.js para gráfica de progreso | ✅ Completado |

### Fase 4 — Pruebas (Semana 5)

| Tarea | Estado |
|---|---|
| Pruebas unitarias TareasServiceTests (13 pruebas) | ✅ Completado |
| Pruebas unitarias ExamenesServiceTests (16 pruebas) | ✅ Completado |
| Pruebas unitarias UsuarioServiceTests (14 pruebas) | ✅ Completado |
| Pruebas unitarias DashboardServiceTests (11 pruebas) | ✅ Completado |
| Pruebas unitarias PerfilServiceTests (15 pruebas) | ✅ Completado |
| Pruebas de integración con Testcontainers (12 pruebas) | ✅ Completado |
| Verificar cobertura de código ≥ 90% | ✅ Completado (99.4%) |

### Fase 5 — Despliegue (Semana 6)

| Tarea | Estado |
|---|---|
| Publicar código en GitHub | ✅ Completado |
| Configurar estructura SDD con GitHub Spec Kit | ✅ Completado |
| Generar artefactos .md (SPEC, PLAN, TASKS) | ✅ Completado |
| Elaborar informe Word final | 🔄 En progreso |

---

## 7. Herramientas de desarrollo

| Herramienta | Uso |
|---|---|
| Visual Studio 2022 | IDE principal de desarrollo |
| SQL Server Management Studio | Gestión de base de datos |
| GitHub Desktop | Control de versiones |
| GitHub Spec Kit | Metodología SDD — artefactos .md |
| Docker Desktop | Contenedores para pruebas de integración |
| Claude (Anthropic) | Asistencia en diseño y desarrollo |
| GitHub Copilot | Generación de pruebas unitarias |