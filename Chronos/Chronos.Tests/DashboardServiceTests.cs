using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Chronos.Domain.Entities;
using Chronos.Infrastructure;

namespace Chronos.Tests
{
    [TestClass]
    public class DashboardServiceTests
    {
        private ChronosDbContext _context;

        [TestInitialize]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ChronosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ChronosDbContext(options);
        }

        [TestCleanup]
        public void TearDown()
        {
            _context?.Dispose();
        }

        #region Clase auxiliar para el Dashboard

        public class ActividadDashboard
        {
            public string Tipo { get; set; } // "Tarea" o "Examen"
            public string Nombre { get; set; }
            public DateTime Fecha { get; set; }
            public string Prioridad { get; set; }
            public int DiasRestantes { get; set; }
            public string Estado { get; set; }
        }

        #endregion

        #region Método para obtener actividades del Dashboard

        private List<ActividadDashboard> ObtenerActividadesDashboard(int usuarioId, DateTime? fechaReferencia = null)
        {
            var ahora = fechaReferencia ?? DateTime.Now;
            var fechaLimite = ahora.AddDays(15);

            var actividades = new List<ActividadDashboard>();

            // Obtener tareas pendientes dentro del rango
            var tareas = _context.Tareas
                .Where(t => t.UsuarioId == usuarioId &&
                            t.Estado == "Pendiente" &&
                            t.FechaEntrega >= ahora &&
                            t.FechaEntrega <= fechaLimite)
                .ToList()
                .Select(t => new ActividadDashboard
                {
                    Tipo = "Tarea",
                    Nombre = t.Titulo,
                    Fecha = t.FechaEntrega,
                    Prioridad = t.Prioridad,
                    DiasRestantes = (int)Math.Ceiling((t.FechaEntrega - ahora).TotalDays),
                    Estado = t.Estado
                })
                .ToList();

            // Obtener exámenes pendientes dentro del rango
            var examenes = _context.Examenes
                .Where(e => e.UsuarioId == usuarioId &&
                            e.Estado == "Pendiente" &&
                            e.FechaExamen >= ahora &&
                            e.FechaExamen <= fechaLimite)
                .ToList()
                .Select(e => new ActividadDashboard
                {
                    Tipo = "Examen",
                    Nombre = e.Tema,
                    Fecha = e.FechaExamen,
                    Prioridad = e.Prioridad,
                    DiasRestantes = (int)Math.Ceiling((e.FechaExamen - ahora).TotalDays),
                    Estado = e.Estado
                })
                .ToList();

            actividades.AddRange(tareas);
            actividades.AddRange(examenes);

            // Ordenar por días restantes (menor primero), luego por prioridad
            var actividadesOrdenadas = actividades
                .OrderBy(a => a.DiasRestantes)
                .ThenBy(a => ObtenerNivelPrioridad(a.Prioridad))
                .ToList();

            return actividadesOrdenadas;
        }

        #endregion

        #region Pruebas del Dashboard

        [TestMethod]
        public void ObtenerActividadesDashboard_MostrarSoloActividadesPendientesEntreLosProximos15Dias()
        {
            // Arrange
            var usuario = CrearUsuario("Juan", "Pérez", "juan@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var ahora = DateTime.Now;
            var tarea = new Tarea
            {
                Titulo = "Tarea dentro del rango",
                Curso = "Curso 1",
                FechaEntrega = ahora.AddDays(5),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tarea);
            _context.SaveChanges();

            // Act
            var actividades = ObtenerActividadesDashboard(usuario.Id);

            // Assert
            Assert.AreEqual(1, actividades.Count);
            Assert.AreEqual("Tarea dentro del rango", actividades[0].Nombre);
            Assert.AreEqual("Pendiente", actividades[0].Estado);
        }

        [TestMethod]
        public void ObtenerActividadesDashboard_NoMostrarActividadesConFechaPasada()
        {
            // Arrange
            var usuario = CrearUsuario("Ana", "García", "ana@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var ahora = DateTime.Now;
            var tareaConFechaPasada = new Tarea
            {
                Titulo = "Tarea con fecha pasada",
                Curso = "Curso 1",
                FechaEntrega = ahora.AddDays(-5),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tareaConFechaPasada);
            _context.SaveChanges();

            // Act
            var actividades = ObtenerActividadesDashboard(usuario.Id);

            // Assert
            Assert.AreEqual(0, actividades.Count);
        }

        [TestMethod]
        public void ObtenerActividadesDashboard_NoMostrarActividadesConMasDe15Dias()
        {
            // Arrange
            var usuario = CrearUsuario("Carlos", "López", "carlos@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var ahora = DateTime.Now;
            var tareaFueraDePlazo = new Tarea
            {
                Titulo = "Tarea fuera de plazo",
                Curso = "Curso 1",
                FechaEntrega = ahora.AddDays(20),
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tareaFueraDePlazo);
            _context.SaveChanges();

            // Act
            var actividades = ObtenerActividadesDashboard(usuario.Id);

            // Assert
            Assert.AreEqual(0, actividades.Count);
        }

        [TestMethod]
        public void ObtenerActividadesDashboard_IncluirTantoCursoComoExamenes()
        {
            // Arrange
            var usuario = CrearUsuario("María", "Rodríguez", "maria@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var ahora = DateTime.Now;
            var tarea = new Tarea
            {
                Titulo = "Tarea 1",
                Curso = "Curso 1",
                FechaEntrega = ahora.AddDays(5),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var examen = new Examen
            {
                Curso = "Curso 2",
                Tema = "Examen 1",
                FechaExamen = ahora.AddDays(7),
                Lugar = "Aula 1",
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tarea);
            _context.Examenes.Add(examen);
            _context.SaveChanges();

            // Act
            var actividades = ObtenerActividadesDashboard(usuario.Id);

            // Assert
            Assert.AreEqual(2, actividades.Count);
            Assert.AreEqual(1, actividades.Count(a => a.Tipo == "Tarea"));
            Assert.AreEqual(1, actividades.Count(a => a.Tipo == "Examen"));
        }

        [TestMethod]
        public void ObtenerActividadesDashboard_ExcluirTareasEntregadasYExamenesRendidos()
        {
            // Arrange
            var usuario = CrearUsuario("Pedro", "González", "pedro@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var ahora = DateTime.Now;
            var tareaEntregada = new Tarea
            {
                Titulo = "Tarea entregada",
                Curso = "Curso 1",
                FechaEntrega = ahora.AddDays(5),
                Prioridad = "Alta",
                Estado = "Entregada",
                UsuarioId = usuario.Id
            };

            var examenRendido = new Examen
            {
                Curso = "Curso 2",
                Tema = "Examen rendido",
                FechaExamen = ahora.AddDays(7),
                Lugar = "Aula 1",
                Prioridad = "Media",
                Estado = "Rendido",
                UsuarioId = usuario.Id
            };

            var tareaPendiente = new Tarea
            {
                Titulo = "Tarea pendiente",
                Curso = "Curso 3",
                FechaEntrega = ahora.AddDays(3),
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tareaEntregada);
            _context.Examenes.Add(examenRendido);
            _context.Tareas.Add(tareaPendiente);
            _context.SaveChanges();

            // Act
            var actividades = ObtenerActividadesDashboard(usuario.Id);

            // Assert
            Assert.AreEqual(1, actividades.Count);
            Assert.AreEqual("Tarea pendiente", actividades[0].Nombre);
            Assert.AreEqual("Pendiente", actividades[0].Estado);
        }

        [TestMethod]
        public void ObtenerActividadesDashboard_OrdenarPorDiasRestantesYPrioridad()
        {
            // Arrange
            var usuario = CrearUsuario("Sofia", "Martínez", "sofia@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var ahora = DateTime.Now;

            // Crear actividades con diferentes fechas y prioridades
            var tarea1 = new Tarea
            {
                Titulo = "Tarea en 10 días - Baja",
                Curso = "Curso 1",
                FechaEntrega = ahora.AddDays(10),
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var tarea2 = new Tarea
            {
                Titulo = "Tarea en 10 días - Alta",
                Curso = "Curso 2",
                FechaEntrega = ahora.AddDays(10),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var examen1 = new Examen
            {
                Curso = "Curso 3",
                Tema = "Examen en 5 días - Media",
                FechaExamen = ahora.AddDays(5),
                Lugar = "Aula 1",
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tarea1);
            _context.Tareas.Add(tarea2);
            _context.Examenes.Add(examen1);
            _context.SaveChanges();

            // Act
            var actividades = ObtenerActividadesDashboard(usuario.Id, ahora);

            // Assert
            Assert.AreEqual(3, actividades.Count);
            // Primero por días restantes (5 < 10), luego por prioridad dentro del mismo día
            Assert.AreEqual("Examen en 5 días - Media", actividades[0].Nombre);
            Assert.IsTrue(actividades[0].DiasRestantes >= 5 && actividades[0].DiasRestantes <= 6);
            Assert.AreEqual("Tarea en 10 días - Alta", actividades[1].Nombre); // Alta antes que Baja
            Assert.IsTrue(actividades[1].DiasRestantes >= 10 && actividades[1].DiasRestantes <= 11);
            Assert.AreEqual("Tarea en 10 días - Baja", actividades[2].Nombre);
            Assert.IsTrue(actividades[2].DiasRestantes >= 10 && actividades[2].DiasRestantes <= 11);
        }

        [TestMethod]
        public void ObtenerActividadesDashboard_AislarActividadesPorUsuarioId()
        {
            // Arrange
            var usuario1 = CrearUsuario("Usuario", "Uno", "usuario1@example.com");
            var usuario2 = CrearUsuario("Usuario", "Dos", "usuario2@example.com");
            _context.Usuarios.Add(usuario1);
            _context.Usuarios.Add(usuario2);
            _context.SaveChanges();

            var ahora = DateTime.Now;

            var tareaUsuario1 = new Tarea
            {
                Titulo = "Tarea del usuario 1",
                Curso = "Curso 1",
                FechaEntrega = ahora.AddDays(5),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario1.Id
            };

            var tareaUsuario2 = new Tarea
            {
                Titulo = "Tarea del usuario 2",
                Curso = "Curso 2",
                FechaEntrega = ahora.AddDays(5),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario2.Id
            };

            _context.Tareas.Add(tareaUsuario1);
            _context.Tareas.Add(tareaUsuario2);
            _context.SaveChanges();

            // Act
            var actividadesUsuario1 = ObtenerActividadesDashboard(usuario1.Id);
            var actividadesUsuario2 = ObtenerActividadesDashboard(usuario2.Id);

            // Assert
            Assert.AreEqual(1, actividadesUsuario1.Count);
            Assert.AreEqual(1, actividadesUsuario2.Count);
            Assert.AreEqual("Tarea del usuario 1", actividadesUsuario1[0].Nombre);
            Assert.AreEqual("Tarea del usuario 2", actividadesUsuario2[0].Nombre);
        }

        [TestMethod]
        public void ObtenerActividadesDashboard_EscenarioComplejo_MezclaCompleta()
        {
            // Arrange
            var usuario = CrearUsuario("Luis", "Sánchez", "luis@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var ahora = DateTime.Now;

            // Tarea vencida (fecha pasada)
            var tareaVencida = new Tarea
            {
                Titulo = "Tarea vencida",
                Curso = "Curso 1",
                FechaEntrega = ahora.AddDays(-5),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            // Tarea entregada (debería excluirse)
            var tareaEntregada = new Tarea
            {
                Titulo = "Tarea entregada",
                Curso = "Curso 2",
                FechaEntrega = ahora.AddDays(8),
                Prioridad = "Media",
                Estado = "Entregada",
                UsuarioId = usuario.Id
            };

            // Tarea fuera del rango (> 15 días)
            var tareaFueraDePlazo = new Tarea
            {
                Titulo = "Tarea fuera de plazo",
                Curso = "Curso 3",
                FechaEntrega = ahora.AddDays(20),
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            // Examen rendido (debería excluirse)
            var examenRendido = new Examen
            {
                Curso = "Curso 4",
                Tema = "Examen rendido",
                FechaExamen = ahora.AddDays(3),
                Lugar = "Aula 1",
                Prioridad = "Alta",
                Estado = "Rendido",
                UsuarioId = usuario.Id
            };

            // Tareas y exámenes pendientes dentro del rango
            var tarea1 = new Tarea
            {
                Titulo = "Tarea en 2 días - Alta",
                Curso = "Curso 5",
                FechaEntrega = ahora.AddDays(2),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var tarea2 = new Tarea
            {
                Titulo = "Tarea en 2 días - Media",
                Curso = "Curso 6",
                FechaEntrega = ahora.AddDays(2),
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var examen1 = new Examen
            {
                Curso = "Curso 7",
                Tema = "Examen en 7 días - Baja",
                FechaExamen = ahora.AddDays(7),
                Lugar = "Aula 2",
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var examen2 = new Examen
            {
                Curso = "Curso 8",
                Tema = "Examen en 7 días - Alta",
                FechaExamen = ahora.AddDays(7),
                Lugar = "Aula 3",
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tareaVencida);
            _context.Tareas.Add(tareaEntregada);
            _context.Tareas.Add(tareaFueraDePlazo);
            _context.Examenes.Add(examenRendido);
            _context.Tareas.Add(tarea1);
            _context.Tareas.Add(tarea2);
            _context.Examenes.Add(examen1);
            _context.Examenes.Add(examen2);
            _context.SaveChanges();

            // Act
            var actividades = ObtenerActividadesDashboard(usuario.Id, ahora);

            // Assert
            Assert.AreEqual(4, actividades.Count, "Solo 4 actividades válidas");

            // Verificar orden correcto:
            // 1. Día 2 - Alta (Tarea)
            // 2. Día 2 - Media (Tarea)
            // 3. Día 7 - Alta (Examen)
            // 4. Día 7 - Baja (Examen)

            Assert.AreEqual("Tarea en 2 días - Alta", actividades[0].Nombre);
            Assert.IsTrue(actividades[0].DiasRestantes >= 2 && actividades[0].DiasRestantes <= 3);
            Assert.AreEqual("Tarea en 2 días - Media", actividades[1].Nombre);
            Assert.IsTrue(actividades[1].DiasRestantes >= 2 && actividades[1].DiasRestantes <= 3);
            Assert.AreEqual("Examen en 7 días - Alta", actividades[2].Nombre);
            Assert.IsTrue(actividades[2].DiasRestantes >= 7 && actividades[2].DiasRestantes <= 8);
            Assert.AreEqual("Examen en 7 días - Baja", actividades[3].Nombre);
            Assert.IsTrue(actividades[3].DiasRestantes >= 7 && actividades[3].DiasRestantes <= 8);
        }

        #endregion

        #region Métodos auxiliares

        private Usuario CrearUsuario(string nombre, string apellido, string email)
        {
            return new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Carrera = "Ingeniería",
                Email = email,
                PasswordHash = "hash123",
                FechaNacimiento = new DateTime(2000, 1, 1)
            };
        }

        private int ObtenerNivelPrioridad(string prioridad)
        {
            return prioridad switch
            {
                "Alta" => 1,
                "Media" => 2,
                "Baja" => 3,
                _ => 4
            };
        }

        #endregion
    }
}
