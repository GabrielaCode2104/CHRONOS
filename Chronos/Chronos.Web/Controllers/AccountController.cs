using Chronos.Domain.Entities;
using Chronos.Infrastructure;
using Chronos.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Chronos.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ChronosDbContext _db;

        public AccountController(ChronosDbContext db) { _db = db; }

        private string HashTexto(string texto)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes);
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var hash = HashTexto(password);
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == hash);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }

            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Registro() => View();

        [HttpPost]
        public async Task<IActionResult> Registro(Usuario usuario, string password)
        {
            if (await _db.Usuarios.AnyAsync(u => u.Email == usuario.Email))
            {
                ViewBag.Error = "El correo ya está registrado";
                return View();
            }

            usuario.PasswordHash = HashTexto(password);
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ── Recuperar contraseña ──────────────────────────────

        public IActionResult RecuperarPassword() => View(new RecuperarPasswordViewModel());

        [HttpPost]
        public async Task<IActionResult> BuscarCuenta(RecuperarPasswordViewModel vm)
        {
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == vm.Email);

            if (usuario == null || string.IsNullOrEmpty(usuario.PreguntaSecreta))
            {
                ViewBag.Error = "No se encontró una cuenta con ese correo o no tiene pregunta secreta configurada.";
                return View("RecuperarPassword", vm);
            }

            vm.PreguntaSecreta = usuario.PreguntaSecreta;
            return View("ResponderPregunta", vm);
        }

        // Paso 1 — Verificar respuesta secreta
        [HttpPost]
        public async Task<IActionResult> VerificarRespuesta(RecuperarPasswordViewModel vm)
        {
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == vm.Email);

            if (usuario == null)
            {
                ViewBag.Error = "Cuenta no encontrada.";
                return View("ResponderPregunta", vm);
            }

            var respuestaHash = HashTexto(vm.RespuestaSecreta.ToLower().Trim());
            if (usuario.RespuestaSecretaHash != respuestaHash)
            {
                ViewBag.Error = "La respuesta secreta es incorrecta. Intenta de nuevo.";
                vm.PreguntaSecreta = usuario.PreguntaSecreta;
                return View("ResponderPregunta", vm);
            }

            // Respuesta correcta — guardar token en sesión
            var token = Guid.NewGuid().ToString();
            HttpContext.Session.SetString("ResetToken", token);
            HttpContext.Session.SetString("ResetEmail", vm.Email);

            return View("NuevaPassword", new RecuperarPasswordViewModel
            {
                Email = vm.Email
            });
        }

        // Paso 2 — Guardar nueva contraseña
        [HttpPost]
        public async Task<IActionResult> ResetearPassword(RecuperarPasswordViewModel vm)
        {
            // Verificar que vino del paso anterior
            var token = HttpContext.Session.GetString("ResetToken");
            var emailSesion = HttpContext.Session.GetString("ResetEmail");

            if (string.IsNullOrEmpty(token) || emailSesion != vm.Email)
            {
                TempData["Error"] = "Sesión inválida. Vuelve a intentarlo.";
                return RedirectToAction("RecuperarPassword");
            }

            if (string.IsNullOrEmpty(vm.PasswordNueva) ||
                vm.PasswordNueva != vm.PasswordConfirmar)
            {
                ViewBag.Error = "Las contraseñas no coinciden o están vacías.";
                return View("NuevaPassword", vm);
            }

            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Email == vm.Email);

            if (usuario == null)
            {
                ViewBag.Error = "Cuenta no encontrada.";
                return View("NuevaPassword", vm);
            }

            usuario.PasswordHash = HashTexto(vm.PasswordNueva);
            await _db.SaveChangesAsync();

            // Limpiar token de sesión
            HttpContext.Session.Remove("ResetToken");
            HttpContext.Session.Remove("ResetEmail");

            TempData["Exito"] = "Contraseña restablecida correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }
    }
}