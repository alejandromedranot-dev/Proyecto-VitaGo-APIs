using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiWebAPI.Data;
using MiWebAPI.Models;
using System.Globalization;
using System.Security.Claims;

namespace MiWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]   // por defecto TODO requiere sesión; lo público se marca con [AllowAnonymous]
    public class DoctoresController : ControllerBase
    {
        private readonly DoctoresData _doctoresData;

        public DoctoresController(DoctoresData doctoresData)
        {
            _doctoresData = doctoresData;
        }

        // ===================== PÚBLICO =====================

        // GET api/doctores/buscar?especialidad=&ciudad=&soloVideo=
        [HttpGet("buscar")]
        [AllowAnonymous]
        public async Task<IActionResult> Buscar(
            [FromQuery] string? especialidad,
            [FromQuery] string? ciudad,
            [FromQuery] bool soloVideo = false)
        {
            List<MedicoBusqueda> lista = await _doctoresData.Buscar(especialidad, ciudad, soloVideo);
            return Ok(lista);
        }

        // GET api/doctores/{id}/perfil
        [HttpGet("{id:guid}/perfil")]
        [AllowAnonymous]
        public async Task<IActionResult> Perfil(Guid id)
        {
            MedicoPerfil? perfil = await _doctoresData.Perfil(id);

            if (perfil is null)
                return NotFound(new RespuestaOperacion
                {
                    NumeroError = 404,
                    Respuesta = "El médico no existe o aún no está aprobado"
                });

            return Ok(perfil);
        }

        // GET api/doctores/{id}/slots?fecha=2026-11-17
        [HttpGet("{id:guid}/slots")]
        [AllowAnonymous]
        public async Task<IActionResult> Slots(Guid id, [FromQuery] string? fecha)
        {
            if (!DateOnly.TryParseExact(fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out DateOnly dia))
                return BadRequest(new RespuestaOperacion
                {
                    NumeroError = 400,
                    Respuesta = "La fecha es obligatoria y debe tener el formato aaaa-mm-dd (por ejemplo 2026-11-17)"
                });

            if (dia < DateOnly.FromDateTime(DateTime.Today))
                return BadRequest(new RespuestaOperacion
                {
                    NumeroError = 400,
                    Respuesta = "La fecha no puede ser anterior a hoy"
                });

            List<SlotDisponible> slots = await _doctoresData.Slots(id, dia);
            return Ok(slots);
        }

        // POST api/doctores   (registro de médico; queda "pendiente" hasta que lo aprueben)
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Crear([FromBody] DoctorRegistro objeto)
        {
            RespuestaOperacion resultado = await _doctoresData.Crear(objeto);
            return resultado.IsSuccess ? Ok(resultado) : BadRequest(resultado);
        }

        // ===================== SOLO ADMIN =====================

        // GET api/doctores   (datos completos de todos, incluida cédula y correo)
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Lista()
        {
            List<Doctor> lista = await _doctoresData.Lista();
            return Ok(lista);
        }

        // GET api/doctores/{id}
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Obtener(Guid id)
        {
            Doctor? doctor = await _doctoresData.Obtener(id);

            if (doctor is null)
                return NotFound(new RespuestaOperacion { NumeroError = 404, Respuesta = "El doctor no existe" });

            return Ok(doctor);
        }

        // DELETE api/doctores/{id}  (baja lógica)
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Eliminar(Guid id)
        {
            RespuestaOperacion resultado = await _doctoresData.Eliminar(id);
            return resultado.IsSuccess ? Ok(resultado) : BadRequest(resultado);
        }

        // ===================== EL PROPIO MÉDICO O ADMIN =====================

        // PUT api/doctores/{id}
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "admin,medico")]
        public async Task<IActionResult> Editar(Guid id, [FromBody] Doctor objeto)
        {
            // Para un médico, "perfil_id" del token es su MEDICO.id
            bool esAdmin = User.IsInRole("admin");
            bool esElMismo = Guid.TryParse(User.FindFirstValue("perfil_id"), out Guid perfilId) && perfilId == id;

            if (!esAdmin && !esElMismo)
                return StatusCode(StatusCodes.Status403Forbidden, new RespuestaOperacion
                {
                    NumeroError = 403,
                    Respuesta = "Solo puedes editar tu propio perfil"
                });

            objeto.Id = id;
            RespuestaOperacion resultado = await _doctoresData.Editar(objeto);
            return resultado.IsSuccess ? Ok(resultado) : BadRequest(resultado);
        }
    }
}