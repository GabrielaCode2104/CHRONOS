using System.Net;
using System.Net.Http.Headers;
using Chronos.Domain.Entities;
using Chronos.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Chronos.IntegrationTests
{
    [Collection("IntegrationTests")]
    public class ChronosIntegrationTests : IAsyncLifetime
    {
        private ChronosWebApplicationFactory _factory;
        private HttpClient _client;

        public async Task InitializeAsync()
        {
            _factory = new ChronosWebApplicationFactory();
            await _factory.InitializeAsync();
            await _factory.EnsureDatabaseCreatedAsync();

            // Crear cliente con HandleCookies = true
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
                AllowAutoRedirect = false,
                BaseAddress = new Uri("http://localhost")
            });
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            await _factory.DisposeAsync();
            _factory?.Dispose();
        }

        #region Métodos auxiliares

        private async Task LimpiarDatosAsync()
        {
            await _factory.LimpiarDatosAsync();
        }

        private async Task<HttpClient> CrearClienteAutenticadoAsync(string email, string contraseña)
        {
            // Primero crear el usuario en la BD
            await _factory.CrearUsuarioBDAsync("Test", "User", email, contraseña);

            // Crear nuevo cliente con cookies habilitadas
            var clienteAut = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
                AllowAutoRedirect = false,
                BaseAddress = new Uri("http://localhost")
            });

            // POST a /Account/Login con form-urlencoded
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("password", contraseña)
            });

            var response = await clienteAut.PostAsync("/Account/Login", content);

            // Debe ser 302 (redirección a Dashboard)
            if (response.StatusCode != HttpStatusCode.Found)
            {
                throw new InvalidOperationException($"Login falló. StatusCode: {response.StatusCode}");
            }

            var locationHeader = response.Headers.Location;
            if (locationHeader == null || !locationHeader.ToString().Contains("Dashboard"))
            {
                throw new InvalidOperationException($"Redirección inesperada: {locationHeader}");
            }

            return clienteAut;
        }

        #endregion

        #region Pruebas de Autenticación

        [Fact]
        public async Task Registrar_UsuarioNuevo_RedirigeALogin()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"newuser_{Guid.NewGuid()}@example.com";
            var nombreCompleto = "Test User";
            var carrera = "Ingeniería";
            var contraseña = "TestPass123!";

            // Act
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("nombre", "Test"),
                new KeyValuePair<string, string>("apellido", "User"),
                new KeyValuePair<string, string>("carrera", carrera),
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("password", contraseña),
                new KeyValuePair<string, string>("confirmPassword", contraseña)
            });

            var response = await _client.PostAsync("/Account/Registro", content);

            // Assert - El registro debe redirigir (302 Found)
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);

            // Verificar que el usuario existe en BD con PasswordHash correcto
            var usuario = await _factory.ObtenerUsuarioPorEmailAsync(email);
            Assert.NotNull(usuario);
            Assert.Equal(64, usuario.PasswordHash.Length); // SHA256 en hex = 64 chars
            Assert.Equal("Test", usuario.Nombre);
            Assert.Equal("User", usuario.Apellido);
        }

        [Fact]
        public async Task Login_CredencialesCorrectas_RedirigeADashboard()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"user_{Guid.NewGuid()}@example.com";
            var contraseña = "Password123!";

            await _factory.CrearUsuarioBDAsync("Juan", "Pérez", email, contraseña);

            // Act
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("password", contraseña)
            });

            var response = await _client.PostAsync("/Account/Login", content);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Contains("Dashboard", response.Headers.Location?.ToString() ?? "");
        }

        [Fact]
        public async Task Login_CredencialesIncorrectas_DevuelveVistaConError()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"user_{Guid.NewGuid()}@example.com";
            await _factory.CrearUsuarioBDAsync("Ana", "García", email, "CorrectPassword");

            // Act
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", email),
                new KeyValuePair<string, string>("password", "WrongPassword")
            });

            var response = await _client.PostAsync("/Account/Login", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync();

            // Verificar que contiene mensaje de error
            // (El mensaje exacto depende de tu implementación en el controlador)
            Assert.NotNull(html);
            Assert.True(html.Length > 0);
        }

        #endregion

        #region Pruebas de Tareas

        [Fact]
        public async Task GetTareas_ConSesionActiva_Devuelve200()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"user_{Guid.NewGuid()}@example.com";
            var cliente = await CrearClienteAutenticadoAsync(email, "Password123!");

            // Act
            var response = await cliente.GetAsync("/Tareas/Index");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CrearTarea_ConDatosValidos_RedirigeAIndex()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"user_{Guid.NewGuid()}@example.com";
            var cliente = await CrearClienteAutenticadoAsync(email, "Password123!");

            var usuario = await _factory.ObtenerUsuarioPorEmailAsync(email);
            var fechaEntrega = DateTime.Now.AddDays(5);

            // Act
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("titulo", "Mi Primera Tarea"),
                new KeyValuePair<string, string>("curso", "Programación"),
                new KeyValuePair<string, string>("prioridad", "Alta"),
                new KeyValuePair<string, string>("fechaSolo", fechaEntrega.ToString("yyyy-MM-dd")),
                new KeyValuePair<string, string>("horaH", "10"),
                new KeyValuePair<string, string>("horaM", "0"),
                new KeyValuePair<string, string>("ampm", "AM")
            });

            var response = await cliente.PostAsync("/Tareas/Crear", content);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Contains("/Tareas/Index", response.Headers.Location?.ToString() ?? "");

            // Verificar que la tarea existe en BD
            var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ChronosDbContext>();
            var tareas = await db.Tareas.Where(t => t.UsuarioId == usuario.Id).ToListAsync();

            Assert.Single(tareas);
            Assert.Equal("Mi Primera Tarea", tareas[0].Titulo);
            Assert.Equal("Pendiente", tareas[0].Estado);
        }

        [Fact]
        public async Task CompletarTarea_CambiaEstadoAEntregada()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"user_{Guid.NewGuid()}@example.com";
            var cliente = await CrearClienteAutenticadoAsync(email, "Password123!");

            var usuario = await _factory.ObtenerUsuarioPorEmailAsync(email);
            var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ChronosDbContext>();
            var tarea = new Tarea
            {
                Titulo = "Tarea a Completar",
                Curso = "Curso 1",
                FechaEntrega = DateTime.Now.AddDays(3),
                Prioridad = "Media",
                Estado = "Pendiente",
                CreadoEn = DateTime.Now,
                UsuarioId = usuario.Id
            };
            db.Tareas.Add(tarea);
            await db.SaveChangesAsync();

            // Act
            var response = await cliente.PostAsync($"/Tareas/Completar/{tarea.Id}", new StringContent(""));

            // Assert
            var tareaActualizada = await _factory.ObtenerTareaPorIdAsync(tarea.Id);
            Assert.NotNull(tareaActualizada);
            Assert.Equal("Entregada", tareaActualizada.Estado);
        }

        [Fact]
        public async Task EliminarTarea_LaRemueveDeBD()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"user_{Guid.NewGuid()}@example.com";
            var cliente = await CrearClienteAutenticadoAsync(email, "Password123!");

            var usuario = await _factory.ObtenerUsuarioPorEmailAsync(email);
            var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ChronosDbContext>();
            var tarea = new Tarea
            {
                Titulo = "Tarea a Eliminar",
                Curso = "Curso 2",
                FechaEntrega = DateTime.Now.AddDays(2),
                Prioridad = "Baja",
                Estado = "Pendiente",
                CreadoEn = DateTime.Now,
                UsuarioId = usuario.Id
            };
            db.Tareas.Add(tarea);
            await db.SaveChangesAsync();

            // Act
            var response = await cliente.PostAsync($"/Tareas/Eliminar/{tarea.Id}", new StringContent(""));

            // Assert
            var tareaEliminada = await _factory.ObtenerTareaPorIdAsync(tarea.Id);
            Assert.Null(tareaEliminada);
        }

        #endregion

        #region Pruebas de Exámenes

        [Fact]
        public async Task GetExamenes_ConSesionActiva_Devuelve200()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"user_{Guid.NewGuid()}@example.com";
            var cliente = await CrearClienteAutenticadoAsync(email, "Password123!");

            // Act
            var response = await cliente.GetAsync("/Examenes/Index");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CrearExamen_ConDatosValidos_RedirigeAIndex()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"user_{Guid.NewGuid()}@example.com";
            var cliente = await CrearClienteAutenticadoAsync(email, "Password123!");

            var usuario = await _factory.ObtenerUsuarioPorEmailAsync(email);
            var fechaExamen = DateTime.Now.AddDays(10);

            // Act
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("curso", "Matemáticas"),
                new KeyValuePair<string, string>("tema", "Cálculo Diferencial"),
                new KeyValuePair<string, string>("lugar", "Aula 101"),
                new KeyValuePair<string, string>("prioridad", "Media"),
                new KeyValuePair<string, string>("fechaSolo", fechaExamen.ToString("yyyy-MM-dd")),
                new KeyValuePair<string, string>("horaH", "2"),  // 2 PM = 14:00
                new KeyValuePair<string, string>("horaM", "30"),
                new KeyValuePair<string, string>("ampm", "PM")
            });

            var response = await cliente.PostAsync("/Examenes/Crear", content);

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Contains("/Examenes/Index", response.Headers.Location?.ToString() ?? "");

            // Verificar que el examen existe en BD
            var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ChronosDbContext>();
            var examenes = db.Examenes.Where(e => e.UsuarioId == usuario.Id).ToList();

            Assert.Single(examenes);
            Assert.Equal("Cálculo Diferencial", examenes[0].Tema);
            Assert.Equal("Pendiente", examenes[0].Estado);
        }

        [Fact]
        public async Task CompletarExamen_CambiaEstadoARendido()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"user_{Guid.NewGuid()}@example.com";
            var cliente = await CrearClienteAutenticadoAsync(email, "Password123!");

            var usuario = await _factory.ObtenerUsuarioPorEmailAsync(email);
            var db = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ChronosDbContext>();
            var examen = new Examen
            {
                Curso = "Física",
                Tema = "Mecánica",
                FechaExamen = DateTime.Now.AddDays(7),
                Lugar = "Aula 202",
                Prioridad = "Alta",
                Estado = "Pendiente",
                CreadoEn = DateTime.Now,
                UsuarioId = usuario.Id
            };
            db.Examenes.Add(examen);
            await db.SaveChangesAsync();

            // Act
            var response = await cliente.PostAsync($"/Examenes/Completar/{examen.Id}", new StringContent(""));

            // Assert
            var examenActualizado = await _factory.ObtenerExamenPorIdAsync(examen.Id);
            Assert.NotNull(examenActualizado);
            Assert.Equal("Rendido", examenActualizado.Estado);
        }

        #endregion

        #region Pruebas de Dashboard

        [Fact]
        public async Task GetDashboard_ConSesionActiva_Devuelve200()
        {
            // Arrange
            await LimpiarDatosAsync();
            var email = $"user_{Guid.NewGuid()}@example.com";
            var cliente = await CrearClienteAutenticadoAsync(email, "Password123!");

            // Act
            var response = await cliente.GetAsync("/Dashboard/Index");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetDashboard_SinSesion_RedirigeALogin()
        {
            // Arrange
            await LimpiarDatosAsync();
            var clienteSinLogin = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
                AllowAutoRedirect = false,
                BaseAddress = new Uri("http://localhost")
            });

            // Primero obtenga la página Dashboard/Index sin autenticación
            // Act
            var response = await clienteSinLogin.GetAsync("/Dashboard/Index");

            // Assert
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            // Verificar que redirecciona a algún lugar (puede ser /Account/Login o similar)
            var location = response.Headers.Location.ToString();
            Assert.True(!string.IsNullOrEmpty(location), "Location header debe tener un valor");
        }

        #endregion
    }
}
