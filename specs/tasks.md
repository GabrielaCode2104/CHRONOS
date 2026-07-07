# TASKS — Chronos
## Sistema de Gestión Académica Personal para Estudiantes Universitarios de la UNSCH basado en Spec-Driven Development (SDD), Ayacucho 2026

**Versión:** 1.1  
**Fecha:** Julio 2026  
**Basado en:** SPEC.md v1.0 + PLAN.md v1.1  

---

## Fase 1 — Análisis

### TASK-01: Identificación del problema
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Identificar y documentar el problema de gestión académica
en estudiantes universitarios de la UNSCH.  
**Criterio de validación:** Problema documentado con justificación clara
de por qué las soluciones existentes no aplican al contexto.

### TASK-02: Definición de requerimientos funcionales
**Feature relacionada:** FEAT-01 al FEAT-10  
**Estado:** ✅ Completado  
**Descripción:** Identificar y documentar los 12 requerimientos funcionales
del sistema con código, nombre y descripción.  
**Criterio de validación:** 12 RF documentados, todos implementados al 100%.

### TASK-03: Definición de requerimientos no funcionales
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Documentar los 8 requerimientos no funcionales: seguridad,
usabilidad, rendimiento, mantenibilidad, calidad, portabilidad,
disponibilidad y trazabilidad.  
**Criterio de validación:** 8 RNF documentados con código y descripción.

### TASK-04: Elaboración de casos de uso
**Feature relacionada:** FEAT-01 al FEAT-10  
**Estado:** ✅ Completado  
**Descripción:** Documentar 8 casos de uso con actor, precondición,
flujo principal, flujo alternativo y postcondición.  
**Criterio de validación:** CU-01 al CU-08 completos con diagrama UML.

---

## Fase 2 — Diseño

### TASK-05: Definición de arquitectura N-Tier
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Diseñar la arquitectura en capas con 5 proyectos:
Domain, Infrastructure, Web, Tests, IntegrationTests.  
**Criterio de validación:** Diagrama de arquitectura con dependencias
entre proyectos documentado.

### TASK-06: Diseño del modelo de datos
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Diseñar las 3 entidades del sistema (Usuario, Tarea, Examen)
con sus atributos, tipos de datos, restricciones y relaciones.  
**Criterio de validación:** Diagrama ER con PK, FK, UNIQUE y CASCADE DELETE.

### TASK-07: Diseño de flujos principales
**Feature relacionada:** FEAT-01, FEAT-02, FEAT-03, FEAT-06, FEAT-09  
**Estado:** ✅ Completado  
**Descripción:** Crear diagramas de flujo para autenticación,
gestión de tareas/exámenes y perfil de usuario.  
**Criterio de validación:** 3 diagramas de flujo completos con
flujos alternativos de error.

### TASK-08: Elaboración de SPEC.md
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Crear la especificación completa del sistema como
fuente de verdad SDD con features, criterios de aceptación y
reglas de negocio.  
**Criterio de validación:** SPEC.md publicado en el repositorio GitHub
con trazabilidad pruebas → features.

---

## Fase 3 — Implementación

### TASK-09: Configuración de la solución
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Crear solución en Visual Studio con 5 proyectos,
configurar referencias entre proyectos e instalar paquetes NuGet.  
**Criterio de validación:** Solución compila sin errores.
Todos los proyectos referenciados correctamente.

### TASK-10: Implementación de entidades del dominio
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Crear clases Usuario, Tarea y Examen en Chronos.Domain
con anotaciones de validación y relaciones de navegación.  
**Criterio de validación:** Entidades con DataAnnotations correctas.
Relación 1:N Usuario→Tareas y 1:N Usuario→Examenes.

### TASK-11: Configuración de ChronosDbContext y migraciones
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Crear ChronosDbContext con DbSets, configurar
CASCADE DELETE, índice UNIQUE en Email y cadena de conexión.  
**Criterio de validación:** 2 migraciones aplicadas correctamente.
Base de datos ChronosDb creada en SQL Server.

### TASK-12: Implementación de autenticación
**Feature relacionada:** FEAT-01, FEAT-02  
**Estado:** ✅ Completado  
**Descripción:** Implementar AccountController con acciones de
registro, login, recuperación de contraseña y verificación
por pregunta secreta.  
**Criterio de validación:** CA-01-1 al CA-01-4 y CA-02-1 al CA-02-4
verificados y pasando.

### TASK-13: Implementación de gestión de tareas
**Feature relacionada:** FEAT-03, FEAT-04, FEAT-05  
**Estado:** ✅ Completado  
**Descripción:** Implementar TareasController con CRUD completo,
alertas de color por vencimiento y búsqueda/filtros en tiempo real.  
**Criterio de validación:** CA-03-1 al CA-05-3 verificados y pasando.

### TASK-14: Implementación de gestión de exámenes
**Feature relacionada:** FEAT-06, FEAT-07, FEAT-08  
**Estado:** ✅ Completado  
**Descripción:** Implementar ExamenesController con CRUD completo,
alertas de color por vencimiento y búsqueda/filtros en tiempo real.  
**Criterio de validación:** CA-06-1 al CA-07-3 verificados y pasando.

### TASK-15: Implementación del Dashboard
**Feature relacionada:** FEAT-08  
**Estado:** ✅ Completado  
**Descripción:** Implementar DashboardController con mini-semana,
estadísticas personalizadas, actividades urgentes de 15 días
ordenadas por días restantes y prioridad, y gráfica Chart.js.  
**Criterio de validación:** CA-08-1 al CA-08-6 verificados y pasando.

### TASK-16: Implementación de perfil de usuario
**Feature relacionada:** FEAT-09, FEAT-10  
**Estado:** ✅ Completado  
**Descripción:** Implementar PerfilController con edición de datos,
cambio de contraseña y configuración de pregunta secreta,
todas con verificación de identidad previa.  
**Criterio de validación:** CA-09-1 al CA-10-2 verificados y pasando.

### TASK-17: Diseño de vistas Razor
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Crear vistas responsivas con Bootstrap 5 para
todas las funcionalidades, incluyendo alertas de color,
filtros en tiempo real y dashboard interactivo.  
**Criterio de validación:** Interfaz responsiva, alertas de color
correctas, filtros funcionando sin recarga de página.

---

## Fase 4 — Pruebas

### TASK-18: Pruebas unitarias de Tareas
**Feature relacionada:** FEAT-03, FEAT-04, FEAT-05  
**Estado:** ✅ Completado  
**Herramienta:** MSTest + EF Core InMemory  
**Pruebas:** 9  
**Escenarios cubiertos:**
- Crear tarea con datos válidos
- Asignar fecha de creación automáticamente
- Editar propiedades correctamente
- Eliminar tarea específica
- Marcar como entregada
- Obtener tareas por usuario
- Validar que fecha pasada no aparezca en urgentes
- Ordenamiento por prioridad Alta > Media > Baja

**Criterio de validación:** 9/9 pruebas en verde ✅

### TASK-19: Pruebas unitarias de Exámenes
**Feature relacionada:** FEAT-06, FEAT-07  
**Estado:** ✅ Completado  
**Herramienta:** MSTest + EF Core InMemory  
**Pruebas:** 9  
**Escenarios cubiertos:**
- Crear examen con datos válidos
- Editar propiedades correctamente
- Eliminar examen específico
- Marcar como rendido
- Obtener exámenes por usuario
- Validar que exámenes con más de 15 días no aparezcan en urgentes
- Ordenamiento por prioridad y fecha

**Criterio de validación:** 9/9 pruebas en verde ✅

### TASK-20: Pruebas unitarias de Usuario
**Feature relacionada:** FEAT-01, FEAT-02, RN-01, RN-02  
**Estado:** ✅ Completado  
**Herramienta:** MSTest + EF Core InMemory  
**Pruebas:** 11  
**Escenarios cubiertos:**
- Registrar usuario con campos requeridos
- Validar email único (no duplicados)
- Verificar hash SHA-256 en formato hexadecimal de 64 caracteres
- Validar longitud máxima de campos
- Editar datos de usuario
- Eliminar usuario

**Criterio de validación:** 11/11 pruebas en verde ✅

### TASK-21: Pruebas unitarias de Dashboard
**Feature relacionada:** FEAT-08, RN-05  
**Estado:** ✅ Completado  
**Herramienta:** MSTest + EF Core InMemory  
**Pruebas:** 8  
**Escenarios cubiertos:**
- Mostrar solo actividades pendientes dentro de 15 días
- Excluir actividades con fecha pasada
- Ordenar por días restantes luego por prioridad
- Aislamiento por usuario

**Criterio de validación:** 8/8 pruebas en verde ✅

### TASK-22: Pruebas unitarias de Perfil
**Feature relacionada:** FEAT-09, RN-03  
**Estado:** ✅ Completado  
**Herramienta:** MSTest + EF Core InMemory  
**Pruebas:** 15  
**Escenarios cubiertos:**
- Editar perfil de usuario
- Configurar pregunta secreta
- Cambiar contraseña con verificación previa
- Recuperación por pregunta secreta

**Criterio de validación:** 15/15 pruebas en verde ✅

### TASK-23: Pruebas de integración con Testcontainers
**Feature relacionada:** FEAT-01 al FEAT-10  
**Estado:** ✅ Completado  
**Herramienta:** xUnit + Testcontainers.MsSql + WebApplicationFactory  
**Pruebas:** 68  
**Escenarios cubiertos:**
- Autenticación: registro, login correcto, login incorrecto, email inexistente,
  password incorrecto, logout
- Recuperación de contraseña: GET vista, buscar cuenta con email existente/inexistente,
  verificar respuesta correcta/incorrecta, resetear contraseña válida/inválida/sin token
- Tareas: GET con/sin sesión, crear, completar, eliminar, editar, validar aislamiento
  por usuario, ModelState inválido, título vacío
- Exámenes: GET con/sin sesión, crear, completar, eliminar, editar, validar aislamiento
  por usuario, ModelState inválido, datos requeridos faltantes
- Dashboard: GET con sesión activa, sin sesión, con actividades y estadísticas,
  con tareas vencidas
- Perfil: GET, actualizar datos, verificar/cambiar contraseña, guardar pregunta secreta
- Vistas Razor: renderizado completo de Dashboard, Tareas Index/Crear, Exámenes Index/Crear
- Home: Index sin sesión, Error, Privacy

**Criterio de validación:** 68/68 pruebas en verde ✅  
**Nota:** Requiere Docker Desktop corriendo. Contenedor SQL Server compartido
mediante IClassFixture — duración total 11 segundos.

### TASK-24: Verificación de cobertura de código
**Feature relacionada:** RNF-05  
**Estado:** ✅ Completado  
**Resultado:**

| Ensamblado | Cobertura Bloques | Cobertura Líneas |
|---|---|---|
| Chronos.Domain | 100% | 100% |
| Chronos.Tests | 99.4% | 99.1% |
| Chronos.Web | 85.3% | 77.8% |
| Chronos.IntegrationTests | 93.6% | 96.0% |
| **Total general** | **92.9%** | **93.8%** |

**Criterio de validación:** Cobertura ≥ 90% ✅ (supera en 2.9% en bloques)

---

## Fase 5 — Despliegue

### TASK-25: Publicación en GitHub
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Publicar el código fuente completo en repositorio
público de GitHub con historial de commits.  
**Repositorio:** https://github.com/GabrielaCode2104/CHRONOS  
**Criterio de validación:** Repositorio público accesible con
todos los proyectos y artefactos SDD.

### TASK-26: Configuración de GitHub Spec Kit
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Instalar GitHub Spec Kit en el repositorio
para gestionar los artefactos SDD (SPEC.md, PLAN.md, TASKS.md).  
**Criterio de validación:** Carpeta specs/ con los 3 artefactos
publicada en GitHub.

### TASK-27: Elaboración del informe final
**Feature relacionada:** Todas  
**Estado:** ✅ Completado  
**Descripción:** Elaborar informe Word único con el ciclo de vida
completo del software: Análisis, Diseño, Implementación, Pruebas
y Despliegue, siguiendo la metodología SDD.  
**Criterio de validación:** Informe entregado el 7 de julio 2026. ✅

---

## Resumen de tareas

| Fase | Total tareas | Completadas | En progreso |
|---|---|---|---|
| Análisis | 4 | 4 | 0 |
| Diseño | 4 | 4 | 0 |
| Implementación | 9 | 9 | 0 |
| Pruebas | 7 | 7 | 0 |
| Despliegue | 3 | 3 | 0 |
| **Total** | **27** | **27** | **0** |

**Pruebas totales:** 52 unitarias + 68 integración = **120 pruebas**  
**Cobertura general:** 92.9% bloques / 93.8% líneas — supera el objetivo del 90% ✅