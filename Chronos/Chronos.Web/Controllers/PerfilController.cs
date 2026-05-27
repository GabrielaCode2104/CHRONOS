using Chronos.Infrastructure;
using Chronos.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Chronos.Web.Controllers
{
    public class PerfilController : Controller
    {
        private readonly ChronosDbContext _db;
        public PerfilController(ChronosDbContext db) { _db = db; }

        private string HashTexto(string texto)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes);
        }

        private async Task CargarViewBag(int usuarioId, string preguntaSecreta = "")
        {
            ViewBag.TotalTareas = await _db.Tareas.CountAsync(t => t.UsuarioId == usuarioId);
            ViewBag.TareasEntregadas = await _db.Tareas.CountAsync(t => t.UsuarioId == usuarioId && t.Estado == "Entregada");
            ViewBag.TotalExamenes = await _db.Examenes.CountAsync(e => e.UsuarioId == usuarioId);
            ViewBag.ExamenesRendidos = await _db.Examenes.CountAsync(e => e.UsuarioId == usuarioId && e.Estado == "Rendido");
            ViewBag.TienePregunta = !string.IsNullOrEmpty(preguntaSecreta);
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            var usuario = await _db.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return RedirectToAction("Login", "Account");

            var vm = new PerfilViewModel
            {
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Carrera = usuario.Carrera,
                Email = usuario.Email,
                FechaNacimiento = usuario.FechaNacimiento,
                PreguntaSecreta = usuario.PreguntaSecreta
            };

            await CargarViewBag((int)usuarioId, usuario.PreguntaSecreta);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(PerfilViewModel vm)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                await CargarViewBag((int)usuarioId, vm.PreguntaSecreta);
                return View(vm);
            }

            var usuario = await _db.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return RedirectToAction("Login", "Account");

            var emailExiste = await _db.Usuarios
                .AnyAsync(u => u.Email == vm.Email && u.Id != usuarioId);
            if (emailExiste)
            {
                ModelState.AddModelError("Email", "Ese email ya está en uso por otra cuenta.");
                await CargarViewBag((int)usuarioId, usuario.PreguntaSecreta);
                return View(vm);
            }

            usuario.Nombre = vm.Nombre;
            usuario.Apellido = vm.Apellido;
            usuario.Carrera = vm.Carrera;
            usuario.Email = vm.Email;
            usuario.FechaNacimiento = vm.FechaNacimiento;

            await _db.SaveChangesAsync();
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);

            TempData["Exito"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Index");
        }

        // ── Contraseña: Paso 1 — Verificar contraseña actual ──
        [HttpPost]
        public async Task<IActionResult> VerificarParaPassword(string passwordConfirm)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            var usuario = await _db.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return RedirectToAction("Login", "Account");

            if (usuario.PasswordHash != HashTexto(passwordConfirm))
            {
                TempData["ErrorPassword"] = "Contraseña incorrecta.";
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetString("PasswordToken", Guid.NewGuid().ToString());
            TempData["PasswordVerificada"] = "true";
            return RedirectToAction("Index");
        }

        // ── Contraseña: Paso 2 — Guardar nueva contraseña ──
        [HttpPost]
        public async Task<IActionResult> CambiarPassword(CambiarPasswordViewModel vm)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("PasswordToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorPassword"] = "Sesión inválida. Verifica tu contraseña primero.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(vm.PasswordNueva) || vm.PasswordNueva != vm.PasswordConfirmar)
            {
                TempData["ErrorPassword"] = "Las contraseñas no coinciden o están vacías.";
                TempData["PasswordVerificada"] = "true";
                return RedirectToAction("Index");
            }

            if (vm.PasswordNueva.Length < 6)
            {
                TempData["ErrorPassword"] = "La contraseña debe tener mínimo 6 caracteres.";
                TempData["PasswordVerificada"] = "true";
                return RedirectToAction("Index");
            }

            var usuario = await _db.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return RedirectToAction("Login", "Account");

            usuario.PasswordHash = HashTexto(vm.PasswordNueva);
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("PasswordToken");

            TempData["ExitoPassword"] = "Contraseña cambiada correctamente.";
            return RedirectToAction("Index");
        }

        // ── Pregunta: Paso 1 — Verificar contraseña ──
        [HttpPost]
        public async Task<IActionResult> VerificarParaPregunta(string passwordConfirm)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            var usuario = await _db.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return RedirectToAction("Login", "Account");

            if (usuario.PasswordHash != HashTexto(passwordConfirm))
            {
                TempData["ErrorPregunta"] = "Contraseña incorrecta. No se puede editar la pregunta secreta.";
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetString("PreguntaToken", Guid.NewGuid().ToString());
            TempData["PreguntaVerificada"] = "true";
            return RedirectToAction("Index");
        }

        // ── Pregunta: Paso 2 — Guardar pregunta secreta ──
        [HttpPost]
        public async Task<IActionResult> GuardarPregunta(string preguntaSecreta, string respuestaSecreta)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("PreguntaToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorPregunta"] = "Sesión inválida. Verifica tu contraseña primero.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(preguntaSecreta) || string.IsNullOrEmpty(respuestaSecreta))
            {
                TempData["ErrorPregunta"] = "La pregunta y respuesta no pueden estar vacías.";
                TempData["PreguntaVerificada"] = "true";
                return RedirectToAction("Index");
            }

            var usuario = await _db.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return RedirectToAction("Login", "Account");

            usuario.PreguntaSecreta = preguntaSecreta;
            usuario.RespuestaSecretaHash = HashTexto(respuestaSecreta.ToLower().Trim());
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("PreguntaToken");

            TempData["ExitoPregunta"] = "Pregunta secreta guardada correctamente.";
            return RedirectToAction("Index");
        }
    }
}