using Chronos.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Chronos.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ChronosDbContext _db;
        public DashboardController(ChronosDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (usuarioId == null) return RedirectToAction("Login", "Account");

            var ahora = DateTime.Now;
            var limite = ahora.AddDays(15);

            // Actividades próximas para urgentes
            var tareas = await _db.Tareas
                .Where(t => t.UsuarioId == usuarioId && t.Estado == "Pendiente"
                         && t.FechaEntrega >= ahora && t.FechaEntrega <= limite)
                .ToListAsync();

            var examenes = await _db.Examenes
                .Where(e => e.UsuarioId == usuarioId && e.Estado == "Pendiente"
                         && e.FechaExamen >= ahora && e.FechaExamen <= limite)
                .ToListAsync();

            var prioridadOrden = new Dictionary<string, int>
                { {"Alta",1}, {"Media",2}, {"Baja",3} };

            var actividades = tareas.Select(t => new {
                Titulo = t.Titulo,
                Subtitulo = t.Curso,
                Fecha = t.FechaEntrega,
                Prioridad = t.Prioridad,
                Tipo = "Tarea",
                Lugar = "",
                DiasRestantes = (t.FechaEntrega - ahora).Days,
                PrioridadOrden = prioridadOrden.GetValueOrDefault(t.Prioridad, 3)
            }).Concat(examenes.Select(e => new {
                Titulo = e.Curso,
                Subtitulo = e.Tema,
                Fecha = e.FechaExamen,
                Prioridad = e.Prioridad,
                Tipo = "Examen",
                Lugar = e.Lugar,
                DiasRestantes = (e.FechaExamen - ahora).Days,
                PrioridadOrden = prioridadOrden.GetValueOrDefault(e.Prioridad, 3)
            })).OrderBy(a => a.DiasRestantes).ThenBy(a => a.PrioridadOrden).ToList();

            ViewBag.Actividades = actividades;
            ViewBag.Nombre = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.Fecha = ahora.ToString("dddd, dd 'de' MMMM yyyy",
                             new System.Globalization.CultureInfo("es-PE"));
            ViewBag.Hoy = ahora;

            // Estadísticas
            var todasTareas = await _db.Tareas.Where(t => t.UsuarioId == usuarioId).ToListAsync();
            var todosExamenes = await _db.Examenes.Where(e => e.UsuarioId == usuarioId).ToListAsync();

            // % completado de tareas
            int totalTareas = todasTareas.Count;
            int tareasEntregadas = todasTareas.Count(t => t.Estado == "Entregada");
            ViewBag.PorcentajeTareas = totalTareas > 0
                ? (int)Math.Round((double)tareasEntregadas / totalTareas * 100) : 0;

            // % completado de exámenes
            int totalExamenes = todosExamenes.Count;
            int examenesRendidos = todosExamenes.Count(e => e.Estado == "Rendido");
            ViewBag.PorcentajeExamenes = totalExamenes > 0
                ? (int)Math.Round((double)examenesRendidos / totalExamenes * 100) : 0;

            // Tareas vencidas
            ViewBag.TareasVencidas = todasTareas
                .Count(t => t.Estado == "Pendiente" && t.FechaEntrega < ahora);

            // Tareas por entregar esta semana
            var finSemana = ahora.AddDays(7);
            ViewBag.TareasSemana = todasTareas
                .Count(t => t.Estado == "Pendiente" && t.FechaEntrega >= ahora && t.FechaEntrega <= finSemana);

            // Exámenes esta semana
            ViewBag.ExamenesSemana = todosExamenes
                .Count(e => e.Estado == "Pendiente" && e.FechaExamen >= ahora && e.FechaExamen <= finSemana);

            return View();
        }
    }
}