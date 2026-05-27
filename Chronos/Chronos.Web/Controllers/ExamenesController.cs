using Chronos.Domain.Entities;
using Chronos.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chronos.Web.Controllers
{
    public class ExamenesController : Controller
    {
        private readonly ChronosDbContext _db;

        public ExamenesController(ChronosDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            var examenes = await _db.Examenes
                .Where(e => e.UsuarioId == usuarioId)
                .OrderBy(e => e.FechaExamen)
                .ToListAsync();

            return View(examenes);
        }

        public IActionResult Crear()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Examen examen, string fechaSolo, int horaH, string horaM, string ampm)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            int hora24 = ampm == "PM" && horaH != 12 ? horaH + 12 : (ampm == "AM" && horaH == 12 ? 0 : horaH);
            var fecha = DateTime.Parse(fechaSolo);
            examen.FechaExamen = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora24, int.Parse(horaM), 0);
            examen.UsuarioId = usuarioId.Value;
            examen.CreadoEn = DateTime.Now;
            _db.Examenes.Add(examen);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            var examen = await _db.Examenes.FindAsync(id);
            if (examen == null || examen.UsuarioId != usuarioId) return NotFound();
            return View(examen);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Examen examen, string fechaSolo, int horaH, string horaM, string ampm)
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            int hora24 = ampm == "PM" && horaH != 12 ? horaH + 12 : (ampm == "AM" && horaH == 12 ? 0 : horaH);
            var fecha = DateTime.Parse(fechaSolo);
            examen.FechaExamen = new DateTime(fecha.Year, fecha.Month, fecha.Day, hora24, int.Parse(horaM), 0);
            examen.UsuarioId = usuarioId.Value;
            _db.Examenes.Update(examen);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var examen = await _db.Examenes.FindAsync(id);
            if (examen != null) { _db.Examenes.Remove(examen); await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Completar(int id)
        {
            var examen = await _db.Examenes.FindAsync(id);
            if (examen != null) { examen.Estado = "Rendido"; await _db.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}
