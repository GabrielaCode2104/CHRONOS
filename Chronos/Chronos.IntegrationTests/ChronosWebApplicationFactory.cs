using System.Security.Cryptography;
using System.Text;
using Chronos.Domain.Entities;
using Chronos.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Chronos.IntegrationTests
{
    /// <summary>
    /// WebApplicationFactory que configura Testcontainers con SQL Server
    /// y maneja el ciclo de vida del contenedor.
    /// </summary>
    public class ChronosWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private MsSqlContainer _msSqlContainer;
        private bool _migrationExecuted = false;

        public async Task InitializeAsync()
        {
            _msSqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .Build();

            await _msSqlContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            if (_msSqlContainer != null)
            {
                await _msSqlContainer.StopAsync();
                await _msSqlContainer.DisposeAsync();
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remover el registro existente de DbContextOptions
                var descriptor = services.FirstOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<ChronosDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Agregar nuevo DbContext con la cadena de conexión del contenedor
                services.AddDbContext<ChronosDbContext>(options =>
                    options.UseSqlServer(_msSqlContainer.GetConnectionString())
                );
            });

            builder.UseEnvironment("IntegrationTest");
        }

        /// <summary>
        /// Ejecuta las migraciones una sola vez cuando se necesite.
        /// </summary>
        public async Task EnsureDatabaseCreatedAsync()
        {
            if (_migrationExecuted)
                return;

            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();
                await db.Database.MigrateAsync();
                _migrationExecuted = true;
            }
        }

        /// <summary>
        /// Limpia todas las tablas en el orden correcto (por FKs).
        /// </summary>
        public async Task LimpiarDatosAsync()
        {
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

                // Limpiar tablas en orden (por relaciones externas)
                db.Tareas.RemoveRange(db.Tareas);
                db.Examenes.RemoveRange(db.Examenes);
                db.Usuarios.RemoveRange(db.Usuarios);

                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Genera un hash SHA256 de una contraseña (formato hexadecimal).
        /// </summary>
        public static string GenerarHashContraseña(string contraseña)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(contraseña));
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        /// Crea un usuario en la BD directamente.
        /// </summary>
        public async Task<Usuario> CrearUsuarioBDAsync(string nombre, string apellido, string email, string contraseña)
        {
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();

                var usuario = new Usuario
                {
                    Nombre = nombre,
                    Apellido = apellido,
                    Carrera = "Ingeniería",
                    Email = email,
                    PasswordHash = GenerarHashContraseña(contraseña),
                    FechaNacimiento = new DateTime(2000, 1, 1),
                    RecordatorioHoras = 24,
                    PreguntaSecreta = "¿Tu mascota?",
                    RespuestaSecretaHash = GenerarHashContraseña("Firulais")
                };

                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync();

                return usuario;
            }
        }

        /// <summary>
        /// Obtiene un usuario de la BD por email.
        /// </summary>
        public async Task<Usuario?> ObtenerUsuarioPorEmailAsync(string email)
        {
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();
                return await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            }
        }

        /// <summary>
        /// Obtiene una tarea por ID.
        /// </summary>
        public async Task<Tarea?> ObtenerTareaPorIdAsync(int id)
        {
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();
                return await db.Tareas.FirstOrDefaultAsync(t => t.Id == id);
            }
        }

        /// <summary>
        /// Obtiene un examen por ID.
        /// </summary>
        public async Task<Examen?> ObtenerExamenPorIdAsync(int id)
        {
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ChronosDbContext>();
                return await db.Examenes.FirstOrDefaultAsync(e => e.Id == id);
            }
        }
    }
}
