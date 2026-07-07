# 🕐 Chronos
## Sistema de Gestión Académica Personal para Estudiantes Universitarios de la UNSCH basado en Spec-Driven Development (SDD), Ayacucho 2026

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)
![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core_MVC-10.0-512BD4?style=flat&logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?style=flat&logo=microsoftsqlserver)
![Cobertura](https://img.shields.io/badge/Cobertura-92.9%25-brightgreen?style=flat)
![Pruebas](https://img.shields.io/badge/Pruebas-120_en_verde-brightgreen?style=flat)
![Metodología](https://img.shields.io/badge/Metodología-SDD-blue?style=flat)

---

## 📋 Descripción

**Chronos** es una aplicación web de gestión académica personal desarrollada
para estudiantes universitarios de la UNSCH. Permite organizar tareas y exámenes
con alertas visuales de vencimiento, estadísticas personalizadas y mecanismos
de seguridad robustos.

Desarrollado siguiendo la metodología **Spec-Driven Development (SDD)** con
asistencia de Inteligencia Artificial (Claude Sonnet de Anthropic y GitHub Copilot
de Microsoft), cubriendo el ciclo de vida completo del software: Análisis, Diseño,
Implementación, Pruebas y Despliegue.

---

## 🎯 Funcionalidades principales

- ✅ Registro e inicio de sesión seguro con hash SHA-256
- ✅ Recuperación de contraseña por pregunta secreta (sin email externo)
- ✅ Gestión completa de tareas académicas (CRUD) con alertas de color
- ✅ Gestión completa de exámenes (CRUD) con alertas de color
- ✅ Panel de control con estadísticas y actividades urgentes de 15 días
- ✅ Filtros y búsqueda en tiempo real
- ✅ Gestión de perfil con verificación de identidad previa
- ✅ Diseño responsivo con Bootstrap 5

---

## 🏗️ Arquitectura

```
Chronos/
├── Chronos.Domain/         # Entidades del dominio
├── Chronos.Infrastructure/ # DbContext + Migraciones
├── Chronos.Web/             # Controladores + Vistas MVC
├── Chronos.Tests/           # 52 pruebas unitarias MSTest
└── Chronos.IntegrationTests/ # 68 pruebas de integración xUnit
```

---

## 🛠️ Stack tecnológico

| Tecnología | Versión | Uso |
|---|---|---|
| ASP.NET Core MVC | .NET 10 | Framework principal |
| Entity Framework Core | 10.0.8 | ORM y migraciones |
| SQL Server | 2022 | Base de datos |
| Bootstrap | 5.3.0 | Diseño responsivo |
| Chart.js | 4.4.0 | Gráfica de progreso |
| MSTest + EF InMemory | 3.x | Pruebas unitarias |
| xUnit + Testcontainers | 2.x / 4.12.0 | Pruebas de integración |

---

## 🧪 Pruebas

| Clase | Módulo | Pruebas | Estado |
|---|---|---|---|
| TareasServiceTests | Tareas | 9 | ✅ Verde |
| ExamenesServiceTests | Exámenes | 9 | ✅ Verde |
| UsuarioServiceTests | Usuario | 11 | ✅ Verde |
| DashboardServiceTests | Dashboard | 8 | ✅ Verde |
| PerfilServiceTests | Perfil | 15 | ✅ Verde |
| ChronosIntegrationTests | Integración | 68 | ✅ Verde |
| **Total** | | **52 + 68 = 120** | **✅ 100% verde** |

**Cobertura de código: 92.9% bloques / 93.8% líneas** — supera el objetivo del 90% ✅

---

## 📁 Artefactos SDD

Los artefactos de la metodología Spec-Driven Development están en la carpeta `/specs`:

| Artefacto | Descripción |
|---|---|
| [SPEC.md](specs/spec.md) | Especificación completa — fuente de verdad |
| [PLAN.md](specs/plan.md) | Arquitectura, decisiones técnicas y planificación |
| [TASKS.md](specs/tasks.md) | 28 tareas del ciclo de vida completo |

---

## 🚀 Cómo ejecutar el proyecto

### Requisitos previos
- Visual Studio 2022
- .NET 10 SDK
- SQL Server 2022
- Docker Desktop (solo para pruebas de integración)

### Pasos

**1. Clonar el repositorio**
```bash
git clone https://github.com/GabrielaCode2104/CHRONOS.git
```

**2. Configurar la cadena de conexión**

En `Chronos.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=ChronosDb;
    Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**3. Aplicar migraciones**
```bash
cd Chronos/Chronos.Web
dotnet ef database update --project ../Chronos.Infrastructure
```

**4. Ejecutar la aplicación**
```bash
dotnet run
```

O presiona **F5** en Visual Studio.

**5. Ejecutar pruebas unitarias**
```bash
dotnet test Chronos.Tests
```

**6. Ejecutar pruebas de integración** (requiere Docker Desktop corriendo)
```bash
dotnet test Chronos.IntegrationTests
```
**7. Ejecutar la aplicación en modo Producción (sin Visual Studio)**
```bash
cd Chronos.Web/bin/Release/net10.0/publish
./Chronos.Web.exe
```
Acceder en: http://localhost:5000

---

## 👩‍💻 Autora

**Gómez Tineo, Angélica Gabriela**  
Código: 27220131  
Escuela Profesional de Ingeniería de Sistemas  
Universidad Nacional de San Cristóbal de Huamanga  

**Curso:** Pruebas y Aseguramiento de la Calidad de Software  
**Docente:** Ing. Zapata Casaverde, Richard  

---

## 🤖 IA utilizada

| IA | Empresa | Uso |
|---|---|---|
| Claude Sonnet 4.6 | Anthropic | Diseño de arquitectura, desarrollo, documentación SDD, informe |
| GitHub Copilot (Haiku 4.5) | Microsoft | Generación de pruebas unitarias MSTest y pruebas de integración xUnit |