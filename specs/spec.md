# SPEC — Chronos: Sistema de Gestión Académica Personal para Estudiantes Universitarios de la UNSCH basado en Spec-Driven Development (SDD), Ayacucho 2026

---

**Versión:** 1.2 
**Fecha:** Julio 2026  
**Autora:** Gómez Tineo, Angélica Gabriela  
**Estado:** Implementado y desplegado ✅  

---

## 1. Propósito

Chronos es una aplicación web de gestión académica personal desarrollada para resolver
la falta de un sistema centralizado donde el estudiante universitario pueda registrar,
organizar y hacer seguimiento de sus tareas y exámenes académicos, con alertas visuales
de vencimiento y estadísticas personalizadas de progreso.

---

## 2. Problema que resuelve

Los estudiantes universitarios enfrentan dificultades para gestionar eficientemente sus
actividades académicas. La falta de un sistema centralizado genera olvidos, entregas
tardías y una deficiente organización del tiempo académico, afectando directamente el
rendimiento estudiantil.

Las soluciones existentes no están orientadas al contexto universitario peruano, carecen
de alertas visuales de vencimiento por prioridad, no integran estadísticas personalizadas
y no ofrecen mecanismos de recuperación de cuenta sin dependencia de correo electrónico.

---

## 3. Usuarios objetivo

- Estudiantes universitarios de la Universidad Nacional de San Cristóbal de Huamanga
- Usuarios con conectividad limitada (sin dependencia de servicios externos de email)

---

## 4. Funcionalidades (Features)

### FEAT-01: Registro e inicio de sesión seguro
El sistema permite registrar nuevos usuarios y autenticarlos mediante correo electrónico
y contraseña. Las contraseñas se almacenan como hash SHA-256 en formato hexadecimal.

**Criterios de aceptación:**
- CA-01-1: El sistema rechaza emails duplicados con mensaje "El correo ya está registrado"
- CA-01-2: La contraseña se almacena como hash SHA-256 de 64 caracteres hexadecimales
- CA-01-3: El login exitoso redirige al Dashboard
- CA-01-4: Las credenciales incorrectas muestran "Correo o contraseña incorrectos"

### FEAT-02: Recuperación de contraseña por pregunta secreta
El sistema permite recuperar el acceso sin dependencia de correo electrónico, mediante
un proceso de dos pasos: verificación de email y respuesta a pregunta secreta.

**Criterios de aceptación:**
- CA-02-1: El sistema muestra la pregunta secreta al verificar el email
- CA-02-2: La respuesta incorrecta muestra "La respuesta secreta es incorrecta"
- CA-02-3: La respuesta correcta permite establecer una nueva contraseña
- CA-02-4: La nueva contraseña se almacena como hash SHA-256

### FEAT-03: Gestión completa de tareas
El sistema permite crear, editar, eliminar y marcar como entregadas las tareas
académicas, con campos de título, curso, fecha de entrega, hora y prioridad.

**Criterios de aceptación:**
- CA-03-1: Una tarea creada aparece en la lista con estado "Pendiente"
- CA-03-2: Al marcar como entregada, el estado cambia a "Entregada"
- CA-03-3: Al eliminar una tarea, desaparece permanentemente de la BD
- CA-03-4: Solo el usuario propietario puede ver y editar sus tareas
- CA-03-5: Las prioridades válidas son únicamente "Alta", "Media" y "Baja"

### FEAT-04: Alertas visuales de vencimiento en tareas
El sistema muestra alertas por colores según la proximidad de la fecha de entrega.

**Criterios de aceptación:**
- CA-04-1: Rojo para tareas vencidas o con menos de 24 horas restantes
- CA-04-2: Amarillo para tareas con 1 a 3 días restantes
- CA-04-3: Verde para tareas con más de 3 días restantes

### FEAT-05: Búsqueda y filtros en tareas
El sistema permite buscar y filtrar tareas en tiempo real.

**Criterios de aceptación:**
- CA-05-1: Búsqueda por título o curso en tiempo real
- CA-05-2: Filtro por prioridad (Alta, Media, Baja, Todas)
- CA-05-3: Filtro por estado (Pendiente, Entregada, Todos)

### FEAT-06: Gestión completa de exámenes
El sistema permite crear, editar, eliminar y marcar como rendidos los exámenes,
con campos de curso, tema, fecha, hora, lugar y prioridad.

**Criterios de aceptación:**
- CA-06-1: Un examen creado aparece en la lista con estado "Pendiente"
- CA-06-2: Al marcar como rendido, el estado cambia a "Rendido"
- CA-06-3: Al eliminar un examen, desaparece permanentemente de la BD
- CA-06-4: Solo el usuario propietario puede ver y editar sus exámenes

### FEAT-07: Alertas visuales de vencimiento en exámenes
El sistema muestra alertas por colores según la proximidad del examen.

**Criterios de aceptación:**
- CA-07-1: Rojo para exámenes vencidos o con menos de 24 horas
- CA-07-2: Amarillo para exámenes con 1 a 3 días restantes
- CA-07-3: Verde para exámenes con más de 3 días restantes

### FEAT-08: Panel de control (Dashboard)
El sistema muestra un resumen personalizado de la actividad académica del estudiante.

**Criterios de aceptación:**
- CA-08-1: Muestra mini-semana con el día actual resaltado
- CA-08-2: Muestra estadísticas: tareas y exámenes de la semana, tareas vencidas,
  porcentaje de completado
- CA-08-3: Muestra actividades urgentes dentro de los próximos 15 días ordenadas
  por días restantes y prioridad
- CA-08-4: Las actividades con fecha pasada NO aparecen en urgentes
- CA-08-5: Si no hay actividades urgentes, muestra "Sin tareas urgentes"
- CA-08-6: Muestra gráfica de barras con progreso académico

### FEAT-09: Gestión de perfil de usuario
El sistema permite visualizar y editar datos personales, configurar pregunta secreta
y cambiar contraseña con verificación de identidad previa.

**Criterios de aceptación:**
- CA-09-1: La edición de datos personales actualiza la sesión activa
- CA-09-2: El cambio de contraseña requiere verificar la contraseña actual
- CA-09-3: La configuración de pregunta secreta requiere verificar la contraseña actual
- CA-09-4: Un email ya en uso no puede asignarse a otro usuario

### FEAT-10: Cierre de sesión
El sistema permite cerrar la sesión activa de forma segura.

**Criterios de aceptación:**
- CA-10-1: Al cerrar sesión se eliminan todos los datos de sesión activa
- CA-10-2: Tras cerrar sesión, las rutas protegidas redirigen al login

---

## 5. Reglas de negocio

- RN-01: El email es único por usuario en todo el sistema
- RN-02: Las contraseñas y respuestas secretas se almacenan SIEMPRE como hash SHA-256
  en formato hexadecimal (Convert.ToHexString), nunca en texto plano ni Base64
- RN-03: Las operaciones sensibles (cambio de contraseña, pregunta secreta) requieren
  verificación de identidad previa mediante token de sesión temporal
- RN-04: Cada tarea y examen pertenece exclusivamente a un usuario (aislamiento por UsuarioId)
- RN-05: El dashboard solo muestra actividades con fecha futura dentro de los próximos
  15 días, ordenadas primero por días restantes y luego por prioridad (Alta > Media > Baja)
- RN-06: El CASCADE DELETE elimina automáticamente tareas y exámenes al eliminar un usuario

---

## 6. Restricciones técnicas

- Framework: ASP.NET Core MVC con .NET 10
- Base de datos: SQL Server 2022
- ORM: Entity Framework Core 10.0.8
- Arquitectura: N-Tier (Domain, Infrastructure, Web, Tests, IntegrationTests)
- Autenticación: Sesiones con HttpContext.Session (UsuarioId, UsuarioNombre)
- Hashing: SHA-256 con Convert.ToHexString() — NO Base64
- Pruebas unitarias: MSTest + EF Core InMemory (cobertura objetivo ≥ 90%)
- Pruebas de integración: xUnit + Testcontainers.MsSql + WebApplicationFactory
- Frontend: Razor Views + Bootstrap 5.3 + Chart.js 4.4

---

## 7. Fuera de alcance

- Pasarela de pagos
- Chat en tiempo real entre usuarios
- Aplicación móvil nativa
- Notificaciones push o por correo electrónico

---

## 8. Trazabilidad pruebas → especificación

| Clase de prueba | Módulo | Pruebas | Features cubiertas |
|---|---|---|---|
| TareasServiceTests | Tareas | 9 | FEAT-03, FEAT-04, FEAT-05 |
| ExamenesServiceTests | Exámenes | 9 | FEAT-06, FEAT-07 |
| UsuarioServiceTests | Usuario | 11 | FEAT-01, FEAT-02, RN-01, RN-02 |
| DashboardServiceTests | Dashboard | 8 | FEAT-08, RN-05 |
| PerfilServiceTests | Perfil | 15 | FEAT-09, RN-03 |
| ChronosIntegrationTests | Integración | 77 | FEAT-01 al FEAT-10 |
| **Total** | | **129** | **Todas las features** |

**Cobertura de código:** 94.1% en bloques, 95.1% en líneas — supera el objetivo del 90% ✅