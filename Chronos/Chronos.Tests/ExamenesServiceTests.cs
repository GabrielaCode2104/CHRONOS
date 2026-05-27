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
    public class ExamenesServiceTests
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
        public void CrearExamen_ConTodosCampos_DebeGuardarseCorrectamente()
        {
            // Arrange
            var usuario = CrearUsuario("Juan", "Pérez", "juan@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var fechaExamen = DateTime.Now.AddDays(5);
            var examen = new Examen
            {
                Curso = "Matemáticas",
                Tema = "Cálculo Diferencial",
                FechaExamen = fechaExamen,
                Lugar = "Aula 101",
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            // Act
            _context.Examenes.Add(examen);
            _context.SaveChanges();

            // Assert
            Assert.AreEqual(1, _context.Examenes.Count());
            var examenGuardado = _context.Examenes.First();
            Assert.AreEqual("Matemáticas", examenGuardado.Curso);
            Assert.AreEqual("Cálculo Diferencial", examenGuardado.Tema);
            Assert.AreEqual(fechaExamen, examenGuardado.FechaExamen);
            Assert.AreEqual("Aula 101", examenGuardado.Lugar);
            Assert.AreEqual("Alta", examenGuardado.Prioridad);
            Assert.AreEqual("Pendiente", examenGuardado.Estado);
            Assert.AreEqual(usuario.Id, examenGuardado.UsuarioId);
        }

        #endregion

        #region Pruebas de Edición

        [TestMethod]
        public void EditarExamen_CursoTemaFechaLugarYPrioridad_DebeActualizarseCorrectamente()
        {
            // Arrange
            var usuario = CrearUsuario("Ana", "García", "ana@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var examen = new Examen
            {
                Curso = "Curso original",
                Tema = "Tema original",
                FechaExamen = DateTime.Now.AddDays(1),
                Lugar = "Lugar original",
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Examenes.Add(examen);
            _context.SaveChanges();
            var examenId = examen.Id;

            // Act
            var examenAEditar = _context.Examenes.Find(examenId);
            var nuevaFecha = DateTime.Now.AddDays(10);
            examenAEditar.Curso = "Curso modificado";
            examenAEditar.Tema = "Tema modificado";
            examenAEditar.FechaExamen = nuevaFecha;
            examenAEditar.Lugar = "Lugar modificado";
            examenAEditar.Prioridad = "Alta";

            _context.SaveChanges();

            // Assert
            var examenActualizado = _context.Examenes.Find(examenId);
            Assert.AreEqual("Curso modificado", examenActualizado.Curso);
            Assert.AreEqual("Tema modificado", examenActualizado.Tema);
            Assert.AreEqual(nuevaFecha, examenActualizado.FechaExamen);
            Assert.AreEqual("Lugar modificado", examenActualizado.Lugar);
            Assert.AreEqual("Alta", examenActualizado.Prioridad);
            Assert.AreEqual("Pendiente", examenActualizado.Estado);
        }

        #endregion

        #region Pruebas de Eliminación

        [TestMethod]
        public void EliminarExamen_DebeEliminarSoloElExamenEspecificado()
        {
            // Arrange
            var usuario = CrearUsuario("Carlos", "López", "carlos@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var examen1 = new Examen
            {
                Curso = "Curso 1",
                Tema = "Tema 1",
                FechaExamen = DateTime.Now.AddDays(1),
                Lugar = "Aula 1",
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var examen2 = new Examen
            {
                Curso = "Curso 2",
                Tema = "Tema 2",
                FechaExamen = DateTime.Now.AddDays(2),
                Lugar = "Aula 2",
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var examen3 = new Examen
            {
                Curso = "Curso 3",
                Tema = "Tema 3",
                FechaExamen = DateTime.Now.AddDays(3),
                Lugar = "Aula 3",
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Examenes.Add(examen1);
            _context.Examenes.Add(examen2);
            _context.Examenes.Add(examen3);
            _context.SaveChanges();

            var examen2Id = examen2.Id;

            // Act
            var examenAEliminar = _context.Examenes.Find(examen2Id);
            _context.Examenes.Remove(examenAEliminar);
            _context.SaveChanges();

            // Assert
            Assert.AreEqual(2, _context.Examenes.Count());
            Assert.IsNull(_context.Examenes.Find(examen2Id));
            Assert.IsNotNull(_context.Examenes.Find(examen1.Id));
            Assert.IsNotNull(_context.Examenes.Find(examen3.Id));
        }

        #endregion

        #region Pruebas de Cambio de Estado

        [TestMethod]
        public void MarcarComoRendido_DebeTransicionarDePendienteARendido()
        {
            // Arrange
            var usuario = CrearUsuario("María", "Rodríguez", "maria@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var examen = new Examen
            {
                Curso = "Física",
                Tema = "Mecánica",
                FechaExamen = DateTime.Now.AddDays(2),
                Lugar = "Aula 201",
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Examenes.Add(examen);
            _context.SaveChanges();
            var examenId = examen.Id;

            // Act
            var examenAMarcar = _context.Examenes.Find(examenId);
            examenAMarcar.Estado = "Rendido";
            _context.SaveChanges();

            // Assert
            var examenActualizado = _context.Examenes.Find(examenId);
            Assert.AreEqual("Rendido", examenActualizado.Estado);
        }

        #endregion

        #region Pruebas de Filtrado por UsuarioId

        [TestMethod]
        public void ObtenerExamenes_FiltrandoPorUsuarioId_DebeMostrarSoloExamenesDelUsuario()
        {
            // Arrange
            var usuario1 = CrearUsuario("Usuario", "Uno", "usuario1@example.com");
            var usuario2 = CrearUsuario("Usuario", "Dos", "usuario2@example.com");
            _context.Usuarios.Add(usuario1);
            _context.Usuarios.Add(usuario2);
            _context.SaveChanges();

            var examen1Usuario1 = new Examen
            {
                Curso = "Curso Usuario 1 - 1",
                Tema = "Tema 1",
                FechaExamen = DateTime.Now.AddDays(1),
                Lugar = "Aula 1",
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario1.Id
            };

            var examen2Usuario1 = new Examen
            {
                Curso = "Curso Usuario 1 - 2",
                Tema = "Tema 2",
                FechaExamen = DateTime.Now.AddDays(2),
                Lugar = "Aula 2",
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario1.Id
            };

            var examen1Usuario2 = new Examen
            {
                Curso = "Curso Usuario 2 - 1",
                Tema = "Tema 3",
                FechaExamen = DateTime.Now.AddDays(3),
                Lugar = "Aula 3",
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario2.Id
            };

            _context.Examenes.Add(examen1Usuario1);
            _context.Examenes.Add(examen2Usuario1);
            _context.Examenes.Add(examen1Usuario2);
            _context.SaveChanges();

            // Act
            var examenesUsuario1 = _context.Examenes.Where(e => e.UsuarioId == usuario1.Id).OrderBy(e => e.Id).ToList();
            var examenesUsuario2 = _context.Examenes.Where(e => e.UsuarioId == usuario2.Id).OrderBy(e => e.Id).ToList();

            // Assert
            Assert.AreEqual(2, examenesUsuario1.Count);
            Assert.AreEqual(1, examenesUsuario2.Count);
            Assert.IsTrue(examenesUsuario1.All(e => e.UsuarioId == usuario1.Id));
            Assert.IsTrue(examenesUsuario2.All(e => e.UsuarioId == usuario2.Id));
            var cursos1 = examenesUsuario1.Select(e => e.Curso).ToList();
            var cursos2 = examenesUsuario2.Select(e => e.Curso).ToList();
            Assert.IsTrue(cursos1.Contains("Curso Usuario 1 - 1"));
            Assert.IsTrue(cursos1.Contains("Curso Usuario 1 - 2"));
            Assert.IsTrue(cursos2.Contains("Curso Usuario 2 - 1"));
        }

        #endregion

        #region Pruebas de Exámenes Urgentes

        [TestMethod]
        public void ObtenerExamenesUrgentes_NoDebeIncluirExamenesConFechaPasada()
        {
            // Arrange
            var usuario = CrearUsuario("Pedro", "González", "pedro@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var fechaPasada = DateTime.Now.AddDays(-5);
            var examenConFechaPasada = new Examen
            {
                Curso = "Curso con fecha pasada",
                Tema = "Tema 1",
                FechaExamen = fechaPasada,
                Lugar = "Aula 1",
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Examenes.Add(examenConFechaPasada);
            _context.SaveChanges();

            // Act
            var ahora = DateTime.Now;
            var fechaLimite = ahora.AddDays(15);
            var examenesUrgentes = _context.Examenes
                .Where(e => e.UsuarioId == usuario.Id &&
                            e.Estado == "Pendiente" &&
                            e.FechaExamen >= ahora &&
                            e.FechaExamen <= fechaLimite)
                .ToList();

            // Assert
            Assert.AreEqual(0, examenesUrgentes.Count);
        }

        [TestMethod]
        public void ObtenerExamenesUrgentes_NoDebeIncluirExamenesConMasDe15Dias()
        {
            // Arrange
            var usuario = CrearUsuario("Sofia", "Martínez", "sofia@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var ahora = DateTime.Now;
            var examenUrgente = new Examen
            {
                Curso = "Curso urgente",
                Tema = "Tema urgente",
                FechaExamen = ahora.AddDays(7),
                Lugar = "Aula 1",
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var examenFueraDePlazo = new Examen
            {
                Curso = "Curso fuera de plazo",
                Tema = "Tema fuera de plazo",
                FechaExamen = ahora.AddDays(20),
                Lugar = "Aula 2",
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Examenes.Add(examenUrgente);
            _context.Examenes.Add(examenFueraDePlazo);
            _context.SaveChanges();

            // Act
            var fechaLimite = ahora.AddDays(15);
            var examenesUrgentes = _context.Examenes
                .Where(e => e.UsuarioId == usuario.Id &&
                            e.Estado == "Pendiente" &&
                            e.FechaExamen >= ahora &&
                            e.FechaExamen <= fechaLimite)
                .ToList();

            // Assert
            Assert.AreEqual(1, examenesUrgentes.Count);
            Assert.AreEqual("Curso urgente", examenesUrgentes[0].Curso);
        }

        #endregion

        #region Pruebas de Ordenamiento por Prioridad

        [TestMethod]
        public void ObtenerExamenes_OrdenadasPorPrioridad_DebeDevolver_AltaMediaBaja()
        {
            // Arrange
            var usuario = CrearUsuario("Luis", "Sánchez", "luis@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var examenBaja = new Examen
            {
                Curso = "Curso Baja",
                Tema = "Tema Baja",
                FechaExamen = DateTime.Now.AddDays(5),
                Lugar = "Aula 1",
                Prioridad = "Baja",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var examenMedia = new Examen
            {
                Curso = "Curso Media",
                Tema = "Tema Media",
                FechaExamen = DateTime.Now.AddDays(3),
                Lugar = "Aula 2",
                Prioridad = "Media",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            var examenAlta = new Examen
            {
                Curso = "Curso Alta",
                Tema = "Tema Alta",
                FechaExamen = DateTime.Now.AddDays(1),
                Lugar = "Aula 3",
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Examenes.Add(examenBaja);
            _context.Examenes.Add(examenMedia);
            _context.Examenes.Add(examenAlta);
            _context.SaveChanges();

            // Act
            var examenesOrdenados = _context.Examenes
                .Where(e => e.UsuarioId == usuario.Id)
                .OrderBy(e => ObtenerNivelPrioridad(e.Prioridad))
                .ToList();

            // Assert
            Assert.AreEqual(3, examenesOrdenados.Count);
            Assert.AreEqual("Curso Alta", examenesOrdenados[0].Curso);
            Assert.AreEqual("Curso Media", examenesOrdenados[1].Curso);
            Assert.AreEqual("Curso Baja", examenesOrdenados[2].Curso);
        }

        #endregion

        #region Pruebas de Exámenes Rendidos

        [TestMethod]
        public void ObtenerExamenesUrgentes_NoDebeIncluirExamenesRendidos()
        {
            // Arrange
            var usuario = CrearUsuario("Laura", "Fernández", "laura@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var ahora = DateTime.Now;
            var examenRendido = new Examen
            {
                Curso = "Examen rendido",
                Tema = "Tema rendido",
                FechaExamen = ahora.AddDays(5),
                Lugar = "Aula 1",
                Prioridad = "Alta",
                Estado = "Rendido",
                UsuarioId = usuario.Id
            };

            var examenPendiente = new Examen
            {
                Curso = "Examen pendiente",
                Tema = "Tema pendiente",
                FechaExamen = ahora.AddDays(3),
                Lugar = "Aula 2",
                Prioridad = "Alta",
                Estado = "Pendiente",
                UsuarioId = usuario.Id
            };

            _context.Examenes.Add(examenRendido);
            _context.Examenes.Add(examenPendiente);
            _context.SaveChanges();

            // Act
            var fechaLimite = ahora.AddDays(15);
            var examenesUrgentes = _context.Examenes
                .Where(e => e.UsuarioId == usuario.Id &&
                            e.Estado == "Pendiente" &&
                            e.FechaExamen >= ahora &&
                            e.FechaExamen <= fechaLimite)
                .ToList();

            // Assert
            Assert.AreEqual(1, examenesUrgentes.Count);
            Assert.AreEqual("Examen pendiente", examenesUrgentes[0].Curso);
            Assert.AreEqual("Pendiente", examenesUrgentes[0].Estado);
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
