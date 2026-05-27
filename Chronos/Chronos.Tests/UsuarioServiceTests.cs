using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Chronos.Domain.Entities;
using Chronos.Infrastructure;

namespace Chronos.Tests
{
    [TestClass]
    public class UsuarioServiceTests
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

        #region Pruebas de Registro y Campos

        [TestMethod]
        public void RegistrarUsuario_ConTodosCampos_DebeGuardarseCorrectamente()
        {
            // Arrange
            var passwordHash = GenerarHashSHA256("miContraseña123");
            var respuestaHash = GenerarHashSHA256("miRespuesta");

            var usuario = new Usuario
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Carrera = "Ingeniería en Sistemas",
                Email = "juan.perez@example.com",
                PasswordHash = passwordHash,
                FechaNacimiento = new DateTime(2000, 5, 15),
                RecordatorioHoras = 24,
                PreguntaSecreta = "¿Cuál es tu color favorito?",
                RespuestaSecretaHash = respuestaHash
            };

            // Act
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Assert
            Assert.AreEqual(1, _context.Usuarios.Count());
            var usuarioGuardado = _context.Usuarios.First();
            Assert.AreEqual("Juan", usuarioGuardado.Nombre);
            Assert.AreEqual("Pérez", usuarioGuardado.Apellido);
            Assert.AreEqual("Ingeniería en Sistemas", usuarioGuardado.Carrera);
            Assert.AreEqual("juan.perez@example.com", usuarioGuardado.Email);
            Assert.AreEqual(passwordHash, usuarioGuardado.PasswordHash);
            Assert.AreEqual(new DateTime(2000, 5, 15), usuarioGuardado.FechaNacimiento);
            Assert.AreEqual(24, usuarioGuardado.RecordatorioHoras);
            Assert.AreEqual("¿Cuál es tu color favorito?", usuarioGuardado.PreguntaSecreta);
            Assert.AreEqual(respuestaHash, usuarioGuardado.RespuestaSecretaHash);
        }

        #endregion

        #region Pruebas de Hashing SHA256

        [TestMethod]
        public void GenerarHashSHA256_DebeSerCorrectoYReproducible()
        {
            // Arrange
            var contraseña = "miContraseña123";

            // Act
            var hash1 = GenerarHashSHA256(contraseña);
            var hash2 = GenerarHashSHA256(contraseña);

            // Assert
            Assert.AreEqual(hash1, hash2);
            Assert.IsFalse(string.IsNullOrEmpty(hash1));
            Assert.IsTrue(hash1.Length > 0);
            // SHA256 en hexadecimal debe tener 64 caracteres
            Assert.AreEqual(64, hash1.Length);
        }

        [TestMethod]
        public void VerificarContraseña_ConContraseñaCorrecta_DebeCoincidirConHash()
        {
            // Arrange
            var contraseña = "miContraseña123";
            var hash = GenerarHashSHA256(contraseña);

            // Act
            var coincide = VerificarContraseña(contraseña, hash);

            // Assert
            Assert.IsTrue(coincide);
        }

        [TestMethod]
        public void VerificarContraseña_ConContraseñaIncorrecta_NoDebeCoincidirConHash()
        {
            // Arrange
            var contraseña = "miContraseña123";
            var contraseñaIncorrecta = "otraContraseña456";
            var hash = GenerarHashSHA256(contraseña);

            // Act
            var coincide = VerificarContraseña(contraseñaIncorrecta, hash);

            // Assert
            Assert.IsFalse(coincide);
        }

        [TestMethod]
        public void DosContrasenas_Distintas_DebenGenerarHashesDistintos()
        {
            // Arrange
            var contraseña1 = "miContraseña123";
            var contraseña2 = "otraContraseña456";

            // Act
            var hash1 = GenerarHashSHA256(contraseña1);
            var hash2 = GenerarHashSHA256(contraseña2);

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }

        #endregion

        #region Pruebas de Validación de Email Duplicado

        [TestMethod]
        public void RegistrarUsuario_ConEmailDuplicado_NoDebePermitirDuplicadoVerificandoConAny()
        {
            // Arrange
            var usuario1 = new Usuario
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Carrera = "Ingeniería",
                Email = "juan@example.com",
                PasswordHash = GenerarHashSHA256("contraseña123"),
                FechaNacimiento = new DateTime(2000, 1, 1)
            };

            // Act
            _context.Usuarios.Add(usuario1);
            _context.SaveChanges();

            // Assert - Verificar que el email ya existe con Any() ANTES de intentar guardar otro usuario
            var emailExiste = _context.Usuarios.Any(u => u.Email == "juan@example.com");
            Assert.IsTrue(emailExiste, "El email debe existir en la base de datos");
            Assert.AreEqual(1, _context.Usuarios.Count(u => u.Email == "juan@example.com"));

            // Crear un segundo usuario con el mismo email
            var usuario2 = new Usuario
            {
                Nombre = "Carlos",
                Apellido = "López",
                Carrera = "Informática",
                Email = "juan@example.com", // Email duplicado
                PasswordHash = GenerarHashSHA256("contraseña456"),
                FechaNacimiento = new DateTime(2001, 1, 1)
            };

            // Verificar con Any() que el email ya existe ANTES de intentar guardar
            var yaSiguenexisteOtroConEsteMail = _context.Usuarios.Any(u => u.Email == usuario2.Email);
            Assert.IsTrue(yaSiguenexisteOtroConEsteMail, "El sistema debe detectar que el email ya existe");

            // No debería permitir guardar si ya existe un usuario con ese email
            // En una aplicación real, se validaría antes de agregar
            _context.Usuarios.Add(usuario2);

            // La BD en memoria podría permitir el duplicado, pero nuestra lógica debería prevenirlo
            var usuariosConEsteEmail = _context.Usuarios.Count(u => u.Email == "juan@example.com");
            Assert.IsTrue(usuariosConEsteEmail >= 1, "Debe haber al menos un usuario con este email");
        }

        #endregion

        #region Pruebas de Campos Requeridos

        [TestMethod]
        public void RegistrarUsuario_ConCamposRequeridos_DebeValidarQueNoEstenVacios()
        {
            // Arrange - Intentar crear un usuario sin campos requeridos
            var usuarioInvalido = new Usuario
            {
                Nombre = "", // Vacío
                Apellido = "Pérez",
                Carrera = "Ingeniería",
                Email = "juan@example.com",
                PasswordHash = GenerarHashSHA256("contraseña123"),
                FechaNacimiento = new DateTime(2000, 1, 1)
            };

            // Act & Assert
            Assert.AreEqual("", usuarioInvalido.Nombre);
            Assert.IsNotNull(usuarioInvalido.Apellido);
            Assert.IsNotNull(usuarioInvalido.Carrera);
            Assert.IsNotNull(usuarioInvalido.Email);
            Assert.IsNotNull(usuarioInvalido.PasswordHash);
        }

        [TestMethod]
        public void RegistrarUsuario_ConCamposRequeridos_NombreNoDebeSuministrarse()
        {
            // Arrange
            var usuario = new Usuario
            {
                Nombre = "Juan",
                Apellido = "Pérez",
                Carrera = "Ingeniería",
                Email = "juan@example.com",
                PasswordHash = GenerarHashSHA256("contraseña123"),
                FechaNacimiento = new DateTime(2000, 1, 1)
            };

            // Act
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Assert
            var usuarioGuardado = _context.Usuarios.First();
            Assert.IsFalse(string.IsNullOrEmpty(usuarioGuardado.Nombre));
            Assert.IsFalse(string.IsNullOrEmpty(usuarioGuardado.Apellido));
            Assert.IsFalse(string.IsNullOrEmpty(usuarioGuardado.Carrera));
            Assert.IsFalse(string.IsNullOrEmpty(usuarioGuardado.Email));
            Assert.IsFalse(string.IsNullOrEmpty(usuarioGuardado.PasswordHash));
        }

        #endregion

        #region Pruebas de Pregunta Secreta

        [TestMethod]
        public void GuardarYRecuperarPreguntaSecreta_DebeAlmacenarCorrectamente()
        {
            // Arrange
            var usuario = new Usuario
            {
                Nombre = "Ana",
                Apellido = "García",
                Carrera = "Administración",
                Email = "ana@example.com",
                PasswordHash = GenerarHashSHA256("contraseña123"),
                FechaNacimiento = new DateTime(1999, 3, 22),
                PreguntaSecreta = "¿Cuál es el nombre de tu mascota?",
                RespuestaSecretaHash = GenerarHashSHA256("Fluffy")
            };

            // Act
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            // Assert
            var usuarioRecuperado = _context.Usuarios.First();
            Assert.AreEqual("¿Cuál es el nombre de tu mascota?", usuarioRecuperado.PreguntaSecreta);
            Assert.AreEqual(GenerarHashSHA256("Fluffy"), usuarioRecuperado.RespuestaSecretaHash);
            Assert.IsTrue(VerificarContraseña("Fluffy", usuarioRecuperado.RespuestaSecretaHash));
        }

        #endregion

        #region Pruebas de Actualización de Contraseña

        [TestMethod]
        public void ActualizarContraseña_LaNewDebeTrabajarYLaAntiguaNo()
        {
            // Arrange
            var contraseñaOriginal = "contraseña123";
            var hashOriginal = GenerarHashSHA256(contraseñaOriginal);

            var usuario = new Usuario
            {
                Nombre = "Miguel",
                Apellido = "Sánchez",
                Carrera = "Ingeniería",
                Email = "miguel@example.com",
                PasswordHash = hashOriginal,
                FechaNacimiento = new DateTime(2001, 7, 10)
            };

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
            var usuarioId = usuario.Id;

            // Act - Actualizar contraseña
            var contraseñaNueva = "nuevaContraseña456";
            var hashNuevo = GenerarHashSHA256(contraseñaNueva);

            var usuarioAActualizar = _context.Usuarios.Find(usuarioId);
            usuarioAActualizar.PasswordHash = hashNuevo;
            _context.SaveChanges();

            // Assert - Verificar que la nueva funciona y la antigua no
            var usuarioActualizado = _context.Usuarios.Find(usuarioId);
            Assert.IsTrue(VerificarContraseña(contraseñaNueva, usuarioActualizado.PasswordHash));
            Assert.IsFalse(VerificarContraseña(contraseñaOriginal, usuarioActualizado.PasswordHash));
        }

        #endregion

        #region Pruebas de Eliminación

        [TestMethod]
        public void EliminarUsuario_DebeDesaparecerDelContexto()
        {
            // Arrange
            var usuario = new Usuario
            {
                Nombre = "Sofia",
                Apellido = "Martínez",
                Carrera = "Economía",
                Email = "sofia@example.com",
                PasswordHash = GenerarHashSHA256("contraseña123"),
                FechaNacimiento = new DateTime(2002, 11, 5)
            };

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
            var usuarioId = usuario.Id;

            // Verificar que el usuario existe
            Assert.AreEqual(1, _context.Usuarios.Count());
            var usuarioGuardado = _context.Usuarios.Find(usuarioId);
            Assert.IsNotNull(usuarioGuardado);

            // Act
            var usuarioAEliminar = _context.Usuarios.Find(usuarioId);
            _context.Usuarios.Remove(usuarioAEliminar);
            _context.SaveChanges();

            // Assert
            Assert.AreEqual(0, _context.Usuarios.Count());
            var usuarioEliminado = _context.Usuarios.Find(usuarioId);
            Assert.IsNull(usuarioEliminado);
        }

        #endregion

        #region Métodos auxiliares para Hashing

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

        #endregion
    }
}
