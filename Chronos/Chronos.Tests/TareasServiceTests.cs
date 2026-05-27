using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Chronos.Domain.Entities;
using Chronos.Infrastructure;

namespace Chronos.Tests
{
    [TestClass]
    public class TareasServiceTests
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

        #region Pruebas de Creación

        [TestMethod]
        public void CrearTarea_ConTodosCampos_DebeGuardarseCorrectamente()
        {
            // Arrange
            var usuario = CrearUsuario("Juan", "Pérez", "juan@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var fechaEntrega = DateTime.Now.AddDays(5);
            var tarea = new Tarea
            {
                Titulo = "Realizar proyecto final",
                Curso = "Programación Avanzada",
                FechaEntrega = fechaEntrega,
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            // Act
            _context.Tareas.Add(tarea);
            _context.SaveChanges();

            // Assert
            Assert.AreEqual(1, _context.Tareas.Count());
            var tareaGuardada = _context.Tareas.First();
            Assert.AreEqual("Realizar proyecto final", tareaGuardada.Titulo);
            Assert.AreEqual("Programación Avanzada", tareaGuardada.Curso);
            Assert.AreEqual(fechaEntrega, tareaGuardada.FechaEntrega);
            Assert.AreEqual("Alta", tareaGuardada.Prioridad);
            Assert.AreEqual("Pendiente", tareaGuardada.Estado);
            Assert.AreEqual(usuario.Id, tareaGuardada.UsuarioId);
        }

        #endregion

        #region Pruebas de Edición

        [TestMethod]
        public void EditarTarea_TituloCursoFechaYPrioridad_DebeActualizarseCorrectamente()
        {
            // Arrange
            var usuario = CrearUsuario("Ana", "García", "ana@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var tarea = new Tarea
            {
                Titulo = "Tarea original",
                Curso = "Curso original",
                FechaEntrega = DateTime.Now.AddDays(1),
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tarea);
            _context.SaveChanges();
            var tareaId = tarea.Id;

            // Act
            var tareaAEditar = _context.Tareas.Find(tareaId);
            var nuevaFecha = DateTime.Now.AddDays(10);
            tareaAEditar.Titulo = "Tarea modificada";
            tareaAEditar.Curso = "Curso modificado";
            tareaAEditar.FechaEntrega = nuevaFecha;
            tareaAEditar.Prioridad = "Alta";

            _context.SaveChanges();

            // Assert
            var tareaActualizada = _context.Tareas.Find(tareaId);
            Assert.AreEqual("Tarea modificada", tareaActualizada.Titulo);
            Assert.AreEqual("Curso modificado", tareaActualizada.Curso);
            Assert.AreEqual(nuevaFecha, tareaActualizada.FechaEntrega);
            Assert.AreEqual("Alta", tareaActualizada.Prioridad);
            Assert.AreEqual("Pendiente", tareaActualizada.Estado);
        }

        #endregion

        #region Pruebas de Eliminación

        [TestMethod]
        public void EliminarTarea_DebeEliminarSoloLaTareaEspecificada()
        {
            // Arrange
            var usuario = CrearUsuario("Carlos", "López", "carlos@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var tarea1 = new Tarea
            {
                Titulo = "Tarea 1",
                Curso = "Curso 1",
                FechaEntrega = DateTime.Now.AddDays(1),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var tarea2 = new Tarea
            {
                Titulo = "Tarea 2",
                Curso = "Curso 2",
                FechaEntrega = DateTime.Now.AddDays(2),
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var tarea3 = new Tarea
            {
                Titulo = "Tarea 3",
                Curso = "Curso 3",
                FechaEntrega = DateTime.Now.AddDays(3),
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tarea1);
            _context.Tareas.Add(tarea2);
            _context.Tareas.Add(tarea3);
            _context.SaveChanges();

            var tarea2Id = tarea2.Id;

            // Act
            var tareaAEliminar = _context.Tareas.Find(tarea2Id);
            _context.Tareas.Remove(tareaAEliminar);
            _context.SaveChanges();

            // Assert
            Assert.AreEqual(2, _context.Tareas.Count());
            Assert.IsNull(_context.Tareas.Find(tarea2Id));
            Assert.IsNotNull(_context.Tareas.Find(tarea1.Id));
            Assert.IsNotNull(_context.Tareas.Find(tarea3.Id));
        }

        #endregion

        #region Pruebas de Cambio de Estado

        [TestMethod]
        public void MarcarComoEntregada_DebeTransicionarDePendienteAEntregada()
        {
            // Arrange
            var usuario = CrearUsuario("María", "Rodríguez", "maria@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var tarea = new Tarea
            {
                Titulo = "Tarea a entregar",
                Curso = "Curso 1",
                FechaEntrega = DateTime.Now.AddDays(2),
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tarea);
            _context.SaveChanges();
            var tareaId = tarea.Id;

            // Act
            var tareaAMarcar = _context.Tareas.Find(tareaId);
            tareaAMarcar.Estado = "Entregada";
            _context.SaveChanges();

            // Assert
            var tareaActualizada = _context.Tareas.Find(tareaId);
            Assert.AreEqual("Entregada", tareaActualizada.Estado);
        }

        #endregion

        #region Pruebas de Filtrado por UsuarioId

        [TestMethod]
        public void ObtenerTareas_FiltrandoPorUsuarioId_DebeMostrarSoloTareasDelUsuario()
        {
            // Arrange
            var usuario1 = CrearUsuario("Usuario", "Uno", "usuario1@example.com");
            var usuario2 = CrearUsuario("Usuario", "Dos", "usuario2@example.com");
            _context.Usuarios.Add(usuario1);
            _context.Usuarios.Add(usuario2);
            _context.SaveChanges();

            var tarea1Usuario1 = new Tarea
            {
                Titulo = "Tarea Usuario 1 - 1",
                Curso = "Curso 1",
                FechaEntrega = DateTime.Now.AddDays(1),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario1.Id
            };

            var tarea2Usuario1 = new Tarea
            {
                Titulo = "Tarea Usuario 1 - 2",
                Curso = "Curso 2",
                FechaEntrega = DateTime.Now.AddDays(2),
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario1.Id
            };

            var tarea1Usuario2 = new Tarea
            {
                Titulo = "Tarea Usuario 2 - 1",
                Curso = "Curso 3",
                FechaEntrega = DateTime.Now.AddDays(3),
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario2.Id
            };

            _context.Tareas.Add(tarea1Usuario1);
            _context.Tareas.Add(tarea2Usuario1);
            _context.Tareas.Add(tarea1Usuario2);
            _context.SaveChanges();

            // Act
            var tareasUsuario1 = _context.Tareas.Where(t => t.UsuarioId == usuario1.Id).OrderBy(t => t.Id).ToList();
            var tareasUsuario2 = _context.Tareas.Where(t => t.UsuarioId == usuario2.Id).OrderBy(t => t.Id).ToList();

            // Assert
            Assert.AreEqual(2, tareasUsuario1.Count);
            Assert.AreEqual(1, tareasUsuario2.Count);
            Assert.IsTrue(tareasUsuario1.All(t => t.UsuarioId == usuario1.Id));
            Assert.IsTrue(tareasUsuario2.All(t => t.UsuarioId == usuario2.Id));
            var titulos1 = tareasUsuario1.Select(t => t.Titulo).ToList();
            var titulos2 = tareasUsuario2.Select(t => t.Titulo).ToList();
            Assert.IsTrue(titulos1.Contains("Tarea Usuario 1 - 1"));
            Assert.IsTrue(titulos1.Contains("Tarea Usuario 1 - 2"));
            Assert.IsTrue(titulos2.Contains("Tarea Usuario 2 - 1"));
        }

        #endregion

        #region Pruebas de Tareas Urgentes

        [TestMethod]
        public void ObtenerTareasUrgentes_NoDebeIncluirTareasConFechaPasada()
        {
            // Arrange
            var usuario = CrearUsuario("Pedro", "González", "pedro@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var fechaPasada = DateTime.Now.AddDays(-5);
            var tareaConFechaPasada = new Tarea
            {
                Titulo = "Tarea con fecha pasada",
                Curso = "Curso 1",
                FechaEntrega = fechaPasada,
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tareaConFechaPasada);
            _context.SaveChanges();

            // Act
            var ahora = DateTime.Now;
            var fechaLimite = ahora.AddDays(15);
            var tareasUrgentes = _context.Tareas
                .Where(t => t.UsuarioId == usuario.Id &&
                            t.Estado == "Pendiente" &&
                            t.FechaEntrega >= ahora &&
                            t.FechaEntrega <= fechaLimite)
                .ToList();

            // Assert
            Assert.AreEqual(0, tareasUrgentes.Count);
        }

        [TestMethod]
        public void ObtenerTareasUrgentes_DebeIncluirTareasEnRangoDe15Dias()
        {
            // Arrange
            var usuario = CrearUsuario("Sofia", "Martínez", "sofia@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var ahora = DateTime.Now;
            var tareaUrgente = new Tarea
            {
                Titulo = "Tarea urgente",
                Curso = "Curso 1",
                FechaEntrega = ahora.AddDays(7),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var tareaFueraDePlazo = new Tarea
            {
                Titulo = "Tarea fuera de plazo",
                Curso = "Curso 2",
                FechaEntrega = ahora.AddDays(20),
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tareaUrgente);
            _context.Tareas.Add(tareaFueraDePlazo);
            _context.SaveChanges();

            // Act
            var fechaLimite = ahora.AddDays(15);
            var tareasUrgentes = _context.Tareas
                .Where(t => t.UsuarioId == usuario.Id &&
                            t.Estado == "Pendiente" &&
                            t.FechaEntrega >= ahora &&
                            t.FechaEntrega <= fechaLimite)
                .ToList();

            // Assert
            Assert.AreEqual(1, tareasUrgentes.Count);
            Assert.AreEqual("Tarea urgente", tareasUrgentes[0].Titulo);
        }

        #endregion

        #region Pruebas de Ordenamiento por Prioridad

        [TestMethod]
        public void ObtenerTareas_OrdenadasPorPrioridad_DebeDevolver_AltaMediaBaja()
        {
            // Arrange
            var usuario = CrearUsuario("Luis", "Sánchez", "luis@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var tareaBaja = new Tarea
            {
                Titulo = "Tarea Baja",
                Curso = "Curso 1",
                FechaEntrega = DateTime.Now.AddDays(5),
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var tareaMedia = new Tarea
            {
                Titulo = "Tarea Media",
                Curso = "Curso 2",
                FechaEntrega = DateTime.Now.AddDays(3),
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var tareaAlta = new Tarea
            {
                Titulo = "Tarea Alta",
                Curso = "Curso 3",
                FechaEntrega = DateTime.Now.AddDays(1),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tareaBaja);
            _context.Tareas.Add(tareaMedia);
            _context.Tareas.Add(tareaAlta);
            _context.SaveChanges();

            // Act
            var tareasOrdenadas = _context.Tareas
                .Where(t => t.UsuarioId == usuario.Id)
                .OrderBy(t => ObtenerNivelPrioridad(t.Prioridad))
                .ToList();

            // Assert
            Assert.AreEqual(3, tareasOrdenadas.Count);
            Assert.AreEqual("Tarea Alta", tareasOrdenadas[0].Titulo);
            Assert.AreEqual("Tarea Media", tareasOrdenadas[1].Titulo);
            Assert.AreEqual("Tarea Baja", tareasOrdenadas[2].Titulo);
        }

        #endregion

        #region Pruebas de Tareas Entregadas

        [TestMethod]
        public void ObtenerTareasUrgentes_NoDebeIncluirTareasEntregadas()
        {
            // Arrange
            var usuario = CrearUsuario("Laura", "Fernández", "laura@example.com");
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

            var tareaPendiente = new Tarea
            {
                Titulo = "Tarea pendiente",
                Curso = "Curso 2",
                FechaEntrega = ahora.AddDays(3),
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Tareas.Add(tareaEntregada);
            _context.Tareas.Add(tareaPendiente);
            _context.SaveChanges();

            // Act
            var fechaLimite = ahora.AddDays(15);
            var tareasUrgentes = _context.Tareas
                .Where(t => t.UsuarioId == usuario.Id &&
                            t.Estado == "Pendiente" &&
                            t.FechaEntrega >= ahora &&
                            t.FechaEntrega <= fechaLimite)
                .ToList();

            // Assert
            Assert.AreEqual(1, tareasUrgentes.Count);
            Assert.AreEqual("Tarea pendiente", tareasUrgentes[0].Titulo);
            Assert.AreEqual("Pendiente", tareasUrgentes[0].Estado);
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
