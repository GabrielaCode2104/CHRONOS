using Chronos.Domain.Entities;
using Chronos.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chronos.Web.Controllers
{
    public class TareasController : Controller
    {
        private readonly ChronosDbContext _db;

        public TareasController(ChronosDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            var tareas = await _db.Tareas
                .Where(t => t.UsuarioId == usuarioId)
                .OrderBy(t => t.FechaEntrega)
                .ToListAsync();

            return View(tareas);
        }

        public IActionResult Crear()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Tarea tarea, string fechaSolo, int horaH, string horaM, string ampm)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            // Convertir a formato 24h
            int hora24 = ampm == "PM" && horaH != 12 ? horaH + 12 : (ampm == "AM" && horaH == 12 ? 0 : horaH);
            var fecha = DateTime.Parse(fechaSolo);
            tarea.FechaEntrega = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora24, int.Parse(horaM), 0);
            tarea.UsuarioId = usuarioId.Value;
            tarea.CreadoEn = DateTime.Now;
            _db.Tareas.Add(tarea);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            var tarea = await _db.Tareas.FindAsync(id);
            if (tarea == null || tarea.UsuarioId != usuarioId) return NotFound();
            return View(tarea);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Tarea tarea, string fechaSolo, int horaH, string horaM, string ampm)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            int hora24 = ampm == "PM" && horaH != 12 ? horaH + 12 : (ampm == "AM" && horaH == 12 ? 0 : horaH);
            var fecha = DateTime.Parse(fechaSolo);
            tarea.FechaEntrega = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora24, int.Parse(horaM), 0);
            tarea.UsuarioId = usuarioId.Value;
            _db.Tareas.Update(tarea);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var tarea = await _db.Tareas.FindAsync(id);
            if (tarea != null) { _db.Tareas.Remove(tarea); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Completar(int id)
        {
            var tarea = await _db.Tareas.FindAsync(id);
            if (tarea != null) { tarea.Estado = "Entregada"; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}
