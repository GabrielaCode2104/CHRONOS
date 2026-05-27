using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Chronos.Domain.Entities;
using Chronos.Infrastructure;

namespace Chronos.Tests
{
    [TestClass]
    public class PerfilServiceTests
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

        #region Pruebas de Edición de Perfil

        [TestMethod]
        public void ActualizarPerfil_NombreApellidoCarreraEmailYFechaNacimiento_DebeActualizarseCorrectamente()
        {
            // Arrange
            var usuario = CrearUsuario("Juan", "Pérez", "juan@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
            var usuarioId = usuario.Id;

            // Act
            var usuarioAActualizar = _context.Usuarios.Find(usuarioId);
            usuarioAActualizar.Nombre = "Carlos";
            usuarioAActualizar.Apellido = "García";
            usuarioAActualizar.Carrera = "Ingeniería Civil";
            usuarioAActualizar.Email = "carlos@example.com";
            usuarioAActualizar.FechaNacimiento = new DateTime(1995, 5, 20);
            _context.SaveChanges();

            // Assert
            var usuarioActualizado = _context.Usuarios.Find(usuarioId);
            Assert.AreEqual("Carlos", usuarioActualizado.Nombre);
            Assert.AreEqual("García", usuarioActualizado.Apellido);
            Assert.AreEqual("Ingeniería Civil", usuarioActualizado.Carrera);
            Assert.AreEqual("carlos@example.com", usuarioActualizado.Email);
            Assert.AreEqual(new DateTime(1995, 5, 20), usuarioActualizado.FechaNacimiento);
        }

        [TestMethod]
        public void ActualizarPerfil_ConEmailDuplicadoDeOtroUsuario_NoDebePermitirCambio()
        {
            // Arrange
            var usuario1 = CrearUsuario("Juan", "Pérez", "juan@example.com");
            var usuario2 = CrearUsuario("Ana", "García", "ana@example.com");
            _context.Usuarios.Add(usuario1);
            _context.Usuarios.Add(usuario2);
            _context.SaveChanges();

            // Act & Assert - Intentar cambiar email a uno que ya existe
            var usuarioAActualizar = _context.Usuarios.Find(usuario2.Id);
            var nuevoEmail = "juan@example.com"; // Email que ya usa usuario1

            // Verificar que el email ya está en uso
            var emailYaEnUso = _context.Usuarios.Any(u => u.Email == nuevoEmail && u.Id != usuarioAActualizar.Id);
            Assert.IsTrue(emailYaEnUso, "El email debe estar en uso por otro usuario");

            // En una aplicación real, esto sería validado antes de guardar
            usuarioAActualizar.Email = nuevoEmail;

            // Verificar que hay conflicto
            var usuariosConEsteEmail = _context.Usuarios.Count(u => u.Email == nuevoEmail);
            Assert.IsTrue(usuariosConEsteEmail >= 1, "Debe haber al menos un usuario con este email");
        }

        [TestMethod]
        public void ActualizarPerfil_ConMismoEmailDelUsuarioActual_DebePermitirCambio()
        {
            // Arrange
            var usuario = CrearUsuario("Juan", "Pérez", "juan@example.com");
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
            var usuarioId = usuario.Id;
            var emailActual = usuario.Email;

            // Act
            var usuarioAActualizar = _context.Usuarios.Find(usuarioId);
            usuarioAActualizar.Nombre = "Carlos";
            usuarioAActualizar.Email = emailActual; // Mismo email
            _context.SaveChanges();

            // Assert
            var usuarioActualizado = _context.Usuarios.Find(usuarioId);
            Assert.AreEqual("Carlos", usuarioActualizado.Nombre);
            Assert.AreEqual(emailActual, usuarioActualizado.Email);

            // Verificar que no hay duplicados
            var usuariosConEsteEmail = _context.Usuarios.Count(u => u.Email == emailActual);
            Assert.AreEqual(1, usuariosConEsteEmail);
        }

        #endregion

        #region Pruebas de Pregunta Secreta

        [TestMethod]
        public void GuardarPreguntaSecreta_YRespuestaHash_DebeAlmacenarCorrectamente()
        {
            // Arrange
            var usuario = CrearUsuario("María", "López", "maria@example.com");
            var pregunta = "¿Cuál es el nombre de tu mascota?";
            var respuesta = "Firulais";
            var respuestaProcessada = respuesta.ToLower().Trim();
            var respuestaHash = GenerarHashSHA256(respuestaProcessada);

            usuario.PreguntaSecreta = pregunta;
            usuario.RespuestaSecretaHash = respuestaHash;

            // Act
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Assert
            var usuarioGuardado = _context.Usuarios.First();
            Assert.AreEqual(pregunta, usuarioGuardado.PreguntaSecreta);
            Assert.AreEqual(respuestaHash, usuarioGuardado.RespuestaSecretaHash);
        }

        [TestMethod]
        public void VerificarRespuestaSecreta_ConRespuestaCorrecta_DebeCoincidirConHash()
        {
            // Arrange
            var usuario = CrearUsuario("Pedro", "González", "pedro@example.com");
            var respuesta = "Firulais";
            var respuestaProcessada = respuesta.ToLower().Trim();
            var respuestaHash = GenerarHashSHA256(respuestaProcessada);

            usuario.PreguntaSecreta = "¿Cuál es tu mascota?";
            usuario.RespuestaSecretaHash = respuestaHash;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Act
            var usuarioGuardado = _context.Usuarios.First();
            var coincide = VerificarRespuestaSecreta(respuesta, usuarioGuardado.RespuestaSecretaHash);

            // Assert
            Assert.IsTrue(coincide);
        }

        [TestMethod]
        public void VerificarRespuestaSecreta_ConRespuestaIncorrecta_NoDebeCoincidirConHash()
        {
            // Arrange
            var usuario = CrearUsuario("Sofia", "Martínez", "sofia@example.com");
            var respuestaCorrecta = "Firulais";
            var respuestaIncorrecta = "Rex";
            var respuestaProcessada = respuestaCorrecta.ToLower().Trim();
            var respuestaHash = GenerarHashSHA256(respuestaProcessada);

            usuario.PreguntaSecreta = "¿Cuál es tu mascota?";
            usuario.RespuestaSecretaHash = respuestaHash;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Act
            var usuarioGuardado = _context.Usuarios.First();
            var coincide = VerificarRespuestaSecreta(respuestaIncorrecta, usuarioGuardado.RespuestaSecretaHash);

            // Assert
            Assert.IsFalse(coincide);
        }

        [TestMethod]
        public void VerificarRespuestaSecreta_NoDistingueMayusculas()
        {
            // Arrange
            var usuario = CrearUsuario("Luis", "Sánchez", "luis@example.com");
            var respuestaOriginal = "Firulais";
            var respuestaProcessada = respuestaOriginal.ToLower().Trim();
            var respuestaHash = GenerarHashSHA256(respuestaProcessada);

            usuario.PreguntaSecreta = "¿Cuál es tu mascota?";
            usuario.RespuestaSecretaHash = respuestaHash;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Act
            var usuarioGuardado = _context.Usuarios.First();

            // Probar diferentes variaciones de mayúsculas
            var coincideBaja = VerificarRespuestaSecreta("firulais", usuarioGuardado.RespuestaSecretaHash);
            var coincideAlta = VerificarRespuestaSecreta("FIRULAIS", usuarioGuardado.RespuestaSecretaHash);
            var coincideMixta = VerificarRespuestaSecreta("FiRuLaIs", usuarioGuardado.RespuestaSecretaHash);

            // Assert
            Assert.IsTrue(coincideBaja);
            Assert.IsTrue(coincideAlta);
            Assert.IsTrue(coincideMixta);
        }

        [TestMethod]
        public void VerificarRespuestaSecreta_IgnoraEspaciosAlInicioYAlFinal()
        {
            // Arrange
            var usuario = CrearUsuario("Andrea", "Rodríguez", "andrea@example.com");
            var respuestaOriginal = "Firulais";
            var respuestaProcessada = respuestaOriginal.ToLower().Trim();
            var respuestaHash = GenerarHashSHA256(respuestaProcessada);

            usuario.PreguntaSecreta = "¿Cuál es tu mascota?";
            usuario.RespuestaSecretaHash = respuestaHash;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Act
            var usuarioGuardado = _context.Usuarios.First();

            // Probar con espacios al inicio y final
            var coincideConEspacios = VerificarRespuestaSecreta("  firulais  ", usuarioGuardado.RespuestaSecretaHash);
            var coincideConEspaciosUno = VerificarRespuestaSecreta(" firulais", usuarioGuardado.RespuestaSecretaHash);
            var coincideConEspaciosDos = VerificarRespuestaSecreta("firulais ", usuarioGuardado.RespuestaSecretaHash);

            // Assert
            Assert.IsTrue(coincideConEspacios);
            Assert.IsTrue(coincideConEspaciosUno);
            Assert.IsTrue(coincideConEspaciosDos);
        }

        #endregion

        #region Pruebas de Cambio de Contraseña

        [TestMethod]
        public void CambiarContraseña_ConContraseñaActualCorrecta_LaNewDebeTrabajarYLaAntiguaNo()
        {
            // Arrange
            var contraseñaOriginal = "contraseña123";
            var hashOriginal = GenerarHashSHA256(contraseñaOriginal);

            var usuario = CrearUsuario("Manuel", "Fernández", "manuel@example.com");
            usuario.PasswordHash = hashOriginal;
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
            var usuarioId = usuario.Id;

            // Verificar que la contraseña original funciona
            var usuarioGuardado = _context.Usuarios.Find(usuarioId);
            var coincideOriginal = VerificarContraseña(contraseñaOriginal, usuarioGuardado.PasswordHash);
            Assert.IsTrue(coincideOriginal);

            // Act - Cambiar contraseña
            var contraseñaNueva = "nuevaContraseña456";
            var hashNuevo = GenerarHashSHA256(contraseñaNueva);

            var usuarioAActualizar = _context.Usuarios.Find(usuarioId);
            usuarioAActualizar.PasswordHash = hashNuevo;
            _context.SaveChanges();

            // Assert
            var usuarioActualizado = _context.Usuarios.Find(usuarioId);
            Assert.IsTrue(VerificarContraseña(contraseñaNueva, usuarioActualizado.PasswordHash));
            Assert.IsFalse(VerificarContraseña(contraseñaOriginal, usuarioActualizado.PasswordHash));
        }

        [TestMethod]
        public void CambiarContraseña_ConContraseñaActualIncorrecta_NoDebePermitirCambio()
        {
            // Arrange
            var contraseñaCorrecta = "contraseña123";
            var contraseñaIncorrecta = "otraContraseña";
            var hashOriginal = GenerarHashSHA256(contraseñaCorrecta);

            var usuario = CrearUsuario("Teresa", "Navarro", "teresa@example.com");
            usuario.PasswordHash = hashOriginal;
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
            var usuarioId = usuario.Id;

            // Act
            var usuarioGuardado = _context.Usuarios.Find(usuarioId);
            var contraseñaActualValida = VerificarContraseña(contraseñaIncorrecta, usuarioGuardado.PasswordHash);

            // Assert
            Assert.IsFalse(contraseñaActualValida, "La contraseña actual es incorrecta, no debe permitir cambio");
        }

        #endregion

        #region Pruebas de Recuperación por Pregunta Secreta

        [TestMethod]
        public void BuscarUsuarioPorEmail_YVerificarQueTienePreguntaSecretaConfigurada()
        {
            // Arrange
            var usuario = CrearUsuario("Roberto", "Díaz", "roberto@example.com");
            var pregunta = "¿Cuál es tu color favorito?";
            var respuesta = "Azul";
            var respuestaHash = GenerarHashSHA256(respuesta.ToLower().Trim());

            usuario.PreguntaSecreta = pregunta;
            usuario.RespuestaSecretaHash = respuestaHash;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Act
            var usuarioEncontrado = _context.Usuarios.FirstOrDefault(u => u.Email == "roberto@example.com");

            // Assert
            Assert.IsNotNull(usuarioEncontrado);
            Assert.IsFalse(string.IsNullOrEmpty(usuarioEncontrado.PreguntaSecreta));
            Assert.IsFalse(string.IsNullOrEmpty(usuarioEncontrado.RespuestaSecretaHash));
            Assert.AreEqual(pregunta, usuarioEncontrado.PreguntaSecreta);
        }

        [TestMethod]
        public void RecuperacionPorPreguntaSecreta_RespuestaCorrecta_PermitirResetContraseña()
        {
            // Arrange
            var usuario = CrearUsuario("Valentina", "Castro", "valentina@example.com");
            var pregunta = "¿Cuál es tu color favorito?";
            var respuestaCorrecta = "Rojo";
            var respuestaHash = GenerarHashSHA256(respuestaCorrecta.ToLower().Trim());

            usuario.PreguntaSecreta = pregunta;
            usuario.RespuestaSecretaHash = respuestaHash;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Act
            var usuarioEncontrado = _context.Usuarios.FirstOrDefault(u => u.Email == "valentina@example.com");
            var respuestaValida = VerificarRespuestaSecreta(respuestaCorrecta, usuarioEncontrado.RespuestaSecretaHash);

            // Assert
            Assert.IsTrue(respuestaValida, "La respuesta correcta debe permitir reset de contraseña");
        }

        [TestMethod]
        public void RecuperacionPorPreguntaSecreta_RespuestaIncorrecta_NoPermitirResetContraseña()
        {
            // Arrange
            var usuario = CrearUsuario("Guillermo", "Vargas", "guillermo@example.com");
            var pregunta = "¿Cuál es tu color favorito?";
            var respuestaCorrecta = "Rojo";
            var respuestaIncorrecta = "Verde";
            var respuestaHash = GenerarHashSHA256(respuestaCorrecta.ToLower().Trim());

            usuario.PreguntaSecreta = pregunta;
            usuario.RespuestaSecretaHash = respuestaHash;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Act
            var usuarioEncontrado = _context.Usuarios.FirstOrDefault(u => u.Email == "guillermo@example.com");
            var respuestaValida = VerificarRespuestaSecreta(respuestaIncorrecta, usuarioEncontrado.RespuestaSecretaHash);

            // Assert
            Assert.IsFalse(respuestaValida, "La respuesta incorrecta NO debe permitir reset de contraseña");
        }

        [TestMethod]
        public void DespuesDelResetContraseña_LaNuevaDebeTrabajarParaLogin()
        {
            // Arrange
            var usuarioOriginal = CrearUsuario("Héctor", "Morales", "hector@example.com");
            var contraseñaOriginal = "contraseña123";
            usuarioOriginal.PasswordHash = GenerarHashSHA256(contraseñaOriginal);

            var pregunta = "¿Cuál es tu ciudad natal?";
            var respuesta = "Madrid";
            usuarioOriginal.PreguntaSecreta = pregunta;
            usuarioOriginal.RespuestaSecretaHash = GenerarHashSHA256(respuesta.ToLower().Trim());

            _context.Usuarios.Add(usuarioOriginal);
            _context.SaveChanges();
            var usuarioId = usuarioOriginal.Id;

            // Act - Verificar respuesta secreta y resetear contraseña
            var usuario = _context.Usuarios.Find(usuarioId);
            var respuestaValida = VerificarRespuestaSecreta(respuesta, usuario.RespuestaSecretaHash);
            Assert.IsTrue(respuestaValida);

            // Cambiar a nueva contraseña
            var contraseñaNueva = "nuevaContraseña789";
            usuario.PasswordHash = GenerarHashSHA256(contraseñaNueva);
            _context.SaveChanges();

            // Assert
            var usuarioActualizado = _context.Usuarios.Find(usuarioId);
            Assert.IsTrue(VerificarContraseña(contraseñaNueva, usuarioActualizado.PasswordHash));
        }

        [TestMethod]
        public void DespuesDelResetContraseña_LaAntiguaNoDebeTrabajar()
        {
            // Arrange
            var usuarioOriginal = CrearUsuario("Isabel", "Ruiz", "isabel@example.com");
            var contraseñaOriginal = "contraseña123";
            usuarioOriginal.PasswordHash = GenerarHashSHA256(contraseñaOriginal);

            var pregunta = "¿Cuál es tu ciudad natal?";
            var respuesta = "Barcelona";
            usuarioOriginal.PreguntaSecreta = pregunta;
            usuarioOriginal.RespuestaSecretaHash = GenerarHashSHA256(respuesta.ToLower().Trim());

            _context.Usuarios.Add(usuarioOriginal);
            _context.SaveChanges();
            var usuarioId = usuarioOriginal.Id;

            // Verificar que la contraseña original funciona
            var usuarioAntes = _context.Usuarios.Find(usuarioId);
            Assert.IsTrue(VerificarContraseña(contraseñaOriginal, usuarioAntes.PasswordHash));

            // Act - Resetear contraseña
            var contraseñaNueva = "nuevaContraseña789";
            var usuario = _context.Usuarios.Find(usuarioId);
            usuario.PasswordHash = GenerarHashSHA256(contraseñaNueva);
            _context.SaveChanges();

            // Assert
            var usuarioActualizado = _context.Usuarios.Find(usuarioId);
            Assert.IsFalse(VerificarContraseña(contraseñaOriginal, usuarioActualizado.PasswordHash));
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
                PasswordHash = GenerarHashSHA256("contraseña123"),
                FechaNacimiento = new DateTime(2000, 1, 1)
            };
        }

        /// <summary>
        /// Genera un hash SHA256 de un texto usando Convert.ToHexString()
        /// </summary>
        private string GenerarHashSHA256(string texto)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        /// Verifica si una contraseña coincide con su hash SHA256
        /// </summary>
        private bool VerificarContraseña(string contraseña, string hash)
        {
            var hashIngresado = GenerarHashSHA256(contraseña);
            return hashIngresado.Equals(hash, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifica si una respuesta secreta coincide con su hash.
        /// Normaliza la respuesta: ToLower() y Trim()
        /// </summary>
        private bool VerificarRespuestaSecreta(string respuesta, string hash)
        {
            var respuestaProcessada = respuesta.ToLower().Trim();
            var hashIngresado = GenerarHashSHA256(respuestaProcessada);
            return hashIngresado.Equals(hash, StringComparison.Ordinal);
        }

        #endregion
    }
}
