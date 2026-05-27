using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chronos.Domain.Entities
{
    public class Tarea
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Curso { get; set; } = string.Empty;

        public DateTime FechaEntrega { get; set; }

        [Required]
        public string Prioridad { get; set; } = "Media";

        public string Estado { get; set; } = "Pendiente";

        public DateTime CreadoEn { get; set; } = DateTime.Now;

        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}
