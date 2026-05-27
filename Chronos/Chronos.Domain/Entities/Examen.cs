using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chronos.Domain.Entities
{
    public class Examen
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Curso { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Tema { get; set; } = string.Empty;

        public DateTime FechaExamen { get; set; }

        [Required, MaxLength(100)]
        public string Lugar { get; set; } = string.Empty;

        public string Prioridad { get; set; } = "Media";

        public string Estado { get; set; } = "Pendiente";

        public DateTime CreadoEn { get; set; } = DateTime.Now;

        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}
