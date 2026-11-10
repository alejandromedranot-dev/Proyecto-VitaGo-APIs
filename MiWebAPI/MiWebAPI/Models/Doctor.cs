using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MiWebAPI.Models
{
    public class Doctor
    {
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }

        [Required, StringLength(80)]
        public string? Nombre { get; set; }

        [Required, StringLength(120)]
        public string? Apellidos { get; set; }

        [Required, StringLength(80)]
        public string? Especialidad { get; set; }

        [Required, StringLength(20)]
        public string? Cedula { get; set; }

        [Required, EmailAddress, StringLength(254)]
        public string? Correo { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        public string? Bio { get; set; }
        public int AniosExperiencia { get; set; }
        public int NumPacientes { get; set; }
        public string? Idiomas { get; set; }

        [Range(0, 100000)]
        public decimal PrecioConsulta { get; set; }

        public bool ConsultaOnline { get; set; }
        public bool Verificado { get; set; }
        public string? EstadoRegistro { get; set; }
        public decimal? RatingPromedio { get; set; }
        public int TotalResenas { get; set; }
        public string? FotoUrl { get; set; }

        // Solo se usa al crear; nunca se devuelve en las respuestas
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Password { get; set; }
    }
}