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
| TareasServiceTests | Tareas | 13 | ✅ Verde |
| ExamenesServiceTests | Exámenes | 16 | ✅ Verde |
| UsuarioServiceTests | Usuario | 14 | ✅ Verde |
| DashboardServiceTests | Dashboard | 11 | ✅ Verde |
| PerfilServiceTests | Perfil | 15 | ✅ Verde |
| ChronosIntegrationTests | Integración | 12 | ✅ Verde |
| **Total** | | **81 + 12 = 93** | **✅ 100% verde** |

**Cobertura de código: 99.4%** — supera el objetivo del 90% ✅

---

## 📁 Artefactos SDD

Los artefactos de la metodología Spec-Driven Development están en la carpeta `/specs`:

| Artefacto | Descripción |
|---|---|
| [SPEC.md](specs/spec.md) | Especificación completa — fuente de verdad |
| [PLAN.md](specs/plan.md) | Arquitectura, decisiones técnicas y planificación |
| [TASKS.md](specs/tasks.md) | 27 tareas del ciclo de vida completo |

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
    "DefaultConnection": "Server=TU_SERVIDOR;Database=ChronosDb;Trusted_Connection=True;MultipleActiveResultSets=true"
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

**6. Ejecutar pruebas de integración** (requiere Docker Desktop)
```bash
dotnet test Chronos.IntegrationTests
```

---

## 👩‍💻 Autora

**Gómez Tineo, Angélica Gabriela**  
Código: 27220131  
Escuela Profesional de Ingeniería de Sistemas  
Universidad Nacional de San Cristóbal de Huamanga  

**Curso:** Laboratorio - Pruebas y Aseguramiento de Calidad de Software  
**Docente:** Ing. Zapata Casaverde, Richard  

---

## 🤖 IA utilizada

| IA | Empresa | Uso |
|---|---|---|
| Claude Sonnet 4.6 | Anthropic | Diseño de arquitectura, desarrollo, documentación SDD |
| GitHub Copilot (Haiku 4.5) | Microsoft | Generación de pruebas unitarias MSTest |