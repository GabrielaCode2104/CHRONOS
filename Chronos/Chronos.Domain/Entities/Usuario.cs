using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Chronos.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string Carrera { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime FechaNacimiento { get; set; }

        public int RecordatorioHoras { get; set; } = 24;

        public string PreguntaSecreta { get; set; } = string.Empty;

        public string RespuestaSecretaHash { get; set; } = string.Empty;

        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
        public ICollection<Examen> Examenes { get; set; } = new List<Examen>();
    }
}