using System.ComponentModel.DataAnnotations;

namespace MiWebAPI.Models
{
    public class DoctorRegistro
    {
        // Cuenta (USUARIO)
        [Required, EmailAddress, StringLength(254)] public string Correo { get; set; } = string.Empty;
        [Required, StringLength(72, MinimumLength = 8)] public string Password { get; set; } = string.Empty;

        // Médico (MEDICO)
        [Required, StringLength(80)] public string Nombre { get; set; } = string.Empty;
        [Required, StringLength(120)] public string Apellidos { get; set; } = string.Empty;
        [Required, StringLength(20)] public string Cedula { get; set; } = string.Empty;
        [StringLength(20)] public string? Telefono { get; set; }
        [Range(0, 100000)] public decimal PrecioConsulta { get; set; }
        public string? Bio { get; set; }
        [Range(0, 80)] public byte? AniosExperiencia { get; set; }
        [StringLength(100)] public string? Idiomas { get; set; }
        [StringLength(500)] public string? FotoUrl { get; set; }
        public bool ConsultaOnline { get; set; }

        // Especialidad principal (ESPECIALIDAD)
        [Required, StringLength(80)] public string Especialidad { get; set; } = string.Empty;

        // Consultorio y horario (CONSULTORIO + HORARIO)
        [Required] public ConsultorioRegistro Consultorio { get; set; } = new();
    }

    public class ConsultorioRegistro
    {
        [Required, StringLength(120)] public string Nombre { get; set; } = string.Empty;
        [Required, StringLength(200)] public string Direccion { get; set; } = string.Empty;
        [Required, StringLength(80)] public string Ciudad { get; set; } = string.Empty;
        [StringLength(80)] public string? Estado { get; set; }
        [StringLength(10)] public string? Cp { get; set; }
        [Range(-90, 90)] public decimal? Latitud { get; set; }
        [Range(-180, 180)] public decimal? Longitud { get; set; }

        [Range(1, 7)] public byte DiaInicio { get; set; } = 1;   // 1 = lunes
        [Range(1, 7)] public byte DiaFin { get; set; } = 5;      // 7 = domingo
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Formato HH:mm")]
        public string HoraInicio { get; set; } = "09:00";
        [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$", ErrorMessage = "Formato HH:mm")]
        public string HoraFin { get; set; } = "17:00";
        [Range(10, 240)] public short DuracionSlotMin { get; set; } = 30;
    }
}