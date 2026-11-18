using System.ComponentModel.DataAnnotations;

namespace MiWebAPI.Models
{
    public class CitaCrear
    {
        [Required]
        public Guid MedicoId { get; set; }

        // Para presencial: el consultorio del slot. Si no lo mandas, el SP usa el primero activo.
        public Guid? ConsultorioId { get; set; }

        // Usa exactamente el "fechaHora" que devolvió /slots, sin zona horaria (ej. 2026-11-17T09:30:00)
        [Required]
        public DateTime FechaHora { get; set; }

        [RegularExpression("^(presencial|video)$", ErrorMessage = "La modalidad debe ser 'presencial' o 'video'")]
        public string Modalidad { get; set; } = "presencial";

        [StringLength(1000)]
        public string? Motivo { get; set; }
    }

    public class ResultadoCita
    {
        public int NumeroError { get; set; }
        public string Respuesta { get; set; } = string.Empty;
        public Guid? IdCita { get; set; }
        public string? Folio { get; set; }          // VG-123
        public bool IsSuccess => NumeroError == 0;
    }

    public class CitaDetalle
    {
        public Guid Id { get; set; }
        public string Folio { get; set; } = string.Empty;        // VG-12
        public DateTime FechaHora { get; set; }
        public int DuracionMin { get; set; }
        public string Estado { get; set; } = string.Empty;       // confirmada, cancelada, ...
        public decimal Costo { get; set; }
        public string Moneda { get; set; } = "MXN";
        public string Modalidad { get; set; } = string.Empty;    // presencial | video
        public string? Motivo { get; set; }
        public Guid MedicoId { get; set; }
        public string Doctor { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public string? Consultorio { get; set; }                 // null si es videoconsulta
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string Paciente { get; set; } = string.Empty;
    }
}